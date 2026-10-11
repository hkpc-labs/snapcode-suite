var __defProp = Object.defineProperty;
var __name = (target, value) => __defProp(target, "name", { value, configurable: true });

// .wrangler/tmp/bundle-nuzaF7/checked-fetch.js
var urls = /* @__PURE__ */ new Set();
function checkURL(request, init) {
  const url = request instanceof URL ? request : new URL(
    (typeof request === "string" ? new Request(request, init) : request).url
  );
  if (url.port && url.port !== "443" && url.protocol === "https:") {
    if (!urls.has(url.toString())) {
      urls.add(url.toString());
      console.warn(
        `WARNING: known issue with \`fetch()\` requests to custom HTTPS ports in published Workers:
 - ${url.toString()} - the custom port will be ignored when the Worker is published using the \`wrangler deploy\` command.
`
      );
    }
  }
}
__name(checkURL, "checkURL");
globalThis.fetch = new Proxy(globalThis.fetch, {
  apply(target, thisArg, argArray) {
    const [request, init] = argArray;
    checkURL(request, init);
    return Reflect.apply(target, thisArg, argArray);
  }
});

// .wrangler/tmp/bundle-nuzaF7/strip-cf-connecting-ip-header.js
function stripCfConnectingIPHeader(input, init) {
  const request = new Request(input, init);
  request.headers.delete("CF-Connecting-IP");
  return request;
}
__name(stripCfConnectingIPHeader, "stripCfConnectingIPHeader");
globalThis.fetch = new Proxy(globalThis.fetch, {
  apply(target, thisArg, argArray) {
    return Reflect.apply(target, thisArg, [
      stripCfConnectingIPHeader.apply(null, argArray)
    ]);
  }
});

// src/middleware/auth.js
var FIREBASE_CERTS_URL = "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";
var cachedKeys = null;
var cacheExpiry = 0;
async function getFirebasePublicKeys() {
  const now = Date.now();
  if (cachedKeys && now < cacheExpiry)
    return cachedKeys;
  const response = await fetch(FIREBASE_CERTS_URL);
  if (!response.ok)
    throw new Error("Failed to fetch Firebase public keys");
  const cacheControl = response.headers.get("Cache-Control") || "";
  const maxAgeMatch = cacheControl.match(/max-age=(\d+)/);
  const maxAge = maxAgeMatch ? parseInt(maxAgeMatch[1], 10) : 3600;
  const certsJson = await response.json();
  cachedKeys = certsJson;
  cacheExpiry = now + maxAge * 1e3;
  return cachedKeys;
}
__name(getFirebasePublicKeys, "getFirebasePublicKeys");
function base64urlDecode(str) {
  const padded = str + "=".repeat((4 - str.length % 4) % 4);
  const b64 = padded.replace(/-/g, "+").replace(/_/g, "/");
  return Uint8Array.from(atob(b64), (c) => c.charCodeAt(0));
}
__name(base64urlDecode, "base64urlDecode");
async function verifyFirebaseToken(token, projectId) {
  if (!token)
    throw new Error("No token provided");
  const parts = token.split(".");
  if (parts.length !== 3)
    throw new Error("Invalid JWT format");
  const [headerB64, payloadB64, signatureB64] = parts;
  const header = JSON.parse(new TextDecoder().decode(base64urlDecode(headerB64)));
  const payload = JSON.parse(new TextDecoder().decode(base64urlDecode(payloadB64)));
  if (header.alg !== "RS256")
    throw new Error("Invalid algorithm: " + header.alg);
  if (!header.kid)
    throw new Error("Missing kid in JWT header");
  const now = Math.floor(Date.now() / 1e3);
  if (payload.exp <= now)
    throw new Error("Token expired");
  if (payload.iat > now + 300)
    throw new Error("Token issued in the future");
  if (payload.aud !== projectId)
    throw new Error("Invalid audience: " + payload.aud);
  if (payload.iss !== `https://securetoken.google.com/${projectId}`) {
    throw new Error("Invalid issuer: " + payload.iss);
  }
  if (!payload.sub || typeof payload.sub !== "string") {
    throw new Error("Invalid sub claim");
  }
  const keys = await getFirebasePublicKeys();
  const pem = keys[header.kid];
  if (!pem)
    throw new Error("Unknown kid: " + header.kid);
  return {
    uid: payload.sub,
    email: payload.email || null,
    name: payload.name || null,
    picture: payload.picture || null,
    email_verified: payload.email_verified || false,
    auth_time: payload.auth_time
  };
}
__name(verifyFirebaseToken, "verifyFirebaseToken");
async function authMiddleware(request, env) {
  const authHeader = request.headers.get("Authorization");
  if (!authHeader || !authHeader.startsWith("Bearer ")) {
    return new Response(JSON.stringify({ error: "Missing or invalid Authorization header" }), {
      status: 401,
      headers: { "Content-Type": "application/json" }
    });
  }
  const token = authHeader.slice(7);
  try {
    const user = await verifyFirebaseToken(token, env.FIREBASE_PROJECT_ID);
    return user;
  } catch (err) {
    return new Response(JSON.stringify({ error: "Unauthorized: " + err.message }), {
      status: 401,
      headers: { "Content-Type": "application/json" }
    });
  }
}
__name(authMiddleware, "authMiddleware");

