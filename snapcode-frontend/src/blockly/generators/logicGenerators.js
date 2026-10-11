// ============================================================
// SnapCode — Logic & Control Flow Dart Generators
// ============================================================

export function registerLogicGenerators(generator) {
  // ── If / Else ──────────────────────────────────────────
  generator.forBlock['logic_if_else'] = function (block, gen) {
    const condition = block.getFieldValue('CONDITION') || 'true';
    const doIf = gen.statementToCode(block, 'DO_IF') || '  // then\n';
    const doElse = gen.statementToCode(block, 'DO_ELSE') || '  // else\n';

    return `if (${condition}) {
${indent(doIf)}
} else {
${indent(doElse)}
}
`;
  };

  // ── For Loop ───────────────────────────────────────────
  generator.forBlock['logic_for_loop'] = function (block, gen) {
    const from = block.getFieldValue('FROM') ?? 0;
    const to = block.getFieldValue('TO') ?? 10;
    const body = gen.statementToCode(block, 'DO') || '  // loop body\n';

    return `for (int i = ${from}; i < ${to}; i++) {
${indent(body)}
}
`;
  };

  // ── While Loop ─────────────────────────────────────────
  generator.forBlock['logic_while_loop'] = function (block, gen) {
    const condition = block.getFieldValue('CONDITION') || 'true';
    const body = gen.statementToCode(block, 'DO') || '  // loop body\n';

    return `while (${condition}) {
${indent(body)}
}
`;
  };

  // ── Print ──────────────────────────────────────────────
  generator.forBlock['logic_print'] = function (block) {
    const message = block.getFieldValue('MESSAGE') || '';
    return `print('${escapeString(message)}');\n`;
  };

  // ── Function ───────────────────────────────────────────
  generator.forBlock['logic_function'] = function (block, gen) {
    const funcName = block.getFieldValue('FUNC_NAME') || 'myFunction';
    const body = gen.statementToCode(block, 'BODY') || '  // function body\n';

    return `void ${funcName}() {
${indent(body)}
}
`;
  };
}

function indent(code) {
  return code
    .split('\n')
    .filter(l => l.trim())
    .map(l => '  ' + l.trimStart())
    .join('\n');
}

function escapeString(str) {
  return str.replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}
