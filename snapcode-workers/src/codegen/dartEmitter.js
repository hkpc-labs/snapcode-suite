// ============================================================
// SnapCode — Dart Code Emitter
// Walks the Blockly AST and generates valid Flutter/Dart code
// This runs server-side in the Consumer Worker
// ============================================================

/**
 * Convert a hex color string to Dart Color format.
 * @param {string} hex - e.g., "#6C63FF"
 * @returns {string} - e.g., "0xFF6C63FF"
 */
function hexToDartColor(hex) {
  return '0xFF' + (hex || '#000000').replace('#', '').toUpperCase();
}

/** Escape single quotes for Dart string literals */
function esc(str) {
  return (str || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}

/** Indent each line of code by a given number of spaces */
function indent(code, spaces = 2) {
  const pad = ' '.repeat(spaces);
  return code.split('\n').filter(l => l.trim()).map(l => pad + l).join('\n');
}

// ── Block-to-Dart mapping ─────────────────────────────────
const generators = {
  flutter_app_scaffold(block, ctx) {
    const title = block.fields?.APP_TITLE || 'My App';
    const color = hexToDartColor(block.fields?.THEME_COLOR);
    const bodyCode = generateChildren(block.inputs?.BODY, ctx) ||
      'const Center(child: Text(\'Hello SnapCode!\'))';

    return `import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: '${esc(title)}',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: const Color(${color}),
        useMaterial3: true,
      ),
      home: Scaffold(
        appBar: AppBar(title: const Text('${esc(title)}'), centerTitle: true),
        body: SafeArea(
          child: ${bodyCode.trim()},
        ),
      ),
    );
  }
}
`;
  },

  flutter_column(block, ctx) {
    const align = block.fields?.ALIGNMENT || 'MainAxisAlignment.start';
    const children = generateChildrenList(block.inputs?.CHILDREN, ctx);
    return `Column(\n  mainAxisAlignment: ${align},\n  children: [\n${indent(children, 4)}\n  ],\n)`;
  },

  flutter_row(block, ctx) {
    const align = block.fields?.ALIGNMENT || 'MainAxisAlignment.start';
    const children = generateChildrenList(block.inputs?.CHILDREN, ctx);
    return `Row(\n  mainAxisAlignment: ${align},\n  children: [\n${indent(children, 4)}\n  ],\n)`;
  },

  flutter_text(block) {
    const text = block.fields?.TEXT_VALUE || '';
    const size = block.fields?.FONT_SIZE || 16;
    const weight = block.fields?.FONT_WEIGHT || 'FontWeight.normal';
    const color = hexToDartColor(block.fields?.TEXT_COLOR);
    return `Text(\n  '${esc(text)}',\n  style: const TextStyle(fontSize: ${size}, fontWeight: ${weight}, color: Color(${color})),\n)`;
  },

  flutter_elevated_button(block, ctx) {
    const text = block.fields?.BUTTON_TEXT || 'Button';
    const onPressed = generateChildren(block.inputs?.ON_PRESSED, ctx) || "print('pressed');";
    return `ElevatedButton(\n  onPressed: () { ${onPressed.trim()} },\n  child: const Text('${esc(text)}'),\n)`;
  },

  flutter_icon(block) {
    const icon = block.fields?.ICON_NAME || 'Icons.star';
    const size = block.fields?.ICON_SIZE || 24;
    const color = hexToDartColor(block.fields?.ICON_COLOR);
    return `Icon(${icon}, size: ${size}, color: const Color(${color}))`;
  },

  flutter_container(block, ctx) {
    const w = block.fields?.WIDTH || 0;
    const h = block.fields?.HEIGHT || 0;
    const color = hexToDartColor(block.fields?.COLOR);
    const radius = block.fields?.BORDER_RADIUS || 0;
    const child = generateChildren(block.inputs?.CHILD, ctx);
    let props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);
    props.push(`decoration: BoxDecoration(color: const Color(${color})${radius > 0 ? `, borderRadius: BorderRadius.circular(${radius})` : ''})`);
    if (child) props.push(`child: ${child.trim()}`);
    return `Container(\n  ${props.join(',\n  ')},\n)`;
  },

  flutter_sized_box(block) {
    const w = block.fields?.WIDTH || 0;
    const h = block.fields?.HEIGHT || 0;
    const props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);
    return `const SizedBox(${props.join(', ')})`;
  },

  flutter_padding(block, ctx) {
    const p = block.fields?.PADDING || 16;
    const child = generateChildren(block.inputs?.CHILD, ctx) || 'const SizedBox.shrink()';
    return `Padding(\n  padding: const EdgeInsets.all(${p}),\n  child: ${child.trim()},\n)`;
  },

  flutter_center(block, ctx) {
    const child = generateChildren(block.inputs?.CHILD, ctx) || 'const SizedBox.shrink()';
    return `Center(child: ${child.trim()})`;
  },

  flutter_card(block, ctx) {
    const elevation = block.fields?.ELEVATION || 4;
    const child = generateChildren(block.inputs?.CHILD, ctx) || 'const SizedBox.shrink()';
    return `Card(\n  elevation: ${elevation},\n  child: Padding(\n    padding: const EdgeInsets.all(16),\n    child: ${child.trim()},\n  ),\n)`;
  },

  flutter_text_field(block) {
    const hint = block.fields?.HINT_TEXT || '';
    const label = block.fields?.LABEL_TEXT || '';
    return `TextField(\n  decoration: const InputDecoration(\n    hintText: '${esc(hint)}',\n    labelText: '${esc(label)}',\n    border: OutlineInputBorder(),\n  ),\n)`;
  },

  flutter_divider(block) {
    const thickness = block.fields?.THICKNESS || 1;
    const color = hexToDartColor(block.fields?.COLOR);
    return `Divider(thickness: ${thickness}, color: const Color(${color}))`;
  },

  logic_print(block) {
    const msg = block.fields?.MESSAGE || '';
    return `print('${esc(msg)}');`;
  },

  logic_if_else(block, ctx) {
    const cond = block.fields?.CONDITION || 'true';
    const doIf = generateChildren(block.inputs?.DO_IF, ctx) || '// then';
    const doElse = generateChildren(block.inputs?.DO_ELSE, ctx) || '// else';
    return `if (${cond}) {\n${indent(doIf)}\n} else {\n${indent(doElse)}\n}`;
  },

  logic_for_loop(block, ctx) {
    const from = block.fields?.FROM ?? 0;
    const to = block.fields?.TO ?? 10;
    const body = generateChildren(block.inputs?.DO, ctx) || '// body';
    return `for (int i = ${from}; i < ${to}; i++) {\n${indent(body)}\n}`;
  },

  variable_set(block) {
    const name = block.fields?.VAR_NAME || 'myVar';
    const value = block.fields?.VALUE || '0';
    return `var ${name} = ${value};`;
  },
};

