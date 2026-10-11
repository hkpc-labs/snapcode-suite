import { useEffect, useRef, useCallback } from 'react';
import * as Blockly from 'blockly';
import SnapCodeTheme from '../blockly/theme';
import { toolboxConfig } from '../blockly/toolbox';

// Register all custom blocks
import { registerAppBlocks } from '../blockly/blocks/appBlocks';
import { registerLayoutBlocks } from '../blockly/blocks/layoutBlocks';
import { registerWidgetBlocks } from '../blockly/blocks/widgetBlocks';
import { registerInputBlocks } from '../blockly/blocks/inputBlocks';
import { registerLogicBlocks } from '../blockly/blocks/logicBlocks';
import { registerVariableBlocks } from '../blockly/blocks/variableBlocks';

import './BlockEditor.css';

// Register blocks once (module level)
let blocksRegistered = false;
function ensureBlocksRegistered() {
  if (blocksRegistered) return;
  registerAppBlocks();
  registerLayoutBlocks();
  registerWidgetBlocks();
  registerInputBlocks();
  registerLogicBlocks();
  registerVariableBlocks();
  blocksRegistered = true;
}

export default function BlockEditor({ initialState, onWorkspaceChange, workspaceRef }) {
  const containerRef = useRef(null);
  const workspaceInstance = useRef(null);

  // Initialize Blockly workspace
  useEffect(() => {
    if (!containerRef.current) return;

    ensureBlocksRegistered();

    const workspace = Blockly.inject(containerRef.current, {
      toolbox: toolboxConfig,
      theme: SnapCodeTheme,
      renderer: 'zelos',
      grid: {
        spacing: 25,
        length: 3,
        colour: '#1e293b',
        snap: true,
      },
      zoom: {
        controls: true,
        wheel: true,
        startScale: 0.9,
        maxScale: 2,
        minScale: 0.3,
        scaleSpeed: 1.1,
        pinch: true,
      },
      trashcan: true,
      move: {
        scrollbars: {
          horizontal: true,
          vertical: true,
        },
        drag: true,
        wheel: true,
      },
      sounds: false,
    });

    workspaceInstance.current = workspace;
    if (workspaceRef) workspaceRef.current = workspace;

    // Load initial state if provided
    if (initialState && Object.keys(initialState).length > 0) {
      try {
        Blockly.serialization.workspaces.load(initialState, workspace);
      } catch (err) {
        console.warn('Failed to load workspace state:', err);
      }
    }

    // Listen for workspace changes
    const handleChange = (event) => {
      // Only trigger on meaningful changes (not UI events like scrolling)
      if (
        event.type === Blockly.Events.BLOCK_CHANGE ||
        event.type === Blockly.Events.BLOCK_CREATE ||
        event.type === Blockly.Events.BLOCK_DELETE ||
        event.type === Blockly.Events.BLOCK_MOVE
      ) {
        if (onWorkspaceChange) {
          onWorkspaceChange(workspace);
        }
      }
    };

    workspace.addChangeListener(handleChange);

    // Handle resize
    const resizeObserver = new ResizeObserver(() => {
      Blockly.svgResize(workspace);
    });
    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
      workspace.removeChangeListener(handleChange);
      workspace.dispose();
      workspaceInstance.current = null;
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className="block-editor-container" ref={containerRef} />
  );
}
