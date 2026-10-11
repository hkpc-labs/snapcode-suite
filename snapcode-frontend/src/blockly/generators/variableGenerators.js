// ============================================================
// SnapCode — Variable & Math Dart Generators
// ============================================================

export function registerVariableGenerators(generator) {
  // ── Set Variable ───────────────────────────────────────
  generator.forBlock['variable_set'] = function (block) {
    const varName = block.getFieldValue('VAR_NAME') || 'myVar';
    const value = block.getFieldValue('VALUE') || '0';
    return `var ${varName} = ${value};\n`;
  };

  // ── Get Variable ───────────────────────────────────────
  generator.forBlock['variable_get'] = function (block) {
    const varName = block.getFieldValue('VAR_NAME') || 'myVar';
    return [varName, generator.ORDER_ATOMIC];
  };

  // ── Math Arithmetic ────────────────────────────────────
  generator.forBlock['math_arithmetic'] = function (block) {
    const left = block.getFieldValue('LEFT') || '0';
    const right = block.getFieldValue('RIGHT') || '0';
    const op = block.getFieldValue('OP') || 'ADD';

    const opMap = {
      ADD: '+',
      SUBTRACT: '-',
      MULTIPLY: '*',
      DIVIDE: '/',
      MODULO: '%',
    };

    const operator = opMap[op] || '+';
    return [`${left} ${operator} ${right}`, generator.ORDER_ADDITIVE];
  };

  // ── Number Literal ─────────────────────────────────────
  generator.forBlock['math_number'] = function (block) {
    const num = block.getFieldValue('NUM') || 0;
    return [String(num), generator.ORDER_ATOMIC];
  };

  // ── String Literal ─────────────────────────────────────
  generator.forBlock['text_literal'] = function (block) {
    const text = block.getFieldValue('TEXT') || '';
    return [`'${escapeString(text)}'`, generator.ORDER_ATOMIC];
  };

  // ── String Join ────────────────────────────────────────
  generator.forBlock['text_join'] = function (block) {
    const left = block.getFieldValue('LEFT') || '';
    const right = block.getFieldValue('RIGHT') || '';
    return [`'${escapeString(left)}${escapeString(right)}'`, generator.ORDER_ATOMIC];
  };
}

function escapeString(str) {
  return str.replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}
