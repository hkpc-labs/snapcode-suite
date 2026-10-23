import sys

file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\src\Views\NativeUIDesigner.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Add UIStateChanged event
if 'public event EventHandler UIStateChanged;' not in code:
    code = code.replace('public event EventHandler<string> WidgetTreeFlatJsonChanged;', 
        'public event EventHandler<string> WidgetTreeFlatJsonChanged;\\n        public event EventHandler UIStateChanged;')

# Hook UIStateChanged in SaveStateForUndo
if 'UIStateChanged?.Invoke(this, EventArgs.Empty);' not in code:
    code = code.replace('private void SaveStateForUndo()\\n        {\\n            if (_isRestoringState) return;',
        'private void SaveStateForUndo()\\n        {\\n            UIStateChanged?.Invoke(this, EventArgs.Empty);\\n            if (_isRestoringState) return;')

# Hook PropertyChanged in WidgetItem
if 'UIStateChanged?.Invoke(this, EventArgs.Empty);' not in code: # Check again, but we just added it to SaveStateForUndo
    pass 
# We need to hook PropertyChanged of _selectedWidget to fire UIStateChanged
# Search for _selectedWidget = item;
target_select = '_selectedWidget = item;'
if target_select in code and '_selectedWidget.PropertyChanged +=' not in code:
    replacement = '''
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged -= SelectedWidget_PropertyChanged;
            }
            _selectedWidget = item;
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged += SelectedWidget_PropertyChanged;
            }
'''
    code = code.replace(target_select, replacement)
    
    # Add handler
    handler = '''
        private void SelectedWidget_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UIStateChanged?.Invoke(this, EventArgs.Empty);
            // Re-render root when a property changes so the mock UI updates!
            RenderRoot();
        }
'''
    code = code.replace('public void SelectNode(WidgetItem item)', handler + '\\n        public void SelectNode(WidgetItem item)')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
    
print('NativeUIDesigner patched!')
