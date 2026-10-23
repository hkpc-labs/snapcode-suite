
// ============================================================
// SnapCode — Block Definitions (all inlined)
// ============================================================

// ── App Blocks ──────────────────────────────────────────
Blockly.defineBlocksWithJsonArray([
    // ==========================================
    // Interaction & Logic Blocks
    // ==========================================
    {
      type: 'flutter_set_widget_property',
      message0: 'Set %1 of widget %2 to %3',
      args0: [
        { 
          type: 'field_dropdown', 
          name: 'PROPERTY', 
          options: [['Text','text'], ['Background Color','backgroundColor'], ['Text Color','textColor'], ['Visible','visible']] 
        },
        { 
          type: 'field_dropdown', 
          name: 'WIDGET_ID',
          options: [['(no widgets in View Designer)', 'none']]
        },
        { type: 'input_value', name: 'VALUE' }
      ],
      previousStatement: null,
      nextStatement: null,
      colour: '#d97706',
      extensions: ['dynamic_widget_dropdown']
    },

  {
    type: 'flutter_app_scaffold',
    message0: 'Flutter App %1 Title %2 %3 Theme Color %4 %5 Body %6',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'APP_TITLE', text: 'My App' },
      { type: 'input_dummy' },
      { type: 'field_colour', name: 'THEME_COLOR', colour: '#6C63FF' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'BODY' },
    ],
    colour: '#7c3aed',
    tooltip: 'Creates a MaterialApp with Scaffold as the root widget',
    hat: 'cap',
  },
  {
    type: 'flutter_app_bar',
    message0: 'AppBar title %1 %2 center title %3',
    args0: [
      { type: 'field_input', name: 'TITLE', text: 'My App' },
      { type: 'input_dummy' },
      { type: 'field_dropdown', name: 'CENTER_TITLE', options: [['yes','true'],['no','false']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#7c3aed',
  },

  // ── Layout Blocks ────────────────────────────────────
  {
    type: 'flutter_column',
    message0: 'Column %1 align %2 %3 children %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_dropdown', name: 'ALIGNMENT', options: [['start','MainAxisAlignment.start'],['center','MainAxisAlignment.center'],['end','MainAxisAlignment.end'],['space between','MainAxisAlignment.spaceBetween'],['space around','MainAxisAlignment.spaceAround']] },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILDREN' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_row',
    message0: 'Row %1 align %2 %3 children %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_dropdown', name: 'ALIGNMENT', options: [['start','MainAxisAlignment.start'],['center','MainAxisAlignment.center'],['end','MainAxisAlignment.end'],['space between','MainAxisAlignment.spaceBetween'],['space around','MainAxisAlignment.spaceAround']] },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILDREN' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_container',
    message0: 'Container %1 width %2 height %3 color %4 radius %5 %6 child %7',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'WIDTH', value: 0, min: 0 },
      { type: 'field_number', name: 'HEIGHT', value: 0, min: 0 },
      { type: 'field_colour', name: 'COLOR', colour: '#3b82f6' },
      { type: 'field_number', name: 'BORDER_RADIUS', value: 0, min: 0 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_sized_box',
    message0: 'SizedBox  width %1 height %2',
    args0: [
      { type: 'field_number', name: 'WIDTH', value: 16, min: 0 },
      { type: 'field_number', name: 'HEIGHT', value: 16, min: 0 },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_padding',
    message0: 'Padding %1 all %2 %3 child %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'PADDING', value: 16, min: 0 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_center',
    message0: 'Center %1 child %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_expanded',
    message0: 'Expanded %1 flex %2 %3 child %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'FLEX', value: 1, min: 1, precision: 1 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_listview',
    message0: 'ListView %1 padding %2 %3 children %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'PADDING', value: 8, min: 0 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILDREN' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_gridview',
    message0: 'GridView %1 columns %2 spacing %3 %4 children %5',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'COLUMNS', value: 2, min: 1, precision: 1 },
      { type: 'field_number', name: 'SPACING', value: 8, min: 0, precision: 1 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILDREN' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_stack',
    message0: 'Stack %1 children %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILDREN' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },
  {
    type: 'flutter_positioned',
    message0: 'Positioned %1 top %2 bottom %3 left %4 right %5 %6 child %7',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'TOP', value: 0 },
      { type: 'field_number', name: 'BOTTOM', value: 0 },
      { type: 'field_number', name: 'LEFT', value: 0 },
      { type: 'field_number', name: 'RIGHT', value: 0 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#2563eb',
  },

  // ── Widget Blocks ────────────────────────────────────
  {
    type: 'flutter_text',
    message0: 'Text %1 %2 font size %3 weight %4 color %5',
    args0: [
      { type: 'field_input', name: 'TEXT_VALUE', text: 'Hello, SnapCode!' },
      { type: 'input_dummy' },
      { type: 'field_number', name: 'FONT_SIZE', value: 16, min: 8, max: 120, precision: 1 },
      { type: 'field_dropdown', name: 'FONT_WEIGHT', options: [['normal','FontWeight.normal'],['bold','FontWeight.bold'],['w300','FontWeight.w300'],['w500','FontWeight.w500']] },
      { type: 'field_colour', name: 'TEXT_COLOR', colour: '#ffffff' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_elevated_button',
    message0: 'ElevatedButton %1 text %2 %3 on pressed %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'BUTTON_TEXT', text: 'Click Me' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'ON_PRESSED' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_icon',
    message0: 'Icon %1 size %2 color %3',
    args0: [
      { type: 'field_dropdown', name: 'ICON_NAME', options: [['home','Icons.home'],['star','Icons.star'],['favorite','Icons.favorite'],['settings','Icons.settings'],['search','Icons.search'],['add','Icons.add']] },
      { type: 'field_number', name: 'ICON_SIZE', value: 24, min: 8, max: 200, precision: 1 },
      { type: 'field_colour', name: 'ICON_COLOR', colour: '#ffffff' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_image_network',
    message0: 'Image.network %1 URL %2 %3 width %4 height %5',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'IMAGE_URL', text: 'https://picsum.photos/200' },
      { type: 'input_dummy' },
      { type: 'field_number', name: 'WIDTH', value: 200, min: 0, precision: 1 },
      { type: 'field_number', name: 'HEIGHT', value: 200, min: 0, precision: 1 },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_image_asset',
    message0: 'Image.asset %1 path %2 %3 width %4 height %5',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'IMAGE_PATH', text: 'assets/image.png' },
      { type: 'input_dummy' },
      { type: 'field_number', name: 'WIDTH', value: 200, min: 0, precision: 1 },
      { type: 'field_number', name: 'HEIGHT', value: 200, min: 0, precision: 1 },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_circle_avatar',
    message0: 'CircleAvatar %1 radius %2 color %3 %4 child %5',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'RADIUS', value: 20, min: 0, precision: 1 },
      { type: 'field_colour', name: 'COLOR', colour: '#7c3aed' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_list_tile',
    message0: 'ListTile %1 leading %2 title %3 subtitle %4 trailing %5 on tap %6',
    args0: [
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'LEADING' },
      { type: 'input_statement', name: 'TITLE' },
      { type: 'input_statement', name: 'SUBTITLE' },
      { type: 'input_statement', name: 'TRAILING' },
      { type: 'input_statement', name: 'ON_TAP' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_card',
    message0: 'Card %1 elevation %2 %3 child %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_number', name: 'ELEVATION', value: 4, min: 0, max: 24, precision: 1 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'CHILD' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_divider',
    message0: 'Divider thickness %1 color %2',
    args0: [
      { type: 'field_number', name: 'THICKNESS', value: 1, min: 0, max: 20, precision: 0.5 },
      { type: 'field_colour', name: 'COLOR', colour: '#374151' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_snackbar',
    message0: 'Show SnackBar %1 text %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'TEXT', text: 'Action completed' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },
  {
    type: 'flutter_alert_dialog',
    message0: 'Show Dialog %1 title %2 content %3',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'TITLE', text: 'Alert' },
      { type: 'field_input', name: 'CONTENT', text: 'Are you sure?' },
    ],
    previousStatement: null, nextStatement: null, colour: '#059669',
  },

  // ── Input Blocks ─────────────────────────────────────
  {
    type: 'flutter_text_field',
    message0: 'TextField %1 hint %2 %3 label %4',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'HINT_TEXT', text: 'Enter text...' },
      { type: 'input_dummy' },
      { type: 'field_input', name: 'LABEL_TEXT', text: 'Label' },
    ],
    previousStatement: null, nextStatement: null, colour: '#d97706',
  },
  {
    type: 'flutter_checkbox',
    message0: 'Checkbox label %1 default %2',
    args0: [
      { type: 'field_input', name: 'LABEL', text: 'Accept terms' },
      { type: 'field_dropdown', name: 'DEFAULT_VALUE', options: [['unchecked','false'],['checked','true']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#d97706',
  },
  {
    type: 'flutter_switch',
    message0: 'Switch label %1 default %2',
    args0: [
      { type: 'field_input', name: 'LABEL', text: 'Dark mode' },
      { type: 'field_dropdown', name: 'DEFAULT_VALUE', options: [['off','false'],['on','true']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#d97706',
  },
  {
    type: 'flutter_slider',
    message0: 'Slider min %1 max %2 value %3',
    args0: [
      { type: 'field_number', name: 'MIN', value: 0 },
      { type: 'field_number', name: 'MAX', value: 100 },
      { type: 'field_number', name: 'VALUE', value: 50 },
    ],
    previousStatement: null, nextStatement: null, colour: '#d97706',
  },
  {
    type: 'flutter_dropdown',
    message0: 'Dropdown options %1',
    args0: [
      { type: 'field_input', name: 'OPTIONS', text: 'Option1,Option2' },
    ],
    previousStatement: null, nextStatement: null, colour: '#d97706',
  },

  // ── Logic Blocks ─────────────────────────────────────
  {
    type: 'logic_if_else',
    message0: 'if %1 %2 then %3 else %4',
    args0: [
      { type: 'field_input', name: 'CONDITION', text: 'true' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO_IF' },
      { type: 'input_statement', name: 'DO_ELSE' },
    ],
    previousStatement: null, nextStatement: null, colour: '#e11d48',
  },
  {
    type: 'logic_for_loop',
    message0: 'for i from %1 to %2 %3 do %4',
    args0: [
      { type: 'field_number', name: 'FROM', value: 0, precision: 1 },
      { type: 'field_number', name: 'TO', value: 10, precision: 1 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#e11d48',
  },
  {
    type: 'logic_while_loop',
    message0: 'while %1 %2 do %3',
    args0: [
      { type: 'field_input', name: 'CONDITION', text: 'true' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#e11d48',
  },
  {
    type: 'logic_print',
    message0: 'print %1',
    args0: [
      { type: 'field_input', name: 'MESSAGE', text: 'Hello World!' },
    ],
    previousStatement: null, nextStatement: null, colour: '#e11d48',
  },
  {
    type: 'logic_function',
    message0: 'function %1 %2 body %3',
    args0: [
      { type: 'field_input', name: 'FUNC_NAME', text: 'myFunction' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'BODY' },
    ],
    previousStatement: null, nextStatement: null, colour: '#e11d48',
  },

  // ── Variable Blocks ──────────────────────────────────
  {
    type: 'variable_set',
    message0: 'set %1 = %2',
    args0: [
      { type: 'field_input', name: 'VAR_NAME', text: 'myVar' },
      { type: 'field_input', name: 'VALUE', text: '0' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0891b2',
  },
  {
    type: 'variable_get',
    message0: 'get %1',
    args0: [
      { type: 'field_input', name: 'VAR_NAME', text: 'myVar' },
    ],
    output: null, colour: '#0891b2',
  },
  {
    type: 'math_number',
    message0: '%1',
    args0: [
      { type: 'field_number', name: 'NUM', value: 0 },
    ],
    output: 'Number', colour: '#0891b2',
  },
  {
    type: 'math_arithmetic',
    message0: '%1 %2 %3',
    args0: [
      { type: 'field_input', name: 'LEFT', text: '0' },
      { type: 'field_dropdown', name: 'OP', options: [['+','ADD'],['-','SUBTRACT'],['×','MULTIPLY'],['÷','DIVIDE'],['%','MODULO']] },
      { type: 'field_input', name: 'RIGHT', text: '0' },
    ],
    output: 'Number', colour: '#0891b2',
  },
  {
    type: 'text_literal',
    message0: '" %1 "',
    args0: [
      { type: 'field_input', name: 'TEXT', text: 'hello' },
    ],
    output: 'String', colour: '#0891b2',
  },
  {
    type: 'text_join',
    message0: 'join %1 + %2',
    args0: [
      { type: 'field_input', name: 'LEFT', text: 'Hello ' },
      { type: 'field_input', name: 'RIGHT', text: 'World' },
    ],
    output: 'String', colour: '#0891b2',
  },

  // ── Firebase Blocks ──────────────────────────────────
  {
    type: 'firebase_init',
    message0: 'Initialize Firebase',
    previousStatement: null, nextStatement: null, colour: '#f59e0b',
  },
  {
    type: 'firebase_auth_signin',
    message0: 'Firebase Sign In email %1 pass %2',
    args0: [
      { type: 'field_input', name: 'EMAIL', text: 'user@example.com' },
      { type: 'field_input', name: 'PASS', text: 'password123' },
    ],
    previousStatement: null, nextStatement: null, colour: '#f59e0b',
  },
  {
    type: 'firebase_firestore_get',
    message0: 'Firestore Get collection %1 doc %2',
    args0: [
      { type: 'field_input', name: 'COL', text: 'users' },
      { type: 'field_input', name: 'DOC', text: 'doc123' },
    ],
    output: null, colour: '#f59e0b',
  },
  {
    type: 'firebase_firestore_add',
    message0: 'Firestore Add to %1 data %2',
    args0: [
      { type: 'field_input', name: 'COL', text: 'users' },
      { type: 'field_input', name: 'DATA', text: '{"name": "John"}' },
    ],
    previousStatement: null, nextStatement: null, colour: '#f59e0b',
  },

  // ── File Access Blocks ───────────────────────────────
  {
    type: 'file_write',
    message0: 'Write to file %1 content %2',
    args0: [
      { type: 'field_input', name: 'FILE', text: 'data.txt' },
      { type: 'field_input', name: 'CONTENT', text: 'hello' },
    ],
    previousStatement: null, nextStatement: null, colour: '#4b5563',
  },
  {
    type: 'file_read',
    message0: 'Read file %1',
    args0: [
      { type: 'field_input', name: 'FILE', text: 'data.txt' },
    ],
    output: 'String', colour: '#4b5563',
  },

  // ── Raw Dart (ASD - Add Source Directly) Blocks ─────────
  {
    type: 'dart_raw_statement',
    message0: 'Raw Dart Statement %1 %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'field_input', name: 'CODE', text: 'print("Raw Dart executed!");' },
    ],
    previousStatement: null, nextStatement: null, colour: '#475569',
    tooltip: 'Directly executes arbitrary Dart code statements (Add Source Directly)',
  },
  {
    type: 'dart_raw_expression',
    message0: 'Raw Dart %1',
    args0: [
      { type: 'field_input', name: 'EXPR', text: 'DateTime.now().year.toString()' },
    ],
    output: null, colour: '#475569',
    tooltip: 'Injects an arbitrary raw Dart expression value inline',
  },
  {
    type: 'dart_raw_import',
    message0: 'Import Package %1',
    args0: [
      { type: 'field_input', name: 'IMPORT', text: "import 'package:http/http.dart';" },
    ],
    previousStatement: null, nextStatement: null, colour: '#475569',
    tooltip: 'Adds a top-level package or library import directive to the Dart file',
  },
  {
    type: 'dart_raw_comment',
    message0: '// Comment: %1',
    args0: [
      { type: 'field_input', name: 'COMMENT', text: 'Custom logic here' },
    ],
    previousStatement: null, nextStatement: null, colour: '#64748b',
    tooltip: 'Generates a single-line Dart comment',
  },

  // ── State & Lifecycle Blocks ─────────────────────────
  {
    type: 'lifecycle_init_state',
    message0: 'on Screen Start (initState) %1 %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#8b5cf6',
    tooltip: 'Code executed once when the screen is first loaded (equivalent to onCreate)',
    hat: 'cap',
  },
  {
    type: 'lifecycle_dispose',
    message0: 'on Screen Closed (dispose) %1 %2',
    args0: [
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#8b5cf6',
    tooltip: 'Cleanup code executed when the screen is destroyed',
    hat: 'cap',
  },
  {
    type: 'state_declare_variable',
    message0: 'State Variable %1 %2 = %3',
    args0: [
      { type: 'field_dropdown', name: 'TYPE', options: [['String', 'String'], ['int', 'int'], ['double', 'double'], ['bool', 'bool'], ['List', 'List<dynamic>'], ['Map', 'Map<String, dynamic>']] },
      { type: 'field_input', name: 'NAME', text: 'myStateVar' },
      { type: 'field_input', name: 'INIT_VAL', text: "''" },
    ],
    colour: '#8b5cf6',
    tooltip: 'Declares a persistent reactive state variable on this screen',
    hat: 'cap',
  },
  {
    type: 'state_set_variable',
    message0: 'Update State %1 = %2',
    args0: [
      { type: 'field_input', name: 'NAME', text: 'myStateVar' },
      { type: 'field_input', name: 'VALUE', text: "'new value'" },
    ],
    previousStatement: null, nextStatement: null, colour: '#8b5cf6',
    tooltip: 'Updates a state variable and calls setState() to re-render UI',
  },
  {
    type: 'state_get_variable',
    message0: 'state.%1',
    args0: [
      { type: 'field_input', name: 'NAME', text: 'myStateVar' },
    ],
    output: null, colour: '#8b5cf6',
    tooltip: 'Gets the current value of a state variable',
  },

  // ── Component Blocks ─────────────────────────────────
  {
    type: 'nav_push_named',
    message0: 'Navigate to Route %1',
    args0: [
      { type: 'field_input', name: 'ROUTE', text: '/profile' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Navigates to another page by named route',
  },
  {
    type: 'nav_pop',
    message0: 'Navigate Back (Go Back)',
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Pops the current screen from the navigation stack',
  },
  {
    type: 'pref_set_string',
    message0: 'Storage: Save String key %1 = value %2',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'username' },
      { type: 'field_input', name: 'VAL', text: 'admin' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Persists a string value to SharedPreferences',
  },
  {
    type: 'pref_get_string',
    message0: 'Storage: Read String key %1',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'username' },
    ],
    output: 'String', colour: '#0284c7',
    tooltip: 'Reads a persistent string value from SharedPreferences',
  },
  {
    type: 'pref_set_number',
    message0: 'Storage: Save Number key %1 = value %2',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'score' },
      { type: 'field_input', name: 'VAL', text: '100' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Persists a number to SharedPreferences',
  },
  {
    type: 'pref_get_number',
    message0: 'Storage: Read Number key %1',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'score' },
    ],
    output: 'Number', colour: '#0284c7',
    tooltip: 'Reads a persistent number from SharedPreferences',
  },
  {
    type: 'pref_set_bool',
    message0: 'Storage: Save Boolean key %1 = value %2',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'isLoggedIn' },
      { type: 'field_dropdown', name: 'VAL', options: [['true','true'],['false','false']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Persists a boolean flag to SharedPreferences',
  },
  {
    type: 'pref_get_bool',
    message0: 'Storage: Read Boolean key %1',
    args0: [
      { type: 'field_input', name: 'KEY', text: 'isLoggedIn' },
    ],
    output: 'Boolean', colour: '#0284c7',
    tooltip: 'Reads a persistent boolean flag from SharedPreferences',
  },
  {
    type: 'timer_delayed',
    message0: 'Wait %1 ms then %2 %3',
    args0: [
      { type: 'field_number', name: 'MS', value: 1000, min: 0 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Executes the inner blocks after a specified delay in milliseconds',
  },
  {
    type: 'timer_periodic',
    message0: 'Every %1 ms do %2 %3',
    args0: [
      { type: 'field_number', name: 'MS', value: 1000, min: 100 },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Repeatedly executes the inner blocks at a regular millisecond interval',
  },
  {
    type: 'http_get_request',
    message0: 'HTTP GET from URL %1 on response %2 %3',
    args0: [
      { type: 'field_input', name: 'URL', text: 'https://api.example.com/data' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Performs a non-blocking HTTP GET request',
  },
  {
    type: 'http_post_request',
    message0: 'HTTP POST to URL %1 body %2 on response %3 %4',
    args0: [
      { type: 'field_input', name: 'URL', text: 'https://api.example.com/data' },
      { type: 'field_input', name: 'BODY', text: '{"key": "value"}' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Performs a non-blocking HTTP POST request with JSON payload',
  },
  {
    type: 'ui_show_snackbar',
    message0: 'Show SnackBar %1',
    args0: [
      { type: 'field_input', name: 'MESSAGE', text: 'Action successful!' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Displays a popup SnackBar message at the bottom of the screen',
  },
  {
    type: 'ui_show_dialog',
    message0: 'Show Alert Dialog title %1 message %2',
    args0: [
      { type: 'field_input', name: 'TITLE', text: 'Notification' },
      { type: 'field_input', name: 'MESSAGE', text: 'Operation completed successfully.' },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Displays a modal alert dialog with an OK button',
  },
  {
    type: 'ui_haptic_feedback',
    message0: 'Trigger Haptic Vibration %1',
    args0: [
      { type: 'field_dropdown', name: 'TYPE', options: [['Light Impact', 'lightImpact'], ['Medium Impact', 'mediumImpact'], ['Heavy Impact', 'heavyImpact'], ['Selection Click', 'selectionClick']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#0284c7',
    tooltip: 'Triggers device haptic feedback vibration',
  },

  // ── Widget Events Blocks (Bridge: UI Designer ↔ Events) ────
  {
    type: 'widget_on_click',
    message0: 'when %1 clicked %2 do %3',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a UI widget from the View Designer is tapped/clicked',
    hat: 'cap',
  },
  {
    type: 'widget_on_long_press',
    message0: 'when %1 long pressed %2 do %3',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a UI widget is long pressed',
    hat: 'cap',
  },
  {
    type: 'widget_on_text_changed',
    message0: 'when %1 text changed %2 value = %3 %4 do %5',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'field_variable', name: 'VALUE_VAR', variable: 'newText' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a TextField value changes. The new text is available as a variable.',
    hat: 'cap',
  },
  {
    type: 'widget_on_checkbox_changed',
    message0: 'when %1 checkbox changed %2 checked = %3 %4 do %5',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'field_variable', name: 'VALUE_VAR', variable: 'isChecked' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a Checkbox value changes',
    hat: 'cap',
  },
  {
    type: 'widget_on_switch_changed',
    message0: 'when %1 switch toggled %2 value = %3 %4 do %5',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'field_variable', name: 'VALUE_VAR', variable: 'isOn' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a Switch is toggled on/off',
    hat: 'cap',
  },
  {
    type: 'widget_on_slider_changed',
    message0: 'when %1 slider changed %2 value = %3 %4 do %5',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'input_dummy' },
      { type: 'field_variable', name: 'VALUE_VAR', variable: 'sliderVal' },
      { type: 'input_dummy' },
      { type: 'input_statement', name: 'DO' },
    ],
    colour: '#ec4899',
    tooltip: 'Runs code when a Slider value changes',
    hat: 'cap',
  },
  {
    type: 'widget_set_text',
    message0: 'set %1 text to %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_input', name: 'TEXT_VALUE', text: 'Hello!' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
    tooltip: 'Sets the displayed text of a widget at runtime',
  },
  {
    type: 'widget_get_text',
    message0: 'text of %1',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
    ],
    output: 'String', colour: '#ec4899',
    tooltip: 'Gets the current text or value from a widget',
  },
  {
    type: 'widget_set_visible',
    message0: 'set %1 visible %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_dropdown', name: 'VISIBLE', options: [['visible','true'],['hidden','false']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
    tooltip: 'Shows or hides a widget at runtime using Visibility widget',
  },
  {
    type: 'widget_set_enabled',
    message0: 'set %1 enabled %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_dropdown', name: 'ENABLED', options: [['enabled','true'],['disabled','false']] },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
    tooltip: 'Enables or disables user interaction on a widget',
  },
  {
    type: 'widget_set_bg_color',
    message0: 'set %1 background color %2',
    args0: [
      { type: 'field_dropdown', name: 'WIDGET_ID', options: [['(select widget)', 'none']] },
      { type: 'field_colour', name: 'COLOR', colour: '#2196F3' },
    ],
    previousStatement: null, nextStatement: null, colour: '#ec4899',
    tooltip: 'Changes the background color of a widget at runtime',
  }
]);

// ── Global Widget ID Registry (populated from C# via bridge) ─────
var _widgetRegistry = [['(no widgets in View Designer)', 'none']];

function _getWidgetDropdownOptions() {
  return _widgetRegistry.length > 0 ? _widgetRegistry : [['(no widgets in View Designer)', 'none']];
}

// Dynamic dropdown extension for widget ID selectors
Blockly.Extensions.register('dynamic_widget_dropdown', function() {
  const dropdown = this.getField('WIDGET_ID');
  if (dropdown) {
    dropdown.menuGenerator_ = _getWidgetDropdownOptions;
  }
});

// Apply the dynamic dropdown to all widget event block types
const widgetEventBlockTypes = [
  'widget_on_click', 'widget_on_long_press', 'widget_on_text_changed',
  'widget_on_checkbox_changed', 'widget_on_switch_changed', 'widget_on_slider_changed',
  'widget_set_text', 'widget_get_text', 'widget_set_visible', 'widget_set_enabled', 'widget_set_bg_color', 'widget_set_text_color', 'widget_set_font_size', 'widget_set_width', 'widget_set_height'
];

// Post-hoc: patch the menu generator on every WIDGET_ID dropdown
// This runs after blocks are created on workspace
function patchWidgetDropdowns() {
  if (!workspace) return;
  const allBlocks = workspace.getAllBlocks(false);
  allBlocks.forEach(block => {
    if (widgetEventBlockTypes.includes(block.type)) {
      const field = block.getField('WIDGET_ID');
      if (field && field.menuGenerator_) {
        field.menuGenerator_ = _getWidgetDropdownOptions;
      }
    }
  });
}

// Called from C# to update the widget ID list
window.updateWidgetIds = function(widgetListJson) {
  try {
    const widgets = JSON.parse(widgetListJson);
    if (Array.isArray(widgets) && widgets.length > 0) {
      _widgetRegistry = widgets.map(w => [w.id + ' (' + w.type + ')', w.id]);
    } else {
      _widgetRegistry = [['(no widgets in View Designer)', 'none']];
    }
    patchWidgetDropdowns();
  } catch(e) {
    console.error('Failed to update widget IDs:', e);
  }
};

// ============================================================
// SnapCode — Dart Code Generator
// ============================================================
const dartGenerator = new Blockly.CodeGenerator('Dart');

dartGenerator.ORDER_ATOMIC = 0;
dartGenerator.ORDER_ADDITIVE = 4;
dartGenerator.ORDER_NONE = 99;
dartGenerator.INDENT = '  ';

dartGenerator.scrub_ = function(block, code, opt_thisOnly) {
  const nextBlock = block.nextConnection && block.nextConnection.targetBlock();
  if (nextBlock && !opt_thisOnly) {
    return code + this.blockToCode(nextBlock);
  }
  return code;
};

dartGenerator.init = function(ws) {
  dartGenerator._workspace = ws;
};

dartGenerator.finish = function(code) {
  const ws = dartGenerator._workspace || workspace;
  if (!ws) return code;

  const scaffoldBlock = ws.getAllBlocks(false).find(b => b.type === 'flutter_app_scaffold');
  if (!scaffoldBlock) {
    return code;
  }

  // Collect all raw imports
  const importBlocks = ws.getAllBlocks(false).filter(b => b.type === 'dart_raw_import');
  const customImports = importBlocks.map(b => b.getFieldValue('IMPORT').trim()).filter(Boolean);

  // Collect declared state variables
  const stateVarBlocks = ws.getAllBlocks(false).filter(b => b.type === 'state_declare_variable');
  const stateVars = stateVarBlocks.map(b => {
    const type = b.getFieldValue('TYPE') || 'String';
    const name = b.getFieldValue('NAME') || 'myStateVar';
    const init = b.getFieldValue('INIT_VAL') || "''";
    return `${type} ${name} = ${init};`;
  });

  // Collect initState statements
  const initBlocks = ws.getAllBlocks(false).filter(b => b.type === 'lifecycle_init_state');
  let initStatements = [];
  initBlocks.forEach(b => {
    const c = dartGenerator.statementToCode(b, 'DO');
    if (c.trim()) initStatements.push(c.trim());
  });

  // Collect dispose statements
  const disposeBlocks = ws.getAllBlocks(false).filter(b => b.type === 'lifecycle_dispose');
  let disposeStatements = [];
  disposeBlocks.forEach(b => {
    const c = dartGenerator.statementToCode(b, 'DO');
    if (c.trim()) disposeStatements.push(c.trim());
  });

  // Collect custom helper functions (logic_function)
  const funcBlocks = ws.getAllBlocks(false).filter(b => b.type === 'logic_function');
  let funcCodes = [];
  funcBlocks.forEach(b => {
    const fnName = b.getFieldValue('FUNC_NAME') || 'myFunction';
    const fBody = dartGenerator.statementToCode(b, 'BODY') || '  // function body';
    funcCodes.push(`void ${fnName}() {\n${indentCode(fBody)}\n}`);
  });

  // Collect all block types to auto-import Flutter packages
  const allBlockTypes = ws.getAllBlocks(false).map(b => b.type);
  let imports = new Set([
    "import 'package:flutter/material.dart';",
  ]);

  if (allBlockTypes.some(t => t.startsWith('pref_'))) {
    imports.add("import 'package:shared_preferences/shared_preferences.dart';");
  }
  if (allBlockTypes.some(t => t.startsWith('timer_'))) {
    imports.add("import 'dart:async';");
  }
  if (allBlockTypes.some(t => t.startsWith('http_'))) {
    imports.add("import 'package:http/http.dart' as http;");
    imports.add("import 'dart:convert';");
  }
  if (allBlockTypes.some(t => t.startsWith('ui_haptic_'))) {
    imports.add("import 'package:flutter/services.dart';");
  }
  if (allBlockTypes.some(t => t.startsWith('file_'))) {
    imports.add("import 'dart:io';");
    imports.add("import 'package:path_provider/path_provider.dart';");
  }
  if (allBlockTypes.some(t => t.startsWith('firebase_'))) {
    imports.add("import 'package:firebase_core/firebase_core.dart';");
    imports.add("import 'package:firebase_auth/firebase_auth.dart';");
    imports.add("import 'package:cloud_firestore/cloud_firestore.dart';");
  }

  // Add custom imports
  customImports.forEach(imp => {
    let trimmed = imp.trim();
    if (!trimmed.endsWith(';')) trimmed += ';';
    imports.add(trimmed);
  });

  const title = scaffoldBlock.getFieldValue('APP_TITLE') || 'My App';
  const colorHex = scaffoldBlock.getFieldValue('THEME_COLOR') || '#6C63FF';
  const dartColor = '0xFF' + colorHex.replace('#', '').toUpperCase();
  const body = dartGenerator.statementToCode(scaffoldBlock, 'BODY') || 'const Center(child: Text(\'Hello SnapCode!\'))';
  const bodyWidget = body.trim() || 'const Center(child: Text(\'Hello SnapCode!\'))';

  const importsStr = Array.from(imports).join('\n');
  const stateVarBlock = stateVars.length > 0 ? '\n  // State Variables\n  ' + stateVars.join('\n  ') + '\n' : '';

  let initStateMethod = '';
  if (initStatements.length > 0) {
    initStateMethod = `\n  @override\n  void initState() {\n    super.initState();\n${indentCode('    ' + initStatements.join('\n'))}\n  }\n`;
  }

  let disposeMethod = '';
  if (disposeStatements.length > 0) {
    disposeMethod = `\n  @override\n  void dispose() {\n${indentCode('    ' + disposeStatements.join('\n'))}\n    super.dispose();\n  }\n`;
  }

  const helperFunctionsBlock = funcCodes.length > 0 ? '\n  // Custom Functions\n' + funcCodes.map(f => indentCode(f)).join('\n\n') + '\n' : '';

  return `${importsStr}

void main() {
  WidgetsFlutterBinding.ensureInitialized();
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
        colorSchemeSeed: const Color(${dartColor}),
        useMaterial3: true,
        brightness: Brightness.light,
      ),
      home: const RootPage(),
    );
  }
}

class RootPage extends StatefulWidget {
  const RootPage({super.key});

  @override
  State<RootPage> createState() => _RootPageState();
}

class _RootPageState extends State<RootPage> {${stateVarBlock}${initStateMethod}${disposeMethod}${helperFunctionsBlock}
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('${esc(title)}'),
        centerTitle: true,
      ),
      body: SafeArea(
        child: ${bodyWidget},
      ),
    );
  }
}
`;
};

// ── Utility functions ──────────────────────────────────
function esc(str) { return (str || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'"); }
function hexToColor(hex) { return '0xFF' + (hex || '#000000').replace('#', '').toUpperCase(); }

function indentCode(code) {
  return code.split('\n').filter(l => l.trim()).map(l => '  ' + l.trimStart()).join('\n');
}

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

// ── App Generators ─────────────────────────────────────

  dartGenerator.forBlock['flutter_set_widget_property'] = function(block) {
    const prop = block.getFieldValue('PROPERTY');
    const widgetId = block.getFieldValue('WIDGET_ID');
    const value = dartGenerator.valueToCode(block, 'VALUE', dartGenerator.ORDER_NONE) || '\'\'';
    return 'setState(() {\n  // ' + widgetId + '.' + prop + ' = ' + value + '\n});\n';
  };
  dartGenerator.forBlock['flutter_app_scaffold'] = function(block, gen) {
  return '';
};

dartGenerator.forBlock['flutter_app_bar'] = function(block) {
  const title = block.getFieldValue('TITLE') || 'App Bar';
  const centerTitle = block.getFieldValue('CENTER_TITLE') || 'true';
  return `AppBar(\n  title: const Text('${title}'),\n  centerTitle: ${centerTitle},\n),\n`;
};

// ── Layout Generators ──────────────────────────────────
dartGenerator.forBlock['flutter_column'] = function(block) {
  const align = block.getFieldValue('ALIGNMENT');
  const children = generateChildrenList(block, 'CHILDREN');
  return `Column(\n  mainAxisAlignment: ${align},\n  children: [\n    ${children}\n  ],\n)`;
};

dartGenerator.forBlock['flutter_row'] = function(block) {
  const align = block.getFieldValue('ALIGNMENT');
  const children = generateChildrenList(block, 'CHILDREN');
  return `Row(\n  mainAxisAlignment: ${align},\n  children: [\n    ${children}\n  ],\n)`;
};

dartGenerator.forBlock['flutter_container'] = function(block) {
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

dartGenerator.forBlock['flutter_sized_box'] = function(block) {
  const w = block.getFieldValue('WIDTH') || 0;
  const h = block.getFieldValue('HEIGHT') || 0;
  let props = [];
  if (w > 0) props.push(`width: ${w}`);
  if (h > 0) props.push(`height: ${h}`);
  return `const SizedBox(${props.join(', ')})`;
};

dartGenerator.forBlock['flutter_padding'] = function(block) {
  const p = block.getFieldValue('PADDING') || 0;
  const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
  return `Padding(\n  padding: const EdgeInsets.all(${p}),\n  child: ${child.trim()},\n)`;
};

dartGenerator.forBlock['flutter_center'] = function(block) {
  const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
  return `Center(child: ${child.trim()})`;
};

dartGenerator.forBlock['flutter_expanded'] = function(block) {
  const flex = block.getFieldValue('FLEX') || 1;
  const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
  return `Expanded(\n  flex: ${flex},\n  child: ${child.trim()},\n)`;
};

dartGenerator.forBlock['flutter_listview'] = function(block) {
  const padding = block.getFieldValue('PADDING') || 0;
  const children = generateChildrenList(block, 'CHILDREN');
  return `ListView(\n  padding: const EdgeInsets.all(${padding}),\n  children: [\n    ${children}\n  ],\n)`;
};

dartGenerator.forBlock['flutter_gridview'] = function(block) {
  const columns = block.getFieldValue('COLUMNS') || 2;
  const spacing = block.getFieldValue('SPACING') || 8;
  const children = generateChildrenList(block, 'CHILDREN');
  return `GridView.count(\n  crossAxisCount: ${columns},\n  mainAxisSpacing: ${spacing},\n  crossAxisSpacing: ${spacing},\n  children: [\n    ${children}\n  ],\n)`;
};

dartGenerator.forBlock['flutter_stack'] = function(block) {
  const children = generateChildrenList(block, 'CHILDREN');
  return `Stack(\n  children: [\n    ${children}\n  ],\n)`;
};

dartGenerator.forBlock['flutter_positioned'] = function(block) {
  const top = block.getFieldValue('TOP') || 0;
  const bottom = block.getFieldValue('BOTTOM') || 0;
  const left = block.getFieldValue('LEFT') || 0;
  const right = block.getFieldValue('RIGHT') || 0;
  const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
  return `Positioned(\n  top: ${top},\n  bottom: ${bottom},\n  left: ${left},\n  right: ${right},\n  child: ${child.trim()},\n)`;
};

// ── Widget Generators ──────────────────────────────────
dartGenerator.forBlock['flutter_text'] = function(block) {
  const text = block.getFieldValue('TEXT_VALUE') || '';
  const size = block.getFieldValue('FONT_SIZE') || 16;
  const weight = block.getFieldValue('FONT_WEIGHT') || 'FontWeight.normal';
  const color = hexToColor(block.getFieldValue('TEXT_COLOR'));
  return `Text(\n  '${esc(text)}',\n  style: const TextStyle(fontSize: ${size}, fontWeight: ${weight}, color: Color(${color})),\n)`;
};

dartGenerator.forBlock['flutter_elevated_button'] = function(block, gen) {
  const text = block.getFieldValue('BUTTON_TEXT') || 'Button';
  const onPressed = gen.statementToCode(block, 'ON_PRESSED') || "print('pressed');\n";
  return `ElevatedButton(\n  onPressed: () {\n${onPressed}  },\n  child: const Text('${esc(text)}'),\n)`;
};

dartGenerator.forBlock['flutter_icon'] = function(block) {
  const icon = block.getFieldValue('ICON_NAME') || 'Icons.star';
  const size = block.getFieldValue('ICON_SIZE') || 24;
  const color = hexToColor(block.getFieldValue('ICON_COLOR'));
  return `Icon(${icon}, size: ${size}, color: const Color(${color}))`;
};

dartGenerator.forBlock['flutter_image_network'] = function(block) {
  const url = block.getFieldValue('IMAGE_URL') || '';
  const w = block.getFieldValue('WIDTH') || 0;
  const h = block.getFieldValue('HEIGHT') || 0;
  let props = [];
  if (w > 0) props.push(`width: ${w}`);
  if (h > 0) props.push(`height: ${h}`);
  props.push(`fit: BoxFit.cover`);
  return `Image.network(\n  '${esc(url)}',\n  ${props.join(',\n  ')}\n)`;
};

dartGenerator.forBlock['flutter_image_asset'] = function(block) {
  const path = block.getFieldValue('IMAGE_PATH') || 'assets/image.png';
  const w = block.getFieldValue('WIDTH') || 0;
  const h = block.getFieldValue('HEIGHT') || 0;
  let props = [];
  if (w > 0) props.push(`width: ${w}`);
  if (h > 0) props.push(`height: ${h}`);
  props.push(`fit: BoxFit.cover`);
  return `Image.asset(\n  '${esc(path)}',\n  ${props.join(',\n  ')}\n)`;
};

dartGenerator.forBlock['flutter_circle_avatar'] = function(block) {
  const radius = block.getFieldValue('RADIUS') || 20;
  const color = hexToColor(block.getFieldValue('COLOR'));
  const child = dartGenerator.statementToCode(block, 'CHILD');
  let props = [`radius: ${radius}`, `backgroundColor: const Color(${color})`];
  if (child.trim()) props.push(`child: ${child.trim().replace(/,\s*$/, '')}`);
  return `CircleAvatar(\n  ${props.join(',\n  ')}\n)`;
};

dartGenerator.forBlock['flutter_list_tile'] = function(block) {
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

dartGenerator.forBlock['flutter_card'] = function(block) {
  const elevation = block.getFieldValue('ELEVATION') || 4;
  const child = dartGenerator.statementToCode(block, 'CHILD') || 'const SizedBox.shrink()\n';
  return `Card(\n  elevation: ${elevation},\n  child: Padding(\n    padding: const EdgeInsets.all(16),\n    child: ${child.trim()},\n  ),\n)`;
};

dartGenerator.forBlock['flutter_divider'] = function(block) {
  const thickness = block.getFieldValue('THICKNESS') || 1;
  const color = hexToColor(block.getFieldValue('COLOR'));
  return `Divider(thickness: ${thickness}, color: const Color(${color}))`;
};

dartGenerator.forBlock['flutter_snackbar'] = function(block) {
  const text = block.getFieldValue('TEXT') || '';
  return `ScaffoldMessenger.of(context).showSnackBar(\n  SnackBar(content: Text('${esc(text)}'))\n);\n`;
};

dartGenerator.forBlock['flutter_alert_dialog'] = function(block) {
  const title = block.getFieldValue('TITLE') || '';
  const content = block.getFieldValue('CONTENT') || '';
  return `showDialog(context: context, builder: (context) => AlertDialog(title: Text('${esc(title)}'), content: Text('${esc(content)}'), actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('OK'))]));\n`;
};

// ── Input Generators ───────────────────────────────────
dartGenerator.forBlock['flutter_text_field'] = function(block) {
  const hint = block.getFieldValue('HINT_TEXT') || '';
  const label = block.getFieldValue('LABEL_TEXT') || '';
  return `TextField(\n  decoration: const InputDecoration(\n    hintText: '${esc(hint)}',\n    labelText: '${esc(label)}',\n    border: OutlineInputBorder(),\n  ),\n),\n`;
};

dartGenerator.forBlock['flutter_checkbox'] = function(block) {
  const label = block.getFieldValue('LABEL') || 'Option';
  const val = block.getFieldValue('DEFAULT_VALUE') || 'false';
  return `Row(\n  mainAxisSize: MainAxisSize.min,\n  children: [\n    Checkbox(value: ${val}, onChanged: (val) {}),\n    const Text('${esc(label)}'),\n  ],\n),\n`;
};

dartGenerator.forBlock['flutter_switch'] = function(block) {
  const label = block.getFieldValue('LABEL') || 'Toggle';
  const val = block.getFieldValue('DEFAULT_VALUE') || 'false';
  return `Row(\n  mainAxisSize: MainAxisSize.min,\n  children: [\n    const Text('${esc(label)}'),\n    Switch(value: ${val}, onChanged: (val) {}),\n  ],\n),\n`;
};

dartGenerator.forBlock['flutter_slider'] = function(block) {
  const min = block.getFieldValue('MIN') || 0;
  const max = block.getFieldValue('MAX') || 100;
  const val = block.getFieldValue('VALUE') || 50;
  return `Slider(min: ${min}, max: ${max}, value: ${val}, onChanged: (val) {}),\n`;
};

dartGenerator.forBlock['flutter_dropdown'] = function(block) {
  const optsStr = block.getFieldValue('OPTIONS') || 'Option1,Option2';
  const opts = optsStr.split(',').map(o => o.trim());
  const items = opts.map(o => `DropdownMenuItem(value: '${esc(o)}', child: Text('${esc(o)}'))`).join(', ');
  return `DropdownButton<String>(\n  value: '${esc(opts[0])}',\n  items: [${items}],\n  onChanged: (val) {},\n),\n`;
};

// ── Logic Generators ───────────────────────────────────
dartGenerator.forBlock['logic_if_else'] = function(block, gen) {
  const condition = block.getFieldValue('CONDITION') || 'true';
  const doIf = gen.statementToCode(block, 'DO_IF') || '  // then\n';
  const doElse = gen.statementToCode(block, 'DO_ELSE') || '  // else\n';
  return `if (${condition}) {\n${indentCode(doIf)}\n} else {\n${indentCode(doElse)}\n}\n`;
};

dartGenerator.forBlock['logic_for_loop'] = function(block, gen) {
  const from = block.getFieldValue('FROM') ?? 0;
  const to = block.getFieldValue('TO') ?? 10;
  const body = gen.statementToCode(block, 'DO') || '  // loop body\n';
  return `for (int i = ${from}; i < ${to}; i++) {\n${indentCode(body)}\n}\n`;
};

dartGenerator.forBlock['logic_while_loop'] = function(block, gen) {
  const condition = block.getFieldValue('CONDITION') || 'true';
  const body = gen.statementToCode(block, 'DO') || '  // loop body\n';
  return `while (${condition}) {\n${indentCode(body)}\n}\n`;
};

dartGenerator.forBlock['logic_print'] = function(block) {
  const message = block.getFieldValue('MESSAGE') || '';
  return `print('${esc(message)}');\n`;
};

dartGenerator.forBlock['logic_function'] = function(block, gen) {
  const funcName = block.getFieldValue('FUNC_NAME') || 'myFunction';
  const body = gen.statementToCode(block, 'BODY') || '  // function body\n';
  return `void ${funcName}() {\n${indentCode(body)}\n}\n`;
};

// ── Variable Generators ────────────────────────────────
dartGenerator.forBlock['variable_set'] = function(block) {
  const varName = block.getFieldValue('VAR_NAME') || 'myVar';
  const value = block.getFieldValue('VALUE') || '0';
  return `var ${varName} = ${value};\n`;
};

dartGenerator.forBlock['variable_get'] = function(block) {
  const varName = block.getFieldValue('VAR_NAME') || 'myVar';
  return [varName, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['math_arithmetic'] = function(block) {
  const left = block.getFieldValue('LEFT') || '0';
  const right = block.getFieldValue('RIGHT') || '0';
  const op = block.getFieldValue('OP') || 'ADD';
  const opMap = { ADD: '+', SUBTRACT: '-', MULTIPLY: '*', DIVIDE: '/', MODULO: '%' };
  return [`${left} ${opMap[op] || '+'} ${right}`, dartGenerator.ORDER_ADDITIVE];
};

dartGenerator.forBlock['math_number'] = function(block) {
  const num = block.getFieldValue('NUM') || 0;
  return [String(num), dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['text_literal'] = function(block) {
  const text = block.getFieldValue('TEXT') || '';
  return [`'${esc(text)}'`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['text_join'] = function(block) {
  const left = block.getFieldValue('LEFT') || '';
  const right = block.getFieldValue('RIGHT') || '';
  return [`'${esc(left)}${esc(right)}'`, dartGenerator.ORDER_ATOMIC];
};

// ── Firebase Generators ────────────────────────────────
dartGenerator.forBlock['firebase_init'] = function(block) {
  return `await Firebase.initializeApp();\n`;
};

dartGenerator.forBlock['firebase_auth_signin'] = function(block) {
  const email = block.getFieldValue('EMAIL') || '';
  const pass = block.getFieldValue('PASS') || '';
  return `await FirebaseAuth.instance.signInWithEmailAndPassword(email: '${esc(email)}', password: '${esc(pass)}');\n`;
};

dartGenerator.forBlock['firebase_firestore_get'] = function(block) {
  const col = block.getFieldValue('COL') || 'users';
  const doc = block.getFieldValue('DOC') || 'doc123';
  return [`await FirebaseFirestore.instance.collection('${esc(col)}').doc('${esc(doc)}').get()`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['firebase_firestore_add'] = function(block) {
  const col = block.getFieldValue('COL') || 'users';
  const data = block.getFieldValue('DATA') || '{}';
  return `await FirebaseFirestore.instance.collection('${esc(col)}').add(${data});\n`;
};

// ── File Access Generators ─────────────────────────────
dartGenerator.forBlock['file_write'] = function(block) {
  const file = block.getFieldValue('FILE') || 'data.txt';
  const content = block.getFieldValue('CONTENT') || '';
  return `final dir = await getApplicationDocumentsDirectory();\nfinal file = File('\${dir.path}/${esc(file)}');\nawait file.writeAsString('${esc(content)}');\n`;
};

dartGenerator.forBlock['file_read'] = function(block) {
  const file = block.getFieldValue('FILE') || 'data.txt';
  return [`await File('\${(await getApplicationDocumentsDirectory()).path}/${esc(file)}').readAsString()`, dartGenerator.ORDER_ATOMIC];
};

// ── Raw Dart (ASD) Generators ───────────────────────────
dartGenerator.forBlock['dart_raw_statement'] = function(block) {
  const code = block.getFieldValue('CODE') || '';
  return `${code.trim()}\n`;
};

dartGenerator.forBlock['dart_raw_expression'] = function(block) {
  const expr = block.getFieldValue('EXPR') || "''";
  return [expr.trim(), dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['dart_raw_import'] = function(block) {
  return '';
};

dartGenerator.forBlock['dart_raw_comment'] = function(block) {
  const comment = block.getFieldValue('COMMENT') || '';
  return `// ${comment}\n`;
};

// ── State & Lifecycle Generators ────────────────────────
dartGenerator.forBlock['lifecycle_init_state'] = function(block) {
  return '';
};

dartGenerator.forBlock['lifecycle_dispose'] = function(block) {
  return '';
};

dartGenerator.forBlock['state_declare_variable'] = function(block) {
  return '';
};

dartGenerator.forBlock['state_set_variable'] = function(block) {
  const name = block.getFieldValue('NAME') || 'myStateVar';
  const val = block.getFieldValue('VALUE') || "''";
  return `setState(() {\n  ${name} = ${val};\n});\n`;
};

dartGenerator.forBlock['state_get_variable'] = function(block) {
  const name = block.getFieldValue('NAME') || 'myStateVar';
  return [name, dartGenerator.ORDER_ATOMIC];
};

// ── Component Generators ────────────────────────────────
dartGenerator.forBlock['nav_push_named'] = function(block) {
  const route = block.getFieldValue('ROUTE') || '/';
  return `Navigator.of(context).pushNamed('${esc(route)}');\n`;
};

dartGenerator.forBlock['nav_pop'] = function(block) {
  return `Navigator.of(context).pop();\n`;
};

dartGenerator.forBlock['pref_set_string'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  const val = block.getFieldValue('VAL') || '';
  return `final prefs = await SharedPreferences.getInstance();\nawait prefs.setString('${esc(key)}', '${esc(val)}');\n`;
};

dartGenerator.forBlock['pref_get_string'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  return [`(await SharedPreferences.getInstance()).getString('${esc(key)}') ?? ''`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['pref_set_number'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  const val = block.getFieldValue('VAL') || '0';
  return `final prefs = await SharedPreferences.getInstance();\nawait prefs.setDouble('${esc(key)}', ${val});\n`;
};

dartGenerator.forBlock['pref_get_number'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  return [`(await SharedPreferences.getInstance()).getDouble('${esc(key)}') ?? 0.0`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['pref_set_bool'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  const val = block.getFieldValue('VAL') || 'true';
  return `final prefs = await SharedPreferences.getInstance();\nawait prefs.setBool('${esc(key)}', ${val});\n`;
};

dartGenerator.forBlock['pref_get_bool'] = function(block) {
  const key = block.getFieldValue('KEY') || 'key';
  return [`(await SharedPreferences.getInstance()).getBool('${esc(key)}') ?? false`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['timer_delayed'] = function(block, gen) {
  const ms = block.getFieldValue('MS') || 1000;
  const body = gen.statementToCode(block, 'DO') || '  // do something\n';
  return `Timer(const Duration(milliseconds: ${ms}), () {\n${indentCode(body)}\n});\n`;
};

dartGenerator.forBlock['timer_periodic'] = function(block, gen) {
  const ms = block.getFieldValue('MS') || 1000;
  const body = gen.statementToCode(block, 'DO') || '  // periodic tick\n';
  return `Timer.periodic(const Duration(milliseconds: ${ms}), (timer) {\n${indentCode(body)}\n});\n`;
};

dartGenerator.forBlock['http_get_request'] = function(block, gen) {
  const url = block.getFieldValue('URL') || '';
  const body = gen.statementToCode(block, 'DO') || '  // handle response\n';
  return `final response = await http.get(Uri.parse('${esc(url)}'));\nif (response.statusCode == 200) {\n${indentCode(body)}\n}\n`;
};

dartGenerator.forBlock['http_post_request'] = function(block, gen) {
  const url = block.getFieldValue('URL') || '';
  const payload = block.getFieldValue('BODY') || '{}';
  const body = gen.statementToCode(block, 'DO') || '  // handle response\n';
  return `final response = await http.post(Uri.parse('${esc(url)}'), body: '${esc(payload)}');\nif (response.statusCode == 200 || response.statusCode == 201) {\n${indentCode(body)}\n}\n`;
};

dartGenerator.forBlock['ui_show_snackbar'] = function(block) {
  const msg = block.getFieldValue('MESSAGE') || '';
  return `ScaffoldMessenger.of(context).showSnackBar(\n  SnackBar(content: Text('${esc(msg)}')),\n);\n`;
};

dartGenerator.forBlock['ui_show_dialog'] = function(block) {
  const title = block.getFieldValue('TITLE') || '';
  const msg = block.getFieldValue('MESSAGE') || '';
  return `showDialog(\n  context: context,\n  builder: (context) => AlertDialog(\n    title: Text('${esc(title)}'),\n    content: Text('${esc(msg)}'),\n    actions: [\n      TextButton(\n        onPressed: () => Navigator.of(context).pop(),\n        child: const Text('OK'),\n      ),\n    ],\n  ),\n);\n`;
};

dartGenerator.forBlock['ui_haptic_feedback'] = function(block) {
  const type = block.getFieldValue('TYPE') || 'lightImpact';
  return `HapticFeedback.${type}();\n`;
};


// ── Widget Events Generators (Bridge) ──────────────────────
dartGenerator.forBlock['widget_on_click'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  return `// EVENT_BINDING: onClick -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_on_long_press'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  return `// EVENT_BINDING: onLongPress -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_on_text_changed'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const valVar = dartGenerator.nameDB_.getName(block.getFieldValue('VALUE_VAR'), Blockly.VARIABLE_CATEGORY_NAME);
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  // Note: the C# compiler engine will need to extract these EVENT_BINDING comments and inject them into the respective widgets
  return `// EVENT_BINDING: onChanged(String ${valVar}) -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_on_checkbox_changed'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const valVar = dartGenerator.nameDB_.getName(block.getFieldValue('VALUE_VAR'), Blockly.VARIABLE_CATEGORY_NAME);
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  return `// EVENT_BINDING: onChanged(bool? ${valVar}) -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_on_switch_changed'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const valVar = dartGenerator.nameDB_.getName(block.getFieldValue('VALUE_VAR'), Blockly.VARIABLE_CATEGORY_NAME);
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  return `// EVENT_BINDING: onChanged(bool ${valVar}) -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_on_slider_changed'] = function(block, gen) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const valVar = dartGenerator.nameDB_.getName(block.getFieldValue('VALUE_VAR'), Blockly.VARIABLE_CATEGORY_NAME);
  const body = gen.statementToCode(block, 'DO') || '  // no-op\n';
  return `// EVENT_BINDING: onChanged(double ${valVar}) -> ${widgetId}\n${body}\n`;
};

dartGenerator.forBlock['widget_set_text'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const text = block.getFieldValue('TEXT_VALUE') || '';
  return `// DYNAMIC_PROP: setText('${esc(text)}') -> ${widgetId}\n`;
};

dartGenerator.forBlock['widget_get_text'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  // This requires a TextEditingController in Flutter, the compiler will handle mapping this.
  return [`_controllers['${widgetId}']?.text ?? ''`, dartGenerator.ORDER_ATOMIC];
};

dartGenerator.forBlock['widget_set_visible'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const visible = block.getFieldValue('VISIBLE') || 'true';
  return `// DYNAMIC_PROP: setVisible(${visible}) -> ${widgetId}\n`;
};

dartGenerator.forBlock['widget_set_enabled'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const enabled = block.getFieldValue('ENABLED') || 'true';
  return `// DYNAMIC_PROP: setEnabled(${enabled}) -> ${widgetId}\n`;
};

dartGenerator.forBlock['widget_set_bg_color'] = function(block) {
  const widgetId = block.getFieldValue('WIDGET_ID') || 'none';
  const color = hexToColor(block.getFieldValue('COLOR'));
  return `// DYNAMIC_PROP: setBgColor(Color(${color})) -> ${widgetId}\n`;
};


// ============================================================
// Theme Definitions
// ============================================================
const darkTheme = Blockly.Theme.defineTheme('snapcode_dark', {
  name: 'snapcode_dark',
  base: Blockly.Themes.Classic,
  blockStyles: {
    app_blocks:      { colourPrimary: '#7c3aed', colourSecondary: '#6d28d9', colourTertiary: '#5b21b6' },
    layout_blocks:   { colourPrimary: '#2563eb', colourSecondary: '#1d4ed8', colourTertiary: '#1e40af' },
    widget_blocks:   { colourPrimary: '#059669', colourSecondary: '#047857', colourTertiary: '#065f46' },
    input_blocks:    { colourPrimary: '#d97706', colourSecondary: '#b45309', colourTertiary: '#92400e' },
    logic_blocks:    { colourPrimary: '#e11d48', colourSecondary: '#be123c', colourTertiary: '#9f1239' },
    variable_blocks: { colourPrimary: '#0891b2', colourSecondary: '#0e7490', colourTertiary: '#155e75' },
  },
  categoryStyles: {
    app_category: { colour: '#7c3aed' }, layout_category: { colour: '#2563eb' },
    widget_category: { colour: '#059669' }, input_category: { colour: '#d97706' },
    logic_category: { colour: '#e11d48' }, variable_category: { colour: '#0891b2' },
    raw_dart_category: { colour: '#475569' }, state_category: { colour: '#8b5cf6' },
    component_category: { colour: '#0284c7' },
  },
  componentStyles: {
    workspaceBackgroundColour: '#0c1020',
    toolboxBackgroundColour: '#111827',
    toolboxForegroundColour: '#e2e8f0',
    flyoutBackgroundColour: '#1a1f35',
    flyoutForegroundColour: '#e2e8f0',
    flyoutOpacity: 0.95,
    scrollbarColour: '#374151',
    scrollbarOpacity: 0.6,
    insertionMarkerColour: '#7c3aed',
    insertionMarkerOpacity: 0.4,
    cursorColour: '#7c3aed',
  },
  fontStyle: { family: "'Segoe UI', 'Inter', sans-serif", weight: '500', size: 11 },
  startHats: true,
});

const lightTheme = Blockly.Theme.defineTheme('snapcode_light', {
  name: 'snapcode_light',
  base: Blockly.Themes.Classic,
  blockStyles: {
    app_blocks:      { colourPrimary: '#7c3aed', colourSecondary: '#8b5cf6', colourTertiary: '#a78bfa' },
    layout_blocks:   { colourPrimary: '#2563eb', colourSecondary: '#3b82f6', colourTertiary: '#60a5fa' },
    widget_blocks:   { colourPrimary: '#059669', colourSecondary: '#10b981', colourTertiary: '#34d399' },
    input_blocks:    { colourPrimary: '#d97706', colourSecondary: '#f59e0b', colourTertiary: '#fbbf24' },
    logic_blocks:    { colourPrimary: '#e11d48', colourSecondary: '#f43f5e', colourTertiary: '#fb7185' },
    variable_blocks: { colourPrimary: '#0891b2', colourSecondary: '#06b6d4', colourTertiary: '#22d3ee' },
  },
  categoryStyles: {
    app_category: { colour: '#7c3aed' }, layout_category: { colour: '#2563eb' },
    widget_category: { colour: '#059669' }, input_category: { colour: '#d97706' },
    logic_category: { colour: '#e11d48' }, variable_category: { colour: '#0891b2' },
    raw_dart_category: { colour: '#475569' }, state_category: { colour: '#8b5cf6' },
    component_category: { colour: '#0284c7' },
  },
  componentStyles: {
    workspaceBackgroundColour: '#f8fafc',
    toolboxBackgroundColour: '#e2e8f0',
    toolboxForegroundColour: '#1e293b',
    flyoutBackgroundColour: '#cbd5e1',
    flyoutForegroundColour: '#1e293b',
    flyoutOpacity: 0.95,
    scrollbarColour: '#94a3b8',
    scrollbarOpacity: 0.6,
    insertionMarkerColour: '#7c3aed',
    insertionMarkerOpacity: 0.4,
    cursorColour: '#7c3aed',
  },
  fontStyle: { family: "'Segoe UI', 'Inter', sans-serif", weight: '500', size: 11 },
  startHats: true,
});


// ============================================================
// Workspace Initialization
// ============================================================
let workspace = null;
let currentThemeMode = 'dark';

function initWorkspace(themeMode) {
  currentThemeMode = themeMode || 'dark';
  document.body.className = currentThemeMode;

  const theme = currentThemeMode === 'light' ? lightTheme : darkTheme;

  workspace = Blockly.inject('blocklyDiv', {
    toolbox: document.getElementById('toolbox'),
    theme: theme,
    renderer: 'thrasos',
    grid: {
      spacing: 25,
      length: 3,
      colour: currentThemeMode === 'light' ? '#cbd5e1' : '#1e293b',
      snap: true,
    },
    zoom: {
      controls: true,
      wheel: true,
      startScale: 0.75,
      maxScale: 3,
      minScale: 0.3,
      scaleSpeed: 1.2,
      pinch: true,
    },
    trashcan: true,
    move: {
      scrollbars: { horizontal: true, vertical: true },
      drag: true,
      wheel: false,
    },
    sounds: false,
  });

  // Auto-generate code on workspace change
  workspace.addChangeListener(function(event) {
    if (event.isUiEvent) return;
    try {
      const code = dartGenerator.workspaceToCode(workspace);
      // Send generated code to C# host
      if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage(JSON.stringify({
          type: 'codeGenerated',
          data: code
        }));
      }
    } catch(e) {
      console.error('Code generation error:', e);
    }
  });

  // Notify host that workspace is ready
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage(JSON.stringify({ type: 'ready' }));
  }
}

// ============================================================
// C# ↔ JS Bridge Functions
// ============================================================

// Called from C# to load a saved workspace
window.loadWorkspace = function(jsonString) {
  try {
    if (!workspace) return;
    Blockly.serialization.workspaces.load(JSON.parse(jsonString), workspace);
  } catch(e) {
    console.error('Failed to load workspace:', e);
  }
};

// Called from C# to initialize an isolated event workspace
window.loadEventWorkspace = function(widgetId, eventType, jsonString) {
  try {
    if (!workspace) return;
    workspace.clear();

    if (jsonString && jsonString.trim() !== '{}' && jsonString.trim() !== 'null' && jsonString.trim() !== '""' && jsonString.trim() !== '"\\"{}\\""') {
      let parsed = JSON.parse(jsonString);
      if (typeof parsed === 'string') {
          try { parsed = JSON.parse(parsed); } catch(e) {}
      }
      if (typeof parsed === 'object' && parsed !== null && Object.keys(parsed).length > 0) {
          Blockly.serialization.workspaces.load(parsed, workspace);
      } else {
          createLockedRootBlock(widgetId, eventType);
      }
    } else {
      createLockedRootBlock(widgetId, eventType);
    }
  } catch(e) {
    console.error('Failed to load event workspace:', e);
    createLockedRootBlock(widgetId, eventType);
  }
};

function createLockedRootBlock(widgetId, eventType) {
      let blockType = '';
      if (eventType === 'onClick') blockType = 'widget_on_click';
      else if (eventType === 'onLongPress') blockType = 'widget_on_long_press';
      else if (eventType === 'onTextChanged') blockType = 'widget_on_text_changed';
      else if (eventType === 'onCheckboxChanged') blockType = 'widget_on_checkbox_changed';
      else if (eventType === 'onSwitchChanged') blockType = 'widget_on_switch_changed';
      else if (eventType === 'onSliderChanged') blockType = 'widget_on_slider_changed';

      if (blockType) {
        const rootBlock = workspace.newBlock(blockType);
        
        // Temporarily patch the options so we can set the field value immediately
        const field = rootBlock.getField('WIDGET_ID');
        if (field) {
            field.menuGenerator_ = [[widgetId, widgetId]];
            rootBlock.setFieldValue(widgetId, 'WIDGET_ID');
            patchWidgetDropdowns(); // Restore normal dropdown options
            rootBlock.setFieldValue(widgetId, 'WIDGET_ID'); // Apply again just in case
        }

        rootBlock.initSvg();
        rootBlock.render();
        // Lock it down
        rootBlock.setMovable(false);
        rootBlock.setDeletable(false);
        rootBlock.setEditable(false);
        // Position it nicely
        rootBlock.moveBy(50, 50);
      }
}

// Called from C# to get current workspace state
window.getWorkspace = function() {
  try {
    if (!workspace) return '{}';
    return JSON.stringify(Blockly.serialization.workspaces.save(workspace));
  } catch(e) {
    console.error('Failed to save workspace:', e);
    return '{}';
  }
};

// Called from C# to get generated Dart code
window.getGeneratedCode = function() {
  try {
    if (!workspace) return '';
    return dartGenerator.workspaceToCode(workspace);
  } catch(e) {
    console.error('Failed to generate code:', e);
    return '';
  }
};

// Called from C# to switch theme
window.setTheme = function(mode) {
  currentThemeMode = mode;
  document.body.className = mode;
  if (workspace) {
    const theme = mode === 'light' ? lightTheme : darkTheme;
    workspace.setTheme(theme);
    // Update grid color
    const gridPattern = workspace.getCanvas().querySelector('.blocklyGridPattern');
    if (gridPattern) {
      const lines = gridPattern.querySelectorAll('line');
      lines.forEach(l => l.setAttribute('stroke', mode === 'light' ? '#cbd5e1' : '#1e293b'));
    }
  }
};

// Called from C# to clear workspace
window.clearWorkspace = function() {
  if (workspace) workspace.clear();
};

// Called from C# to get block count
window.getBlockCount = function() {
  if (!workspace) return 0;
  return workspace.getAllBlocks(false).length;
};

let originalToolbox = null;

// Called from C# to set the active file context type (page, dialog, custom_view)
window.setWorkspaceContext = function(type) {
  const blocklyDiv = document.getElementById('blocklyDiv');
  let msgDiv = document.getElementById('noBlocksMsg');
  
  if (!msgDiv) {
    msgDiv = document.createElement('div');
    msgDiv.id = 'noBlocksMsg';
    msgDiv.style.position = 'absolute';
    msgDiv.style.top = '0';
    msgDiv.style.left = '0';
    msgDiv.style.width = '100%';
    msgDiv.style.height = '100%';
    msgDiv.style.display = 'flex';
    msgDiv.style.alignItems = 'center';
    msgDiv.style.justifyContent = 'center';
    msgDiv.style.fontSize = '1.5rem';
    msgDiv.style.fontWeight = 'bold';
    msgDiv.style.color = currentThemeMode === 'light' ? '#64748b' : '#94a3b8';
    msgDiv.style.backgroundColor = currentThemeMode === 'light' ? '#f0f4f8' : '#0c1020';
    msgDiv.style.zIndex = '1000';
    msgDiv.innerText = 'No blocks available for Custom Views';
    document.body.appendChild(msgDiv);
  }

  if (type === 'custom_view') {
    blocklyDiv.style.display = 'none';
    msgDiv.style.display = 'flex';
  } else {
    blocklyDiv.style.display = 'block';
    msgDiv.style.display = 'none';
    
    // Dynamically filter toolbox for 'dialog' vs 'page'
    if (workspace && originalToolbox) {
      let currentToolbox = originalToolbox.cloneNode(true);
      if (type === 'dialog' || type === 'dialogfragment') {
        let appStructureCategory = currentToolbox.querySelector('category[name="App Structure"]');
        if (appStructureCategory) {
          appStructureCategory.remove();
        }
      }
      workspace.updateToolbox(currentToolbox);
    }
  }
};

// Auto-initialize with dark theme on load
window.addEventListener('load', function() {
  originalToolbox = document.getElementById('toolbox').cloneNode(true);
  initWorkspace('dark');
});