// src/state-worker.js
var state_worker_default = {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    const path = url.pathname;
    const method = request.method;
    const corsHeaders = {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
      "Access-Control-Allow-Headers": "Content-Type, Authorization"
    };
    if (method === "OPTIONS") {
      return new Response(null, { status: 204, headers: corsHeaders });
    }
    const authResult = await authMiddleware(request, env);
    if (authResult instanceof Response) {
      return new Response(authResult.body, {
        status: authResult.status,
        headers: { ...corsHeaders, "Content-Type": "application/json" }
      });
    }
    const user = authResult;
    try {
      if (path === "/api/users" && method === "POST") {
        await env.DB.prepare(
          `INSERT INTO users (id, email, display_name, auth_provider_id)
           VALUES (?, ?, ?, ?)
           ON CONFLICT(id) DO UPDATE SET
             display_name = excluded.display_name,
             updated_at = datetime('now')`
        ).bind(user.uid, user.email, user.name || user.email, user.uid).run();
        return json({ success: true, user_id: user.uid }, corsHeaders);
      }
      if (path === "/api/projects" && method === "GET") {
        const { results } = await env.DB.prepare(
          `SELECT id, name, description, target_platforms, version,
                  json_extract(blockly_ast, '$.metadata.total_block_count') as block_count,
                  created_at, updated_at
           FROM projects WHERE user_id = ? ORDER BY updated_at DESC`
        ).bind(user.uid).all();
        return json({ projects: results || [] }, corsHeaders);
      }
      if (path === "/api/projects" && method === "POST") {
        const body = await request.json();
        const id = crypto.randomUUID();
        await env.DB.prepare(
          `INSERT INTO projects (id, user_id, name, description, target_platforms)
           VALUES (?, ?, ?, ?, ?)`
        ).bind(id, user.uid, body.name, body.description || "", JSON.stringify(body.target_platforms || ["mobile"])).run();
        return json({ id, name: body.name }, corsHeaders, 201);
      }
      const projectMatch = path.match(/^\/api\/projects\/([a-f0-9-]+)$/);
      if (projectMatch && method === "GET") {
        const projectId = projectMatch[1];
        const project = await env.DB.prepare(
          `SELECT * FROM projects WHERE id = ? AND user_id = ?`
        ).bind(projectId, user.uid).first();
        if (!project)
          return json({ error: "Project not found" }, corsHeaders, 404);
        return json({ project }, corsHeaders);
      }
      if (projectMatch && method === "PUT") {
        const projectId = projectMatch[1];
        const body = await request.json();
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
          return json({ error: "Conflict \u2014 project was modified elsewhere" }, corsHeaders, 409);
        }
        return json({ success: true, version: (body.version || 1) + 1 }, corsHeaders);
      }
      if (projectMatch && method === "DELETE") {
        const projectId = projectMatch[1];
        await env.DB.prepare(
          `DELETE FROM projects WHERE id = ? AND user_id = ?`
        ).bind(projectId, user.uid).run();
        return json({ success: true }, corsHeaders);
      }
      const statusMatch = path.match(/^\/api\/status\/([a-f0-9-]+)$/);
      if (statusMatch && method === "GET") {
        const jobId = statusMatch[1];
        const statusData = await env.KV.get(`export:${jobId}`, "json");
        if (!statusData)
          return json({ error: "Job not found" }, corsHeaders, 404);
        if (statusData.user_id !== user.uid) {
          return json({ error: "Unauthorized" }, corsHeaders, 403);
        }
        return json(statusData, corsHeaders);
      }
      return json({ error: "Not found" }, corsHeaders, 404);
    } catch (err) {
      console.error("State Worker error:", err);
      return json({ error: "Internal server error" }, corsHeaders, 500);
    }
  }
};
function json(data, corsHeaders, status = 200) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { ...corsHeaders, "Content-Type": "application/json" }
  });
}
__name(json, "json");

// node_modules/wrangler/templates/middleware/middleware-ensure-req-body-drained.ts
var drainBody = /* @__PURE__ */ __name(async (request, env, _ctx, middlewareCtx) => {
  try {
    return await middlewareCtx.next(request, env);
  } finally {
    try {
      if (request.body !== null && !request.bodyUsed) {
        const reader = request.body.getReader();
        while (!(await reader.read()).done) {
        }
      }
    } catch (e) {
      console.error("Failed to drain the unused request body.", e);
    }
  }
}, "drainBody");
var middleware_ensure_req_body_drained_default = drainBody;

// node_modules/wrangler/templates/middleware/middleware-miniflare3-json-error.ts
function reduceError(e) {
  return {
    name: e?.name,
    message: e?.message ?? String(e),
    stack: e?.stack,
    cause: e?.cause === void 0 ? void 0 : reduceError(e.cause)
  };
}
__name(reduceError, "reduceError");
var jsonError = /* @__PURE__ */ __name(async (request, env, _ctx, middlewareCtx) => {
  try {
    return await middlewareCtx.next(request, env);
  } catch (e) {
    const error = reduceError(e);
    return Response.json(error, {
      status: 500,
      headers: { "MF-Experimental-Error-Stack": "true" }
    });
  }
}, "jsonError");
var middleware_miniflare3_json_error_default = jsonError;

