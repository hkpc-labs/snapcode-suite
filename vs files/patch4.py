import sys

file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\vs files\Views\NativeUIDesigner.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Fix SaveStateForUndo
code = code.replace('private void SaveStateForUndo()\n        {\n            if (_isRestoringState)', 'private void SaveStateForUndo()\n        {\n            UIStateChanged?.Invoke(this, EventArgs.Empty);\n            if (_isRestoringState)')

# Fix SelectNode
code = code.replace('_selectedWidget = item;\n            UpdatePropertiesPanel();', '''
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged -= SelectedWidget_PropertyChanged;
            }
            _selectedWidget = item;
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged += SelectedWidget_PropertyChanged;
            }
            UpdatePropertiesPanel();
''')

if 'SelectedWidget_PropertyChanged' not in code:
    code = code.replace('public void SelectNode(WidgetItem item)', '''
        private void SelectedWidget_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UIStateChanged?.Invoke(this, EventArgs.Empty);
            RenderRoot();
        }

        public void SelectNode(WidgetItem item)''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
print('Patched 4!')
