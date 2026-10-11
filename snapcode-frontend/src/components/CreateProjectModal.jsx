import React, { useState } from 'react';

export default function CreateProjectModal({ onClose, onCreate }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [platforms, setPlatforms] = useState({
    mobile: true,
    web: false,
    desktop: false
  });
  const [loading, setLoading] = useState(false);

  const handlePlatformChange = (key) => {
    setPlatforms(prev => ({ ...prev, [key]: !prev[key] }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const selectedPlatforms = Object.keys(platforms).filter(k => platforms[k]);
    if (selectedPlatforms.length === 0) {
      alert('Please select at least one target platform.');
      return;
    }

    setLoading(true);
    await onCreate({ name, description, target_platforms: selectedPlatforms });
    setLoading(false);
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        <h2 className="modal-title">New Project</h2>
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <label className="input-label">Project Name</label>
            <input
              type="text"
              className="input-field"
              placeholder="e.g. My Awesome App"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              autoFocus
            />
          </div>

          <div className="input-group">
            <label className="input-label">Description (Optional)</label>
            <textarea
              className="input-field"
              placeholder="What does this app do?"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
            />
          </div>

          <div className="input-group">
            <label className="input-label">Target Platforms</label>
            <div className="flex gap-md mt-sm">
              <label className="flex items-center gap-xs cursor-pointer">
                <input 
                  type="checkbox" 
                  checked={platforms.mobile} 
                  onChange={() => handlePlatformChange('mobile')} 
                />
                <span className="text-sm">Mobile</span>
              </label>
              <label className="flex items-center gap-xs cursor-pointer">
                <input 
                  type="checkbox" 
                  checked={platforms.web} 
                  onChange={() => handlePlatformChange('web')} 
                />
                <span className="text-sm">Web</span>
              </label>
              <label className="flex items-center gap-xs cursor-pointer">
                <input 
                  type="checkbox" 
                  checked={platforms.desktop} 
                  onChange={() => handlePlatformChange('desktop')} 
                />
                <span className="text-sm">Desktop</span>
              </label>
            </div>
          </div>

          <div className="modal-actions">
            <button type="button" className="btn btn-ghost" onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? <div className="spinner" /> : 'Create Project'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