// .wrangler/tmp/bundle-nuzaF7/middleware-insertion-facade.js
var __INTERNAL_WRANGLER_MIDDLEWARE__ = [
  middleware_ensure_req_body_drained_default,
  middleware_miniflare3_json_error_default
];
var middleware_insertion_facade_default = state_worker_default;

// node_modules/wrangler/templates/middleware/common.ts
var __facade_middleware__ = [];
function __facade_register__(...args) {
  __facade_middleware__.push(...args.flat());
}
__name(__facade_register__, "__facade_register__");
function __facade_invokeChain__(request, env, ctx, dispatch, middlewareChain) {
  const [head, ...tail] = middlewareChain;
  const middlewareCtx = {
    dispatch,
    next(newRequest, newEnv) {
      return __facade_invokeChain__(newRequest, newEnv, ctx, dispatch, tail);
    }
  };
  return head(request, env, ctx, middlewareCtx);
}
__name(__facade_invokeChain__, "__facade_invokeChain__");
function __facade_invoke__(request, env, ctx, dispatch, finalMiddleware) {
  return __facade_invokeChain__(request, env, ctx, dispatch, [
    ...__facade_middleware__,
    finalMiddleware
  ]);
}
__name(__facade_invoke__, "__facade_invoke__");

// .wrangler/tmp/bundle-nuzaF7/middleware-loader.entry.ts
var __Facade_ScheduledController__ = class {
  constructor(scheduledTime, cron, noRetry) {
    this.scheduledTime = scheduledTime;
    this.cron = cron;
    this.#noRetry = noRetry;
  }
  #noRetry;
  noRetry() {
    if (!(this instanceof __Facade_ScheduledController__)) {
      throw new TypeError("Illegal invocation");
    }
    this.#noRetry();
  }
};
__name(__Facade_ScheduledController__, "__Facade_ScheduledController__");
function wrapExportedHandler(worker) {
  if (__INTERNAL_WRANGLER_MIDDLEWARE__ === void 0 || __INTERNAL_WRANGLER_MIDDLEWARE__.length === 0) {
    return worker;
  }
  for (const middleware of __INTERNAL_WRANGLER_MIDDLEWARE__) {
    __facade_register__(middleware);
  }
  const fetchDispatcher = /* @__PURE__ */ __name(function(request, env, ctx) {
    if (worker.fetch === void 0) {
      throw new Error("Handler does not export a fetch() function.");
    }
    return worker.fetch(request, env, ctx);
  }, "fetchDispatcher");
  return {
    ...worker,
    fetch(request, env, ctx) {
      const dispatcher = /* @__PURE__ */ __name(function(type, init) {
        if (type === "scheduled" && worker.scheduled !== void 0) {
          const controller = new __Facade_ScheduledController__(
            Date.now(),
            init.cron ?? "",
            () => {
            }
          );
          return worker.scheduled(controller, env, ctx);
        }
      }, "dispatcher");
      return __facade_invoke__(request, env, ctx, dispatcher, fetchDispatcher);
    }
  };
}
__name(wrapExportedHandler, "wrapExportedHandler");
function wrapWorkerEntrypoint(klass) {
  if (__INTERNAL_WRANGLER_MIDDLEWARE__ === void 0 || __INTERNAL_WRANGLER_MIDDLEWARE__.length === 0) {
    return klass;
  }
  for (const middleware of __INTERNAL_WRANGLER_MIDDLEWARE__) {
    __facade_register__(middleware);
  }
  return class extends klass {
    #fetchDispatcher = (request, env, ctx) => {
      this.env = env;
      this.ctx = ctx;
      if (super.fetch === void 0) {
        throw new Error("Entrypoint class does not define a fetch() function.");
      }
      return super.fetch(request);
    };
    #dispatcher = (type, init) => {
      if (type === "scheduled" && super.scheduled !== void 0) {
        const controller = new __Facade_ScheduledController__(
          Date.now(),
          init.cron ?? "",
          () => {
          }
        );
        return super.scheduled(controller);
      }
    };
    fetch(request) {
      return __facade_invoke__(
        request,
        this.env,
        this.ctx,
        this.#dispatcher,
        this.#fetchDispatcher
      );
    }
  };
}
__name(wrapWorkerEntrypoint, "wrapWorkerEntrypoint");
var WRAPPED_ENTRY;
if (typeof middleware_insertion_facade_default === "object") {
  WRAPPED_ENTRY = wrapExportedHandler(middleware_insertion_facade_default);
} else if (typeof middleware_insertion_facade_default === "function") {
  WRAPPED_ENTRY = wrapWorkerEntrypoint(middleware_insertion_facade_default);
}
var middleware_loader_entry_default = WRAPPED_ENTRY;
export {
  __INTERNAL_WRANGLER_MIDDLEWARE__,
  middleware_loader_entry_default as default
};
//# sourceMappingURL=state-worker.js.map
