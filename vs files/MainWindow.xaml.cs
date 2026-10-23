using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using System;
using System.IO;
using WinRT.Interop;
using Snapcode.Services;
using Snapcode.Views;

namespace Snapcode
{
    /// <summary>
    /// Main application window. On launch, checks if first-run setup is complete.
    /// If not, navigates to the SetupWizardPage. Otherwise, navigates to the main editor.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public new AppWindow AppWindow { get; private set; } = null!;

        public MainWindow()
        {
            InitializeComponent();
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };

            // Configure the window
            ConfigureWindow();

            // Load config and decide which page to show
            LoadInitialPage();
        }

        private void ConfigureWindow()
        {
            // Get the AppWindow for advanced window management
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow = AppWindow.GetFromWindowId(windowId);

            // Set window size
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1100, 750));

            // Set title
            AppWindow.Title = "SnapCode — Setup";

            // Set window and taskbar icon
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(iconPath))
            {
                AppWindow.SetIcon(iconPath);
            }

            // Center the window on screen
            CenterWindow();

            // Extend content into title bar for a modern look
            ExtendsContentIntoTitleBar = true;
        }

        private void CenterWindow()
        {
            var displayArea = DisplayArea.GetFromWindowId(
                AppWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var workArea = displayArea.WorkArea;
                int x = (workArea.Width - AppWindow.Size.Width) / 2;
                int y = (workArea.Height - AppWindow.Size.Height) / 2;
                AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
            }
        }

        private async void LoadInitialPage()
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

            if (!string.IsNullOrEmpty(App.LaunchProjectPath) && Directory.Exists(App.LaunchProjectPath))
            {
                AppWindow.Title = $"SnapCode — {Path.GetFileName(App.LaunchProjectPath)}";
                RootFrame.Navigate(typeof(EditorPage), App.LaunchProjectPath);
            }
            else if (config.IsFirstRunComplete)
            {
                RootFrame.Navigate(typeof(HomePage));
            }
            else
            {
                NavigateToSetupWizard();
            }
        }

        private void NavigateToSetupWizard()
        {
            RootFrame.Navigate(typeof(SetupWizardPage));

            // Listen for wizard completion
            if (RootFrame.Content is SetupWizardPage wizardPage)
            {
                wizardPage.SetupCompleted += OnSetupCompleted;
            }
        }

        private void OnSetupCompleted(object? sender, EventArgs e)
        {
            AppWindow.Title = "SnapCode";
            RootFrame.Navigate(typeof(HomePage));
        }
    }
}
