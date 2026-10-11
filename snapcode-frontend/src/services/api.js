// ============================================================
// SnapCode — API Service (fetch wrapper with JWT)
// Currently uses localStorage. Will switch to Cloudflare Workers
// backend when deployed.
// ============================================================

const API_BASE = import.meta.env.VITE_API_URL || '';

/**
 * Get the Firebase ID token for authenticated requests
 */
async function getAuthToken() {
  try {
    const { auth } = await import('../config/firebase');
    const user = auth.currentUser;
    if (user) {
      return await user.getIdToken();
    }
  } catch {
    // Firebase not configured yet
  }
  return null;
}

/**
 * Make an authenticated API request
 */
async function apiRequest(endpoint, options = {}) {
  const token = await getAuthToken();

  const headers = {
    'Content-Type': 'application/json',
    ...(token && { Authorization: `Bearer ${token}` }),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorBody = await response.text().catch(() => '');
    throw new Error(`API Error ${response.status}: ${errorBody}`);
  }

  if (response.status === 204) return null;
  return response.json();
}

export const api = {
  // Projects
  getProjects: () => apiRequest('/api/projects'),
  getProject: (id) => apiRequest(`/api/projects/${id}`),
  createProject: (data) => apiRequest('/api/projects', { method: 'POST', body: JSON.stringify(data) }),
  updateProject: (id, data) => apiRequest(`/api/projects/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteProject: (id) => apiRequest(`/api/projects/${id}`, { method: 'DELETE' }),
  
  // Export
  startExport: (projectId) => apiRequest(`/api/export/${projectId}`, { method: 'POST' }),
  getExportStatus: (jobId) => apiRequest(`/api/status/${jobId}`),
};

export default apiRequest;
