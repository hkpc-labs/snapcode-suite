// ============================================================
// SnapCode — Logic & Control Flow Block Definitions
// ============================================================
import * as Blockly from 'blockly';

export function registerLogicBlocks() {
 Blockly.defineBlocksWithJsonArray([
 // ── If / Else ────────────────────────────────────────
 {
 type: 'logic_if_else',
 message0: ' if %1 %2 then %3 else %4',
 args0: [
 { type: 'field_input', name: 'CONDITION', text: 'true' },
 { type: 'input_dummy' },
 { type: 'input_statement', name: 'DO_IF' },
 { type: 'input_statement', name: 'DO_ELSE' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'logic_blocks',
 tooltip: 'If the condition is true, run the first set of statements; otherwise run the second',
 helpUrl: '',
 },

 // ── For Loop ─────────────────────────────────────────
 {
 type: 'logic_for_loop',
 message0: ' for i from %1 to %2 %3 do %4',
 args0: [
 { type: 'field_number', name: 'FROM', value: 0, precision: 1 },
 { type: 'field_number', name: 'TO', value: 10, precision: 1 },
 { type: 'input_dummy' },
 { type: 'input_statement', name: 'DO' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'logic_blocks',
 tooltip: 'Repeat a set of statements a specific number of times',
 helpUrl: '',
 },

 // ── While Loop ───────────────────────────────────────
 {
 type: 'logic_while_loop',
 message0: ' while %1 %2 do %3',
 args0: [
 { type: 'field_input', name: 'CONDITION', text: 'true' },
 { type: 'input_dummy' },
 { type: 'input_statement', name: 'DO' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'logic_blocks',
 tooltip: 'Repeat statements while the condition is true',
 helpUrl: '',
 },

 // ── Print ────────────────────────────────────────────
 {
 type: 'logic_print',
 message0: '️ print %1',
 args0: [
 { type: 'field_input', name: 'MESSAGE', text: 'Hello World!' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'logic_blocks',
 tooltip: 'Print a message to the debug console',
 helpUrl: '',
 },

 // ── Custom Function ──────────────────────────────────
 {
 type: 'logic_function',
 message0: '️ function %1 %2 body %3',
 args0: [
 { type: 'field_input', name: 'FUNC_NAME', text: 'myFunction' },
 { type: 'input_dummy' },
 { type: 'input_statement', name: 'BODY' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'logic_blocks',
 tooltip: 'Define a reusable function',
 helpUrl: '',
 },
 ]);
}
