// ============================================================
// SnapCode — Widget Block Definitions (Advanced)
// ============================================================
import * as Blockly from 'blockly';

export function registerWidgetBlocks() {
  Blockly.defineBlocksWithJsonArray([
    // ── Text ─────────────────────────────────────────────
    {
      type: 'flutter_text',
      message0: 'Text %1 %2 font size %3 weight %4 color %5',
      args0: [
        { type: 'field_input', name: 'TEXT_VALUE', text: 'Hello, SnapCode!' },
        { type: 'input_dummy' },
        { type: 'field_number', name: 'FONT_SIZE', value: 16, min: 8, max: 120, precision: 1 },
        {
          type: 'field_dropdown',
          name: 'FONT_WEIGHT',
          options: [
            ['normal', 'FontWeight.normal'],
            ['bold', 'FontWeight.bold'],
            ['w300', 'FontWeight.w300'],
            ['w500', 'FontWeight.w500'],
          ],
        },
        { type: 'field_colour', name: 'TEXT_COLOR', colour: '#ffffff' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── ElevatedButton ───────────────────────────────────
    {
      type: 'flutter_elevated_button',
      message0: 'ElevatedButton %1 text %2 %3 on pressed %4',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_input', name: 'BUTTON_TEXT', text: 'Click Me' },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'ON_PRESSED' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── Icon ─────────────────────────────────────────────
    {
      type: 'flutter_icon',
      message0: 'Icon %1 size %2 color %3',
      args0: [
        {
          type: 'field_dropdown',
          name: 'ICON_NAME',
          options: [
            ['home', 'Icons.home'],
            ['star', 'Icons.star'],
            ['favorite', 'Icons.favorite'],
            ['settings', 'Icons.settings'],
            ['search', 'Icons.search'],
            ['add', 'Icons.add'],
          ],
        },
        { type: 'field_number', name: 'ICON_SIZE', value: 24, min: 8, max: 200, precision: 1 },
        { type: 'field_colour', name: 'ICON_COLOR', colour: '#ffffff' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── Image.network ────────────────────────────────────
    {
      type: 'flutter_image_network',
      message0: 'Image.network %1 URL %2 %3 width %4 height %5',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_input', name: 'IMAGE_URL', text: 'https://picsum.photos/200' },
        { type: 'input_dummy' },
        { type: 'field_number', name: 'WIDTH', value: 200, min: 0, precision: 1 },
        { type: 'field_number', name: 'HEIGHT', value: 200, min: 0, precision: 1 },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── Image.asset (NEW) ────────────────────────────────
    {
      type: 'flutter_image_asset',
      message0: 'Image.asset %1 path %2 %3 width %4 height %5',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_input', name: 'IMAGE_PATH', text: 'assets/image.png' },
        { type: 'input_dummy' },
        { type: 'field_number', name: 'WIDTH', value: 200, min: 0, precision: 1 },
        { type: 'field_number', name: 'HEIGHT', value: 200, min: 0, precision: 1 },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── CircleAvatar (NEW) ───────────────────────────────
    {
      type: 'flutter_circle_avatar',
      message0: 'CircleAvatar %1 radius %2 color %3 %4 child %5',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'RADIUS', value: 20, min: 0, precision: 1 },
        { type: 'field_colour', name: 'COLOR', colour: '#7c3aed' },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── ListTile (NEW) ───────────────────────────────────
    {
      type: 'flutter_list_tile',
      message0: 'ListTile %1 leading %2 title %3 subtitle %4 trailing %5 on tap %6',
      args0: [
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'LEADING' },
        { type: 'input_statement', name: 'TITLE' },
        { type: 'input_statement', name: 'SUBTITLE' },
        { type: 'input_statement', name: 'TRAILING' },
        { type: 'input_statement', name: 'ON_TAP' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── Card ─────────────────────────────────────────────
    {
      type: 'flutter_card',
      message0: 'Card %1 elevation %2 %3 child %4',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_number', name: 'ELEVATION', value: 4, min: 0, max: 24, precision: 1 },
        { type: 'input_dummy' },
        { type: 'input_statement', name: 'CHILD' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },

    // ── Divider ──────────────────────────────────────────
    {
      type: 'flutter_divider',
      message0: 'Divider  thickness %1 color %2',
      args0: [
        { type: 'field_number', name: 'THICKNESS', value: 1, min: 0, max: 20, precision: 0.5 },
        { type: 'field_colour', name: 'COLOR', colour: '#374151' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },
    
    // ── SnackBar (NEW) ───────────────────────────────────
    {
      type: 'flutter_snackbar',
      message0: 'Show SnackBar %1 text %2',
      args0: [
        { type: 'input_dummy' },
        { type: 'field_input', name: 'TEXT', text: 'Action completed' },
      ],
      previousStatement: null,
      nextStatement: null,
      style: 'widget_blocks',
    },
  ]);
}
