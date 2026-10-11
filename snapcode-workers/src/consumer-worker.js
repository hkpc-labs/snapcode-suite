// ============================================================
// SnapCode — Consumer Worker (Queue Handler)
// Reads from Cloudflare Queue, generates Dart, zips, uploads to R2
// Bindings: D1, KV, R2 (EXPORTS_BUCKET)
// ============================================================

import { buildFlutterProject, createZip } from './codegen/projectBuilder.js';

export default {
  /**
   * Queue handler — called automatically when messages arrive.
   */
  async queue(batch, env, ctx) {
    for (const message of batch.messages) {
      try {
        await processExportJob(message.body, env);
        message.ack(); // Acknowledge successful processing
      } catch (err) {
        console.error(`Failed to process job ${message.body?.job_id}:`, err);

        // Update KV with failure status
        if (message.body?.job_id) {
          await markJobFailed(message.body.job_id, message.body.user_id, err.message, env);
        }

        message.retry(); // Retry the message
      }
    }
  },
};

/**
 * Process a single export job.
 */
async function processExportJob(jobData, env) {
  const { job_id, project_id, user_id } = jobData;

  if (!job_id || !project_id || !user_id) {
    throw new Error('Invalid job data: missing required fields');
  }

  // 1. Read the project's AST from D1
  const project = await env.DB.prepare(
    `SELECT blockly_ast, dart_config, name FROM projects WHERE id = ?`
  ).bind(project_id).first();

  if (!project) {
    throw new Error(`Project ${project_id} not found`);
  }

  // 2. Parse AST and config
  let workspaceJson = {};
  try {
    workspaceJson = JSON.parse(project.blockly_ast || '{}');
  } catch {
    throw new Error('Invalid AST JSON in project');
  }

  let dartConfig = {};
  try {
    dartConfig = JSON.parse(project.dart_config || '{}');
  } catch {
    // Use defaults
  }
  dartConfig.project_name = sanitizeProjectName(project.name);

  // 3. Generate Flutter project files
  const files = buildFlutterProject(workspaceJson, dartConfig);

  // 4. Create ZIP archive
  const zipBytes = createZip(files);

  // 5. Upload to R2
  const r2Key = `exports/${user_id}/${job_id}/snapcode_project.zip`;
  await env.EXPORTS_BUCKET.put(r2Key, zipBytes, {
    httpMetadata: {
      contentType: 'application/zip',
    },
    customMetadata: {
      project_id,
      user_id,
      job_id,
    },
  });

  // 6. Generate a download URL (presigned or public)
  // Note: For simplicity, we use the R2 public URL pattern.
  // In production, use presigned URLs with expiry.
  const downloadUrl = `https://${env.R2_PUBLIC_DOMAIN || 'snapcode-exports.r2.dev'}/${r2Key}`;

  // 7. Update KV with completed status
  const kvPayload = {
    status: 'completed',
    project_id,
    user_id,
    created_at: jobData.created_at || new Date().toISOString(),
    completed_at: new Date().toISOString(),
    download_url: downloadUrl,
    file_size_bytes: zipBytes.length,
  };
  await env.KV.put(`export:${job_id}`, JSON.stringify(kvPayload), {
    expirationTtl: 86400,
  });

  // 8. Update D1 exports table
  await env.DB.prepare(
    `UPDATE exports
     SET status = 'completed',
         r2_object_key = ?,
         download_url = ?,
         file_size_bytes = ?,
         completed_at = datetime('now')
     WHERE id = ?`
  ).bind(r2Key, downloadUrl, zipBytes.length, job_id).run();

  console.log(`Export job ${job_id} completed: ${zipBytes.length} bytes`);
}

/**
 * Mark a job as failed in KV and D1.
 */
async function markJobFailed(jobId, userId, errorMessage, env) {
  try {
    const kvPayload = {
      status: 'failed',
      user_id: userId,
      failed_at: new Date().toISOString(),
      error_message: errorMessage,
    };
    await env.KV.put(`export:${jobId}`, JSON.stringify(kvPayload), {
      expirationTtl: 86400,
    });

    await env.DB.prepare(
      `UPDATE exports SET status = 'failed', error_message = ?, completed_at = datetime('now')
       WHERE id = ?`
    ).bind(errorMessage, jobId).run();
  } catch (err) {
    console.error('Failed to mark job as failed:', err);
  }
}

/**
 * Sanitize project name for use as Dart package name.
 */
function sanitizeProjectName(name) {
  return (name || 'snapcode_app')
    .toLowerCase()
    .replace(/[^a-z0-9_]/g, '_')
    .replace(/^[0-9]/, '_')
    .replace(/__+/g, '_')
    .slice(0, 30);
}
