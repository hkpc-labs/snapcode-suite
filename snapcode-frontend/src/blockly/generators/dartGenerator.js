// ============================================================
// SnapCode — Custom Dart Code Generator (Base)
// Extends Blockly's CodeGenerator to output valid Dart/Flutter
// ============================================================
import * as Blockly from 'blockly';

// Create the Dart generator instance
export const dartGenerator = new Blockly.CodeGenerator('Dart');

// ── Operator Precedence (Dart-specific) ──────────────────
dartGenerator.ORDER_ATOMIC = 0;
dartGenerator.ORDER_UNARY_POSTFIX = 1;
dartGenerator.ORDER_UNARY_PREFIX = 2;
dartGenerator.ORDER_MULTIPLICATIVE = 3;
dartGenerator.ORDER_ADDITIVE = 4;
dartGenerator.ORDER_SHIFT = 5;
dartGenerator.ORDER_RELATIONAL = 6;
dartGenerator.ORDER_EQUALITY = 7;
dartGenerator.ORDER_BITWISE_AND = 8;
dartGenerator.ORDER_BITWISE_XOR = 9;
dartGenerator.ORDER_BITWISE_OR = 10;
dartGenerator.ORDER_LOGICAL_AND = 11;
dartGenerator.ORDER_LOGICAL_OR = 12;
dartGenerator.ORDER_CONDITIONAL = 13;
dartGenerator.ORDER_ASSIGNMENT = 14;
dartGenerator.ORDER_NONE = 99;

// ── Indentation ──────────────────────────────────────────
dartGenerator.INDENT = '  '; // 2-space indent for Dart convention

// ── Statement chaining (next block connection) ───────────
dartGenerator.scrub_ = function (block, code, opt_thisOnly) {
  const nextBlock = block.nextConnection && block.nextConnection.targetBlock();
  if (nextBlock && !opt_thisOnly) {
    const nextCode = this.blockToCode(nextBlock);
    return code + nextCode;
  }
  return code;
};

// ── Init (called before code generation starts) ──────────
dartGenerator.init = function (_workspace) {
  // Reset any state if needed
};

// ── Finish (called after all blocks are processed) ───────
dartGenerator.finish = function (code) {
  return code;
};

// Import all block generators
import { registerAppGenerators } from './appGenerators';
import { registerLayoutGenerators } from './layoutGenerators';
import { registerWidgetGenerators } from './widgetGenerators';
import { registerInputGenerators } from './inputGenerators';
import { registerLogicGenerators } from './logicGenerators';
import { registerVariableGenerators } from './variableGenerators';

// Register all generators
registerAppGenerators(dartGenerator);
registerLayoutGenerators(dartGenerator);
registerWidgetGenerators(dartGenerator);
registerInputGenerators(dartGenerator);
registerLogicGenerators(dartGenerator);
registerVariableGenerators(dartGenerator);
