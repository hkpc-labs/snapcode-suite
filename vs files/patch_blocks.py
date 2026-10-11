import re

with open('Assets/BlockEditor/index.html', 'r', encoding='utf-8') as f:
    content = f.read()

toolbox_addition = """<block type="widget_set_bg_color"></block>
      <block type="widget_set_text_color"></block>
      <block type="widget_set_font_size"></block>
      <block type="widget_set_width"></block>
      <block type="widget_set_height"></block>"""

content = content.replace('<block type="widget_set_bg_color"></block>', toolbox_addition)

json_addition = """{
    type: 'widget_set_bg_color',
    message0: 'set %1 background color %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_colour', name: 'COLOR', colour: '#2196F3' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
    tooltip: 'Sets the background color of a widget',
  },
  {
    type: 'widget_set_text_color',
    message0: 'set %1 text color %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_colour', name: 'COLOR', colour: '#000000' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
  },
  {
    type: 'widget_set_font_size',
    message0: 'set %1 font size %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_number', name: 'SIZE', value: 14 },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
  },
  {
    type: 'widget_set_width',
    message0: 'set %1 width %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_input', name: 'WIDTH', text: '100' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
  },
  {
    type: 'widget_set_height',
    message0: 'set %1 height %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_input', name: 'HEIGHT', text: '100' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
  }"""

content = re.sub(r'\{\s*type:\s*\'widget_set_bg_color\'.*?tooltip:\s*\'Sets the background color of a widget\',\s*\}', json_addition, content, flags=re.DOTALL)

content = content.replace("'widget_set_text', 'widget_get_text', 'widget_set_visible', 'widget_set_enabled', 'widget_set_bg_color'", "'widget_set_text', 'widget_get_text', 'widget_set_visible', 'widget_set_enabled', 'widget_set_bg_color', 'widget_set_text_color', 'widget_set_font_size', 'widget_set_width', 'widget_set_height'")

generator_addition = """dartGenerator.forBlock['widget_set_bg_color'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const color = hexToColor(block.getFieldValue('COLOR'));
  return `// DYNAMIC_PROP: setBgColor(Color(${color})) -> ${widgetId}\\n`;
};

dartGenerator.forBlock['widget_set_text_color'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const color = hexToColor(block.getFieldValue('COLOR'));
  return `// DYNAMIC_PROP: setTextColor(Color(${color})) -> ${widgetId}\\n`;
};

dartGenerator.forBlock['widget_set_font_size'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const size = block.getFieldValue('SIZE') || '14';
  return `// DYNAMIC_PROP: setFontSize(${size}) -> ${widgetId}\\n`;
};

dartGenerator.forBlock['widget_set_width'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const width = block.getFieldValue('WIDTH') || '100';
  return `// DYNAMIC_PROP: setWidth('${esc(width)}') -> ${widgetId}\\n`;
};

dartGenerator.forBlock['widget_set_height'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const height = block.getFieldValue('HEIGHT') || '100';
  return `// DYNAMIC_PROP: setHeight('${esc(height)}') -> ${widgetId}\\n`;
};"""

content = re.sub(r'dartGenerator\.forBlock\[\'widget_set_bg_color\'\].*?return `// DYNAMIC_PROP: setBgColor.*?\\n`;\n  };', generator_addition, content, flags=re.DOTALL)

with open('Assets/BlockEditor/index.html', 'w', encoding='utf-8') as f:
    f.write(content)

print('Patch applied')
