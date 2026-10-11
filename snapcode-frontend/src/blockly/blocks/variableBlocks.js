// ============================================================
// SnapCode — Variable & Math Block Definitions
// ============================================================
import * as Blockly from 'blockly';

export function registerVariableBlocks() {
 Blockly.defineBlocksWithJsonArray([
 // ── Set Variable ─────────────────────────────────────
 {
 type: 'variable_set',
 message0: ' set %1 = %2',
 args0: [
 { type: 'field_input', name: 'VAR_NAME', text: 'myVar' },
 { type: 'field_input', name: 'VALUE', text: '0' },
 ],
 previousStatement: null,
 nextStatement: null,
 style: 'variable_blocks',
 tooltip: 'Set a variable to a value',
 helpUrl: '',
 },

 // ── Get Variable (expression output) ─────────────────
 {
 type: 'variable_get',
 message0: ' get %1',
 args0: [
 { type: 'field_input', name: 'VAR_NAME', text: 'myVar' },
 ],
 output: 'String',
 style: 'variable_blocks',
 tooltip: 'Returns the value of a variable',
 helpUrl: '',
 },

 // ── Math Arithmetic ──────────────────────────────────
 {
 type: 'math_arithmetic',
 message0: '%1 %2 %3',
 args0: [
 { type: 'field_input', name: 'LEFT', text: '0' },
 {
 type: 'field_dropdown',
 name: 'OP',
 options: [
 ['+', 'ADD'],
 ['−', 'SUBTRACT'],
 ['×', 'MULTIPLY'],
 ['÷', 'DIVIDE'],
 ['%', 'MODULO'],
 ],
 },
 { type: 'field_input', name: 'RIGHT', text: '0' },
 ],
 output: 'Number',
 style: 'variable_blocks',
 tooltip: 'Perform arithmetic on two values',
 helpUrl: '',
 },

 // ── Number Literal ───────────────────────────────────
 {
 type: 'math_number',
 message0: ' %1',
 args0: [
 { type: 'field_number', name: 'NUM', value: 0 },
 ],
 output: 'Number',
 style: 'variable_blocks',
 tooltip: 'A number value',
 helpUrl: '',
 },

 // ── String Literal ───────────────────────────────────
 {
 type: 'text_literal',
 message0: '" %1 "',
 args0: [
 { type: 'field_input', name: 'TEXT', text: 'hello' },
 ],
 output: 'String',
 style: 'variable_blocks',
 tooltip: 'A text string value',
 helpUrl: '',
 },

 // ── String Join ──────────────────────────────────────
 {
 type: 'text_join',
 message0: ' join %1 + %2',
 args0: [
 { type: 'field_input', name: 'LEFT', text: 'Hello' },
 { type: 'field_input', name: 'RIGHT', text: 'World' },
 ],
 output: 'String',
 style: 'variable_blocks',
 tooltip: 'Concatenate two text strings',
 helpUrl: '',
 },
 ]);
}
