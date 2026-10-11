import { serve } from '@hono/node-server';
import { serveStatic } from '@hono/node-server/serve-static';
import { Hono } from 'hono';
import { cors } from 'hono/cors';
import { exec } from 'child_process';
import util from 'util';
import fs from 'fs';
import path from 'path';
import crypto from 'crypto';
import AdmZip from 'adm-zip';

const execAsync = util.promisify(exec);
const app = new Hono();

app.use('*', cors());

// Health check
app.get('/', (c) => c.text('SnapCode Compiler API (Hono) is running!'));

// Serve compiled apps statically
app.use('/apps/*', serveStatic({ root: './' }));

// Compile endpoint
app.post('/compile/web', async (c) => {
  try {
    const body = await c.req.arrayBuffer();
    
    // Create unique build directory
    const buildId = crypto.randomUUID();
    const buildDir = path.join(process.cwd(), 'builds', buildId);
    
    // Ensure builds directory exists
    if (!fs.existsSync(path.join(process.cwd(), 'builds'))) {
      fs.mkdirSync(path.join(process.cwd(), 'builds'));
    }
    
    fs.mkdirSync(buildDir);

    // Save incoming ZIP
    const zipPath = path.join(buildDir, 'project.zip');
    fs.writeFileSync(zipPath, Buffer.from(body));

    // Extract ZIP
    const zip = new AdmZip(zipPath);
    zip.extractAllTo(buildDir, true);

    // Ensure it's a valid Flutter project
    if (!fs.existsSync(path.join(buildDir, 'pubspec.yaml'))) {
      return c.json({ error: 'Invalid Flutter project structure' }, 400);
    }

    // Run Flutter build web
    console.log(`Building project ${buildId}...`);
    try {
      // In production (Docker), the Flutter SDK must be in PATH
      await execAsync('flutter pub get', { cwd: buildDir });
      await execAsync('flutter build web --web-renderer canvaskit', { cwd: buildDir });
    } catch (buildError) {
      console.error('Build failed:', buildError.stderr || buildError.message);
      return c.json({ error: 'Build failed', details: buildError.stderr || buildError.message }, 500);
    }

    // Move compiled files to a public serving directory
    const appsDir = path.join(process.cwd(), 'apps', buildId);
    if (!fs.existsSync(path.join(process.cwd(), 'apps'))) {
      fs.mkdirSync(path.join(process.cwd(), 'apps'));
    }
    
    fs.renameSync(path.join(buildDir, 'build', 'web'), appsDir);

    // Clean up temporary build directory
    fs.rmSync(buildDir, { recursive: true, force: true });

    // Return the URL to access the compiled app
    const appUrl = `/apps/${buildId}/index.html`;
    return c.json({ success: true, url: appUrl });

  } catch (err) {
    console.error('Compiler error:', err);
    return c.json({ error: 'Internal server error', details: err.message }, 500);
  }
});

const port = process.env.PORT || 8080;
console.log(`Server is running on port ${port}`);

serve({
  fetch: app.fetch,
  port
});
