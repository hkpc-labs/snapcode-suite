using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Snapcode.Services;
using Snapcode.Models;
using Microsoft.UI;

namespace Snapcode.Views
{
    public sealed partial class NewProjectWindow : Window
    {
        private int _currentStep = 1;
        private string _baseLocation = "";
        
        public ObservableCollection<DiagnosticViewModel> Diagnostics { get; } = new();

        public delegate void ProjectCreationHandler(string name, string org, string path, string platforms, bool useKotlin);
        public event ProjectCreationHandler? ProjectCreationRequested;

        public NewProjectWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            
            appWindow.Resize(new Windows.Graphics.SizeInt32(1120, 700));
            appWindow.Title = "Create New Project";
            
            var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.IsResizable = false;
            }
            
            ExtendsContentIntoTitleBar = true;

            // Center window
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var workArea = displayArea.WorkArea;
                int x = (workArea.Width - appWindow.Size.Width) / 2;
                int y = (workArea.Height - appWindow.Size.Height) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
            
            _ = ApplyThemeAsync();
            
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _baseLocation = Path.Combine(docsPath, "Snapcode", "repos");
            
            if (!Directory.Exists(_baseLocation))
            {
                Directory.CreateDirectory(_baseLocation);
            }

            DiagnosticsList.ItemsSource = Diagnostics;
            UpdateLocationText();
            ValidateCurrentStep();
        }

        private async Task ApplyThemeAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (this.Content is FrameworkElement fe)
            {
                fe.RequestedTheme = config.AppTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
        }

        private void ProjectNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLocationText();
            ValidateCurrentStep();
        }

        private void UpdateLocationText()
        {
            if (LocationTextBox == null) return;
            string projectName = ProjectNameTextBox?.Text.Trim() ?? "";
            LocationTextBox.Text = string.IsNullOrEmpty(projectName) ? _baseLocation : Path.Combine(_baseLocation, projectName);
        }

        private void TogglePlatform_Changed(object sender, RoutedEventArgs e)
        {
            if (AndroidOptionsPanel != null && ToggleAndroid != null)
            {
                AndroidOptionsPanel.Visibility = ToggleAndroid.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            ValidateCurrentStep();
        }

        private void ValidateCurrentStep()
        {
            bool isValid = true;
            
            if (_currentStep == 1)
            {
                string name = ProjectNameTextBox?.Text.Trim() ?? "";
                if (string.IsNullOrEmpty(name))
                {
                    if (NameErrorTextBlock != null) NameErrorTextBlock.Visibility = Visibility.Collapsed;
                    isValid = false;
                }
                else if (!Regex.IsMatch(name, @"^[a-z_][a-z0-9_]*$"))
                {
                    if (NameErrorTextBlock != null)
                    {
                        NameErrorTextBlock.Text = "Invalid Flutter project name. Use lowercase letters and underscores.";
                        NameErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    isValid = false;
                }
                else if (Directory.Exists(Path.Combine(_baseLocation, name)))
                {
                    if (NameErrorTextBlock != null)
                    {
                        NameErrorTextBlock.Text = "A directory with this name already exists.";
                        NameErrorTextBlock.Visibility = Visibility.Visible;
                    }
                    isValid = false;
                }
                else
                {
                    if (NameErrorTextBlock != null) NameErrorTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            else if (_currentStep == 2)
            {
                if (ToggleWeb != null && ToggleAndroid != null && ToggleIos != null && 
                    ToggleWindows != null && ToggleMacOs != null && ToggleLinux != null)
                {
                    if (ToggleWeb.IsChecked != true && ToggleAndroid.IsChecked != true && 
                        ToggleIos.IsChecked != true && ToggleWindows.IsChecked != true && 
                        ToggleMacOs.IsChecked != true && ToggleLinux.IsChecked != true)
                    {
                        isValid = false;
                    }
                }
            }
            else if (_currentStep == 3)
            {
                if (Diagnostics != null && Diagnostics.Any(d => d.IsCritical && d.Item.Status == DiagnosticStatus.Error))
                {
                    isValid = false;
                    if (EnvErrorTextBlock != null)
                    {
                        EnvErrorTextBlock.Text = "Cannot create project. Critical toolchains are missing.";
                        EnvErrorTextBlock.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (EnvErrorTextBlock != null) EnvErrorTextBlock.Visibility = Visibility.Collapsed;
                }
            }

            if (BtnNext != null)
            {
                BtnNext.IsEnabled = isValid;
            }
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _baseLocation = folder.Path;
                UpdateLocationText();
                ValidateCurrentStep();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                UpdateView();
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep < 3)
            {
                _currentStep++;
                UpdateView();
                
                if (_currentStep == 3)
                {
                    await RescanToolchainsAsync();
                }
            }
            else
            {
                // Finish!
                string projectName = ProjectNameTextBox.Text.Trim();
                string org = OrgTextBox.Text.Trim();
                
                List<string> platforms = new();
                if (ToggleWeb.IsChecked == true) platforms.Add("web");
                if (ToggleAndroid.IsChecked == true) platforms.Add("android");
                if (ToggleIos.IsChecked == true) platforms.Add("ios");
                if (ToggleWindows.IsChecked == true) platforms.Add("windows");
                if (ToggleMacOs.IsChecked == true) platforms.Add("macos");
                if (ToggleLinux.IsChecked == true) platforms.Add("linux");

                string platformStr = platforms.Count > 0 ? string.Join(",", platforms) : "web";
                bool useKotlin = ChkKotlin.IsChecked == true;

                ProjectCreationRequested?.Invoke(projectName, org, _baseLocation, platformStr, useKotlin);
                this.Close();
            }
        }

        private void UpdateView()
        {
            if (Step1Panel == null) return;
            
            Step1Panel.Visibility = Visibility.Collapsed;
            Step2Panel.Visibility = Visibility.Collapsed;
            Step3Panel.Visibility = Visibility.Collapsed;

            if (_currentStep == 1)
            {
                Step1Panel.Visibility = Visibility.Visible;
                HeaderTitleTextBlock.Text = "Step 1: Project Details";
                BtnBack.IsEnabled = false;
                BtnNext.Content = "Next";
            }
            else if (_currentStep == 2)
            {
                Step2Panel.Visibility = Visibility.Visible;
                HeaderTitleTextBlock.Text = "Step 2: Target Platforms";
                BtnBack.IsEnabled = true;
                BtnNext.Content = "Next";
            }
            else if (_currentStep == 3)
            {
                Step3Panel.Visibility = Visibility.Visible;
                HeaderTitleTextBlock.Text = "Step 3: Environment Setup";
                BtnBack.IsEnabled = true;
                BtnNext.Content = "Finish";
            }

            ValidateCurrentStep();
        }

        private async void BtnRescan_Click(object sender, RoutedEventArgs e)
        {
            await RescanToolchainsAsync();
        }

        private async Task RescanToolchainsAsync()
        {
            Diagnostics.Clear();
            var allItems = ToolchainService.CreateDiagnosticItems();
            var config = await ConfigService.LoadAsync();
            
            var flutterItem = allItems.First(i => i.Id == "flutter");
            if (!string.IsNullOrEmpty(config.FlutterSdkPath)) flutterItem.CustomPath = config.FlutterSdkPath;
            
            var gitItem = allItems.First(i => i.Id == "git");
            if (!string.IsNullOrEmpty(config.GitPath)) gitItem.CustomPath = config.GitPath;

            var requiredItems = new List<DiagnosticItem>();
            requiredItems.Add(flutterItem);
            requiredItems.Add(gitItem);

            if (ToggleWindows.IsChecked == true)
            {
                requiredItems.Add(allItems.First(i => i.Id == "vs_cpp"));
                requiredItems.Add(allItems.First(i => i.Id == "dev_mode"));
            }
            if (ToggleAndroid.IsChecked == true)
            {
                requiredItems.Add(allItems.First(i => i.Id == "android_sdk"));
                requiredItems.Add(allItems.First(i => i.Id == "java_jdk"));
            }

            foreach(var req in requiredItems)
            {
                var vm = new DiagnosticViewModel(req, req.Requirement == ToolRequirement.Critical);
                Diagnostics.Add(vm);
            }

            var itemsToValidate = Diagnostics.ToList();
            foreach(var vm in itemsToValidate)
            {
                await ToolchainService.ValidateItemAsync(vm.Item);
                vm.Update();
            }
            
            ValidateCurrentStep();
        }
    }

    public class DiagnosticViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public DiagnosticItem Item { get; }
        public bool IsCritical { get; }

        public DiagnosticViewModel(DiagnosticItem item, bool isCritical)
        {
            Item = item;
            IsCritical = isCritical;
        }

        public string IconGlyph => Item.IconGlyph;
        public string DisplayName => Item.DisplayName;
        public string StatusMessage => Item.StatusMessage;
        
        public bool IsFound => Item.Status == DiagnosticStatus.Found;
        public bool IsError => Item.Status == DiagnosticStatus.Error;
        public bool IsValidating => Item.IsValidating;

        public string StatusGlyph 
        {
            get
            {
                if (IsFound) return "\uE73E";
                if (IsError) return "\uE783";
                return "";
            }
        }

        public Microsoft.UI.Xaml.Media.SolidColorBrush StatusBrush
        {
            get
            {
                if (IsFound) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                if (IsError) return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            }
        }

        public void Update()
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(null));
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}
