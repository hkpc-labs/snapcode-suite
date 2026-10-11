// ============================================================
// SnapCode — Layout Code Generators
// ============================================================
import { dartGenerator } from './dartGenerator';

export function registerLayoutGenerators() {
  const hexToColor = (hex) => '0xFF' + (hex || '#000000').replace('#', '').toUpperCase();

  // Helper to generate a comma-separated list of children
  function generateChildrenList(block, inputName) {
    let childBlock = block.getInputTargetBlock(inputName);
    let parts = [];
    while (childBlock) {
      const code = dartGenerator.blockToCode(childBlock);
      if (code) parts.push(code.trim().replace(/,\s*$/, ''));
      childBlock = childBlock.getNextBlock();
    }
    if (parts.length === 0) return 'const SizedBox.shrink(),';
    return parts.join(',\n    ') + ',';
  }

  dartGenerator.forBlock['flutter_column'] = function (block) {
    const align = block.getFieldValue('ALIGNMENT');
    const children = generateChildrenList(block, 'CHILDREN');
    
    return `Column(\n  mainAxisAlignment: ${align},\n  children: [\n    ${children}\n  ],\n)`;
  };

  dartGenerator.forBlock['flutter_row'] = function (block) {
    const align = block.getFieldValue('ALIGNMENT');
    const children = generateChildrenList(block, 'CHILDREN');
    
    return `Row(\n  mainAxisAlignment: ${align},\n  children: [\n    ${children}\n  ],\n)`;
  };

  dartGenerator.forBlock['flutter_container'] = function (block) {
    const w = block.getFieldValue('WIDTH') || 0;
    const h = block.getFieldValue('HEIGHT') || 0;
    const color = hexToColor(block.getFieldValue('COLOR'));
    const radius = block.getFieldValue('BORDER_RADIUS') || 0;
    
    const child = dartGenerator.statementToCode(block, 'CHILD').trim().replace(/,\s*$/, '');

    let props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);
    
    let decProps = [`color: const Color(${color})`];
    if (radius > 0) decProps.push(`borderRadius: BorderRadius.circular(${radius})`);
    props.push(`decoration: BoxDecoration(${decProps.join(', ')})`);
    
    if (child) props.push(`child: ${child}`);

    return `Container(\n  ${props.join(',\n  ')},\n)`;
  };

  dartGenerator.forBlock['flutter_sized_box'] = function (block) {
    const w = block.getFieldValue('WIDTH') || 0;
    const h = block.getFieldValue('HEIGHT') || 0;

    let props = [];
    if (w > 0) props.push(`width: ${w}`);
    if (h > 0) props.push(`height: ${h}`);

    return `const SizedBox(${props.join(', ')})`;
  };

  dartGenerator.forBlock['flutter_padding'] = function (block) {
    const p = block.getFieldValue('PADDING') || 0;
    const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';

    return `Padding(\n  padding: const EdgeInsets.all(${p}),\n  child: ${child.trim()},\n)`;
  };

  dartGenerator.forBlock['flutter_center'] = function (block) {
    const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
    return `Center(child: ${child.trim()})`;
  };

  dartGenerator.forBlock['flutter_expanded'] = function (block) {
    const flex = block.getFieldValue('FLEX') || 1;
    const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
    return `Expanded(\n  flex: ${flex},\n  child: ${child.trim()},\n)`;
  };

  dartGenerator.forBlock['flutter_listview'] = function (block) {
    const padding = block.getFieldValue('PADDING') || 0;
    const children = generateChildrenList(block, 'CHILDREN');
    
    return `ListView(\n  padding: const EdgeInsets.all(${padding}),\n  children: [\n    ${children}\n  ],\n)`;
  };

  dartGenerator.forBlock['flutter_gridview'] = function (block) {
    const columns = block.getFieldValue('COLUMNS') || 2;
    const spacing = block.getFieldValue('SPACING') || 8;
    const children = generateChildrenList(block, 'CHILDREN');
    
    return `GridView.count(\n  crossAxisCount: ${columns},\n  mainAxisSpacing: ${spacing},\n  crossAxisSpacing: ${spacing},\n  children: [\n    ${children}\n  ],\n)`;
  };

  dartGenerator.forBlock['flutter_stack'] = function (block) {
    const children = generateChildrenList(block, 'CHILDREN');
    
    return `Stack(\n  children: [\n    ${children}\n  ],\n)`;
  };

  dartGenerator.forBlock['flutter_positioned'] = function (block) {
    const top = block.getFieldValue('TOP') || 0;
    const bottom = block.getFieldValue('BOTTOM') || 0;
    const left = block.getFieldValue('LEFT') || 0;
    const right = block.getFieldValue('RIGHT') || 0;
    const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
    
    return `Positioned(\n  top: ${top},\n  bottom: ${bottom},\n  left: ${left},\n  right: ${right},\n  child: ${child.trim()},\n)`;
  };
}
