using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;
using Microsoft.UI;
using System.Linq;

namespace Snapcode.Views
{
    public sealed partial class SettingsWindow : Window
    {
        private readonly string? _initialSection;

        public SettingsWindow(string? initialSection = null)
        {
            _initialSection = initialSection;
            this.InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            
            appWindow.Resize(new Windows.Graphics.SizeInt32(960, 600));
            appWindow.Title = "Settings";
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
        }

        private bool _isLoaded = false;
        private async void SettingsNav_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_initialSection))
            {
                var targetItem = SettingsNav.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(m => m.Tag?.ToString() == _initialSection);
                SettingsNav.SelectedItem = targetItem ?? SettingsNav.MenuItems[0];
            }
            else
            {
                SettingsNav.SelectedItem = SettingsNav.MenuItems[0];
            }
            
            var config = await Snapcode.Services.ConfigService.LoadAsync();
            TextBoxFlutterSdk.Text = config.FlutterSdkPath ?? "";
            TextBoxGitPath.Text = config.GitPath ?? "";
            TextBoxAndroidSdk.Text = config.AndroidSdkPath ?? "";

            ComboAppTheme.SelectedItem = config.AppTheme ?? "System Default";
            ComboEditorFont.SelectedItem = config.EditorFont ?? "Consolas";
            
            if (this.Content is FrameworkElement fe)
            {
                fe.RequestedTheme = config.AppTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
            
            _isLoaded = true;
        }

        private async void BtnSavePaths_Click(object sender, RoutedEventArgs e)
        {
            BtnSavePaths.IsEnabled = false;
            var config = await Snapcode.Services.ConfigService.LoadAsync();
            config.FlutterSdkPath = TextBoxFlutterSdk.Text.Trim();
            config.GitPath = TextBoxGitPath.Text.Trim();
            config.AndroidSdkPath = TextBoxAndroidSdk.Text.Trim();
            await Snapcode.Services.ConfigService.SaveAsync(config);
            
            BtnSavePaths.Content = "Saved!";
            await System.Threading.Tasks.Task.Delay(1500);
            BtnSavePaths.Content = "Save Paths";
            BtnSavePaths.IsEnabled = true;
        }

        private async void BtnBrowseFlutter_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                TextBoxFlutterSdk.Text = folder.Path;
            }
        }

        private async void BtnBrowseGit_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".exe");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                TextBoxGitPath.Text = file.Path;
            }
        }

        private async void BtnBrowseAndroid_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                TextBoxAndroidSdk.Text = folder.Path;
            }
        }

        private async void ComboAppTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            var config = await Snapcode.Services.ConfigService.LoadAsync();
            config.AppTheme = ComboAppTheme.SelectedItem as string ?? "System Default";
            await Snapcode.Services.ConfigService.SaveAsync(config);
            
            // Apply to Settings Window
            if (this.Content is FrameworkElement fe)
            {
                fe.RequestedTheme = config.AppTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
            
            // Apply to Main Window
            if (App.MainWindow?.Content is FrameworkElement mainFe)
            {
                mainFe.RequestedTheme = config.AppTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default
                };
            }
        }

        private async void ComboEditorFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;
            var config = await Snapcode.Services.ConfigService.LoadAsync();
            config.EditorFont = ComboEditorFont.SelectedItem as string ?? "Consolas";
            await Snapcode.Services.ConfigService.SaveAsync(config);
        }

        private void SettingsNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = args.SelectedItem as NavigationViewItem;
            if (item != null)
            {
                ToolchainsContent.Visibility = Visibility.Collapsed;
                EnvVarsContent.Visibility = Visibility.Collapsed;
                AndroidDevicesContent.Visibility = Visibility.Collapsed;
                AppearanceContent.Visibility = Visibility.Collapsed;
                UpdatesContent.Visibility = Visibility.Collapsed;
                AboutContent.Visibility = Visibility.Collapsed;

                switch (item.Tag?.ToString())
                {
                    case "Toolchains":
                        ToolchainsContent.Visibility = Visibility.Visible;
                        break;
                    case "EnvVars":
                        EnvVarsContent.Visibility = Visibility.Visible;
                        break;
                    case "AndroidDevices":
                        AndroidDevicesContent.Visibility = Visibility.Visible;
                        _ = RefreshDevicesAsync();
                        break;
                    case "Appearance":
                        AppearanceContent.Visibility = Visibility.Visible;
                        break;
                    case "Updates":
                        UpdatesContent.Visibility = Visibility.Visible;
                        break;
                    case "About":
                        AboutContent.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private string? _missingFlutterPath = null;
        private string? _missingJavaPath = null;

        private async void BtnScanEnv_Click(object sender, RoutedEventArgs e)
        {
            var config = await Snapcode.Services.ConfigService.LoadAsync();
            string sysPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            
            bool pathOk = false;
            _missingFlutterPath = null;

            if (!string.IsNullOrEmpty(config.FlutterSdkPath))
            {
                string flutterBin = System.IO.Path.Combine(config.FlutterSdkPath, "bin");
                if (sysPath.Contains(flutterBin, StringComparison.OrdinalIgnoreCase))
                {
                    pathOk = true;
                }
                else
                {
                    _missingFlutterPath = flutterBin;
                }
            }

            if (pathOk)
            {
                PathStatusIcon.Glyph = "\uE73E"; // Checkmark
                PathStatusIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                PathStatusText.Text = "Configured";
                PathStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                PathStatusIcon.Glyph = "\uEA39"; // Error
                PathStatusIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                PathStatusText.Text = string.IsNullOrEmpty(config.FlutterSdkPath) ? "Flutter SDK not set in Toolchains" : "Missing Flutter /bin";
                PathStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User) ?? 
                              Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine) ?? "";
            
            _missingJavaPath = null;
            bool javaOk = !string.IsNullOrEmpty(javaHome);

            if (!javaOk && !string.IsNullOrEmpty(config.AndroidSdkPath))
            {
                // Attempt to find Android Studio jbr
                string studioPath = @"C:\Program Files\Android\Android Studio\jbr";
                if (System.IO.Directory.Exists(studioPath))
                {
                    _missingJavaPath = studioPath;
                }
            }

            if (javaOk)
            {
                JavaStatusIcon.Glyph = "\uE73E";
                JavaStatusIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                JavaStatusText.Text = "Configured";
                JavaStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                JavaStatusIcon.Glyph = "\uEA39";
                JavaStatusIcon.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                JavaStatusText.Text = _missingJavaPath != null ? "Not Set (Found in Android Studio)" : "Not Set";
                JavaStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            BtnAutoFixEnv.IsEnabled = (!pathOk && _missingFlutterPath != null) || (!javaOk && _missingJavaPath != null);
        }

        private async void BtnAutoFixEnv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_missingFlutterPath != null)
                {
                    string sysPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                    if (!string.IsNullOrEmpty(sysPath) && !sysPath.EndsWith(";")) sysPath += ";";
                    sysPath += _missingFlutterPath;
                    Environment.SetEnvironmentVariable("PATH", sysPath, EnvironmentVariableTarget.User);
                }

                if (_missingJavaPath != null)
                {
                    Environment.SetEnvironmentVariable("JAVA_HOME", _missingJavaPath, EnvironmentVariableTarget.User);
                }

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Environment variables have been updated successfully. You may need to restart your computer or Snapcode for changes to take full effect in all terminals.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };
                await dialog.ShowAsync();

                BtnScanEnv_Click(sender, e);
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to update variables.\n\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };
                await dialog.ShowAsync();
            }
        }

        private async System.Threading.Tasks.Task RefreshDevicesAsync()
        {
            BtnRefreshDevices.IsEnabled = false;
            
            AndroidLoadingPanel.Visibility = Visibility.Visible;
            AndroidErrorPanel.Visibility = Visibility.Collapsed;
            AndroidDataPanel.Visibility = Visibility.Collapsed;

            var config = await Snapcode.Services.ConfigService.LoadAsync();
            
            // Wait 500ms artificially to show the loading spinner for better UX
            await System.Threading.Tasks.Task.Delay(500);

            if (string.IsNullOrEmpty(config.AndroidSdkPath) || !System.IO.Directory.Exists(config.AndroidSdkPath))
            {
                AndroidLoadingPanel.Visibility = Visibility.Collapsed;
                AndroidErrorPanel.Visibility = Visibility.Visible;
                AndroidErrorText.Text = "Please configure a valid Android SDK path in the Toolchains tab.";
                BtnRefreshDevices.IsEnabled = true;
                return;
            }

            string adbPath = System.IO.Path.Combine(config.AndroidSdkPath, "platform-tools", "adb.exe");
            if (!System.IO.File.Exists(adbPath))
            {
                AndroidLoadingPanel.Visibility = Visibility.Collapsed;
                AndroidErrorPanel.Visibility = Visibility.Visible;
                AndroidErrorText.Text = "adb.exe was not found. Please open Android Studio -> SDK Manager -> SDK Tools and install 'Android SDK Platform-Tools'.";
                BtnRefreshDevices.IsEnabled = true;
                return;
            }
            
            string emuPath = System.IO.Path.Combine(config.AndroidSdkPath, "emulator", "emulator.exe");
            if (!System.IO.File.Exists(emuPath))
            {
                AndroidLoadingPanel.Visibility = Visibility.Collapsed;
                AndroidErrorPanel.Visibility = Visibility.Visible;
                AndroidErrorText.Text = "emulator.exe was not found. Please open Android Studio -> SDK Manager -> SDK Tools and install 'Android Emulator'.";
                BtnRefreshDevices.IsEnabled = true;
                return;
            }

            var running = await Snapcode.Services.DeviceService.GetRunningDevicesAsync();
            var avds = await Snapcode.Services.DeviceService.GetInstalledAvdsAsync();

            ListRunningDevices.ItemsSource = running;
            TxtNoRunningDevices.Visibility = running.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            ListInstalledAvds.ItemsSource = avds;
            TxtNoAvds.Visibility = avds.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

            AndroidLoadingPanel.Visibility = Visibility.Collapsed;
            AndroidDataPanel.Visibility = Visibility.Visible;
            BtnRefreshDevices.IsEnabled = true;
        }

        private async void BtnRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDevicesAsync();
        }

        private void BtnFixAndroidSdk_Click(object sender, RoutedEventArgs e)
        {
            SettingsNav.SelectedItem = SettingsNav.MenuItems.FirstOrDefault(m => (m as NavigationViewItem)?.Tag?.ToString() == "Toolchains");
        }

        private async void BtnLaunchAvd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string avdId)
            {
                var dialog = new ContentDialog
                {
                    Title = "Starting Emulator",
                    Content = new StackPanel 
                    { 
                        Spacing = 24, 
                        Margin = new Thickness(0, 16, 0, 0),
                        Children = { 
                            new ProgressBar 
                            { 
                                IsIndeterminate = true, 
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Height = 6,
                                CornerRadius = new CornerRadius(3)
                            }, 
                            new TextBlock 
                            { 
                                Text = $"Waking up {avdId}...", 
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemControlForegroundBaseMediumBrush"]
                            } 
                        } 
                    },
                    CloseButtonText = "Cancel",
                    XamlRoot = this.Content.XamlRoot,
                    RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };
                
                var showTask = dialog.ShowAsync();

                try
                {
                    await Snapcode.Services.DeviceService.LaunchAvdAsync(avdId);
                    
                    bool found = false;
                    bool cancelled = false;
                    for (int i = 0; i < 15; i++)
                    {
                        if (showTask.Status == Windows.Foundation.AsyncStatus.Completed || showTask.Status == Windows.Foundation.AsyncStatus.Canceled)
                        {
                            cancelled = true;
                            break;
                        }

                        await System.Threading.Tasks.Task.Delay(2000);
                        var running = await Snapcode.Services.DeviceService.GetRunningDevicesAsync();
                        if (running.Any(d => d.IsEmulator)) // Wait until an emulator appears
                        {
                            found = true;
                            break;
                        }
                    }
                    
                    dialog.Hide();
                    await RefreshDevicesAsync();
                    
                    if (cancelled) return;
                    
                    if (!found)
                    {
                        var errDialog = new ContentDialog
                        {
                            Title = "Launch Timeout",
                            Content = "The emulator was launched but did not respond within the expected time. It might still be starting up in the background.",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot,
                            RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                        };
                        await errDialog.ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    dialog.Hide();
                    var errDialog = new ContentDialog
                    {
                        Title = "Emulator Launch Failed",
                        Content = $"An error occurred: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot,
                        RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                    };
                    await errDialog.ShowAsync();
                }
            }
        }

        private async void BtnResetData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Reset App Data?",
                Content = "This will wipe all configurations, toolchain paths, and setup data. Snapcode will close and you will need to start from the Welcome screen upon reopening. Are you sure?",
                PrimaryButtonText = "Reset and Close",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.Content.XamlRoot,
                RequestedTheme = (this.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Snapcode.Services.ConfigService.Reset();
                Application.Current.Exit();
            }
        }
    }
}
