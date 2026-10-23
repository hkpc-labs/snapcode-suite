using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Snapcode.Services;

namespace Snapcode.Views
{
    public sealed partial class NewProjectDialog : ContentDialog
    {
        private readonly bool _isWebOnly;
        private string _baseLocation = "";
        public string? CreatedProjectPath { get; private set; }

        public NewProjectDialog(bool isWebOnly = false)
        {
            this.InitializeComponent();
            _isWebOnly = isWebOnly;
            
            if (App.MainWindow?.Content is FrameworkElement fe)
            {
                this.RequestedTheme = fe.RequestedTheme;
            }
            
            // Set default location to Documents/Snapcode/repos
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _baseLocation = Path.Combine(docsPath, "Snapcode", "repos");
            
            if (!Directory.Exists(_baseLocation))
            {
                Directory.CreateDirectory(_baseLocation);
            }
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (_isWebOnly)
            {
                Title = "New Flutter Web Project";
                ChkAndroid.IsChecked = false;
                ChkIos.IsChecked = false;
                ChkWindows.IsChecked = false;
                ChkMacOs.IsChecked = false;
                ChkLinux.IsChecked = false;

                ChkAndroid.IsEnabled = false;
                ChkIos.IsEnabled = false;
                ChkWindows.IsEnabled = false;
                ChkMacOs.IsEnabled = false;
                ChkLinux.IsEnabled = false;
            }
            
            UpdateLocationText();
            ValidateInput();
        }

        private void ProjectNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLocationText();
            ValidateInput();
        }

        private void UpdateLocationText()
        {
            string projectName = ProjectNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(projectName))
            {
                LocationTextBox.Text = _baseLocation;
            }
            else
            {
                LocationTextBox.Text = Path.Combine(_baseLocation, projectName);
            }
        }

        private void ValidateInput()
        {
            string name = ProjectNameTextBox.Text.Trim();
            bool isValid = true;
            
            if (string.IsNullOrEmpty(name))
            {
                NameErrorTextBlock.Visibility = Visibility.Collapsed;
                isValid = false;
            }
            else if (!Regex.IsMatch(name, @"^[a-z_][a-z0-9_]*$"))
            {
                NameErrorTextBlock.Text = "Invalid Flutter project name. Use lowercase letters and underscores.";
                NameErrorTextBlock.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (Directory.Exists(Path.Combine(_baseLocation, name)))
            {
                NameErrorTextBlock.Text = "A directory with this name already exists in the selected location.";
                NameErrorTextBlock.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                NameErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            IsPrimaryButtonEnabled = isValid;
        }

        private async void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _baseLocation = folder.Path;
                UpdateLocationText();
                ValidateInput();
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            args.Cancel = true; // Keep dialog open while creating

            string projectName = ProjectNameTextBox.Text.Trim();
            string org = OrgTextBox.Text.Trim();
            string fullPath = Path.Combine(_baseLocation, projectName);

            List<string> platforms = new();
            if (ChkWeb.IsChecked == true) platforms.Add("web");
            if (ChkAndroid.IsChecked == true) platforms.Add("android");
            if (ChkIos.IsChecked == true) platforms.Add("ios");
            if (ChkWindows.IsChecked == true) platforms.Add("windows");
            if (ChkMacOs.IsChecked == true) platforms.Add("macos");
            if (ChkLinux.IsChecked == true) platforms.Add("linux");

            string platformStr = platforms.Count > 0 ? string.Join(",", platforms) : "web";
            
            IsPrimaryButtonEnabled = false;
            ProjectNameTextBox.IsEnabled = false;
            OrgTextBox.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;
            LogTextBlock.Text = "";

            bool success = await CreateFlutterProjectAsync(projectName, org, _baseLocation, platformStr);

            if (success)
            {
                CreatedProjectPath = fullPath;
                
                // Add to recent projects
                var config = await ConfigService.LoadAsync();
                if (!config.RecentProjects.Contains(fullPath))
                {
                    config.RecentProjects.Insert(0, fullPath);
                    if (config.RecentProjects.Count > 20)
                    {
                        config.RecentProjects.RemoveAt(20);
                    }
                    await ConfigService.SaveAsync(config);
                }
                
                deferral.Complete();
                this.Hide();
            }
            else
            {
                StatusTextBlock.Text = "Failed to create project. See log.";
                StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                CreationProgressBar.Visibility = Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
                ProjectNameTextBox.IsEnabled = true;
                OrgTextBox.IsEnabled = true;
                deferral.Complete();
            }
        }

        private async Task<bool> CreateFlutterProjectAsync(string name, string org, string baseDir, string platforms)
        {
            var config = await ConfigService.LoadAsync();
            string flutterPath = string.IsNullOrEmpty(config.FlutterSdkPath) ? "flutter" : Path.Combine(config.FlutterSdkPath, "bin", "flutter.bat");

            try
            {
                string args = $"create --org {org} --platforms {platforms} {name}";
                LogTextBlock.Text += $"> {flutterPath} {args}\n";

                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterPath,
                    Arguments = args,
                    WorkingDirectory = baseDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                
                process.OutputDataReceived += (s, e) => 
                {
                    if (e.Data != null)
                    {
                        DispatcherQueue.TryEnqueue(() => LogTextBlock.Text += e.Data + "\n");
                    }
                };
                
                process.ErrorDataReceived += (s, e) => 
                {
                    if (e.Data != null)
                    {
                        DispatcherQueue.TryEnqueue(() => LogTextBlock.Text += "[ERROR] " + e.Data + "\n");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
                
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                LogTextBlock.Text += $"\nException: {ex.Message}";
                return false;
            }
        }
    }
}
