// ============================================================
// SnapCode Custom Blockly Theme — Dark Mode
// ============================================================
import * as Blockly from 'blockly';

const SnapCodeTheme = Blockly.Theme.defineTheme('snapcode_dark', {
  name: 'snapcode_dark',
  base: Blockly.Themes.Classic,
  blockStyles: {
    app_blocks: {
      colourPrimary: '#7c3aed',
      colourSecondary: '#6d28d9',
      colourTertiary: '#5b21b6',
      hat: 'cap',
    },
    layout_blocks: {
      colourPrimary: '#2563eb',
      colourSecondary: '#1d4ed8',
      colourTertiary: '#1e40af',
    },
    widget_blocks: {
      colourPrimary: '#059669',
      colourSecondary: '#047857',
      colourTertiary: '#065f46',
    },
    input_blocks: {
      colourPrimary: '#d97706',
      colourSecondary: '#b45309',
      colourTertiary: '#92400e',
    },
    logic_blocks: {
      colourPrimary: '#e11d48',
      colourSecondary: '#be123c',
      colourTertiary: '#9f1239',
    },
    variable_blocks: {
      colourPrimary: '#0891b2',
      colourSecondary: '#0e7490',
      colourTertiary: '#155e75',
    },
  },
  categoryStyles: {
    app_category: { colour: '#7c3aed' },
    layout_category: { colour: '#2563eb' },
    widget_category: { colour: '#059669' },
    input_category: { colour: '#d97706' },
    logic_category: { colour: '#e11d48' },
    variable_category: { colour: '#0891b2' },
  },
  componentStyles: {
    workspaceBackgroundColour: '#0c1020',
    toolboxBackgroundColour: '#111827',
    toolboxForegroundColour: '#e2e8f0',
    flyoutBackgroundColour: '#1a1f35',
    flyoutForegroundColour: '#e2e8f0',
    flyoutOpacity: 0.95,
    scrollbarColour: '#374151',
    scrollbarOpacity: 0.6,
    insertionMarkerColour: '#7c3aed',
    insertionMarkerOpacity: 0.4,
    cursorColour: '#7c3aed',
  },
  fontStyle: {
    family: "'Inter', sans-serif",
    weight: '500',
    size: 11,
  },
  startHats: true,
});

export default SnapCodeTheme;
