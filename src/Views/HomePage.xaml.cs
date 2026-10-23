using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.IO;
using Snapcode.Services;
using System;
using System.Linq;
using Windows.Storage.Pickers;
using WinRT.Interop;
using System.Text.RegularExpressions;

namespace Snapcode.Views
{
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<ProjectItem> RecentProjects { get; } = new();

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadRecentProjectsAsync();
        }

        private async void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavItemProjects;
            await LoadRecentProjectsAsync();
        }

        private async System.Threading.Tasks.Task LoadRecentProjectsAsync()
        {
            var config = await ConfigService.LoadAsync();
            RecentProjects.Clear();
            var addedPaths = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            // First add from config
            foreach (var path in config.RecentProjects)
            {
                if (Directory.Exists(path) && addedPaths.Add(path))
                {
                    RecentProjects.Add(new ProjectItem
                    {
                        Name = Path.GetFileName(path),
                        Path = path
                    });
                }
            }
            
            // Then scan default folder
            string defaultReposPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Snapcode", "repos");
            if (Directory.Exists(defaultReposPath))
            {
                foreach (var dir in Directory.GetDirectories(defaultReposPath))
                {
                    if (addedPaths.Add(dir))
                    {
                        RecentProjects.Add(new ProjectItem
                        {
                            Name = Path.GetFileName(dir),
                            Path = dir
                        });
                    }
                }
            }

            if (RecentProjects.Count > 0)
            {
                RecentProjectsList.ItemsSource = RecentProjects;
                RecentProjectsList.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                RecentProjectsList.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Visible;
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                DispatcherQueue.TryEnqueue(() => 
                {
                    var settingsWindow = new SettingsWindow();
                    settingsWindow.Activate();
                });
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                return; // Do nothing on selection, handled by ItemInvoked
            }

            var item = args.SelectedItem as NavigationViewItem;
            if (item != null)
            {
                switch (item.Tag?.ToString())
                {
                    case "Projects":
                        ShowContent(ProjectsContent);
                        break;
                    case "Templates":
                        ShowContent(TemplatesContent);
                        break;
                    case "Learn":
                        ShowContent(LearnContent);
                        break;
                    case "Help":
                        ShowContent(HelpContent);
                        NavView.IsPaneOpen = false; // Minimize sidebar as requested
                        if (HelpTopicsList.SelectedIndex == -1) HelpTopicsList.SelectedIndex = 0;
                        break;
                }
            }
        }

        private void ShowContent(UIElement content)
        {
            ProjectsContent.Visibility = Visibility.Collapsed;
            TemplatesContent.Visibility = Visibility.Collapsed;
            LearnContent.Visibility = Visibility.Collapsed;
            HelpContent.Visibility = Visibility.Collapsed;
            content.Visibility = Visibility.Visible;
        }

        private async void BtnLearnFlutter_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://flutter.dev/docs"));
        }

        private async void BtnLearnDart_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://dart.dev/guides"));
        }

        private async void BtnLearnMore_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://pub.dev/"));
        }

        private void HelpTopicsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HelpTopicsList.SelectedItem is string topic)
            {
                if (HelpDetailTitle != null)
                {
                    HelpDetailTitle.Text = topic;
                }
                
                if (HelpLinksPanel != null) HelpLinksPanel.Children.Clear();

                if (HelpDetailText != null)
                {
                    if (topic == "What is Flutter?")
                    {
                        HelpDetailText.Text = "Flutter is Google's highly popular open-source UI software development kit. It is used to develop cross-platform applications from a single codebase for any web browser, Android, iOS, Linux, macOS, and Windows.\n\n" +
                            "Why we use Flutter in Snapcode:\n" +
                            "• True Cross-Platform: You write your application code once, and the Flutter engine compiles it to native machine code for all platforms seamlessly.\n" +
                            "• High Performance: Because it compiles to native code (using Dart), Flutter applications easily achieve 60fps or even 120fps on modern devices.\n" +
                            "• Rich Widget Ecosystem: Flutter doesn't use OEM widgets; it draws its own, allowing for pixel-perfect designs across all operating systems without strange rendering inconsistencies.\n" +
                            "• Hot Reload: This is a game changer for developer velocity. You can tweak UI, fix bugs, and instantly see the results without restarting the application or losing state.\n\n" +
                            "Important Resources to bookmark:";
                            
                        if (HelpLinksPanel != null)
                        {
                            HelpLinksPanel.Children.Add(new HyperlinkButton { Content = "Official Documentation", NavigateUri = new Uri("https://docs.flutter.dev") });
                            HelpLinksPanel.Children.Add(new HyperlinkButton { Content = "Dart Language Guide", NavigateUri = new Uri("https://dart.dev/guides") });
                            HelpLinksPanel.Children.Add(new HyperlinkButton { Content = "Flutter Packages (Pub.dev)", NavigateUri = new Uri("https://pub.dev") });
                        }
                    }
                    else if (topic == "Step-by-step: Creating a Project")
                    {
                        HelpDetailText.Text = "Creating a new project in Snapcode is a highly automated and validated process to ensure you don't run into configuration nightmares later on.\n\n" +
                            "Detailed Breakdown:\n" +
                            "1. Project Details:\n" +
                            "   You must provide a valid Flutter project name. Flutter restricts names to lowercase letters and underscores (e.g., 'awesome_app'). The Organization ID is critical as it acts as your Reverse Domain Name (like 'com.hkpclabs') used in App Stores for unique identification.\n\n" +
                            "2. Target Platforms:\n" +
                            "   Select exactly what platforms you intend to build for. If you select Android, Snapcode exposes specific compiler options. We highly recommend keeping Kotlin enabled as it is the modern standard for Android development, replacing Java.\n\n" +
                            "3. Environment Verification:\n" +
                            "   Unlike other IDEs that let you create a broken project, Snapcode actively scans your system registry and PATH environment variables. It verifies the Flutter SDK, Git, Android toolchains, and Visual Studio Build Tools. If a cross appears, you must install the missing component before proceeding. You can click 'Rescan' to re-check after installing a missing tool.\n\n" +
                            "4. Background Generation:\n" +
                            "   Upon clicking Finish, Snapcode runs 'flutter create' securely in a background process, piping the logs back to you in real-time so you can monitor the exact filesystem operations being performed.";
                    }
                    else if (topic == "Common Errors & Fixes")
                    {
                        HelpDetailText.Text = "The Snapcode toolchain analyzer might flag certain errors if your development environment isn't fully set up. Here is a detailed guide on resolving the most notorious issues:\n\n" +
                            "Issue: 'Flutter SDK not found'\n" +
                            "Diagnosis: Snapcode could not find 'flutter' in your system's PATH variable, or the SDK path in Settings is empty/invalid.\n" +
                            "Resolution: Download the latest Flutter SDK from flutter.dev. Extract it to a permanent location (e.g., C:\\src\\flutter). Open Windows Environment Variables, edit 'Path', and add 'C:\\src\\flutter\\bin'. Alternatively, set the explicit path in Snapcode Settings -> Toolchains.\n\n" +
                            "Issue: 'Android Toolchain / Java JDK Missing'\n" +
                            "Diagnosis: Android compilation requires the Android SDK, Android SDK Command-line Tools, and a valid JDK.\n" +
                            "Resolution: \n" +
                            "   1. Install Android Studio.\n" +
                            "   2. Open Android Studio -> SDK Manager -> SDK Tools tab.\n" +
                            "   3. Check 'Android SDK Command-line Tools (latest)' and click Apply.\n" +
                            "   4. Open your terminal and run: flutter doctor --android-licenses and accept all prompts.\n\n" +
                            "Issue: 'Visual Studio C++ Build Tools Missing'\n" +
                            "Diagnosis: Required to compile Windows desktop apps.\n" +
                            "Resolution: Download the Visual Studio Installer. Select the 'Desktop development with C++' workload. Ensure 'Windows 10/11 SDK' is checked in the right panel and install.";
                    }
                }
            }
        }

        private System.Diagnostics.Process? _creationProcess;

        private async void BtnOpenExistingProject_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");

            IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string path = folder.Path;
                bool isValid = ProjectService.IsSnapcodeProject(path) 
                    || ProjectService.IsLegacyFlutterProject(path)
                    || File.Exists(Path.Combine(path, ProjectService.FlutterFolderName, "pubspec.yaml"));

                if (!isValid)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Invalid Project",
                        Content = "The selected folder does not appear to be a valid Flutter or SnapCode project.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot,
                        RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                    };
                    await dialog.ShowAsync();
                    return;
                }

                var config = await ConfigService.LoadAsync();
                config.RecentProjects.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
                config.RecentProjects.Insert(0, path);
                
                if (config.RecentProjects.Count > 20)
                    config.RecentProjects.RemoveAt(20);
                    
                await ConfigService.SaveAsync(config);
                await LoadRecentProjectsAsync();
                
                OpenProject(path);
            }
        }

        private void MenuReveal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string path)
            {
                if (Directory.Exists(path))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
            }
        }

        private async void MenuRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string path)
            {
                var config = await ConfigService.LoadAsync();
                int removed = config.RecentProjects.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
                
                if (removed > 0)
                {
                    await ConfigService.SaveAsync(config);
                }
                
                // If the project is physically located in the default "repos" directory, 
                // it will be auto-scanned and added back. We should warn the user.
                string defaultReposPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Snapcode", "repos");
                if (path.StartsWith(defaultReposPath, StringComparison.OrdinalIgnoreCase))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Cannot Remove",
                        Content = "This project is located in the default Snapcode workspace directory, so it will always appear in the list. To remove it completely, use 'Delete Project' instead.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot,
                        RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                    };
                    await dialog.ShowAsync();
                }
                
                await LoadRecentProjectsAsync();
            }
        }

        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string path)
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Project?",
                    Content = $"Are you sure you want to permanently delete '{path}' from your disk? This cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                        
                        var config = await ConfigService.LoadAsync();
                        config.RecentProjects.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
                        await ConfigService.SaveAsync(config);
                    }
                    catch (Exception ex)
                    {
                        var errDialog = new ContentDialog
                        {
                            Title = "Error Deleting Project",
                            Content = ex.Message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot,
                            RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                        };
                        await errDialog.ShowAsync();
                    }
                    finally
                    {
                        await LoadRecentProjectsAsync();
                    }
                }
            }
        }

        private async void BtnCreateTicTacToeTemplate_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox
            {
                Text = "tic_tac_toe",
                Header = "Project Name",
                PlaceholderText = "e.g. tic_tac_toe"
            };

            string defaultReposPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Snapcode", "repos");
            Directory.CreateDirectory(defaultReposPath);

            var locationBox = new TextBox
            {
                Text = defaultReposPath,
                Header = "Project Location",
                IsReadOnly = true
            };

            var contentPanel = new StackPanel
            {
                Spacing = 12
            };
            contentPanel.Children.Add(new TextBlock 
            { 
                Text = "Create a new project pre-configured with the Tic-Tac-Toe visual widgets and Blockly event blocks.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemControlForegroundBaseMediumBrush"]
            });
            contentPanel.Children.Add(nameBox);
            contentPanel.Children.Add(locationBox);

            var dialog = new ContentDialog
            {
                Title = "Create Tic-Tac-Toe Project",
                Content = contentPanel,
                PrimaryButtonText = "Create Project",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
                RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string projectName = nameBox.Text.Trim();
                if (string.IsNullOrEmpty(projectName)) projectName = "tic_tac_toe";
                projectName = Regex.Replace(projectName.ToLowerInvariant(), @"[^a-z0-9_]", "_");

                LoadingOverlay.Visibility = Visibility.Visible;
                LogTextBlock.Text = "Creating Tic-Tac-Toe Template project...\n";

                try
                {
                    string projectRoot = Path.Combine(defaultReposPath, projectName);
                    int counter = 1;
                    string baseRoot = projectRoot;
                    while (Directory.Exists(projectRoot))
                    {
                        projectRoot = $"{baseRoot}_{counter++}";
                    }

                    LogTextBlock.Text += $"[OK] Target folder: {projectRoot}\n";
                    LogTextBlock.Text += "[...] Generating visual UI tree, Blockly blocks, and Flutter source...\n";

                    await TemplateService.CreateTicTacToeProjectAsync(projectRoot, Path.GetFileName(projectRoot));

                    LogTextBlock.Text += "[OK] Tic-Tac-Toe Template generated successfully!\n";

                    var config = await ConfigService.LoadAsync();
                    config.RecentProjects.RemoveAll(p => string.Equals(p, projectRoot, StringComparison.OrdinalIgnoreCase));
                    config.RecentProjects.Insert(0, projectRoot);
                    if (config.RecentProjects.Count > 20) config.RecentProjects.RemoveAt(20);
                    await ConfigService.SaveAsync(config);

                    await LoadRecentProjectsAsync();
                    LoadingOverlay.Visibility = Visibility.Collapsed;

                    OpenProject(projectRoot);
                }
                catch (Exception ex)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    var errDialog = new ContentDialog
                    {
                        Title = "Error Creating Template",
                        Content = $"Failed to generate template: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errDialog.ShowAsync();
                }
            }
        }

        private void BtnCreateProject_Click(object sender, RoutedEventArgs e)
        {
            var window = new NewProjectWindow();
            window.ProjectCreationRequested += OnProjectCreationRequested;
            window.Activate();
        }

        private async void OnProjectCreationRequested(string name, string org, string path, string platforms, bool useKotlin)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LogTextBlock.Text = "Initializing project structure...\n";

            string projectRoot = Path.Combine(path, name);
            string flutterDir = Path.Combine(projectRoot, ProjectService.FlutterFolderName);

            try
            {
                // Step 1: Create project root directory
                Directory.CreateDirectory(projectRoot);
                LogTextBlock.Text += $"[OK] Created project root: {projectRoot}\n";

                // Step 2: Initialize .snapcode metadata folder
                LogTextBlock.Text += "[...] Initializing .snapcode metadata...\n";
                await ProjectService.InitializeProjectAsync(projectRoot, name, org, platforms, useKotlin);
                LogTextBlock.Text += "[OK] .snapcode metadata initialized.\n";

                // Step 3: Create flutter/ subdirectory and run flutter create inside it
                Directory.CreateDirectory(flutterDir);
                LogTextBlock.Text += "[...] Running flutter create...\n";

                bool success = await CreateFlutterProjectInSubdirAsync(name, org, flutterDir, platforms, useKotlin);

                if (success)
                {
                    LogTextBlock.Text += "\n[SUCCESS] Project created successfully!\n";
                    await ProjectService.AppendBuildLogAsync(projectRoot, $"Project created: {name} ({platforms})");
                    AssetService.SyncAssetsToFlutterProject(projectRoot);

                    var config = await ConfigService.LoadAsync();
                    config.RecentProjects.RemoveAll(p => string.Equals(p, projectRoot, StringComparison.OrdinalIgnoreCase));
                    config.RecentProjects.Insert(0, projectRoot);
                    if (config.RecentProjects.Count > 20)
                        config.RecentProjects.RemoveAt(20);
                    await ConfigService.SaveAsync(config);

                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    await LoadRecentProjectsAsync();

                    // Auto-open the project in the editor
                    OpenProject(projectRoot);
                }
                else
                {
                    LogTextBlock.Text += "\n[FAILED] Flutter create failed or was canceled.\n";
                }
            }
            catch (Exception ex)
            {
                LogTextBlock.Text += $"\n[ERROR] {ex.Message}\n";
            }
        }

        private void BtnCancelCreation_Click(object sender, RoutedEventArgs e)
        {
            if (_creationProcess != null && !_creationProcess.HasExited)
            {
                try
                {
                    _creationProcess.Kill(true);
                }
                catch { }
            }
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async System.Threading.Tasks.Task<bool> CreateFlutterProjectInSubdirAsync(string name, string org, string flutterDir, string platforms, bool useKotlin)
        {
            var config = await ConfigService.LoadAsync();
            string flutterPath = string.IsNullOrEmpty(config.FlutterSdkPath) ? "flutter" : Path.Combine(config.FlutterSdkPath, "bin", "flutter.bat");

            try
            {
                // flutter create --project-name <name> --org <org> --platforms <platforms> .
                string args = $"create --project-name {name} --org {org} --platforms {platforms} ";
                if (useKotlin) args += "-a kotlin ";
                else args += "-a java ";
                args += "."; // Create in current directory (the flutter/ subfolder)

                LogTextBlock.Text += $"> {flutterPath} {args}\n";

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = flutterPath,
                    Arguments = args,
                    WorkingDirectory = flutterDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _creationProcess = new System.Diagnostics.Process { StartInfo = startInfo };
                
                _creationProcess.OutputDataReceived += (s, e) => 
                {
                    if (e.Data != null)
                    {
                        DispatcherQueue.TryEnqueue(() => LogTextBlock.Text += e.Data + "\n");
                    }
                };
                
                _creationProcess.ErrorDataReceived += (s, e) => 
                {
                    if (e.Data != null)
                    {
                        DispatcherQueue.TryEnqueue(() => LogTextBlock.Text += "[ERROR] " + e.Data + "\n");
                    }
                };

                _creationProcess.Start();
                _creationProcess.BeginOutputReadLine();
                _creationProcess.BeginErrorReadLine();

                await _creationProcess.WaitForExitAsync();
                
                return _creationProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                LogTextBlock.Text += $"\nException: {ex.Message}";
                return false;
            }
        }


        private void BtnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string path)
            {
                OpenProject(path);
            }
        }

        private void RecentProjectsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ProjectItem project && Directory.Exists(project.Path))
            {
                OpenProject(project.Path);
            }
        }

        private void OpenProject(string path)
        {
            // Update window title
            if (App.MainWindow is MainWindow mw)
            {
                mw.Title = $"SnapCode — {Path.GetFileName(path)}";
            }

            // Navigate to the block editor with the selected project
            Frame.Navigate(typeof(EditorPage), path);
        }
    }

    public class ProjectItem
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }
}
