// ============================================================
// SnapCode — AST Serializer Utilities
// Converts between Blockly workspace state and our AST format
// ============================================================
import * as Blockly from 'blockly';

/**
 * Serialize a Blockly workspace to our SnapCode AST JSON format.
 * This is what gets stored in projects.blockly_ast in D1.
 */
export function serializeWorkspace(workspace) {
  const state = Blockly.serialization.workspaces.save(workspace);
  const allBlocks = workspace.getAllBlocks(false);

  return {
    schema_version: '1.0.0',
    blockly_version: Blockly.VERSION,
    workspace: state,
    metadata: {
      exported_at: null,
      last_edited_block_id: allBlocks.length > 0 ? allBlocks[allBlocks.length - 1].id : null,
      total_block_count: allBlocks.length,
      canvas_scroll_x: workspace.scrollX || 0,
      canvas_scroll_y: workspace.scrollY || 0,
      canvas_zoom: workspace.scale || 1,
    },
  };
}

/**
 * Deserialize our SnapCode AST JSON back into a Blockly workspace.
 */
export function deserializeWorkspace(workspace, astJson) {
  if (!astJson || !astJson.workspace) return;

  try {
    Blockly.serialization.workspaces.load(astJson.workspace, workspace);

    // Restore viewport position if available
    if (astJson.metadata) {
      const { canvas_scroll_x, canvas_scroll_y, canvas_zoom } = astJson.metadata;
      if (canvas_zoom) workspace.setScale(canvas_zoom);
      if (canvas_scroll_x || canvas_scroll_y) {
        workspace.scroll(canvas_scroll_x || 0, canvas_scroll_y || 0);
      }
    }
  } catch (err) {
    console.error('Failed to deserialize workspace:', err);
  }
}

/**
 * Get a summary of blocks in the workspace (for project card metadata).
 */
export function getWorkspaceSummary(workspace) {
  const blocks = workspace.getAllBlocks(false);
  const typeCounts = {};

  blocks.forEach(block => {
    typeCounts[block.type] = (typeCounts[block.type] || 0) + 1;
  });

  return {
    total_block_count: blocks.length,
    block_types: typeCounts,
  };
}
