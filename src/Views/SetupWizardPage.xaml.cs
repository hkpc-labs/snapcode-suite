using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snapcode.Models;
using Snapcode.Services;
using Windows.Storage.Pickers;

namespace Snapcode.Views
{
    public sealed partial class SetupWizardPage : Page
    {
        private int _currentStep = 1;
        private List<DiagnosticItem> _diagnosticItems = new();
        private AppConfig _config = new();

        public event EventHandler? SetupCompleted;

        public SetupWizardPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _config = await ConfigService.LoadAsync();
            _diagnosticItems = ToolchainService.CreateDiagnosticItems();
            GoToStep(1);
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentStep)
            {
                case 1:
                    await RunDiagnosticsAsync();
                    GoToStep(2);
                    break;
                case 2:
                    GoToStep(3);
                    BuildFixPanel();
                    break;
                case 3:
                    GoToStep(4);
                    await BuildReadySummaryAsync();
                    break;
                case 4:
                    await FinishWizardAsync();
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
            {
                GoToStep(_currentStep - 1);
            }
        }

        private async void RescanButton_Click(object sender, RoutedEventArgs e)
        {
            await RunDiagnosticsAsync();
            GoToStep(2);
        }

        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "SnapCode Setup Help",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = "What to keep in mind:\n\n" +
                               "1. Flutter SDK:\n" +
                               "   - Ensure you select the ROOT folder of the Flutter SDK (e.g., C:\\flutter or C:\\src\\flutter).\n" +
                               "   - The folder must contain a 'bin' subdirectory with flutter.bat inside.\n\n" +
                               "2. Git:\n" +
                               "   - Required by Flutter. The path can usually be auto-detected.\n\n" +
                               "3. Android SDK:\n" +
                               "   - Typically located at %LOCALAPPDATA%\\Android\\Sdk.\n" +
                               "   - It must contain 'platform-tools' and 'build-tools'.\n\n" +
                               "4. Java JDK:\n" +
                               "   - Android Studio bundles a JDK which is fine to use. Otherwise, install JDK 17.\n\n" +
                               "Use the 'Browse' button in step 3 (Configure) to manually set the paths if they were not found automatically.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    MaxHeight = 400
                },
                CloseButtonText = "Got it",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void GoToStep(int step)
        {
            _currentStep = step;

            WelcomePanel.Visibility = Visibility.Collapsed;
            DiagnosticsPanel.Visibility = Visibility.Collapsed;
            ConfigurePanel.Visibility = Visibility.Collapsed;
            ReadyPanel.Visibility = Visibility.Collapsed;

            switch (step)
            {
                case 1:
                    WelcomePanel.Visibility = Visibility.Visible;
                    BackButton.Visibility = Visibility.Collapsed;
                    TopRescanButton.Visibility = Visibility.Collapsed;
                    NextButton.Content = "Get Started";
                    break;
                case 2:
                    DiagnosticsPanel.Visibility = Visibility.Visible;
                    BackButton.Visibility = Visibility.Visible;
                    TopRescanButton.Visibility = Visibility.Visible;
                    NextButton.Content = "Next";
                    NextButton.IsEnabled = true;
                    break;
                case 3:
                    ConfigurePanel.Visibility = Visibility.Visible;
                    BackButton.Visibility = Visibility.Visible;
                    TopRescanButton.Visibility = Visibility.Visible;
                    NextButton.Content = "Next";
                    NextButton.IsEnabled = true;
                    break;
                case 4:
                    ReadyPanel.Visibility = Visibility.Visible;
                    BackButton.Visibility = Visibility.Visible;
                    TopRescanButton.Visibility = Visibility.Collapsed;
                    NextButton.Content = "Launch SnapCode";
                    NextButton.IsEnabled = true;
                    break;
            }

            UpdateStepIndicators(step);
        }

        private void UpdateStepIndicators(int activeStep)
        {
            var circles = new[] { Step1Circle, Step2Circle, Step3Circle, Step4Circle };
            var texts = new[] { Step1Text, Step2Text, Step3Text, Step4Text };
            var labels = new[] { Step1Label, Step2Label, Step3Label, Step4Label };
            
            var accentBrush = (SolidColorBrush)Resources["SnapAccentBrush"];
            var accentLightBrush = (SolidColorBrush)Resources["SnapAccentLightBrush"];
            var dimBrush = (SolidColorBrush)Resources["SnapTextDimBrush"];
            var successBrush = (SolidColorBrush)Resources["SnapSuccessBrush"];
            var inactiveBg = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 61, 61, 61));
            var inactiveFg = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 136, 136, 136));

            for (int i = 0; i < 4; i++)
            {
                int stepNum = i + 1;
                if (stepNum < activeStep)
                {
                    circles[i].Background = successBrush;
                    texts[i].Text = "\uE73E"; // Checkmark icon
                    texts[i].FontFamily = new FontFamily("Segoe Fluent Icons");
                    texts[i].Foreground = new SolidColorBrush(Colors.White);
                    labels[i].Foreground = successBrush;
                    labels[i].FontWeight = Microsoft.UI.Text.FontWeights.Normal;
                }
                else if (stepNum == activeStep)
                {
                    circles[i].Background = accentBrush;
                    texts[i].Text = stepNum.ToString();
                    texts[i].FontFamily = new FontFamily("Segoe UI");
                    texts[i].Foreground = new SolidColorBrush(Colors.White);
                    labels[i].Foreground = accentLightBrush;
                    labels[i].FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                }
                else
                {
                    circles[i].Background = inactiveBg;
                    texts[i].Text = stepNum.ToString();
                    texts[i].FontFamily = new FontFamily("Segoe UI");
                    texts[i].Foreground = inactiveFg;
                    labels[i].Foreground = dimBrush;
                    labels[i].FontWeight = Microsoft.UI.Text.FontWeights.Normal;
                }
            }
        }

        private async Task RunDiagnosticsAsync()
        {
            // Hide content and show loading
            WelcomePanel.Visibility = Visibility.Collapsed;
            DiagnosticsPanel.Visibility = Visibility.Collapsed;
            ConfigurePanel.Visibility = Visibility.Collapsed;
            ReadyPanel.Visibility = Visibility.Collapsed;
            StepIndicatorsPanel.Visibility = Visibility.Collapsed;
            BottomNavPanel.Visibility = Visibility.Collapsed;
            TopRescanButton.Visibility = Visibility.Collapsed;
            
            LoadingOverlay.Visibility = Visibility.Visible;

            await ToolchainService.RunAllChecksAsync(_diagnosticItems);

            // Rebuild lists
            DiagnosticsList.Children.Clear();
            foreach (var item in _diagnosticItems)
            {
                DiagnosticsList.Children.Add(CreateDiagnosticCard(item, isReadOnly: true));
            }

            // Restore UI
            LoadingOverlay.Visibility = Visibility.Collapsed;
            StepIndicatorsPanel.Visibility = Visibility.Visible;
            BottomNavPanel.Visibility = Visibility.Visible;
            TopRescanButton.Visibility = Visibility.Visible;
        }

        private void BuildFixPanel()
        {
            FixList.Children.Clear();
            var failedItems = _diagnosticItems.Where(i => i.Status == DiagnosticStatus.Error || i.Status == DiagnosticStatus.Warning).ToList();
            
            if (failedItems.Count == 0)
            {
                ConfigSubtitle.Text = "All checks passed! No issues to fix.";
            }
            else
            {
                ConfigSubtitle.Text = $"{failedItems.Count} issue(s) found. Set custom paths below if needed.";
            }

            foreach (var item in _diagnosticItems)
            {
                FixList.Children.Add(CreateDiagnosticCard(item, isReadOnly: false));
            }
        }

        private Border CreateDiagnosticCard(DiagnosticItem item, bool isReadOnly)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(40) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            var icon = new FontIcon
            {
                Glyph = item.IconGlyph,
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 0, 0)
            };
            Grid.SetColumn(icon, 0);

            var textStack = new StackPanel { Spacing = 4 };
            Grid.SetColumn(textStack, 1);

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            headerPanel.Children.Add(new TextBlock { Text = item.DisplayName, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 16 });
            
            if (item.Requirement == ToolRequirement.Critical)
            {
                headerPanel.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 108, 92, 231)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 2, 6, 2),
                    Child = new TextBlock { Text = "REQUIRED", FontSize = 10, Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 92, 231)) }
                });
            }

            textStack.Children.Add(headerPanel);
            
            var desc = new TextBlock { Text = item.Description, Foreground = (SolidColorBrush)Resources["SnapTextDimBrush"], FontSize = 12, TextWrapping = TextWrapping.Wrap };
            textStack.Children.Add(desc);

            var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 8, 0, 0) };
            
            var statusIcon = new FontIcon { FontSize = 14 };
            var statusText = new TextBlock { FontSize = 13 };

            item.PropertyChanged += (s, e) =>
            {
                this.DispatcherQueue.TryEnqueue(() => { UpdateStatusUI(item, statusIcon, statusText); });
            };
            UpdateStatusUI(item, statusIcon, statusText);

            statusPanel.Children.Add(statusIcon);
            statusPanel.Children.Add(statusText);

            var loadingRing = new ProgressRing { Width = 16, Height = 16, Visibility = Visibility.Collapsed };
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DiagnosticItem.IsValidating))
                {
                    this.DispatcherQueue.TryEnqueue(() => loadingRing.Visibility = item.IsValidating ? Visibility.Visible : Visibility.Collapsed);
                }
                else if (e.PropertyName == nameof(DiagnosticItem.Status) || e.PropertyName == nameof(DiagnosticItem.StatusMessage))
                {
                    this.DispatcherQueue.TryEnqueue(() => UpdateStatusUI(item, statusIcon, statusText));
                }
            };
            statusPanel.Children.Add(loadingRing);

            textStack.Children.Add(statusPanel);

            if (!isReadOnly)
            {
                if (item.CanBrowse)
                {
                    var pathGrid = new Grid { Margin = new Thickness(0, 12, 0, 0), ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition { Width = GridLength.Auto } } };
                    
                    var pathBox = new TextBox 
                    { 
                        PlaceholderText = "Default path or auto-detected", 
                        Text = item.CustomPath,
                        IsSpellCheckEnabled = false 
                    };
                    
                    if (!string.IsNullOrEmpty(item.DetectedPath) && string.IsNullOrEmpty(item.CustomPath))
                    {
                         pathBox.Text = item.DetectedPath;
                    }

                    pathBox.TextChanged += async (s, e) => 
                    { 
                        item.CustomPath = pathBox.Text; 
                        await ToolchainService.ValidateItemAsync(item);
                        // Also re-check Ready step logic if needed, but the main Next button relies on the final step re-scan or we can rebuild ready summary
                    };
                    pathBox.LostFocus += async (s, e) => { await ToolchainService.ValidateItemAsync(item); };

                    Grid.SetColumn(pathBox, 0);

                    var browseBtn = new Button { Content = "Browse...", Margin = new Thickness(8, 0, 0, 0) };
                    browseBtn.Click += async (s, e) =>
                    {
                        var folderPicker = new FolderPicker();
                        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);
                        
                        folderPicker.FileTypeFilter.Add("*");
                        var folder = await folderPicker.PickSingleFolderAsync();
                        if (folder != null)
                        {
                            pathBox.Text = folder.Path;
                            item.CustomPath = folder.Path;
                            await ToolchainService.ValidateItemAsync(item);
                        }
                    };
                    Grid.SetColumn(browseBtn, 1);

                    pathGrid.Children.Add(pathBox);
                    pathGrid.Children.Add(browseBtn);
                    textStack.Children.Add(pathGrid);
                }

                if (item.PrimaryFix != FixAction.None && !string.IsNullOrEmpty(item.DownloadUrl))
                {
                    var actionBtn = new HyperlinkButton 
                    { 
                        Content = "Download / Learn More", 
                        NavigateUri = new Uri(item.DownloadUrl),
                        Margin = new Thickness(0, 8, 0, 0)
                    };
                    textStack.Children.Add(actionBtn);
                }
            }

            mainGrid.Children.Add(icon);
            mainGrid.Children.Add(textStack);

            card.Child = mainGrid;
            return card;
        }

        private void UpdateStatusUI(DiagnosticItem item, FontIcon icon, TextBlock text)
        {
            icon.Glyph = item.Status switch
            {
                DiagnosticStatus.Found => "\uE73E",
                DiagnosticStatus.Warning => "\uE7BA",
                DiagnosticStatus.Error => "\uEA39",
                DiagnosticStatus.Scanning => "\uE895",
                _ => "\uE9CE"
            };

            var color = item.Status switch
            {
                DiagnosticStatus.Found => (SolidColorBrush)Resources["SnapSuccessBrush"],
                DiagnosticStatus.Warning => (SolidColorBrush)Resources["SnapWarningBrush"],
                DiagnosticStatus.Error => (SolidColorBrush)Resources["SnapErrorBrush"],
                DiagnosticStatus.Scanning => (SolidColorBrush)Resources["SnapAccentBrush"],
                _ => (SolidColorBrush)Resources["SnapTextDimBrush"]
            };

            icon.Foreground = color;
            text.Foreground = color;
            text.Text = item.StatusMessage;
        }

        private async Task BuildReadySummaryAsync()
        {
            ReadySummary.Children.Clear();
            var report = new DiagnosticReport { Items = _diagnosticItems };

            if (report.AllCriticalsPassed)
            {
                ReadyIcon.Glyph = "\uE73E"; // Checkmark
                ReadyIcon.Foreground = (SolidColorBrush)Resources["SnapSuccessBrush"];
                ReadyTitle.Text = "You're All Set!";
                ReadySubtitle.Text = "Your development environment is configured and ready.";
                NextButton.IsEnabled = true;
                NextButton.Content = "Launch SnapCode";
            }
            else
            {
                ReadyIcon.Glyph = "\uEA39"; // Red Cross / Warning
                ReadyIcon.Foreground = (SolidColorBrush)Resources["SnapErrorBrush"];
                ReadyTitle.Text = "Setup Incomplete";
                ReadySubtitle.Text = "Critical tools are missing. You must fix them in the previous step before launching SnapCode.";
                NextButton.IsEnabled = false;
                NextButton.Content = "Cannot Launch Yet";
            }

            foreach (var item in _diagnosticItems)
            {
                var statusColor = item.Status == DiagnosticStatus.Found ? Windows.UI.Color.FromArgb(255, 0, 184, 148) : Windows.UI.Color.FromArgb(255, 225, 112, 85);
                var statusGlyph = item.Status == DiagnosticStatus.Found ? "\uE73E" : "\uEA39";

                var row = new Grid 
                { 
                    ColumnDefinitions = 
                    { 
                        new ColumnDefinition { Width = new GridLength(32) }, // Increased width to prevent overflow
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } 
                    }, 
                    Padding = new Thickness(12, 12, 12, 12) 
                };
                
                var icon = new FontIcon 
                { 
                    Glyph = statusGlyph, 
                    FontSize = 16, 
                    Foreground = new SolidColorBrush(statusColor), 
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                
                var name = new TextBlock 
                { 
                    Text = item.DisplayName, 
                    FontSize = 13, 
                    Foreground = new SolidColorBrush(Colors.White), 
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap // Ensure text wraps instead of overflowing
                };
                
                Grid.SetColumn(icon, 0);
                Grid.SetColumn(name, 1);
                row.Children.Add(icon);
                row.Children.Add(name);

                var card = new Border { Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 35, 35, 35)), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 0, 4), Child = row };
                ReadySummary.Children.Add(card);
            }

            foreach (var item in _diagnosticItems)
            {
                if (item.Status != DiagnosticStatus.Found) continue;
                string p = item.EffectivePath;
                switch (item.Id)
                {
                    case "flutter": _config.FlutterSdkPath = p; break;
                    case "git": _config.GitPath = p; break;
                    case "android_sdk": _config.AndroidSdkPath = p; break;
                    case "java_jdk": _config.JavaHomePath = p; break;
                    case "vs_cpp": _config.VisualStudioPath = p; break;
                    case "dev_mode": _config.WindowsDevModeEnabled = true; break;
                }
            }

            _config.LastDiagnosticScan = DateTime.UtcNow;
            await ConfigService.SaveAsync(_config);
        }

        private async Task FinishWizardAsync()
        {
            _config.IsFirstRunComplete = true;
            await ConfigService.SaveAsync(_config);
            SetupCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
