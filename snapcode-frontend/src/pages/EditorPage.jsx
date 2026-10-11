import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import * as Blockly from 'blockly';
import Navbar from '../components/Navbar';
import BlockEditor from '../components/BlockEditor';
import CodePreview from '../components/CodePreview';
import ExportPanel from '../components/ExportPanel';
import { dartGenerator } from '../blockly/generators/dartGenerator';
import { api } from '../services/api';
import './EditorPage.css';

export default function EditorPage() {
  const { projectId } = useParams();
  const navigate = useNavigate();
  
  const [project, setProject] = useState(null);
  const [fileMap, setFileMap] = useState({}); // { 'lib/main.dart': { ast: {...}, code: '...' } }
  const [activeFile, setActiveFile] = useState('lib/main.dart');
  const [previewTab, setPreviewTab] = useState('code');
  
  const [saveStatus, setSaveStatus] = useState('saved'); // 'saved' | 'saving' | 'unsaved'
  const workspaceRef = useRef(null);
  const saveTimerRef = useRef(null);

  // Load project
  useEffect(() => {
    loadProject();
  }, [projectId]);

  const loadProject = async () => {
    try {
      const data = await api.getProject(projectId);
      setProject(data);
      
      let parsedMap = {};
      try {
        parsedMap = JSON.parse(data.blockly_ast || '{}');
      } catch {
        parsedMap = { 'lib/main.dart': { metadata: { total_block_count: 0 }, blocks: { blocks: [] } } };
      }
      
      // Ensure main.dart exists
      if (!parsedMap['lib/main.dart']) {
        parsedMap['lib/main.dart'] = { metadata: { total_block_count: 0 }, blocks: { blocks: [] } };
      }

      setFileMap(parsedMap);
    } catch (error) {
      console.error('Failed to load project:', error);
      navigate('/dashboard');
    }
  };

  const handleAddFile = () => {
    const filename = prompt('Enter file name (e.g., lib/screens/home.dart):');
    if (!filename) return;
    if (!filename.endsWith('.dart')) {
      alert('Filename must end with .dart');
      return;
    }
    if (fileMap[filename]) {
      alert('File already exists');
      return;
    }

    setFileMap(prev => ({
      ...prev,
      [filename]: { metadata: { total_block_count: 0 }, blocks: { blocks: [] } }
    }));
    setActiveFile(filename);
    setSaveStatus('unsaved');
  };

  const handleDeleteFile = (filename) => {
    if (filename === 'lib/main.dart') {
      alert('Cannot delete lib/main.dart');
      return;
    }
    if (window.confirm(`Delete ${filename}?`)) {
      setFileMap(prev => {
        const newMap = { ...prev };
        delete newMap[filename];
        return newMap;
      });
      if (activeFile === filename) {
        setActiveFile('lib/main.dart');
      }
      setSaveStatus('unsaved');
    }
  };

  // Generate Dart code from workspace
  const generateCode = useCallback((workspace) => {
    try {
      const code = dartGenerator.workspaceToCode(workspace);
      return code || '// No blocks on workspace';
    } catch (err) {
      console.error('Code generation error:', err);
      return '// Error generating code: ' + err.message;
    }
  }, []);

  // Debounced auto-save
  const handleWorkspaceChange = useCallback((workspace) => {
    const code = generateCode(workspace);
    const state = Blockly.serialization.workspaces.save(workspace);
    const blockCount = workspace.getAllBlocks(false).length;

    setFileMap(prev => {
      const newMap = { ...prev };
      newMap[activeFile] = state; // Store AST state
      // We don't store code in state to avoid bloat, we'll re-generate it for preview
      return newMap;
    });

    setSaveStatus('unsaved');

    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveTimerRef.current = setTimeout(async () => {
      await saveProjectToServer();
    }, 2000);
  }, [activeFile, generateCode]);

  const saveProjectToServer = async () => {
    if (!project) return;
    setSaveStatus('saving');
    try {
      // Need latest fileMap, use functional state update trick or ref. 
      // For simplicity, we just use the current state in scope (might be slightly stale if typing fast, but handleWorkspaceChange updates it)
      setFileMap(currentMap => {
        api.updateProject(projectId, {
          blockly_ast: JSON.stringify(currentMap),
          version: project.version
        }).then(res => {
          setProject(p => ({ ...p, version: res.version }));
          setSaveStatus('saved');
        }).catch(err => {
          console.error('Save failed:', err);
          setSaveStatus('unsaved');
        });
        return currentMap;
      });
    } catch (err) {
      console.error('Save failed:', err);
      setSaveStatus('unsaved');
    }
  };

  // Manual save
  function handleManualSave() {
    if (saveTimerRef.current) clearTimeout(saveTimerRef.current);
    saveProjectToServer();
  }

  // Keyboard shortcuts
  useEffect(() => {
    function handleKeyDown(e) {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        handleManualSave();
      }
    }
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [projectId]); 

  if (!project) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="spinner spinner-lg"></div>
      </div>
    );
  }

  // Get current AST and generate code
  const currentAst = fileMap[activeFile] || { metadata: {}, blocks: { blocks: [] } };
  let currentCode = '// Drag blocks to start generating Dart code...';
  if (workspaceRef.current) {
    currentCode = generateCode(workspaceRef.current);
  }

  let platforms = [];
  try {
    platforms = typeof project.target_platforms === 'string' 
      ? JSON.parse(project.target_platforms) 
      : (project.target_platforms || ['mobile']);
  } catch {
    platforms = ['mobile'];
  }

  return (
    <div className="editor-page flex-col h-screen">
      <Navbar />

      {/* Toolbar */}
      <div className="editor-toolbar flex items-center justify-between p-sm border-b border-subtle bg-bg-secondary">
        <div className="flex items-center gap-md">
          <h3 className="font-bold">{project.name}</h3>
          <span className={`text-sm ${saveStatus === 'unsaved' ? 'text-accent-warning' : 'text-text-tertiary'}`}>
            {saveStatus === 'saved' ? 'Saved' : saveStatus === 'saving' ? 'Saving...' : 'Unsaved'}
          </span>
        </div>
        <div className="flex items-center gap-sm">
          <button className="btn btn-ghost btn-sm" onClick={() => workspaceRef.current?.undo(false)} title="Undo (Ctrl+Z)">Undo</button>
          <button className="btn btn-ghost btn-sm" onClick={() => workspaceRef.current?.undo(true)} title="Redo (Ctrl+Shift+Z)">Redo</button>
          <button className="btn btn-primary btn-sm" onClick={handleManualSave} title="Save (Ctrl+S)">Save</button>
        </div>
        <div className="flex items-center gap-xs">
          {platforms.map(p => (
            <span key={p} className="badge badge-purple uppercase">{p}</span>
          ))}
        </div>
      </div>

      {/* Main layout */}
      <div className="flex flex-1 overflow-hidden">
        
        {/* File Explorer */}
        <div className="w-64 border-r border-subtle bg-bg-secondary flex-col">
          <div className="flex justify-between items-center p-sm border-b border-subtle">
            <h4 className="text-sm font-bold text-text-secondary uppercase tracking-wider">Explorer</h4>
            <button className="btn-icon btn-ghost btn-sm" onClick={handleAddFile} title="New File">+</button>
          </div>
          <div className="flex-1 overflow-y-auto p-xs">
            {Object.keys(fileMap).sort().map(filename => (
              <div 
                key={filename} 
                onClick={() => setActiveFile(filename)}
                className={`flex justify-between items-center p-sm cursor-pointer rounded-md mb-xs transition-fast
                  ${activeFile === filename ? 'bg-accent-primary text-white' : 'hover:bg-bg-tertiary text-text-secondary'}`}
              >
                <span className="text-sm truncate">{filename}</span>
                {filename !== 'lib/main.dart' && (
                  <button 
                    className="btn-icon btn-ghost btn-sm opacity-50 hover:opacity-100 hover:text-accent-danger" 
                    onClick={(e) => { e.stopPropagation(); handleDeleteFile(filename); }}
                  >
                    ×
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Blockly Workspace */}
        <div className="flex-1 relative">
          {/* Key on activeFile forces re-mount of BlockEditor when file changes, ensuring clean state load */}
          <BlockEditor
            key={activeFile}
            initialState={currentAst}
            onWorkspaceChange={handleWorkspaceChange}
            workspaceRef={workspaceRef}
          />
        </div>

        {/* Code Preview & Export */}
        <div className="w-96 border-l border-subtle flex-col bg-bg-primary">
          <div className="flex p-xs border-b border-subtle bg-bg-secondary gap-xs">
            <button 
              className={`btn-sm flex-1 ${previewTab === 'code' ? 'btn-primary' : 'btn-ghost'}`}
              onClick={() => setPreviewTab('code')}
            >
              Dart Code
            </button>
            <button 
              className={`btn-sm flex-1 ${previewTab === 'live' ? 'btn-primary' : 'btn-ghost'}`}
              onClick={() => setPreviewTab('live')}
            >
              Live Preview
            </button>
          </div>
          
          <div className="flex-1 overflow-hidden flex-col">
            {previewTab === 'code' ? (
              <div className="flex-1 overflow-y-auto">
                <div className="p-xs bg-bg-secondary border-b border-subtle text-xs text-text-tertiary">
                  {activeFile}
                </div>
                <CodePreview code={currentCode} />
              </div>
            ) : (
              <div className="flex-1 flex-col items-center justify-center p-md text-center bg-white">
                {/* Live Preview Iframe (mocking compilation service for now) */}
                <p className="text-secondary text-sm mb-sm text-text-inverse">
                  Send to compiler to view live app
                </p>
                <div className="border border-subtle rounded-md w-full h-[600px] bg-gray-100 flex items-center justify-center">
                   {/* In a real scenario, this would point to the compiled URL from the Hono API */}
                   <span className="text-text-inverse opacity-50">Web Preview Area</span>
                </div>
              </div>
            )}
          </div>
          <div className="border-t border-subtle bg-bg-secondary">
            <ExportPanel projectId={projectId} />
          </div>
        </div>
      </div>
    </div>
  );
}
