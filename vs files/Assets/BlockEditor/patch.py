import sys
html = open('index.html', 'r', encoding='utf-8').read()

generator_code = '''
  dartGenerator.forBlock['flutter_set_widget_property'] = function(block) {
    const prop = block.getFieldValue('PROPERTY');
    const widgetId = block.getFieldValue('WIDGET_ID');
    const value = dartGenerator.valueToCode(block, 'VALUE', dartGenerator.ORDER_NONE) || '\\'\\'';
    return 'setState(() {\\n  // ' + widgetId + '.' + prop + ' = ' + value + '\\n});\\n';
  };
'''

if "dartGenerator.forBlock['flutter_set_widget_property']" not in html:
    html = html.replace("dartGenerator.forBlock['flutter_app_scaffold']", generator_code + "\\n  dartGenerator.forBlock['flutter_app_scaffold']")
    open('index.html', 'w', encoding='utf-8').write(html)
    print('Generator added!')
