// ============================================================
// SnapCode — Input Block Definitions
// ============================================================
import * as Blockly from 'blockly';

export function registerInputBlocks() {
 Blockly.defineBlocksWithJsonArray([
 // ── TextField ────────────────────────────────────────
 {
 type: 'flutter_text_field',
 message0: '⌨️ TextField %1 hint %2 %3 label %4',
 args0: [
 { type: 'input_dummy' },
 { type: 'field_input', name: 'HINT_TEXT', text: 'Enter text...' },
 { type: 'input_dummy' },
 { type: 'field_input', name: 'LABEL_TEXT', text: 'Label' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'input_blocks',
 tooltip: 'A text input field for user data entry',
 helpUrl: '',
 },

 // ── Checkbox ─────────────────────────────────────────
 {
 type: 'flutter_checkbox',
 message0: '️ Checkbox label %1 default %2',
 args0: [
 { type: 'field_input', name: 'LABEL', text: 'Accept terms' },
 {
 type: 'field_dropdown',
 name: 'DEFAULT_VALUE',
 options: [
 ['unchecked', 'false'],
 ['checked', 'true'],
 ],
 },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'input_blocks',
 tooltip: 'A Material Design checkbox with a label',
 helpUrl: '',
 },

 // ── Switch ───────────────────────────────────────────
 {
 type: 'flutter_switch',
 message0: ' Switch label %1 default %2',
 args0: [
 { type: 'field_input', name: 'LABEL', text: 'Dark mode' },
 {
 type: 'field_dropdown',
 name: 'DEFAULT_VALUE',
 options: [
 ['off', 'false'],
 ['on', 'true'],
 ],
 },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'input_blocks',
 tooltip: 'A Material Design toggle switch with a label',
 helpUrl: '',
 },
 ]);
}
