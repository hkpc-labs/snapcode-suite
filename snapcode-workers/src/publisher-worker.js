// ============================================================
// SnapCode — Publisher Worker (Async Export Pipeline)
// Handles: POST /api/export/:projectId
// Bindings: D1, KV, QUEUE
// ============================================================

import { authMiddleware } from './middleware/auth.js';

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    const method = request.method;

    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    };

    if (method === 'OPTIONS') {
      return new Response(null, { status: 204, headers: corsHeaders });
    }

    // Auth check
    const authResult = await authMiddleware(request, env);
    if (authResult instanceof Response) {
      return new Response(authResult.body, {
        status: authResult.status,
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
      });
    }
    const user = authResult;

    // ── Route: POST /api/export/:projectId ────────────────
    const match = url.pathname.match(/^\/api\/export\/([a-f0-9-]+)$/);
    if (!match || method !== 'POST') {
      return json({ error: 'Not found' }, corsHeaders, 404);
    }

    const projectId = match[1];

    try {
      // Verify project exists and belongs to user
      const project = await env.DB.prepare(
        `SELECT id, name FROM projects WHERE id = ? AND user_id = ?`
      ).bind(projectId, user.uid).first();

      if (!project) {
        return json({ error: 'Project not found' }, corsHeaders, 404);
      }

      // Generate job ID
      const jobId = crypto.randomUUID();

      // Write initial status to KV (TTL: 24 hours)
      const kvPayload = {
        status: 'processing',
        project_id: projectId,
        user_id: user.uid,
        created_at: new Date().toISOString(),
      };
      await env.KV.put(`export:${jobId}`, JSON.stringify(kvPayload), {
        expirationTtl: 86400,
      });

      // Insert export record into D1
      await env.DB.prepare(
        `INSERT INTO exports (id, project_id, user_id, status)
         VALUES (?, ?, ?, 'processing')`
      ).bind(jobId, projectId, user.uid).run();

      // Enqueue the job for the Consumer Worker
      await env.QUEUE.send({
        job_id: jobId,
        project_id: projectId,
        user_id: user.uid,
      });

      // Return 202 Accepted immediately
      return json({ job_id: jobId, status: 'processing' }, corsHeaders, 202);

    } catch (err) {
      console.error('Publisher Worker error:', err);
      return json({ error: 'Failed to start export' }, corsHeaders, 500);
    }
  },
};

function json(data, corsHeaders, status = 200) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { ...corsHeaders, 'Content-Type': 'application/json' },
  });
}
