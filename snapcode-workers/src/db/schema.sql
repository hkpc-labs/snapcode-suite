-- ============================================================
-- SnapCode D1 Schema v1.0
-- Target: Cloudflare D1 (Serverless SQLite)
-- ============================================================

-- ── USERS ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS users (
    id              TEXT PRIMARY KEY,
    email           TEXT NOT NULL UNIQUE,
    display_name    TEXT NOT NULL,
    avatar_url      TEXT,
    auth_provider   TEXT NOT NULL DEFAULT 'firebase',
    auth_provider_id TEXT NOT NULL,
    created_at      TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at      TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_provider ON users(auth_provider, auth_provider_id);

-- ── PROJECTS ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS projects (
    id              TEXT PRIMARY KEY,
    user_id         TEXT NOT NULL,
    name            TEXT NOT NULL,
    description     TEXT DEFAULT '',
    target_platforms TEXT NOT NULL DEFAULT '["mobile"]',
    blockly_ast     TEXT NOT NULL DEFAULT '{}',
    dart_config     TEXT NOT NULL DEFAULT '{}',
    version         INTEGER NOT NULL DEFAULT 1,
    created_at      TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at      TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_projects_user_id ON projects(user_id, updated_at DESC);
CREATE INDEX IF NOT EXISTS idx_projects_name ON projects(user_id, name);

-- ── EXPORTS ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS exports (
    id              TEXT PRIMARY KEY,
    project_id      TEXT NOT NULL,
    user_id         TEXT NOT NULL,
    status          TEXT NOT NULL DEFAULT 'processing',
    r2_object_key   TEXT,
    download_url    TEXT,
    file_size_bytes INTEGER,
    error_message   TEXT,
    created_at      TEXT NOT NULL DEFAULT (datetime('now')),
    completed_at    TEXT,
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_exports_user_id ON exports(user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_exports_project_id ON exports(project_id, created_at DESC);

-- ── PROJECT TEMPLATES (Optional) ─────────────────────────
CREATE TABLE IF NOT EXISTS project_templates (
    id              TEXT PRIMARY KEY,
    name            TEXT NOT NULL,
    description     TEXT DEFAULT '',
    category        TEXT NOT NULL DEFAULT 'general',
    blockly_ast     TEXT NOT NULL DEFAULT '{}',
    dart_config     TEXT NOT NULL DEFAULT '{}',
    author_id       TEXT,
    is_public       INTEGER NOT NULL DEFAULT 1,
    created_at      TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at      TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (author_id) REFERENCES users(id) ON DELETE SET NULL
);
