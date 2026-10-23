import sys
file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\src\Views\EditorPage.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace('NativeDesigner_WidgetTreeFlatJsonChanged;\\n                NativeDesigner.UIStateChanged', 'NativeDesigner_WidgetTreeFlatJsonChanged;\n                NativeDesigner.UIStateChanged')
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
