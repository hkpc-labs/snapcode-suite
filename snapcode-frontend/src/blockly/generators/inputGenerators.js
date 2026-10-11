// ============================================================
// SnapCode — Input Widget Dart Generators
// ============================================================

export function registerInputGenerators(generator) {
  // ── TextField ──────────────────────────────────────────
  generator.forBlock['flutter_text_field'] = function (block) {
    const hint = block.getFieldValue('HINT_TEXT') || '';
    const label = block.getFieldValue('LABEL_TEXT') || '';

    return `TextField(
  decoration: const InputDecoration(
    hintText: '${escapeString(hint)}',
    labelText: '${escapeString(label)}',
    border: OutlineInputBorder(),
  ),
),\n`;
  };

  // ── Checkbox ───────────────────────────────────────────
  generator.forBlock['flutter_checkbox'] = function (block) {
    const label = block.getFieldValue('LABEL') || 'Option';
    const defaultVal = block.getFieldValue('DEFAULT_VALUE') || 'false';

    return `Row(
  mainAxisSize: MainAxisSize.min,
  children: [
    Checkbox(
      value: ${defaultVal},
      onChanged: (val) {
        // TODO: Add state management
      },
    ),
    const Text('${escapeString(label)}'),
  ],
),\n`;
  };

  // ── Switch ─────────────────────────────────────────────
  generator.forBlock['flutter_switch'] = function (block) {
    const label = block.getFieldValue('LABEL') || 'Toggle';
    const defaultVal = block.getFieldValue('DEFAULT_VALUE') || 'false';

    return `Row(
  mainAxisSize: MainAxisSize.min,
  children: [
    const Text('${escapeString(label)}'),
    Switch(
      value: ${defaultVal},
      onChanged: (val) {
        // TODO: Add state management
      },
    ),
  ],
),\n`;
  };
}

function escapeString(str) {
  return str.replace(/\\/g, '\\\\').replace(/'/g, "\\'");
}
