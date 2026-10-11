// ============================================================
// SnapCode — Layout Block Definitions (Advanced)
// ============================================================
import * as Blockly from 'blockly';

export function registerLayoutBlocks() {
  Blockly.defineBlocksWithJsonArray([
    // ── Column ───────────────────────────────────────────
    {
      type: 'flutter_column',
      message0: 'Column %1 align %2 %3 children %4',
      args0: [
        { type: 'input_dummy' },
        {
          type: 'field_dropdown',
          name: 'ALIGNMENT',
          options: [
            ['start', 'MainAxisAlignment.start'],
            ['center', 'MainAxisAlignment.center'],
            ['end', 'MainAxisAlignment.end'],
            ['space between', 'MainAxisAlignment.spaceBetween'],
            ['space around', 'MainAxisAlignment.spaceAround'],
          ],
        },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILDREN' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Row ──────────────────────────────────────────────
    {
      type: 'flutter_row',
      message0: 'Row %1 align %2 %3 children %4',
      args0: [
        { type: 'input_dummy' },
        {
          type: 'field_dropdown',
          name: 'ALIGNMENT',
          options: [
            ['start', 'MainAxisAlignment.start'],
            ['center', 'MainAxisAlignment.center'],
            ['end', 'MainAxisAlignment.end'],
            ['space between', 'MainAxisAlignment.spaceBetween'],
            ['space around', 'MainAxisAlignment.spaceAround'],
          ],
        },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILDREN' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Container ────────────────────────────────────────
    {
      type: 'flutter_container',
      message0: 'Container %1 width %2 height %3 color %4 radius %5 %6 child %7',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'WIDTH', value: 0, min: 0 },
        { type: 'field_number', name: 'HEIGHT', value: 0, min: 0 },
        { type: 'field_colour', name: 'COLOR', colour: '#3b82f6' },
        { type: 'field_number', name: 'BORDER_RADIUS', value: 0, min: 0 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── SizedBox ─────────────────────────────────────────
    {
      type: 'flutter_sized_box',
      message0: 'SizedBox  width %1 height %2',
      args0: [
        { type: 'field_number', name: 'WIDTH', value: 16, min: 0 },
        { type: 'field_number', name: 'HEIGHT', value: 16, min: 0 },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Padding ──────────────────────────────────────────
    {
      type: 'flutter_padding',
      message0: 'Padding %1 all %2 %3 child %4',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'PADDING', value: 16, min: 0 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Center ───────────────────────────────────────────
    {
      type: 'flutter_center',
      message0: 'Center %1 child %2',
      args0: [
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },
    
    // ── Expanded ─────────────────────────────────────────
    {
      type: 'flutter_expanded',
      message0: 'Expanded %1 flex %2 %3 child %4',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'FLEX', value: 1, min: 1, precision: 1 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── ListView (NEW) ───────────────────────────────────
    {
      type: 'flutter_listview',
      message0: 'ListView %1 padding %2 %3 children %4',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'PADDING', value: 8, min: 0 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILDREN' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── GridView (NEW) ───────────────────────────────────
    {
      type: 'flutter_gridview',
      message0: 'GridView %1 columns %2 spacing %3 %4 children %5',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'COLUMNS', value: 2, min: 1, precision: 1 },
        { type: 'field_number', name: 'SPACING', value: 8, min: 0, precision: 1 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILDREN' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Stack (NEW) ──────────────────────────────────────
    {
      type: 'flutter_stack',
      message0: 'Stack %1 children %2',
      args0: [
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILDREN' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    },

    // ── Positioned (NEW) ─────────────────────────────────
    {
      type: 'flutter_positioned',
      message0: 'Positioned %1 top %2 bottom %3 left %4 right %5 %6 child %7',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'TOP', value: 0 },
        { type: 'field_number', name: 'BOTTOM', value: 0 },
        { type: 'field_number', name: 'LEFT', value: 0 },
        { type: 'field_number', name: 'RIGHT', value: 0 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'layout_blocks',
    }
  ]);
}
