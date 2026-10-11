import React from 'react';
import { formatDistanceToNow } from 'date-fns';

export default function ProjectCard({ project, onDelete }) {
  // Handle both old schema (target_platform string) and new schema (target_platforms array)
  let platforms = [];
  if (project.target_platforms) {
    try {
      platforms = typeof project.target_platforms === 'string' 
        ? JSON.parse(project.target_platforms) 
        : project.target_platforms;
    } catch {
      platforms = ['mobile'];
    }
  } else if (project.target_platform) {
    platforms = [project.target_platform.replace('flutter_', '')];
  } else {
    platforms = ['mobile'];
  }

  return (
    <div className="glass-card p-lg flex-col gap-sm">
      <div className="flex justify-between items-start">
        <h3 className="text-xl font-bold truncate pr-sm">{project.name}</h3>
        <button className="btn-icon btn-ghost text-secondary hover:text-accent-danger" onClick={onDelete} title="Delete">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M10 11v6M14 11v6"/>
          </svg>
        </button>
      </div>
      
      <p className="text-secondary text-sm line-clamp-2 min-h-[40px]">
        {project.description || 'No description'}
      </p>

      <div className="flex gap-xs flex-wrap mt-xs">
        {platforms.map(platform => (
          <span key={platform} className={`badge ${
            platform === 'mobile' ? 'badge-purple' : 
            platform === 'web' ? 'badge-cyan' : 'badge-amber'
          }`}>
            {platform.charAt(0).toUpperCase() + platform.slice(1)}
          </span>
        ))}
      </div>

      <div className="flex justify-between items-end mt-md pt-md border-t border-subtle">
        <div className="flex-col gap-xs">
          <span className="text-xs text-tertiary">
            Updated {project.updated_at ? formatDistanceToNow(new Date(project.updated_at)) : 'just now'} ago
          </span>
          <span className="text-xs text-tertiary">
            {project.block_count || 0} blocks
          </span>
        </div>
        <a href={`/editor/${project.id}`} className="btn btn-secondary btn-sm">
          Open Editor
        </a>
      </div>
    </div>
  );
}
