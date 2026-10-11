// ============================================================
// SnapCode — Blockly Toolbox Definition (Advanced)
// No Emojis
// ============================================================

export const toolboxConfig = {
  kind: 'categoryToolbox',
  contents: [
    {
      kind: 'category',
      name: 'App Structure',
      colour: '#7c3aed',
      contents: [
        { kind: 'block', type: 'flutter_app_scaffold' },
        { kind: 'block', type: 'flutter_app_bar' },
      ],
    },
    {
      kind: 'category',
      name: 'Layout',
      colour: '#3b82f6',
      contents: [
        { kind: 'block', type: 'flutter_column' },
        { kind: 'block', type: 'flutter_row' },
        { kind: 'block', type: 'flutter_listview' },
        { kind: 'block', type: 'flutter_gridview' },
        { kind: 'block', type: 'flutter_stack' },
        { kind: 'block', type: 'flutter_container' },
        { kind: 'block', type: 'flutter_padding' },
        { kind: 'block', type: 'flutter_center' },
        { kind: 'block', type: 'flutter_expanded' },
        { kind: 'block', type: 'flutter_positioned' },
        { kind: 'block', type: 'flutter_sized_box' },
      ],
    },
    {
      kind: 'category',
      name: 'Widgets',
      colour: '#10b981',
      contents: [
        { kind: 'block', type: 'flutter_text' },
        { kind: 'block', type: 'flutter_elevated_button' },
        { kind: 'block', type: 'flutter_icon' },
        { kind: 'block', type: 'flutter_image_network' },
        { kind: 'block', type: 'flutter_image_asset' },
        { kind: 'block', type: 'flutter_circle_avatar' },
        { kind: 'block', type: 'flutter_list_tile' },
        { kind: 'block', type: 'flutter_card' },
        { kind: 'block', type: 'flutter_divider' },
        { kind: 'block', type: 'flutter_snackbar' },
      ],
    },
    {
      kind: 'category',
      name: 'Input & Forms',
      colour: '#f59e0b',
      contents: [
        { kind: 'block', type: 'flutter_text_field' },
        { kind: 'block', type: 'flutter_checkbox' },
        { kind: 'block', type: 'flutter_switch' },
      ],
    },
    {
      kind: 'category',
      name: 'Logic',
      colour: '#ef4444',
      contents: [
        { kind: 'block', type: 'logic_if_else' },
        { kind: 'block', type: 'logic_for_loop' },
        { kind: 'block', type: 'logic_while_loop' },
        { kind: 'block', type: 'logic_function' },
        { kind: 'block', type: 'logic_print' },
      ],
    },
    {
      kind: 'category',
      name: 'Variables',
      colour: '#06b6d4',
      contents: [
        { kind: 'block', type: 'variable_set' },
        { kind: 'block', type: 'variable_get' },
        { kind: 'block', type: 'math_number' },
        { kind: 'block', type: 'math_arithmetic' },
        { kind: 'block', type: 'text_literal' },
        { kind: 'block', type: 'text_join' },
      ],
    },
  ],
};
