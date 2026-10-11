// ============================================================
// SnapCode — Widget Code Generators
// ============================================================
import { dartGenerator } from './dartGenerator';

export function registerWidgetGenerators() {
  const esc = (str) => (str || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
  const hexToColor = (hex) => '0xFF' + (hex || '#000000').replace('#', '').toUpperCase();

  dartGenerator.forBlock['flutter_text'] = function (block) {
    const text = block.getFieldValue('TEXT_VALUE') || '';
    const size = block.getFieldValue('FONT_SIZE') || 16;
    const weight = block.getFieldValue('FONT_WEIGHT') || 'FontWeight.normal';
    const color = hexToColor(block.getFieldValue('TEXT_COLOR'));

    const code = `Text(\n  '${esc(text)}',\n  style: const TextStyle(fontSize: ${size}, fontWeight: ${weight}, color: Color(${color})),\n)`;
    return code;
  };

  dartGenerator.forBlock['flutter_elevated_button'] = function (block) {
    const text = block.getFieldValue('BUTTON_TEXT') || 'Button';
    const onPressed = dartGenerator.statementToCode(block, 'ON_PRESSED') || "print('pressed');\n";

    const code = `ElevatedButton(\n  onPressed: () {\n${onPressed}  },\n  child: const Text('${esc(text)}'),\n)`;
    return code;
  };

  dartGenerator.forBlock['flutter_icon'] = function (block) {
    const icon = block.getFieldValue('ICON_NAME') || 'Icons.star';
    const size = block.getFieldValue('ICON_SIZE') || 24;
    const color = hexToColor(block.getFieldValue('ICON_COLOR'));

    const code = `Icon(${icon}, size: ${size}, color: const Color(${color}))`;
    return code;
  };

  dartGenerator.forBlock['flutter_image_network'] = function (block) {
    const url = block.getFieldValue('IMAGE_URL') || '';
    const w = block.getFieldValue('WIDTH') || 0;
    const h = block.getFieldValue('HEIGHT') || 0;

    let props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);
    props.push(`fit: BoxFit.cover`);

    const code = `Image.network(\n  '${esc(url)}',\n  ${props.join(',\n  ')}\n)`;
    return code;
  };

  dartGenerator.forBlock['flutter_image_asset'] = function (block) {
    const path = block.getFieldValue('IMAGE_PATH') || 'assets/image.png';
    const w = block.getFieldValue('WIDTH') || 0;
    const h = block.getFieldValue('HEIGHT') || 0;

    let props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);
    props.push(`fit: BoxFit.cover`);

    const code = `Image.asset(\n  '${esc(path)}',\n  ${props.join(',\n  ')}\n)`;
    return code;
  };

  dartGenerator.forBlock['flutter_circle_avatar'] = function (block) {
    const radius = block.getFieldValue('RADIUS') || 20;
    const color = hexToColor(block.getFieldValue('COLOR'));
    const child = dartGenerator.statementToCode(block, 'CHILD');
    
    let props = [`radius: ${radius}`, `backgroundColor: const Color(${color})`];
    if (child.trim()) props.push(`child: ${child.trim().replace(/,\s*$/, '')}`);

    return `CircleAvatar(\n  ${props.join(',\n  ')}\n)`;
  };

  dartGenerator.forBlock['flutter_list_tile'] = function (block) {
    const leading = dartGenerator.statementToCode(block, 'LEADING').trim().replace(/,\s*$/, '');
    const title = dartGenerator.statementToCode(block, 'TITLE').trim().replace(/,\s*$/, '');
    const subtitle = dartGenerator.statementToCode(block, 'SUBTITLE').trim().replace(/,\s*$/, '');
    const trailing = dartGenerator.statementToCode(block, 'TRAILING').trim().replace(/,\s*$/, '');
    const onTap = dartGenerator.statementToCode(block, 'ON_TAP');

    let props = [];
    if (leading) props.push(`leading: ${leading}`);
    if (title) props.push(`title: ${title}`);
    if (subtitle) props.push(`subtitle: ${subtitle}`);
    if (trailing) props.push(`trailing: ${trailing}`);
    if (onTap) props.push(`onTap: () {\n${onTap}  }`);

    return `ListTile(\n  ${props.join(',\n  ')}\n)`;
  };

  dartGenerator.forBlock['flutter_card'] = function (block) {
    const elevation = block.getFieldValue('ELEVATION') || 4;
    const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';

    const code = `Card(\n  elevation: ${elevation},\n  child: Padding(\n    padding: const EdgeInsets.all(16),\n    child: ${child.trim()},\n  ),\n)`;
    return code;
  };

  dartGenerator.forBlock['flutter_divider'] = function (block) {
    const thickness = block.getFieldValue('THICKNESS') || 1;
    const color = hexToColor(block.getFieldValue('COLOR'));

    return `Divider(thickness: ${thickness}, color: const Color(${color}))`;
  };

  dartGenerator.forBlock['flutter_snackbar'] = function (block) {
    const text = block.getFieldValue('TEXT') || '';
    return `ScaffoldMessenger.of(context).showSnackBar(\n  SnackBar(content: Text('${esc(text)}'))\n);\n`;
  };
}
