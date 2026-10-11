import React, { useState } from 'react';
import { api } from '../services/api';

export default function ExportPanel({ projectId }) {
  const [status, setStatus] = useState('idle'); // idle, processing, completed, error
  const [errorMsg, setErrorMsg] = useState('');
  const [downloadUrl, setDownloadUrl] = useState('');

  const handleExport = async () => {
    setStatus('processing');
    setErrorMsg('');
    
    try {
      const response = await api.startExport(projectId);
      const jobId = response.job_id;
      
      // Poll for status
      const pollInterval = setInterval(async () => {
        try {
          const statusRes = await api.getExportStatus(jobId);
          if (statusRes.status === 'completed') {
            clearInterval(pollInterval);
            setDownloadUrl(statusRes.download_url);
            setStatus('completed');
          } else if (statusRes.status === 'failed') {
            clearInterval(pollInterval);
            setErrorMsg(statusRes.error_message || 'Export failed');
            setStatus('error');
          }
        } catch (pollErr) {
          clearInterval(pollInterval);
          setErrorMsg('Failed to check export status');
          setStatus('error');
        }
      }, 2000);
      
    } catch (err) {
      setErrorMsg('Failed to start export');
      setStatus('error');
    }
  };

  return (
    <div className="p-md flex-col gap-sm">
      <div className="flex justify-between items-center">
        <h4 className="text-sm font-bold text-text-secondary uppercase tracking-wider">Export Project</h4>
      </div>
      
      {status === 'idle' && (
        <button className="btn btn-primary w-full" onClick={handleExport}>
          Download Flutter ZIP
        </button>
      )}
      
      {status === 'processing' && (
        <div className="flex-col gap-sm">
          <div className="flex items-center gap-sm text-sm text-accent-secondary">
            <div className="spinner"></div> Generating code and packaging...
          </div>
          <div className="progress-bar"><div className="progress-bar-fill w-full"></div></div>
        </div>
      )}
      
      {status === 'completed' && (
        <div className="flex-col gap-sm">
          <div className="text-sm text-accent-success font-medium">Ready!</div>
          <a href={downloadUrl} target="_blank" rel="noreferrer" className="btn btn-primary w-full text-center">
            Download ZIP
          </a>
          <button className="btn btn-ghost w-full" onClick={() => setStatus('idle')}>
            Export Again
          </button>
        </div>
      )}
      
      {status === 'error' && (
        <div className="flex-col gap-sm">
          <div className="text-sm text-accent-danger bg-accent-danger bg-opacity-10 p-sm rounded-md border border-accent-danger border-opacity-20">
            {errorMsg}
          </div>
          <button className="btn btn-secondary w-full" onClick={() => setStatus('idle')}>
            Try Again
          </button>
        </div>
      )}
    </div>
  );
}
