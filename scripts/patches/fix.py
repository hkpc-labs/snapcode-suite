import sys

# Fix EditorPage.xaml.cs
file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\src\Views\EditorPage.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace('\\n        private async void NativeDesigner_WidgetTreeFlatJsonChanged', '\n        private async void NativeDesigner_WidgetTreeFlatJsonChanged')
code = code.replace('\\uE7BA', '\uE7BA')
with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

# Fix NativeUIDesigner.xaml.cs
file_path2 = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\src\Views\NativeUIDesigner.xaml.cs'
with open(file_path2, 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace('\\n        public event EventHandler UIStateChanged;', '\n        public event EventHandler UIStateChanged;')
code = code.replace('\\n        {\\n            UIStateChanged?.Invoke(this, EventArgs.Empty);\\n            if (_isRestoringState) return;',
    '\n        {\n            UIStateChanged?.Invoke(this, EventArgs.Empty);\n            if (_isRestoringState) return;')
code = code.replace('\\n        public void SelectNode(WidgetItem item)', '\n        public void SelectNode(WidgetItem item)')

with open(file_path2, 'w', encoding='utf-8') as f:
    f.write(code)
    
print('Fixed!')
