using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Snapcode.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Snapcode.Views
{
    public sealed partial class AssetManagerDialog : ContentDialog
    {
        private string _projectRoot;
        private string _filterType;
        public ObservableCollection<AssetInfo> Assets { get; set; } = new();
        public AssetInfo SelectedAsset { get; private set; } // Set if used as a picker

        public AssetManagerDialog(string projectRoot, bool isPickerMode = false, string filterType = "all")
        {
            this.InitializeComponent();
            _projectRoot = projectRoot;
            _filterType = filterType;
            
            // Sync theme
            this.RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default;

            if (isPickerMode)
            {
                this.Title = "Select an Asset";
                this.PrimaryButtonText = "Select";
                this.SecondaryButtonText = "Cancel";
                this.PrimaryButtonClick += AssetManagerDialog_PrimaryButtonClick;
            }

            AssetGridView.ItemsSource = Assets;
            LoadAssets();
        }

        private void AssetManagerDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (AssetGridView.SelectedItem is AssetInfo selected)
            {
                SelectedAsset = selected;
            }
            else
            {
                args.Cancel = true; // Don't close if they haven't selected anything in picker mode
            }
        }

        private void LoadAssets()
        {
            Assets.Clear();
            var list = AssetService.GetAssets(_projectRoot);
            foreach (var item in list)
            {
                if (_filterType == "image" && item.Type != "Image") continue;
                if (_filterType == "font" && item.Type != "Font") continue;
                if (_filterType == "audio" && item.Type != "Audio") continue;
                Assets.Add(item);
            }
        }

        private async void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var window = App.MainWindow;
            if (window == null) return;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            
            if (_filterType == "image")
            {
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".gif");
            }
            else if (_filterType == "font")
            {
                picker.FileTypeFilter.Add(".ttf");
                picker.FileTypeFilter.Add(".otf");
            }
            else if (_filterType == "audio")
            {
                picker.FileTypeFilter.Add(".mp3");
                picker.FileTypeFilter.Add(".wav");
                picker.FileTypeFilter.Add(".ogg");
            }
            else
            {
                picker.FileTypeFilter.Add("*");
            }

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await AssetService.ImportAssetAsync(_projectRoot, file.Path);
                LoadAssets();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (AssetGridView.SelectedItem is AssetInfo selected)
            {
                AssetService.DeleteAsset(_projectRoot, selected.FileName);
                LoadAssets();
            }
        }
    }
}
