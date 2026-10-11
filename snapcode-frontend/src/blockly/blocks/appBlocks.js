// ============================================================
// SnapCode — App Structure Block Definitions
// ============================================================
import * as Blockly from 'blockly';

export function registerAppBlocks() {
 Blockly.defineBlocksWithJsonArray([
 // ── MaterialApp + Scaffold ───────────────────────────
 {
 type: 'flutter_app_scaffold',
 message0: ' Flutter App %1 Title %2 %3 Theme Color %4 %5 Body %6',
 args0: [
 { type: 'input_dummy' },
 { type: 'field_input', name: 'APP_TITLE', text: 'My App' },
 { type: 'input_dummy' },
 { type: 'field_colour', name: 'THEME_COLOR', colour: '#6C63FF' },
 { type: 'input_dummy' },
 { type: 'input_statement', name: 'BODY' },
 ],
 style: 'app_blocks',
 tooltip: 'Creates a MaterialApp with Scaffold as the root widget',
 helpUrl: '',
 },

 // ── AppBar ───────────────────────────────────────────
 {
 type: 'flutter_app_bar',
 message0: ' AppBar title %1 %2 center title %3',
 args0: [
 { type: 'field_input', name: 'TITLE', text: 'My App' },
 { type: 'input_dummy' },
 {
 type: 'field_dropdown',
 name: 'CENTER_TITLE',
 options: [
 ['yes', 'true'],
 ['no', 'false'],
 ],
 },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'app_blocks',
 tooltip: 'AppBar widget with a title',
 helpUrl: '',
 },
 ]);
}
