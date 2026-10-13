using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Snapcode.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace Snapcode.Views
{
    public sealed partial class ProjectExplorerControl : UserControl
    {
        public ProjectExplorerViewModel ViewModel { get; } = new();

        public event EventHandler<string>? FileOpened;

        private FileItemViewModel? _contextTargetItem;

        public ProjectExplorerControl()
        {
            this.InitializeComponent();
            ViewModel.OpenFileRequested += (s, path) => FileOpened?.Invoke(this, path);
        }

        public void LoadProject(string projectRoot)
        {
            ViewModel.LoadProject(projectRoot);
        }

        private void ProjectTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is FileItemViewModel item)
            {
                ViewModel.SelectedItem = item;
                if (!item.IsDirectory)
                {
                    FileOpened?.Invoke(this, item.FullPath);
                }
            }
        }

        private void TreeViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is FileItemViewModel item)
            {
                _contextTargetItem = item;
                ViewModel.SelectedItem = item;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Refresh();
        }

        private void BtnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            CollapseRecursive(ViewModel.RootItems);
        }

        private void CollapseRecursive(System.Collections.Generic.IEnumerable<FileItemViewModel> items)
        {
            foreach (var item in items)
            {
                if (item.IsDirectory)
                {
                    item.IsExpanded = false;
                    CollapseRecursive(item.Children);
                }
            }
        }

        private void BtnReveal_Click(object sender, RoutedEventArgs e)
        {
            string target = ViewModel.SelectedItem?.FullPath ?? ViewModel.FlutterRootPath ?? ViewModel.ProjectRootPath ?? "";
            RevealInExplorer(target);
        }

        private async void BtnCreateFileConfirm_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteCreateFileAsync();
        }

        private async void TxtNewFileName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await ExecuteCreateFileAsync();
            }
        }

        private async Task ExecuteCreateFileAsync()
        {
            string name = TxtNewFileName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            FlyoutNewFile.Hide();
            TxtNewFileName.Text = "";

            try
            {
                var created = await ViewModel.CreateFileAsync(ViewModel.SelectedItem, name);
                if (created != null)
                {
                    FileOpened?.Invoke(this, created.FullPath);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Create File Failed", ex.Message);
            }
        }

        private void BtnCreateFolderConfirm_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCreateFolder();
        }

        private void TxtNewFolderName_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ExecuteCreateFolder();
            }
        }

        private void ExecuteCreateFolder()
        {
            string name = TxtNewFolderName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            FlyoutNewFolder.Hide();
            TxtNewFolderName.Text = "";

            try
            {
                ViewModel.CreateFolder(ViewModel.SelectedItem, name);
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialogAsync("Create Folder Failed", ex.Message);
            }
        }

        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            BtnNewFile.Flyout?.ShowAt(BtnNewFile);
        }

        private void MenuNewFolder_Click(object sender, RoutedEventArgs e)
        {
            BtnNewFolder.Flyout?.ShowAt(BtnNewFolder);
        }

        private async void MenuRename_Click(object sender, RoutedEventArgs e)
        {
            var item = _contextTargetItem ?? ViewModel.SelectedItem;
            if (item == null) return;

            var inputTextBox = new TextBox
            {
                Text = item.Name,
                SelectionStart = 0,
                SelectionLength = item.Name.Length
            };

            var dialog = new ContentDialog
            {
                Title = item.IsDirectory ? "Rename Folder" : "Rename File",
                Content = inputTextBox,
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                bool success = ViewModel.RenameItem(item, inputTextBox.Text.Trim());
                if (!success)
                {
                    await ShowErrorDialogAsync("Rename Failed", "Unable to rename item. Make sure the name is valid and not already taken.");
                }
            }
        }

        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = _contextTargetItem ?? ViewModel.SelectedItem;
            if (item == null) return;

            string itemType = item.IsDirectory ? "folder and all its contents" : "file";
            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to permanently delete '{item.Name}' ({itemType})?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                bool success = ViewModel.DeleteItem(item);
                if (!success)
                {
                    await ShowErrorDialogAsync("Delete Failed", "Could not delete the selected item.");
                }
            }
        }

        private void MenuReveal_Click(object sender, RoutedEventArgs e)
        {
            var item = _contextTargetItem ?? ViewModel.SelectedItem;
            if (item != null)
            {
                RevealInExplorer(item.FullPath);
            }
        }

        private void MenuCopyPath_Click(object sender, RoutedEventArgs e)
        {
            var item = _contextTargetItem ?? ViewModel.SelectedItem;
            if (item != null)
            {
                var package = new DataPackage();
                package.SetText(item.FullPath);
                Clipboard.SetContent(package);
            }
        }

        private void MenuCopyRelativePath_Click(object sender, RoutedEventArgs e)
        {
            var item = _contextTargetItem ?? ViewModel.SelectedItem;
            if (item != null && !string.IsNullOrEmpty(ViewModel.FlutterRootPath))
            {
                string rel = Path.GetRelativePath(ViewModel.FlutterRootPath, item.FullPath);
                var package = new DataPackage();
                package.SetText(rel);
                Clipboard.SetContent(package);
            }
        }

        private void RevealInExplorer(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
                }
                else if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectExplorer] RevealInExplorer error: {ex.Message}");
            }
        }

        private async Task ShowErrorDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
