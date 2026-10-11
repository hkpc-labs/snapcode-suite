// ============================================================
// SnapCode — App Structure Dart Generators
// ============================================================

export function registerAppGenerators(generator) {
  // ── flutter_app_scaffold → Full main.dart ──────────────
  generator.forBlock['flutter_app_scaffold'] = function (block, gen) {
    const title = block.getFieldValue('APP_TITLE') || 'My App';
    const colorHex = block.getFieldValue('THEME_COLOR') || '#6C63FF';
    const dartColor = '0xFF' + colorHex.slice(1).toUpperCase();
    const body = gen.statementToCode(block, 'BODY') || '  const Center(child: Text(\'Hello SnapCode!\'))';

    // Indent body lines for proper nesting inside the Scaffold
    const bodyWidget = body.trim();

    return `import 'package:flutter/material.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: '${title}',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: const Color(${dartColor}),
        useMaterial3: true,
        brightness: Brightness.light,
      ),
      home: Scaffold(
        appBar: AppBar(
          title: const Text('${title}'),
          centerTitle: true,
        ),
        body: SafeArea(
          child: ${bodyWidget},
        ),
      ),
    );
  }
}
`;
  };

  // ── flutter_app_bar ────────────────────────────────────
  generator.forBlock['flutter_app_bar'] = function (block, gen) {
    const title = block.getFieldValue('TITLE') || 'App Bar';
    const centerTitle = block.getFieldValue('CENTER_TITLE') || 'true';
    return `AppBar(\n  title: const Text('${title}'),\n  centerTitle: ${centerTitle},\n),\n`;
  };
}
