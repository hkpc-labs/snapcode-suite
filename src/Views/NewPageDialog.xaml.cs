using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Snapcode.Views
{
    public sealed partial class NewPageDialog : ContentDialog
    {
        public string FileName => FileNameTextBox.Text;
        
        public string FileType => (FileTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Page";

        public bool HideToolbar => HideToolbarCheckBox.IsChecked ?? false;
        public bool HideStatusBar => HideStatusBarCheckBox.IsChecked ?? false;
        public bool AddFab => AddFabCheckBox.IsChecked ?? false;
        public bool AddNavDrawer => AddNavDrawerCheckBox.IsChecked ?? false;

        public NewPageDialog()
        {
            this.InitializeComponent();
        }

        private void FileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TogglesPanel == null) return;
            
            if (FileType == "Page")
            {
                TogglesPanel.Visibility = Visibility.Visible;
            }
            else
            {
                TogglesPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
