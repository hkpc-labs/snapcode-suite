// ============================================================
// SnapCode — State Worker (Synchronous CRUD)
// Handles: /api/users, /api/projects/*, /api/status/*
// Bindings: D1 (database), KV (job status)
// ============================================================

import { authMiddleware } from './middleware/auth.js';

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    const path = url.pathname;
    const method = request.method;

    // CORS headers
    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    };

    // Handle CORS preflight
    if (method === 'OPTIONS') {
      return new Response(null, { status: 204, headers: corsHeaders });
    }

    // ── Auth check ───────────────────────────────────────
    const authResult = await authMiddleware(request, env);
    if (authResult instanceof Response) {
      // Auth failed — return the 401 response
      return new Response(authResult.body, {
        status: authResult.status,
        headers: { ...corsHeaders, 'Content-Type': 'application/json' },
      });
    }
    const user = authResult; // Decoded user claims

    try {
      // ── Route: POST /api/users (upsert on first login) ──
      if (path === '/api/users' && method === 'POST') {
        await env.DB.prepare(
          `INSERT INTO users (id, email, display_name, auth_provider_id)
           VALUES (?, ?, ?, ?)
           ON CONFLICT(id) DO UPDATE SET
             display_name = excluded.display_name,
             updated_at = datetime('now')`
        ).bind(user.uid, user.email, user.name || user.email, user.uid).run();

        return json({ success: true, user_id: user.uid }, corsHeaders);
      }

      // ── Route: GET /api/projects ────────────────────────
      if (path === '/api/projects' && method === 'GET') {
        const { results } = await env.DB.prepare(
          `SELECT id, name, description, target_platforms, version,
                  json_extract(blockly_ast, '$.metadata.total_block_count') as block_count,
                  created_at, updated_at
           FROM projects WHERE user_id = ? ORDER BY updated_at DESC`
        ).bind(user.uid).all();

        return json({ projects: results || [] }, corsHeaders);
      }

      // ── Route: POST /api/projects ───────────────────────
      if (path === '/api/projects' && method === 'POST') {
        const body = await request.json();
        const id = crypto.randomUUID();

        await env.DB.prepare(
          `INSERT INTO projects (id, user_id, name, description, target_platforms)
           VALUES (?, ?, ?, ?, ?)`
        ).bind(id, user.uid, body.name, body.description || '', JSON.stringify(body.target_platforms || ['mobile'])).run();

        return json({ id, name: body.name }, corsHeaders, 201);
      }

      // ── Route: GET /api/projects/:id ────────────────────
      const projectMatch = path.match(/^\/api\/projects\/([a-f0-9-]+)$/);
      if (projectMatch && method === 'GET') {
        const projectId = projectMatch[1];
        const project = await env.DB.prepare(
          `SELECT * FROM projects WHERE id = ? AND user_id = ?`
        ).bind(projectId, user.uid).first();

        if (!project) return json({ error: 'Project not found' }, corsHeaders, 404);
        return json({ project }, corsHeaders);
      }

      // ── Route: PUT /api/projects/:id ────────────────────
      if (projectMatch && method === 'PUT') {
        const projectId = projectMatch[1];
        const body = await request.json();

        // Optimistic concurrency: check version
        const result = await env.DB.prepare(
          `UPDATE projects
           SET name = COALESCE(?, name),
               description = COALESCE(?, description),
               target_platforms = COALESCE(?, target_platforms),
               blockly_ast = COALESCE(?, blockly_ast),
               dart_config = COALESCE(?, dart_config),
               version = version + 1,
               updated_at = datetime('now')
           WHERE id = ? AND user_id = ? AND version = ?`
        ).bind(
          body.name || null,
          body.description || null,
          body.target_platforms ? JSON.stringify(body.target_platforms) : null,
          body.blockly_ast || null,
          body.dart_config || null,
          projectId,
          user.uid,
          body.version || 1
        ).run();

        if (result.meta.changes === 0) {
          return json({ error: 'Conflict — project was modified elsewhere' }, corsHeaders, 409);
        }

        return json({ success: true, version: (body.version || 1) + 1 }, corsHeaders);
      }

      // ── Route: DELETE /api/projects/:id ─────────────────
      if (projectMatch && method === 'DELETE') {
        const projectId = projectMatch[1];
        await env.DB.prepare(
          `DELETE FROM projects WHERE id = ? AND user_id = ?`
        ).bind(projectId, user.uid).run();

        return json({ success: true }, corsHeaders);
      }

      // ── Route: GET /api/status/:jobId ───────────────────
      const statusMatch = path.match(/^\/api\/status\/([a-f0-9-]+)$/);
      if (statusMatch && method === 'GET') {
        const jobId = statusMatch[1];
        const statusData = await env.KV.get(`export:${jobId}`, 'json');

        if (!statusData) return json({ error: 'Job not found' }, corsHeaders, 404);

        // Verify ownership
        if (statusData.user_id !== user.uid) {
          return json({ error: 'Unauthorized' }, corsHeaders, 403);
        }

        return json(statusData, corsHeaders);
      }

      // ── 404 fallback ───────────────────────────────────
      return json({ error: 'Not found' }, corsHeaders, 404);

    } catch (err) {
      console.error('State Worker error:', err);
      return json({ error: 'Internal server error' }, corsHeaders, 500);
    }
  },
};

function json(data, corsHeaders, status = 200) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { ...corsHeaders, 'Content-Type': 'application/json' },
  });
}