// ── Tree Walker ────────────────────────────────────────────

/**
 * Generate code for a single block node (from saved AST JSON).
 */
function generateBlock(blockData, ctx = {}) {
  if (!blockData || !blockData.type) return '';

  const gen = generators[blockData.type];
  if (!gen) return `// Unknown block: ${blockData.type}\n`;

  let code = gen(blockData, ctx);

  // Process "next" block (statement chaining)
  if (blockData.next?.block) {
    code += ',\n' + generateBlock(blockData.next.block, ctx);
  }

  return code;
}

/**
 * Generate code for blocks connected to a statement input.
 */
function generateChildren(input, ctx = {}) {
  if (!input?.block) return '';
  return generateBlock(input.block, ctx);
}

/**
 * Generate a comma-separated list of children (for Column/Row).
 */
function generateChildrenList(input, ctx = {}) {
  if (!input?.block) return 'const SizedBox.shrink(),';

  let parts = [];
  let current = input.block;
  while (current) {
    const gen = generators[current.type];
    if (gen) {
      parts.push(gen(current, ctx) + ',');
    }
    current = current.next?.block || null;
  }
  return parts.join('\n');
}

/**
 * Main entry point: Takes a full Blockly workspace JSON and returns Dart code.
 */
export function generateDartFromAST(workspaceJson) {
  if (!workspaceJson) return '// Empty workspace\n';

  // Handle both our custom AST format and raw Blockly serialization format
  const workspace = workspaceJson.workspace || workspaceJson;
  const blocks = workspace.blocks?.blocks || workspace.blocks || [];

  if (blocks.length === 0) return '// No blocks in workspace\n';

  let fullCode = '';
  for (const block of blocks) {
    fullCode += generateBlock(block) + '\n';
  }

  return fullCode;
}
