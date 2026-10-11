import sys

file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\vs files\Views\NativeUIDesigner.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Fix SaveStateForUndo
code = code.replace('public void SaveStateForUndo()\n        {\n            if (_isRestoringState)', 'public void SaveStateForUndo()\n        {\n            UIStateChanged?.Invoke(this, EventArgs.Empty);\n            if (_isRestoringState)')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
print('Patched 5!')
