import sys

file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\vs files\Views\NativeUIDesigner.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

target = '''            _selectedNodeControl = node;
            _selectedWidget = node?.NodeItem;

            if (_selectedWidget != null)'''

replacement = '''            _selectedNodeControl = node;
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged -= SelectedWidget_PropertyChanged;
            }
            _selectedWidget = node?.NodeItem;
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged += SelectedWidget_PropertyChanged;
            }

            if (_selectedWidget != null)'''

code = code.replace(target, replacement)

if 'SelectedWidget_PropertyChanged' not in code:
    code = code.replace('public void SelectNode(DesignerNodeControl node)', '''
        private void SelectedWidget_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UIStateChanged?.Invoke(this, EventArgs.Empty);
            RenderRoot();
        }

        public void SelectNode(DesignerNodeControl node)''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
print('Patched 6!')
