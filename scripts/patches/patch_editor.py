import sys

file_path = r'c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\src\Views\EditorPage.xaml.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Add UIStateChanged event handler hook
if 'NativeDesigner_UIStateChanged' not in code:
    code = code.replace('NativeDesigner.WidgetTreeFlatJsonChanged += NativeDesigner_WidgetTreeFlatJsonChanged;',
        'NativeDesigner.WidgetTreeFlatJsonChanged += NativeDesigner_WidgetTreeFlatJsonChanged;\\n                NativeDesigner.UIStateChanged += NativeDesigner_UIStateChanged;')
        
    handler = '''
        private void NativeDesigner_UIStateChanged(object sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusSaveIcon.Glyph = "\\uE7BA"; // Unsaved dot
                TxtSaveStatus.Text = "Unsaved changes";
            });
        }
'''
    code = code.replace('private async void NativeDesigner_WidgetTreeFlatJsonChanged', handler + '\\n        private async void NativeDesigner_WidgetTreeFlatJsonChanged')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)
    
print('EditorPage patched!')
