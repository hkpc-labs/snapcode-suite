using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Snapcode.Models;
using Snapcode.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Snapcode.Views
{
    /// <summary>
    /// The block coding editor page. Hosts a WebView2 Blockly workspace
    /// and a live Dart code preview panel.
    /// </summary>
    public sealed partial class EditorPage : Page
    {
        private string _projectPath = "";
        private string _projectName = "";
        private bool _isCodePanelVisible = true;
        private bool _isWebViewReady = false;
        private string _lastGeneratedCode = "";
        private bool _hasUnsavedChanges = false;
        private string _currentPageName = "main";
        private Snapcode.Models.PageMetadata? _currentMetadata = null;
        private bool _isSwitchingPage = false;
        private string _currentEventContext = null;
        private (string WidgetId, string EventType, string WidgetType)? _pendingEventTarget = null;
        private bool _isSelectingEventProgrammatically = false;
        private DispatcherTimer _autoSaveTimer;
        private DebugLogsWindow? _standaloneDebugWindow;
        private List<FlutterDevice> _discoveredDevices = new();
        private int _deviceQueryVersion = 0;

        public EditorPage()
        {
            this.InitializeComponent();

            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;

            if (NativeDesigner != null)
            {
                NativeDesigner.WidgetTreeFlatJsonChanged += NativeDesigner_WidgetTreeFlatJsonChanged;
                NativeDesigner.UIStateChanged += NativeDesigner_UIStateChanged;
                NativeDesigner.OpenEventWorkspaceRequested += NativeDesigner_OpenEventWorkspaceRequested;
                NativeDesigner.OpenDrawerDesignerRequested += async (s, drawerPage) => await SwitchToPageAsync(drawerPage);
                NativeDesigner.SaveRequested += NativeDesigner_SaveRequested;
            }

            if (ProjectExplorer != null)
            {
                ProjectExplorer.FileOpened += async (s, filePath) =>
                {
                    if (MonacoEditor != null)
                    {
                        await MonacoEditor.OpenFileAsync(filePath);
                    }
                };
            }

            if (DebugConsole != null)
            {
                DebugConsole.CloseRequested += (s, e) => ToggleDebugConsole(false);
                DebugConsole.PopOutRequested += (s, e) => BtnPopOutConsole_Click(s, new RoutedEventArgs());
            }

            // Bind global & Flutter actions to ShortcutManager
            var sm = Snapcode.Services.ShortcutManager.Instance;
            sm.BindAction("file.save", () => DispatcherQueue.TryEnqueue(async () =>
            {
                if (EditorTabs?.SelectedItem is NavigationViewItem navItem && navItem.Tag?.ToString() == "code")
                {
                    if (MonacoEditor != null)
                    {
                        await MonacoEditor.SaveCurrentFileAsync();
                    }
                    return;
                }
                await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
            }));
            sm.BindAction("page.next", () => DispatcherQueue.TryEnqueue(() => NavigateNextPage()));
            sm.BindAction("page.prev", () => DispatcherQueue.TryEnqueue(() => NavigatePreviousPage()));
            sm.BindAction("general.settings", () => DispatcherQueue.TryEnqueue(() => { var sw = new SettingsWindow(); sw.Activate(); }));
            sm.BindAction("flutter.run", () => DispatcherQueue.TryEnqueue(() => BtnRun_Click(this, new RoutedEventArgs())));
            sm.BindAction("flutter.hot_reload", () => DispatcherQueue.TryEnqueue(async () => await Snapcode.Services.FlutterExecutionService.Instance.HotReloadAsync()));
            sm.BindAction("flutter.hot_restart", () => DispatcherQueue.TryEnqueue(async () => await Snapcode.Services.FlutterExecutionService.Instance.HotRestartAsync()));
            sm.BindAction("flutter.stop", () => DispatcherQueue.TryEnqueue(async () => await Snapcode.Services.FlutterExecutionService.Instance.StopAsync()));
            sm.BindAction("flutter.clean", () => DispatcherQueue.TryEnqueue(() => BtnClearBlocks_Click(this, new RoutedEventArgs())));
            sm.BindAction("flutter.build_apk", () => DispatcherQueue.TryEnqueue(async () => await BuildApkShortcutAsync()));

            this.ActualThemeChanged += (s, e) => UpdateQuickPageNavigationActiveState();
            this.Loaded += (s, e) => UpdateQuickPageNavigationActiveState();
        }

        private void Page_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(this.XamlRoot);
            bool isTextEditing = focused is TextBox || focused is PasswordBox || focused is RichEditBox;

            var mods = Snapcode.Services.ShortcutManager.GetCurrentModifiers();

            if (isTextEditing)
            {
                if (Snapcode.Services.ShortcutManager.IsStandardTextEditingShortcut(e.Key, mods))
                {
                    return;
                }
            }

            // Quick Page Shift: Ctrl+Tab, Ctrl+Shift+Tab, Ctrl+PageDown, Ctrl+PageUp
            if ((mods & Windows.System.VirtualKeyModifiers.Control) != 0 && (e.Key == Windows.System.VirtualKey.Tab || e.Key == Windows.System.VirtualKey.PageDown))
            {
                e.Handled = true;
                if ((mods & Windows.System.VirtualKeyModifiers.Shift) != 0) NavigatePreviousPage();
                else NavigateNextPage();
                return;
            }
            if ((mods & Windows.System.VirtualKeyModifiers.Control) != 0 && e.Key == Windows.System.VirtualKey.PageUp)
            {
                e.Handled = true;
                NavigatePreviousPage();
                return;
            }

            string currentScope = "Global";
            if (EditorTabs?.SelectedItem is NavigationViewItem navItem)
            {
                if (navItem.Tag?.ToString() == "view") currentScope = "Designer";
                else if (navItem.Tag?.ToString() == "code") currentScope = "Editor";
            }

            if (Snapcode.Services.ShortcutManager.Instance.TryExecute(e.Key, mods, currentScope))
            {
                e.Handled = true;
            }
        }

        private async System.Threading.Tasks.Task BuildApkShortcutAsync()
        {
            try
            {
                var exec = Snapcode.Services.FlutterExecutionService.Instance;
                exec.AddLog("Starting Flutter APK build (debug mode)...", Models.LogEntryType.System);

                string flutterDir = System.IO.Path.Combine(_projectPath, Services.ProjectService.FlutterFolderName);
                if (!System.IO.Directory.Exists(flutterDir)) flutterDir = _projectPath;

                var config = await Services.ConfigService.LoadAsync();
                string flutterExe = !string.IsNullOrEmpty(config.FlutterSdkPath)
                    ? System.IO.Path.Combine(config.FlutterSdkPath, "bin", "flutter.bat")
                    : "flutter";

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = "build apk --debug",
                    WorkingDirectory = flutterDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var proc = System.Diagnostics.Process.Start(psi);
                if (proc != null)
                {
                    proc.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) exec.AddLog(e.Data, Models.LogEntryType.Info); };
                    proc.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) exec.AddLog(e.Data, Models.LogEntryType.Warning); };
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }
            }
            catch (Exception ex)
            {
                Snapcode.Services.FlutterExecutionService.Instance.AddLog($"Build APK error: {ex.Message}", Models.LogEntryType.Error);
            }
        }

        private async void AutoSaveTimer_Tick(object sender, object e)
        {
            _autoSaveTimer.Stop();
            if (_hasUnsavedChanges)
            {
                await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
            }
        }

        private async void NativeDesigner_SaveRequested(object sender, EventArgs e)
        {
            _autoSaveTimer.Stop();
            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
        }

        private async void GlobalSave_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            _autoSaveTimer.Stop();

            if (EditorTabs?.SelectedItem is NavigationViewItem navItem && navItem.Tag?.ToString() == "code")
            {
                if (MonacoEditor != null)
                {
                    await MonacoEditor.SaveCurrentFileAsync();
                }
                return;
            }

            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
        }

        private async void NativeDesigner_OpenEventWorkspaceRequested(object sender, (string WidgetId, string EventType) args)
        {
            // Resolve widget type
            string widgetType = "";
            if (NativeDesigner?.AllWidgetsFlat != null)
            {
                var matching = NativeDesigner.AllWidgetsFlat.FirstOrDefault(w => w.Id == args.WidgetId);
                if (matching != null) widgetType = matching.Type ?? "";
            }

            _pendingEventTarget = (args.WidgetId, args.EventType, widgetType);

            // Switch the tab to the "event" pane
            foreach (var item in EditorTabs.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "event")
                {
                    if (EditorTabs.SelectedItem == navItem)
                    {
                        // Already on event tab, execute transition immediately
                        await HandleEventTabNavigationAsync();
                    }
                    else
                    {
                        // Switching tab triggers EditorTabs_SelectionChanged which invokes HandleEventTabNavigationAsync
                        EditorTabs.SelectedItem = navItem;
                    }
                    break;
                }
            }
        }

        private async Task HandleEventTabNavigationAsync()
        {
            PopulateEventList();

            if (_pendingEventTarget.HasValue)
            {
                var target = _pendingEventTarget.Value;
                _pendingEventTarget = null;

                // Highlight matching item in right drawer (ListEvents) without re-triggering selection event
                if (ListEvents?.ItemsSource is System.Collections.ObjectModel.ObservableCollection<EventListItem> list)
                {
                    var matched = list.FirstOrDefault(x => x.WidgetId == target.WidgetId && string.Equals(x.EventType, target.EventType, StringComparison.OrdinalIgnoreCase));
                    if (matched != null)
                    {
                        _isSelectingEventProgrammatically = true;
                        try
                        {
                            ListEvents.SelectedItem = matched;
                        }
                        finally
                        {
                            _isSelectingEventProgrammatically = false;
                        }
                    }
                }

                await SwitchToEventWorkspaceAsync(target.WidgetId, target.EventType, target.WidgetType);
            }
            else
            {
                // Manual click on "Events" navigation item: load Overview workspace
                _currentEventContext = null;
                if (ListEvents?.ItemsSource is System.Collections.ObjectModel.ObservableCollection<EventListItem> list && list.Count > 0)
                {
                    _isSelectingEventProgrammatically = true;
                    try
                    {
                        ListEvents.SelectedIndex = 0; // Select Overview
                    }
                    finally
                    {
                        _isSelectingEventProgrammatically = false;
                    }
                }

                await LoadSavedWorkspaceAsync(loadNativeUi: false);
            }

            await PushWidgetListToBlocklyAsync();
        }

        
        private void NativeDesigner_UIStateChanged(object sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusSaveIcon.Glyph = ""; // Unsaved dot
                TxtSaveStatus.Text = "Saving...";
            });

            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
        }

        private async void NativeDesigner_WidgetTreeFlatJsonChanged(object sender, string jsonList)
        {
            await PushWidgetListToBlocklyAsync(jsonList);
        }

        private async Task PushWidgetListToBlocklyAsync(string? explicitJson = null)
        {
            if (!_isWebViewReady || BlocklyWebView?.CoreWebView2 == null) return;
            try
            {
                if (!string.IsNullOrEmpty(explicitJson))
                {
                    string safeExplicit = JsonSerializer.Serialize(explicitJson);
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.updateWidgetIds({safeExplicit})");
                    return;
                }

                List<object> simpleList = new List<object>();
                if (NativeDesigner != null && NativeDesigner.AllWidgetsFlat != null && NativeDesigner.AllWidgetsFlat.Count > 0)
                {
                    simpleList = NativeDesigner.AllWidgetsFlat.Select(w => new { id = w.Id, type = w.Type } as object).ToList();
                }
                else if (!string.IsNullOrEmpty(_projectPath) && !string.IsNullOrEmpty(_currentPageName))
                {
                    string uiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
                    if (!string.IsNullOrWhiteSpace(uiJson) && uiJson != "[]" && uiJson != "{}")
                    {
                        using var doc = JsonDocument.Parse(uiJson);
                        var flat = new List<object>();
                        SnapCode.Services.FlutterCompilerService.TraverseWidgetTree(doc.RootElement, el =>
                        {
                            string id = el.TryGetProperty("id", out var p) ? p.GetString() ?? "" : "";
                            string type = el.TryGetProperty("type", out var tp) ? tp.GetString() ?? "" : "";
                            if (!string.IsNullOrEmpty(id))
                            {
                                flat.Add(new { id = id, type = type });
                            }
                        });
                        simpleList = flat;
                    }
                }

                if (simpleList.Count > 0)
                {
                    string json = JsonSerializer.Serialize(simpleList);
                    string safeJson = JsonSerializer.Serialize(json);
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.updateWidgetIds({safeJson})");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] PushWidgetListToBlocklyAsync error: {ex.Message}");
            }
        }

        private async Task PushProjectPagesToBlocklyAsync()
        {
            if (!_isWebViewReady || BlocklyWebView?.CoreWebView2 == null || string.IsNullOrEmpty(_projectPath)) return;
            try
            {
                var pages = ProjectService.GetProjectPages(_projectPath);
                var visualPages = pages.Where(p => ProjectFileClassifier.IsVisualPage(_projectPath, p)).ToList();
                if (visualPages.Count == 0) visualPages.Add("main");
                string json = JsonSerializer.Serialize(visualPages);
                string safeJson = JsonSerializer.Serialize(json);
                await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.updateProjectPages({safeJson})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] PushProjectPagesToBlocklyAsync error: {ex.Message}");
            }
        }

        private bool _wasMaximizedBeforeEntering = false;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.MainWindow != null)
            {
                App.MainWindow.AppWindow.Closing += AppWindow_Closing;
                
                // Maximize the window when opening the editor, but remember previous state
                if (App.MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    _wasMaximizedBeforeEntering = presenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized;
                    if (!_wasMaximizedBeforeEntering)
                    {
                        presenter.Maximize();
                    }
                }
            }

            if (e.Parameter is string projectPath)
            {
                _projectPath = projectPath;
                _projectName = Path.GetFileName(projectPath);

                if (NativeDesigner != null) NativeDesigner.ProjectRoot = _projectPath;

                // Load project metadata
                var project = await ProjectService.LoadProjectAsync(projectPath);
                if (project != null)
                {
                    TxtProjectName.Text = project.Name;
                    TxtProjectOrg.Text = project.Organization;
                    
                    // Ensure Web, Android, and Windows targets are all available and visible
                    ComboItemWeb.Visibility = Visibility.Visible;
                    ComboItemAndroid.Visibility = Visibility.Visible;
                    ComboItemWindows.Visibility = Visibility.Visible;

                    // Automatically select default platform based on project metadata or host OS
                    if (project.Platforms != null)
                    {
                        if (project.Platforms.Contains("windows", StringComparison.OrdinalIgnoreCase) &&
                            System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                        {
                            ComboTarget.SelectedItem = ComboItemWindows;
                        }
                        else if (project.Platforms.Contains("android", StringComparison.OrdinalIgnoreCase))
                        {
                            ComboTarget.SelectedItem = ComboItemAndroid;
                        }
                        else if (project.Platforms.Contains("web", StringComparison.OrdinalIgnoreCase))
                        {
                            ComboTarget.SelectedItem = ComboItemWeb;
                        }
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        ComboTarget.SelectedItem = ComboItemWindows;
                    }
                }
                else
                {
                    // Legacy project (no .snapcode folder)
                    TxtProjectName.Text = _projectName;
                    TxtProjectOrg.Text = "";
                    ComboItemWeb.Visibility = Visibility.Visible;
                    ComboItemAndroid.Visibility = Visibility.Visible;
                    ComboItemWindows.Visibility = Visibility.Visible;
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        ComboTarget.SelectedItem = ComboItemWindows;
                    }
                }

                // Populate Pages (ensuring pure custom Dart files never appear here)
                var pages = ProjectService.GetProjectPages(projectPath);
                _isSwitchingPage = true;
                ListPages.Items.Clear();
                foreach (var p in pages)
                {
                    if (ProjectFileClassifier.IsVisualPage(projectPath, p))
                    {
                        ListPages.Items.Add(p);
                    }
                }
                _currentPageName = "main";
                ListPages.SelectedItem = _currentPageName;
                _isSwitchingPage = false;
                RefreshQuickPageNavigation();

                // Immediately load Native UI state for the current page so widgets render on canvas instantly
                try
                {
                    string uiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
                    if (NativeDesigner != null)
                    {
                        _currentMetadata = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName);
                        NativeDesigner.ApplyPageMetadata(_currentMetadata);
                        NativeDesigner.SetPageName(_currentPageName);
                        NativeDesigner.LoadUIJson(uiJson);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to load initial UI state: {ex.Message}");
                }

                await InitializeWebViewAsync();

                FlutterExecutionService.Instance.Initialize(this.DispatcherQueue);
                FlutterExecutionService.Instance.StateChanged += ExecutionService_StateChanged;
                _ = PopulateDevicesAsync(GetSelectedTarget());
            }
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow != null)
            {
                App.MainWindow.SetTitleBar(AppTitleBar);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            FlutterExecutionService.Instance.StateChanged -= ExecutionService_StateChanged;
            if (App.MainWindow != null)
            {
                App.MainWindow.SetTitleBar(null);
                App.MainWindow.AppWindow.Closing -= AppWindow_Closing;

                // Restore window state if we maximized it
                if (!_wasMaximizedBeforeEntering && App.MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    presenter.Restore();
                }
            }

            if (NativeDesigner != null)
            {
                NativeDesigner.WidgetTreeFlatJsonChanged -= NativeDesigner_WidgetTreeFlatJsonChanged;
                NativeDesigner.UIStateChanged -= NativeDesigner_UIStateChanged;
                NativeDesigner.OpenEventWorkspaceRequested -= NativeDesigner_OpenEventWorkspaceRequested;
                NativeDesigner.SaveRequested -= NativeDesigner_SaveRequested;
            }

            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
            }
        }

        private bool _isForceClosing = false;

        private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            if (_hasUnsavedChanges && !_isForceClosing)
            {
                args.Cancel = true;
                DispatcherQueue.TryEnqueue(async () => await PromptSaveOnCloseAsync());
            }
        }

        private async Task PromptSaveOnCloseAsync()
        {
            var dialog = new ContentDialog
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved changes. Would you like to save before closing?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
                RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await SaveWorkspaceAsync();
                _isForceClosing = true;
                App.MainWindow?.Close();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                _isForceClosing = true;
                App.MainWindow?.Close();
            }
            // Cancel: do nothing
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // Determine the theme first so event handlers can reference it
                string themeMode = "dark";
                try
                {
                    var config = await ConfigService.LoadAsync();
                    if (config.AppTheme == "Light")
                        themeMode = "light";
                    else if (config.AppTheme == "System Default")
                    {
                        // Check actual system theme
                        if (this.ActualTheme == ElementTheme.Light ||
                            (this.ActualTheme == ElementTheme.Default && 
                             Application.Current.RequestedTheme == ApplicationTheme.Light))
                            themeMode = "light";
                    }
                }
                catch { /* Default to dark */ }

                // Initialize WebView2
                await BlocklyWebView.EnsureCoreWebView2Async();
                
                // Set up message handler for JS → C# communication
                BlocklyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // Navigation completed handler for error catching & waking up workspace
                BlocklyWebView.CoreWebView2.NavigationCompleted += (s, navArgs) =>
                {
                    if (!navArgs.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BlockEditor] Navigation failed: {navArgs.WebErrorStatus}");
                        string fallbackPath = Path.Combine(AppContext.BaseDirectory, "Assets", "BlockEditor", "index.html");
                        if (!File.Exists(fallbackPath))
                        {
                            fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "BlockEditor", "index.html");
                        }
                        if (File.Exists(fallbackPath))
                        {
                            var fallbackUri = new Uri(fallbackPath).AbsoluteUri + $"?theme={themeMode}";
                            if (BlocklyWebView.Source?.AbsoluteUri != fallbackUri)
                            {
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    BlocklyWebView.CoreWebView2.Navigate(fallbackUri);
                                });
                                return;
                            }
                        }

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            BlocklyWebView.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        // Navigation succeeded! Ensure Blockly wakes up and resizes
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            try
                            {
                                await Task.Delay(150);
                                await BlocklyWebView.CoreWebView2.ExecuteScriptAsync(
                                    "if (typeof notifyReady === 'function') notifyReady(); if (window.resizeWorkspace) window.resizeWorkspace();");
                            }
                            catch { }
                        });
                    }
                };

                if (MonacoEditor != null)
                {
                    MonacoEditor.SetTheme(themeMode);
                }

                // Load the Blockly HTML via direct file URI (rock-solid, zero external DNS/SSL dependency, same as Monaco Editor)
                string blockEditorDir = Path.Combine(AppContext.BaseDirectory, "Assets", "BlockEditor");
                string indexPath = Path.Combine(blockEditorDir, "index.html");
                if (!File.Exists(indexPath))
                {
                    string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "BlockEditor", "index.html");
                    if (File.Exists(altPath))
                    {
                        indexPath = altPath;
                        blockEditorDir = Path.GetDirectoryName(altPath)!;
                    }
                }

                if (File.Exists(indexPath))
                {
                    var fileUri = new Uri(indexPath).AbsoluteUri + $"?theme={themeMode}";
                    BlocklyWebView.CoreWebView2.Navigate(fileUri);
                }
                else
                {
                    EditorLoadingPanel.Visibility = Visibility.Collapsed;
                    BlocklyWebView.Visibility = Visibility.Visible;
                }

                // Safety timeout guard: If WebView2 or Blockly doesn't send "ready" within 4 seconds,
                // collapse loading panel and show editor so the UI never spins indefinitely.
                _ = Task.Run(async () =>
                {
                    await Task.Delay(4000);
                    if (!_isWebViewReady)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            BlocklyWebView.Visibility = Visibility.Visible;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                EditorLoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                string messageJson = args.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(messageJson)) return;

                using var doc = JsonDocument.Parse(messageJson);
                var root = doc.RootElement;

                string type = root.GetProperty("type").GetString() ?? "";

                switch (type)
                {
                    case "ready":
                        _isWebViewReady = true;
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            BlocklyWebView.Visibility = Visibility.Visible;

                            // Push latest widget IDs to Blockly immediately
                            await PushWidgetListToBlocklyAsync();

                            // Load saved workspace if it exists
                            await LoadSavedWorkspaceAsync();
                            
                            // Load custom blocks from Sketchware JSON
                            await LoadSketchwareBlocksAsync();
                        });
                        break;

                    case "error":
                        string errMsg = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
                        System.Diagnostics.Debug.WriteLine($"[BlockEditor JS Error] {errMsg}");
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            BlocklyWebView.Visibility = Visibility.Visible;
                        });
                        break;

                    case "codeGenerated":
                        string code = root.GetProperty("data").GetString() ?? "";
                        _lastGeneratedCode = code;
                        _hasUnsavedChanges = true;

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            StatusSaveIcon.Glyph = ""; // Unsaved dot
                            TxtSaveStatus.Text = "Unsaved changes";

                            // Update block count
                            if (_isWebViewReady)
                            {
                                try
                                {
                                    string countStr = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getBlockCount()");
                                    TxtBlockCount.Text = $"{countStr} blocks";
                                }
                                catch { }
                            }
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] WebMessage error: {ex.Message}");
            }
        }

        private async Task LoadSavedWorkspaceAsync(bool loadNativeUi = true)
        {
            if (!_isWebViewReady || string.IsNullOrEmpty(_projectPath)) return;

            try
            {
                string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, _currentPageName);
                
                var metadata = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName);
                string pageType = metadata?.Type?.ToLower() ?? "page";
                if (pageType == "custom view") pageType = "custom_view";
                
                await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.setWorkspaceContext('{pageType}')");
                await PushProjectPagesToBlocklyAsync();

                if (workspaceJson != "{}" && !string.IsNullOrWhiteSpace(workspaceJson))
                {
                    string safeJson = JsonSerializer.Serialize(workspaceJson);
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadWorkspace({safeJson})");
                    DispatcherQueue?.TryEnqueue(() =>
                    {
                        TxtSaveStatus.Text = "Loaded";
                    });
                }
                else
                {
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.loadWorkspace('{}')");
                    DispatcherQueue?.TryEnqueue(() =>
                    {
                        TxtSaveStatus.Text = "Loaded";
                    });
                }
                _hasUnsavedChanges = false;
                
                // Load Native UI state only when requested (e.g. project load or page change, NOT on tab switch)
                if (loadNativeUi && (NativeDesigner == null || NativeDesigner.CanvasItems.Count == 0))
                {
                    try
                    {
                        string uiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
                        if (NativeDesigner != null)
                        {
                            NativeDesigner.SetPageName(_currentPageName);
                            NativeDesigner.LoadUIJson(uiJson);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to inject UI state: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to load workspace: {ex.Message}");
            }
        }

        private async Task LoadSketchwareBlocksAsync()
        {
            if (!_isWebViewReady) return;
            try
            {
                string basePath = Path.Combine(AppContext.BaseDirectory, "Assets", "BlockEditor", "custom_blocks");
                string palettePath = Path.Combine(basePath, "flutter_palette.json");
                string blockPath = Path.Combine(basePath, "flutter_blocks.json");

                if (File.Exists(palettePath) && File.Exists(blockPath))
                {
                    string paletteJson = await File.ReadAllTextAsync(palettePath);
                    string blockJson = await File.ReadAllTextAsync(blockPath);

                    string safePalette = JsonSerializer.Serialize(paletteJson);
                    string safeBlock = JsonSerializer.Serialize(blockJson);

                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.parseSketchwareBlocks({safePalette}, {safeBlock})");
                }
            }
            catch (Exception)
            {
            }
        }

        private System.Threading.SemaphoreSlim _saveLock = new System.Threading.SemaphoreSlim(1, 1);

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTabs?.SelectedItem is NavigationViewItem navItem && navItem.Tag?.ToString() == "code")
            {
                if (MonacoEditor != null)
                {
                    await MonacoEditor.SaveCurrentFileAsync();
                }
                return;
            }

            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
        }

        private async Task SaveWorkspaceAsync(
            string? overridePageName = null, 
            string? overrideEventContext = null,
            string? overrideUiJson = null,
            string? overrideWorkspaceJson = null,
            string? overrideActiveDartCode = null)
        {
            if (string.IsNullOrEmpty(_projectPath)) return;

            string pageToSave = overridePageName ?? _currentPageName;
            string contextToSave = overrideEventContext ?? _currentEventContext;

            await _saveLock.WaitAsync();
            try
            {
                string currentActiveCode = overrideActiveDartCode;
                if (!string.IsNullOrEmpty(overrideWorkspaceJson))
                {
                    string workspaceFile = contextToSave != null ? $"{pageToSave}_{contextToSave}" : pageToSave;
                    await ProjectService.SavePageWorkspaceAsync(_projectPath, workspaceFile, overrideWorkspaceJson);

                    if (currentActiveCode != null)
                    {
                        await ProjectService.SavePageEventCodeAsync(_projectPath, workspaceFile, currentActiveCode);
                    }
                }
                else if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null && (DispatcherQueue?.HasThreadAccess ?? false))
                {
                    try
                    {
                        string workspaceJson = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getWorkspace()");

                        if (workspaceJson.StartsWith("\"") && workspaceJson.EndsWith("\""))
                        {
                            workspaceJson = JsonSerializer.Deserialize<string>(workspaceJson) ?? "{}";
                        }

                        string workspaceFile = contextToSave != null ? $"{pageToSave}_{contextToSave}" : pageToSave;
                        await ProjectService.SavePageWorkspaceAsync(_projectPath, workspaceFile, workspaceJson);

                        // Query window.getGeneratedCode() from WebView2 to get the freshest Dart code
                        string rawCode = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getGeneratedCode()");
                        if (!string.IsNullOrEmpty(rawCode) && rawCode != "null")
                        {
                            if (rawCode.StartsWith("\"") && rawCode.EndsWith("\""))
                            {
                                try
                                {
                                    currentActiveCode = JsonSerializer.Deserialize<string>(rawCode) ?? string.Empty;
                                }
                                catch
                                {
                                    currentActiveCode = rawCode;
                                }
                            }
                            else
                            {
                                currentActiveCode = rawCode;
                            }
                            _lastGeneratedCode = currentActiveCode;
                        }

                        // Save current workspace event code via ProjectService.SavePageEventCodeAsync
                        if (currentActiveCode != null)
                        {
                            await ProjectService.SavePageEventCodeAsync(_projectPath, workspaceFile, currentActiveCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to save Blockly workspace: {ex.Message}");
                    }
                }

                // Also scan .snapcode/pages/ for any ${pageToSave}_*.json event files that don't have .event.dart yet,
                // compile them via window.generateCodeForWorkspaceJson, and save them.
                try
                {
                    string pagesDir = ProjectService.GetPagesDirectory(_projectPath);
                    if (Directory.Exists(pagesDir))
                    {
                        var eventJsonFiles = Directory.GetFiles(pagesDir, $"{pageToSave}_*.json");
                        foreach (var jsonFile in eventJsonFiles)
                        {
                            string baseName = Path.GetFileNameWithoutExtension(jsonFile);
                            if (baseName.EndsWith("_ui", StringComparison.OrdinalIgnoreCase) ||
                                baseName.EndsWith("_meta", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            string targetEventDart = Path.Combine(pagesDir, $"{baseName}.event.dart");
                            bool needsCompile = !File.Exists(targetEventDart) || 
                                                File.GetLastWriteTimeUtc(jsonFile) > File.GetLastWriteTimeUtc(targetEventDart);
                            if (needsCompile && _isWebViewReady && BlocklyWebView?.CoreWebView2 != null && (DispatcherQueue?.HasThreadAccess ?? false))
                            {
                                string jsonContent = await File.ReadAllTextAsync(jsonFile);
                                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{}")
                                {
                                    string safeArg = JsonSerializer.Serialize(jsonContent);
                                    string compiledCode = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.generateCodeForWorkspaceJson({safeArg})");
                                    if (!string.IsNullOrEmpty(compiledCode) && compiledCode != "null")
                                    {
                                        string dartCode = compiledCode;
                                        if (compiledCode.StartsWith("\"") && compiledCode.EndsWith("\""))
                                        {
                                            try
                                            {
                                                dartCode = JsonSerializer.Deserialize<string>(compiledCode) ?? string.Empty;
                                            }
                                            catch
                                            {
                                                dartCode = compiledCode;
                                            }
                                        }
                                        await ProjectService.SavePageEventCodeAsync(_projectPath, baseName, dartCode);
                                    }
                                }
                            }
                        }

                        // Clean up any orphaned contextual .event.dart files that have no matching .json file
                        var eventDartFiles = Directory.GetFiles(pagesDir, $"{pageToSave}_*.event.dart");
                        foreach (var dartFile in eventDartFiles)
                        {
                            string fName = Path.GetFileName(dartFile);
                            // Strip .event.dart
                            string baseName = fName.Substring(0, fName.Length - ".event.dart".Length);
                            string matchingJson = Path.Combine(pagesDir, $"{baseName}.json");
                            if (!File.Exists(matchingJson))
                            {
                                try
                                {
                                    File.Delete(dartFile);
                                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Cleaned up orphaned event file without json: {dartFile}");
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Could not delete orphaned event file: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed compiling pending event files: {ex.Message}");
                }

                string savedUiJson = overrideUiJson;
                if (savedUiJson == null && NativeDesigner != null && (DispatcherQueue?.HasThreadAccess ?? false))
                {
                    string uiJson = NativeDesigner.GetUIJson();
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] NativeDesigner.GetUIJson() returned: {uiJson}");
                    if (!string.IsNullOrEmpty(uiJson) && uiJson != "null")
                    {
                        if (uiJson.StartsWith("\"") && uiJson.EndsWith("\""))
                        {
                            try
                            {
                                uiJson = JsonSerializer.Deserialize<string>(uiJson) ?? "{}";
                                System.Diagnostics.Debug.WriteLine($"[EditorPage] Deserialized double-escaped UI JSON: {uiJson}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to deserialize double-escaped UI JSON: {ex.Message}");
                            }
                        }
                        
                        // Prevent accidental wipeout if designer hasn't loaded canvas items yet
                        bool isCanvasEmpty = NativeDesigner.CanvasItems == null || NativeDesigner.CanvasItems.Count == 0;
                        if (!isCanvasEmpty || _hasUnsavedChanges)
                        {
                            savedUiJson = uiJson;
                            await ProjectService.SavePageUIAsync(_projectPath, pageToSave, uiJson);
                            System.Diagnostics.Debug.WriteLine($"[EditorPage] Saved UI JSON for '{pageToSave}': {uiJson}");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(savedUiJson))
                {
                    await ProjectService.SavePageUIAsync(_projectPath, pageToSave, savedUiJson);
                }

                if (string.IsNullOrEmpty(savedUiJson) || savedUiJson == "[]" || savedUiJson == "{}")
                {
                    try
                    {
                        string existingUi = await ProjectService.LoadPageUIAsync(_projectPath, pageToSave);
                        if (!string.IsNullOrEmpty(existingUi) && existingUi != "[]" && existingUi != "{}")
                        {
                            savedUiJson = existingUi;
                            System.Diagnostics.Debug.WriteLine($"[EditorPage] Loaded existing UI JSON fallback for '{pageToSave}': {savedUiJson}");
                        }
                    }
                    catch { }
                }

                // Compile complete Flutter Dart page combining visual UI widgets and event blocks
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Generating complete Flutter page for '{pageToSave}' with UI JSON: {savedUiJson}");
                    var pageMeta = await ProjectService.LoadPageMetadataAsync(_projectPath, pageToSave);
                    string combinedEventCode = await ProjectService.LoadAllPageEventCodeAsync(
                        _projectPath,
                        pageToSave,
                        currentActiveCode ?? _lastGeneratedCode ?? string.Empty,
                        contextToSave
                    );

                    string completeDartSource = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart(
                        pageToSave,
                        !string.IsNullOrEmpty(savedUiJson) ? savedUiJson : "[]",
                        combinedEventCode,
                        pageMeta
                    );

                    bool isDrawerPage = (pageMeta?.Type == "Drawer") || pageToSave.EndsWith("_drawer", StringComparison.OrdinalIgnoreCase);
                    string fileName = pageToSave.Equals("main", StringComparison.OrdinalIgnoreCase) 
                        ? "main.dart" 
                        : $"{pageToSave.ToLowerInvariant()}.dart";

                    string flutterLibPath = Path.Combine(_projectPath, ProjectService.FlutterFolderName, "lib");
                    if (Directory.Exists(flutterLibPath))
                    {
                        string targetDir = isDrawerPage ? Path.Combine(flutterLibPath, "models", "drawers") : flutterLibPath;
                        Directory.CreateDirectory(targetDir);
                        string dartFile = Path.Combine(targetDir, fileName);
                        await UserCodePreservationService.SaveDartFileWithPreservationAsync(dartFile, completeDartSource);
                    }

                    string legacyLibPath = Path.Combine(_projectPath, "lib");
                    if (Directory.Exists(legacyLibPath))
                    {
                        string targetDir = isDrawerPage ? Path.Combine(legacyLibPath, "models", "drawers") : legacyLibPath;
                        Directory.CreateDirectory(targetDir);
                        string dartFile = Path.Combine(targetDir, fileName);
                        await UserCodePreservationService.SaveDartFileWithPreservationAsync(dartFile, completeDartSource);
                    }

                    // If saving a normal page that has a Drawer, ensure its companion drawer exists and is compiled
                    if (pageMeta?.HasDrawer == true && !isDrawerPage)
                    {
                        _ = Task.Run(async () =>
                        {
                            await ProjectService.EnsureDrawerForPageAsync(_projectPath, pageToSave);
                        });
                    }

                    // Sync assets to flutter/assets and pubspec.yaml
                    AssetService.SyncAssetsToFlutterProject(_projectPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed generating page Dart code: {ex.Message}");
                }

                _hasUnsavedChanges = false;
                DispatcherQueue?.TryEnqueue(() =>
                {
                    StatusSaveIcon.Glyph = "\uE73E";
                    TxtSaveStatus.Text = $"Saved at {DateTime.Now:HH:mm:ss}";
                });

                if (FlutterExecutionService.Instance.IsRunning)
                {
                    _ = FlutterExecutionService.Instance.HotReloadAsync();
                }

                await ProjectService.AppendBuildLogAsync(_projectPath, $"Workspace {pageToSave} saved successfully.");
            }
            catch (Exception ex)
            {
                DispatcherQueue?.TryEnqueue(() =>
                {
                    StatusSaveIcon.Glyph = "\uEA39";
                    TxtSaveStatus.Text = $"Save failed: {ex.Message}";
                });
            }
            finally
            {
                _saveLock.Release();
            }
        }

        private void BtnToggleCode_Click(object sender, RoutedEventArgs e)
        {
            _isCodePanelVisible = !_isCodePanelVisible;
            if (_isCodePanelVisible)
            {
                ColCodePreview.Width = new GridLength(350);
                EventNavigatorPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ColCodePreview.Width = new GridLength(0);
                EventNavigatorPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSettingsQuick_Click(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => 
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Activate();
            });
        }

        private async void BtnClearBlocks_Click(object sender, RoutedEventArgs e)
        {
            if (!_isWebViewReady) return;

            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Clear All Blocks?",
                    Content = "This will remove all blocks from the workspace. This action cannot be undone.",
                    PrimaryButtonText = "Clear",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.clearWorkspace()");
                    TxtBlockCount.Text = "0 blocks";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] BtnClearBlocks_Click failed: {ex.Message}");
            }
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var execService = FlutterExecutionService.Instance;
                if (execService.IsRunning)
                {
                    await execService.StopAsync();
                    return;
                }

                // Save first so code is compiled
                await SaveWorkspaceAsync();

                FlutterDevice targetDevice;
                if (ComboDevice?.SelectedItem is ComboBoxItem item && item.Tag is FlutterDevice dev)
                {
                    targetDevice = dev;
                }
                else
                {
                    string targetPlatform = GetSelectedTarget();
                    if (targetPlatform == "windows")
                    {
                        targetDevice = _discoveredDevices.FirstOrDefault(d => d.Platform == DevicePlatform.Windows)
                            ?? new FlutterDevice { Id = "windows", Name = "Windows (Desktop)", Platform = DevicePlatform.Windows, IsRunning = true };
                    }
                    else if (targetPlatform == "android")
                    {
                        targetDevice = _discoveredDevices.FirstOrDefault(d => d.Platform == DevicePlatform.Android)
                            ?? new FlutterDevice { Id = "android", Name = "Android Device / Emulator", Platform = DevicePlatform.Android, IsEmulator = true };
                    }
                    else
                    {
                        targetDevice = _discoveredDevices.FirstOrDefault(d => d.Platform == DevicePlatform.Web)
                            ?? new FlutterDevice { Id = "chrome", Name = "Chrome", Platform = DevicePlatform.Web };
                    }
                }

                if (TxtTargetInfo != null)
                {
                    TxtTargetInfo.Text = $"Target: {targetDevice.DisplayName}";
                }

                // Open debug console drawer
                ToggleDebugConsole(true);

                await execService.StartRunAsync(_projectPath, targetDevice);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] BtnRun_Click error: {ex.Message}");
            }
        }

        private void ExecutionService_StateChanged(object? sender, ExecutionState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                bool isRunning = state != ExecutionState.Idle;
                if (IconRunQuick != null && TxtRunQuick != null)
                {
                    if (isRunning)
                    {
                        IconRunQuick.Glyph = "\uE71A"; // Stop
                        TxtRunQuick.Text = "Stop";
                        BtnRunQuick.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54));
                    }
                    else
                    {
                        IconRunQuick.Glyph = "\uE768"; // Start
                        TxtRunQuick.Text = "Start";
                        if (Application.Current.Resources.TryGetValue("AccentFillColorDefaultBrush", out var brushObj) && brushObj is Microsoft.UI.Xaml.Media.Brush accentBrush)
                        {
                            BtnRunQuick.Background = accentBrush;
                        }
                        else if (Application.Current.Resources.TryGetValue("SystemAccentColor", out var colorObj) && colorObj is Windows.UI.Color accentColor)
                        {
                            BtnRunQuick.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(accentColor);
                        }
                    }
                }

                if (BtnHotReloadQuick != null)
                    BtnHotReloadQuick.IsEnabled = isRunning && state != ExecutionState.Starting && state != ExecutionState.Building;
                if (BtnHotRestartQuick != null)
                    BtnHotRestartQuick.IsEnabled = isRunning && state != ExecutionState.Starting && state != ExecutionState.Building;

                if (TxtStatusConsole != null)
                {
                    TxtStatusConsole.Text = state switch
                    {
                        ExecutionState.Idle => "Console: Idle",
                        ExecutionState.Starting => "Console: Starting...",
                        ExecutionState.Building => "Console: Building...",
                        ExecutionState.Running => "Console: Running",
                        ExecutionState.Reloading => "Console: Reloading...",
                        ExecutionState.Restarting => "Console: Restarting...",
                        ExecutionState.Stopping => "Console: Stopping...",
                        _ => "Console"
                    };
                }
            });
        }

        private async void BtnHotReload_Click(object sender, RoutedEventArgs e)
        {
            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
            await FlutterExecutionService.Instance.HotReloadAsync();
        }

        private async void BtnHotRestart_Click(object sender, RoutedEventArgs e)
        {
            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
            await FlutterExecutionService.Instance.HotRestartAsync();
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            await FlutterExecutionService.Instance.StopAsync();
        }

        private void BtnToggleConsole_Click(object sender, RoutedEventArgs e)
        {
            ToggleDebugConsole();
        }

        private void ToggleDebugConsole(bool? open = null)
        {
            bool isOpen = open ?? (PanelDebugConsole.Visibility != Visibility.Visible);
            if (isOpen)
            {
                PanelDebugConsole.Visibility = Visibility.Visible;
                RowDebugConsole.Height = new GridLength(280);
            }
            else
            {
                PanelDebugConsole.Visibility = Visibility.Collapsed;
                RowDebugConsole.Height = new GridLength(0);
            }
        }

        private void BtnPopOutConsole_Click(object sender, RoutedEventArgs e)
        {
            if (_standaloneDebugWindow == null)
            {
                _standaloneDebugWindow = new DebugLogsWindow();
                _standaloneDebugWindow.Closed += (s, args) => _standaloneDebugWindow = null;
            }
            _standaloneDebugWindow.Activate();
        }

        private void BtnOpenEmulatorManager_Click(object sender, RoutedEventArgs e)
        {
            OpenAndroidSettings();
        }

        private void OpenAndroidSettings()
        {
            var settingsWindow = new SettingsWindow("AndroidDevices");
            settingsWindow.Activate();
        }

        private async void BtnRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            await PopulateDevicesAsync();
        }

        private void ComboTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboTarget?.SelectedItem is ComboBoxItem item)
            {
                if (TxtTargetInfo != null)
                {
                    TxtTargetInfo.Text = $"Target: {item.Content}";
                }
                string tag = item.Tag?.ToString() ?? "web";
                _ = PopulateDevicesAsync(tag);
            }
        }

        private void ComboDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboDevice?.SelectedItem is ComboBoxItem item)
            {
                if (item.Tag is FlutterDevice dev)
                {
                    if (TxtTargetInfo != null)
                    {
                        TxtTargetInfo.Text = $"Target: {dev.DisplayName}";
                    }
                }
                else if (item.Tag is string tag && (tag == "manage_emulators" || tag == "settings_android"))
                {
                    if (ComboDevice.Items.Count > 1)
                    {
                        ComboDevice.SelectedIndex = 0;
                    }
                    OpenAndroidSettings();
                }
            }
        }

        private string GetSelectedTarget()
        {
            return (ComboTarget?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "web";
        }

        private async Task PopulateDevicesAsync(string target = null)
        {
            int currentVersion = System.Threading.Interlocked.Increment(ref _deviceQueryVersion);
            string activeTarget = target ?? GetSelectedTarget();
            string targetName = (ComboTarget?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? activeTarget;

            // Immediately switch ComboDevice to loading state so user doesn't see old platform's devices
            DispatcherQueue?.TryEnqueue(() =>
            {
                if (ComboDevice != null)
                {
                    ComboDevice.Items.Clear();
                    ComboDevice.Items.Add(new ComboBoxItem
                    {
                        Content = $"Detecting {targetName} devices...",
                        Tag = "loading",
                        IsEnabled = false
                    });
                    ComboDevice.SelectedIndex = 0;
                    ComboDevice.IsEnabled = false;
                }

                if (RingDeviceLoading != null)
                {
                    RingDeviceLoading.Visibility = Visibility.Visible;
                    RingDeviceLoading.IsActive = true;
                }

                if (BtnRefreshDevices != null)
                {
                    BtnRefreshDevices.IsEnabled = false;
                }

                if (TxtTargetInfo != null)
                {
                    TxtTargetInfo.Text = $"Target: {targetName} (Detecting...)";
                }
            });

            List<FlutterDevice> devices = new();
            try
            {
                devices = await DeviceService.GetAllDevicesAndEmulatorsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] PopulateDevicesAsync failed: {ex.Message}");
            }

            DispatcherQueue?.TryEnqueue(() =>
            {
                // Discard stale result if a newer target switch occurred while this was running
                if (currentVersion != _deviceQueryVersion)
                {
                    return;
                }

                if (RingDeviceLoading != null)
                {
                    RingDeviceLoading.IsActive = false;
                    RingDeviceLoading.Visibility = Visibility.Collapsed;
                }

                if (BtnRefreshDevices != null)
                {
                    BtnRefreshDevices.IsEnabled = true;
                }

                if (ComboDevice == null) return;

                ComboDevice.IsEnabled = true;
                _discoveredDevices = devices;
                ComboDevice.Items.Clear();

                var matchingDevices = _discoveredDevices.Where(d =>
                {
                    if (activeTarget == "android") return d.Platform == DevicePlatform.Android;
                    if (activeTarget == "web") return d.Platform == DevicePlatform.Web;
                    if (activeTarget == "windows") return d.Platform == DevicePlatform.Windows;
                    return true;
                }).ToList();

                if (activeTarget == "windows" && matchingDevices.Count == 0 && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    matchingDevices.Add(new FlutterDevice
                    {
                        Id = "windows",
                        Name = "Windows (Desktop)",
                        Platform = DevicePlatform.Windows,
                        IsRunning = true
                    });
                }

                foreach (var dev in matchingDevices)
                {
                    var item = new ComboBoxItem
                    {
                        Content = dev.DisplayName,
                        Tag = dev
                    };
                    ComboDevice.Items.Add(item);
                }

                if (activeTarget == "android")
                {
                    ComboDevice.Items.Add(new ComboBoxItem
                    {
                        Content = "⚙️ Android Emulator Settings...",
                        Tag = "settings_android"
                    });
                }

                if (ComboDevice.Items.Count > 0)
                {
                    ComboDevice.SelectedIndex = 0;
                    if (ComboDevice.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is FlutterDevice selDev && TxtTargetInfo != null)
                    {
                        TxtTargetInfo.Text = $"Target: {selDev.DisplayName}";
                    }
                }
                else
                {
                    ComboDevice.Items.Add(new ComboBoxItem { Content = $"No {targetName} devices detected", Tag = null, IsEnabled = false });
                    ComboDevice.SelectedIndex = 0;
                    if (TxtTargetInfo != null)
                    {
                        TxtTargetInfo.Text = $"Target: {targetName} (No devices)";
                    }
                }
            });
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                PromptSaveBeforeNavigating();
            }
            else
            {
                NavigateBack();
            }
        }

        private async void PromptSaveBeforeNavigating()
        {
            var dialog = new ContentDialog
            {
                Title = "Unsaved Changes",
                Content = "You have unsaved changes. Would you like to save before leaving?",
                PrimaryButtonText = "Save & Go Back",
                SecondaryButtonText = "Discard",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
                RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await SaveWorkspaceAsync();
                NavigateBack();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                NavigateBack();
            }
        }

        private void NavigateBack()
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(HomePage));
        }

        private async void EditorTabs_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString() ?? "";

                // Flush auto-save and compile page before changing tabs if there are unsaved changes
                _autoSaveTimer.Stop();
                if (_hasUnsavedChanges && !_pendingEventTarget.HasValue)
                {
                    await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
                }

                if (tag == "event")
                {
                    EditorLoadingPanel.Visibility = Visibility.Collapsed;
                    BlocklyWebView.Visibility = Visibility.Visible;
                    if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null)
                    {
                        _ = BlocklyWebView.CoreWebView2.ExecuteScriptAsync("if (window.resizeWorkspace) window.resizeWorkspace();");
                    }
                    await HandleEventTabNavigationAsync();
                }
                
                if (tag == "code")
                {
                    ViewPanel.Opacity = 0;
                    ViewPanel.IsHitTestVisible = false;
                    Canvas.SetZIndex(ViewPanel, 1);

                    EventPanel.Opacity = 0;
                    EventPanel.IsHitTestVisible = false;
                    Canvas.SetZIndex(EventPanel, 1);

                    ComponentPanel.Opacity = 0;
                    ComponentPanel.IsHitTestVisible = false;
                    Canvas.SetZIndex(ComponentPanel, 1);

                    CodePanel.Opacity = 1;
                    CodePanel.IsHitTestVisible = true;
                    Canvas.SetZIndex(CodePanel, 10);

                    if (!string.IsNullOrEmpty(_projectPath))
                    {
                        ProjectExplorer.LoadProject(_projectPath);

                        string mainDartPath = Path.Combine(_projectPath, ProjectService.FlutterFolderName, "lib", "main.dart");
                        if (!File.Exists(mainDartPath))
                        {
                            mainDartPath = Path.Combine(_projectPath, "lib", "main.dart");
                        }
                        if (File.Exists(mainDartPath))
                        {
                            await MonacoEditor.OpenFileAsync(mainDartPath);
                        }
                    }
                }
                else if (tag == "view" || tag == "event" || tag == "component")
                {
                    CodePanel.Opacity = 0;
                    CodePanel.IsHitTestVisible = false;
                    Canvas.SetZIndex(CodePanel, 1);

                    ViewPanel.Opacity = tag == "view" ? 1 : 0;
                    ViewPanel.IsHitTestVisible = tag == "view";
                    Canvas.SetZIndex(ViewPanel, tag == "view" ? 10 : 1);
                    
                    EventPanel.Opacity = tag == "event" ? 1 : 0;
                    EventPanel.IsHitTestVisible = tag == "event";
                    Canvas.SetZIndex(EventPanel, tag == "event" ? 10 : 1);
                    
                    ComponentPanel.Opacity = tag == "component" ? 1 : 0;
                    ComponentPanel.IsHitTestVisible = tag == "component";
                    Canvas.SetZIndex(ComponentPanel, tag == "component" ? 10 : 1);
                }
            }
        }

        private void BtnToggleViewManager_Click(object sender, RoutedEventArgs e)
        {
            ViewManagerSplitView.IsPaneOpen = !ViewManagerSplitView.IsPaneOpen;
        }

        private async void BtnAssets_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AssetManagerDialog(_projectPath)
            {
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void BtnPageConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentMeta = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName)
                    ?? new Snapcode.Models.PageMetadata { Name = _currentPageName };

                var dialog = new PageConfigurationDialog(_currentPageName, currentMeta)
                {
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var updated = dialog.GetUpdatedMetadata();
                    _currentMetadata = updated;
                    await ProjectService.SavePageMetadataAsync(_projectPath, _currentPageName, updated);

                    if (updated.HasDrawer)
                    {
                        await ProjectService.EnsureDrawerForPageAsync(_projectPath, _currentPageName);
                        string drawerName = $"{_currentPageName}_drawer";
                        if (!ListPages.Items.Contains(drawerName))
                        {
                            ListPages.Items.Add(drawerName);
                        }
                        RefreshQuickPageNavigation();
                        await PushProjectPagesToBlocklyAsync();
                    }

                    // Apply to visual designer canvas immediately
                    if (NativeDesigner != null)
                    {
                        NativeDesigner.ApplyPageMetadata(updated);
                    }

                    // Save workspace to regenerate Dart code with new page properties
                    await SaveWorkspaceAsync();

                    DispatcherQueue?.TryEnqueue(() =>
                    {
                        TxtSaveStatus.Text = $"Page configured: {_currentPageName}";
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] Page configuration dialog error: {ex.Message}");
            }
        }

        private async void ListPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSwitchingPage) return;
            
            if (ListPages.SelectedItem is string pageName && pageName != _currentPageName)
            {
                await SwitchToPageAsync(pageName);
            }
        }

        private int _pendingPageSyncCount = 0;

        public async Task SwitchToPageAsync(string pageName)
        {
            if (string.IsNullOrWhiteSpace(pageName) || pageName == _currentPageName) return;

            // Determine navigation direction for fluid directional animation
            var visualPages = GetVisualPageNames();
            int oldIndex = visualPages.IndexOf(_currentPageName);
            int newIndex = visualPages.IndexOf(pageName);
            int direction = (newIndex >= oldIndex) ? 1 : -1;

            string oldPageName = _currentPageName;
            string oldEventContext = _currentEventContext;

            // 1. Snapshot previous page UI and Blockly workspace instantly on the UI thread
            string snapshotUiJson = NativeDesigner?.GetUIJson();
            string snapshotWorkspaceJson = null;
            string snapshotActiveCode = null;

            if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null)
            {
                try
                {
                    snapshotWorkspaceJson = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getWorkspace()");
                    if (!string.IsNullOrEmpty(snapshotWorkspaceJson) && snapshotWorkspaceJson.StartsWith("\"") && snapshotWorkspaceJson.EndsWith("\""))
                    {
                        snapshotWorkspaceJson = JsonSerializer.Deserialize<string>(snapshotWorkspaceJson) ?? "{}";
                    }

                    string rawCode = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getGeneratedCode()");
                    if (!string.IsNullOrEmpty(rawCode) && rawCode != "null")
                    {
                        if (rawCode.StartsWith("\"") && rawCode.EndsWith("\""))
                        {
                            try
                            {
                                snapshotActiveCode = JsonSerializer.Deserialize<string>(rawCode) ?? string.Empty;
                            }
                            catch
                            {
                                snapshotActiveCode = rawCode;
                            }
                        }
                        else
                        {
                            snapshotActiveCode = rawCode;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Snapshot of old page Blockly state failed: {ex.Message}");
                }
            }

            // 2. Set target page as current
            _currentPageName = pageName;
            _currentEventContext = null;

            _isSwitchingPage = true;
            if (ListPages != null)
            {
                ListPages.SelectedItem = pageName;
            }
            _isSwitchingPage = false;

            UpdateQuickPageNavigationActiveState();

            // 3. Show slim Fluent progress bar indicating background compiling & saving
            if (PageSwitchProgressBar != null)
            {
                PageSwitchProgressBar.Visibility = Visibility.Visible;
            }

            // 4. Dispatch the previous page save, code compilation, preservation, formatting, and hot reload to background
            Interlocked.Increment(ref _pendingPageSyncCount);
            _ = Task.Run(async () =>
            {
                try
                {
                    await SaveWorkspaceAsync(
                        oldPageName, 
                        oldEventContext, 
                        overrideUiJson: snapshotUiJson, 
                        overrideWorkspaceJson: snapshotWorkspaceJson, 
                        overrideActiveDartCode: snapshotActiveCode);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Background save of '{oldPageName}' failed: {ex.Message}");
                }
                finally
                {
                    if (Interlocked.Decrement(ref _pendingPageSyncCount) == 0)
                    {
                        DispatcherQueue?.TryEnqueue(() =>
                        {
                            if (PageSwitchProgressBar != null)
                            {
                                PageSwitchProgressBar.Visibility = Visibility.Collapsed;
                            }
                        });
                    }
                }
            });

            // 5. Load target page UI JSON & metadata from disk
            Snapcode.Models.PageMetadata? metadata = null;
            string targetUiJson = "[]";
            try
            {
                metadata = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName);
                _currentMetadata = metadata;
                targetUiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed loading new page UI/meta: {ex.Message}");
            }

            // 6. Smooth directional slide & crossfade transition on the visual designer
            if (NativeDesigner != null)
            {
                await NativeDesigner.AnimatePageTransitionAsync(direction, () =>
                {
                    NativeDesigner.ApplyPageMetadata(metadata);
                    NativeDesigner.SetPageName(_currentPageName);
                    NativeDesigner.LoadUIJson(targetUiJson);
                });
            }

            // 7. Update Blockly Workspace for the new page
            if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null)
            {
                try
                {
                    string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, _currentPageName);
                    string pageType = metadata?.Type?.ToLower() ?? "page";
                    if (pageType == "custom view") pageType = "custom_view";

                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.setWorkspaceContext('{pageType}')");
                    await PushProjectPagesToBlocklyAsync();

                    if (workspaceJson != "{}" && !string.IsNullOrWhiteSpace(workspaceJson))
                    {
                        string safeJson = JsonSerializer.Serialize(workspaceJson);
                        await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadWorkspace({safeJson})");
                    }
                    else
                    {
                        await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.clearWorkspace()");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to switch Blockly workspace: {ex.Message}");
                }
            }

            // 8. If Code Editor tab is active, update Monaco editor
            if (EditorTabs?.SelectedItem is NavigationViewItem navItem && navItem.Tag?.ToString() == "code" && MonacoEditor != null)
            {
                string fileName = _currentPageName.Equals("main", StringComparison.OrdinalIgnoreCase) 
                    ? "main.dart" 
                    : $"{_currentPageName.ToLowerInvariant()}.dart";
                string targetDartPath = Path.Combine(_projectPath, ProjectService.FlutterFolderName, "lib", fileName);
                if (!File.Exists(targetDartPath)) targetDartPath = Path.Combine(_projectPath, "lib", fileName);
                if (File.Exists(targetDartPath))
                {
                    await MonacoEditor.OpenFileAsync(targetDartPath);
                }
            }

            DispatcherQueue?.TryEnqueue(() =>
            {
                TxtSaveStatus.Text = $"Loaded {pageName}.dart";
            });
        }

        private bool IsCurrentThemeLight()
        {
            var theme = this.ActualTheme;
            if (theme == ElementTheme.Default)
            {
                theme = (App.MainWindow?.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Dark;
            }
            return theme == ElementTheme.Light;
        }

        public void RefreshQuickPageNavigation()
        {
            if (QuickPageTabsPanel == null) return;

            QuickPageTabsPanel.Children.Clear();

            var pages = new List<string>();
            if (ListPages != null && ListPages.Items.Count > 0)
            {
                foreach (var item in ListPages.Items)
                {
                    if (item is string p && !pages.Contains(p)) pages.Add(p);
                }
            }
            else if (!string.IsNullOrEmpty(_projectPath))
            {
                var discovered = ProjectService.GetProjectPages(_projectPath);
                foreach (var p in discovered)
                {
                    if (ProjectFileClassifier.IsVisualPage(_projectPath, p) && !pages.Contains(p))
                    {
                        pages.Add(p);
                    }
                }
            }

            if (pages.Count == 0) pages.Add("main");

            // Ensure "main" is always the first tab
            if (pages.Contains("main") && pages.IndexOf("main") != 0)
            {
                pages.Remove("main");
                pages.Insert(0, "main");
            }

            foreach (var pageName in pages)
            {
                var btn = new Button
                {
                    Height = 24,
                    Padding = new Thickness(8, 2, 8, 2),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(1, 0, 1, 0),
                    Tag = pageName,
                    BorderThickness = new Thickness(0),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)
                };

                bool isDrawerTab = pageName.EndsWith("_drawer", StringComparison.OrdinalIgnoreCase);
                string tip = isDrawerTab ? $"Switch to {pageName}.dart (Navigation Drawer)" : $"Switch to {pageName}.dart (Ctrl+Tab)";
                ToolTipService.SetToolTip(btn, tip);

                var sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new FontIcon
                {
                    Glyph = isDrawerTab ? "\uE700" : "\uE7C3",
                    FontSize = 11
                };

                var text = new TextBlock
                {
                    Text = $"{pageName}.dart",
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                };

                sp.Children.Add(icon);
                sp.Children.Add(text);
                btn.Content = sp;

                string capturedPage = pageName;
                btn.Click += async (s, e) =>
                {
                    await SwitchToPageAsync(capturedPage);
                };

                // Pointer hover feedback for inactive tabs
                btn.PointerEntered += (s, e) =>
                {
                    if (!string.Equals(capturedPage, _currentPageName, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isLight = IsCurrentThemeLight();
                        btn.Background = isLight 
                            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(18, 0, 0, 0))
                            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(24, 255, 255, 255));
                    }
                };
                btn.PointerExited += (s, e) =>
                {
                    if (!string.Equals(capturedPage, _currentPageName, StringComparison.OrdinalIgnoreCase))
                    {
                        btn.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    }
                };

                // Right click context menu on individual tab
                var flyout = new MenuFlyout();
                var itemSwitch = new MenuFlyoutItem { Text = $"Switch to {capturedPage}.dart" };
                itemSwitch.Click += async (s, e) => await SwitchToPageAsync(capturedPage);
                flyout.Items.Add(itemSwitch);

                var itemConfig = new MenuFlyoutItem { Text = "Page Configuration..." };
                itemConfig.Click += (s, e) => BtnPageConfig_Click(s, e);
                flyout.Items.Add(itemConfig);

                flyout.Items.Add(new MenuFlyoutSeparator());

                var itemHide = new MenuFlyoutItem { Text = "Hide Quick Page Navigation" };
                itemHide.Click += (s, e) => MenuHideQuickPageNav_Click(s, e);
                flyout.Items.Add(itemHide);

                btn.ContextFlyout = flyout;

                QuickPageTabsPanel.Children.Add(btn);
            }

            UpdateQuickPageNavigationActiveState();
        }

        private void UpdateQuickPageNavigationActiveState()
        {
            if (QuickPageTabsPanel == null) return;

            bool isLight = IsCurrentThemeLight();

            // Active Tab Brushes:
            var brushActiveBg = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"]
                ?? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 103, 192));
            var brushActiveFg = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);

            // Inactive Tab Brushes:
            // Light Theme: Deep dark charcoal (#1A1A1A) text so it is crystal clear on the light card host.
            // Dark Theme:  Crisp white/light gray (#F3F3F3) text on the dark card host.
            var brushInactiveText = isLight
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26))
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 243, 243));

            var brushInactiveIcon = isLight
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 103, 192))
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 205, 255));

            var brushTransparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

            foreach (var child in QuickPageTabsPanel.Children)
            {
                if (child is Button btn && btn.Tag is string pageName)
                {
                    bool isActive = string.Equals(pageName, _currentPageName, StringComparison.OrdinalIgnoreCase);

                    btn.Background = isActive ? brushActiveBg : brushTransparent;

                    if (btn.Content is StackPanel sp && sp.Children.Count >= 2)
                    {
                        if (sp.Children[0] is FontIcon icon)
                        {
                            icon.Foreground = isActive ? brushActiveFg : brushInactiveIcon;
                        }
                        if (sp.Children[1] is TextBlock text)
                        {
                            text.FontWeight = isActive ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal;
                            text.Foreground = isActive ? brushActiveFg : brushInactiveText;
                        }
                    }
                }
            }
        }

        public void NavigateNextPage()
        {
            var pages = GetVisualPageNames();
            if (pages.Count <= 1) return;

            int idx = pages.IndexOf(_currentPageName);
            int nextIdx = (idx >= 0) ? (idx + 1) % pages.Count : 0;
            _ = SwitchToPageAsync(pages[nextIdx]);
        }

        public void NavigatePreviousPage()
        {
            var pages = GetVisualPageNames();
            if (pages.Count <= 1) return;

            int idx = pages.IndexOf(_currentPageName);
            int prevIdx = (idx >= 0) ? (idx - 1 + pages.Count) % pages.Count : pages.Count - 1;
            _ = SwitchToPageAsync(pages[prevIdx]);
        }

        private List<string> GetVisualPageNames()
        {
            var pages = new List<string>();
            if (ListPages != null && ListPages.Items.Count > 0)
            {
                foreach (var item in ListPages.Items)
                {
                    if (item is string p && !pages.Contains(p)) pages.Add(p);
                }
            }
            if (!pages.Contains("main")) pages.Insert(0, "main");
            return pages;
        }

        private void MenuHideQuickPageNav_Click(object sender, RoutedEventArgs e)
        {
            if (QuickPageNavHost != null)
            {
                QuickPageNavHost.Visibility = Visibility.Collapsed;
            }
            if (MenuToggleQuickPageNav != null) MenuToggleQuickPageNav.IsChecked = false;
            if (ContextTogglePageNav != null) ContextTogglePageNav.IsChecked = false;
        }

        private void MenuToggleQuickPageNav_Click(object sender, RoutedEventArgs e)
        {
            bool show;
            if (sender is ToggleMenuFlyoutItem item)
            {
                show = item.IsChecked;
            }
            else
            {
                show = QuickPageNavHost?.Visibility != Visibility.Visible;
            }

            if (QuickPageNavHost != null)
            {
                QuickPageNavHost.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
            if (MenuToggleQuickPageNav != null) MenuToggleQuickPageNav.IsChecked = show;
            if (ContextTogglePageNav != null) ContextTogglePageNav.IsChecked = show;
        }

        private void MenuNextPage_Click(object sender, RoutedEventArgs e)
        {
            NavigateNextPage();
        }

        private void MenuPrevPage_Click(object sender, RoutedEventArgs e)
        {
            NavigatePreviousPage();
        }

        private async void BtnAddPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Snapcode.Views.NewPageDialog
                {
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    string newPageName = dialog.FileName;
                    if (string.IsNullOrWhiteSpace(newPageName))
                    {
                        newPageName = $"page_{ListPages.Items.Count}";
                    }

                    // Sanitize page name to prevent file system crashes
                    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                    {
                        newPageName = newPageName.Replace(c, '_');
                    }
                    newPageName = newPageName.Replace(" ", "_").Trim();

                    var metadata = new Snapcode.Models.PageMetadata
                    {
                        Name = newPageName,
                        Type = dialog.FileType,
                        IsToolbarHidden = dialog.HideToolbar,
                        IsStatusBarHidden = dialog.HideStatusBar,
                        HasFAB = dialog.AddFab,
                        HasDrawer = dialog.AddNavDrawer
                    };

                    await ProjectService.SavePageMetadataAsync(_projectPath, newPageName, metadata);
                    await ProjectService.SavePageWorkspaceAsync(_projectPath, newPageName, "{}");

                    if (dialog.AddNavDrawer)
                    {
                        await ProjectService.EnsureDrawerForPageAsync(_projectPath, newPageName);
                        string drawerName = $"{newPageName}_drawer";
                        if (!ListPages.Items.Contains(drawerName))
                        {
                            ListPages.Items.Add(drawerName);
                        }
                    }

                    _isSwitchingPage = true;
                    if (!ListPages.Items.Contains(newPageName))
                    {
                        ListPages.Items.Add(newPageName);
                    }
                    _isSwitchingPage = false;
                    
                    RefreshQuickPageNavigation();
                    await PushProjectPagesToBlocklyAsync();
                    await SwitchToPageAsync(newPageName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] BtnAddPage_Click failed: {ex.Message}");
            }
        }

        public class EventListItem
        {
            public string WidgetId { get; set; }
            public string EventType { get; set; }
            public string WidgetType { get; set; }
            public string DisplayName { get; set; }
            public string Icon { get; set; }
        }

        private void PopulateEventList()
        {
            var events = new System.Collections.ObjectModel.ObservableCollection<EventListItem>();
            
            // All Page Events (Overview)
            events.Add(new EventListItem 
            { 
                WidgetId = "", 
                EventType = "", 
                WidgetType = "Page", 
                DisplayName = "★ All Page Events (Overview)", 
                Icon = "\uE71D" 
            });

            // Flutter Lifecycle Events
            events.Add(new EventListItem { WidgetId = "page", EventType = "initState", WidgetType = "Page", DisplayName = "Page: initState (on Create)", Icon = "\uE81E" });
            events.Add(new EventListItem { WidgetId = "page", EventType = "build", WidgetType = "Page", DisplayName = "Page: build (on Render)", Icon = "\uE81E" });
            events.Add(new EventListItem { WidgetId = "page", EventType = "dispose", WidgetType = "Page", DisplayName = "Page: dispose (on Destroy)", Icon = "\uE81E" });

            // Scaffold, FAB, and Drawer Events for current page
            var meta = _currentMetadata ?? NativeDesigner?.CurrentMetadata;
            if (meta?.HasFAB == true)
            {
                events.Add(new EventListItem 
                { 
                    WidgetId = "fab", 
                    EventType = "onClick", 
                    WidgetType = "FloatingActionButton", 
                    DisplayName = "Floating Action Button (onPressed)", 
                    Icon = "\uE710" 
                });
            }

            if (meta?.HasDrawer == true)
            {
                events.Add(new EventListItem 
                { 
                    WidgetId = "drawer", 
                    EventType = "onOpen", 
                    WidgetType = "Drawer", 
                    DisplayName = "Navigation Drawer (onDrawerChanged)", 
                    Icon = "\uE700" 
                });
            }

            if (meta?.ShowAppBar == true && (meta.HasDrawer || meta.AppBarLeadingIcon == "menu"))
            {
                events.Add(new EventListItem 
                { 
                    WidgetId = "appbar_leading", 
                    EventType = "onClick", 
                    WidgetType = "AppBar", 
                    DisplayName = "AppBar: Drawer Menu Icon (onPressed)", 
                    Icon = "\uE700" 
                });
            }
            else if (meta?.ShowAppBar == true && meta.AppBarLeadingIcon == "back")
            {
                events.Add(new EventListItem 
                { 
                    WidgetId = "appbar_leading", 
                    EventType = "onClick", 
                    WidgetType = "AppBar", 
                    DisplayName = "AppBar: Back Navigation Icon (onPressed)", 
                    Icon = "\uE72B" 
                });
            }

            if (NativeDesigner != null && NativeDesigner.AllWidgetsFlat != null)
            {
                foreach (var widget in NativeDesigner.AllWidgetsFlat)
                {
                    string t = widget.Type?.ToLower() ?? "";
                    if (t == "button" || t == "image" || t == "column" || t == "row" || t == "container" || t == "card" || t == "text" || t == "textview")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onClick", WidgetType = widget.Type, DisplayName = $"{widget.Name} (onTap / onClick)", Icon = widget.Icon });
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onLongPress", WidgetType = widget.Type, DisplayName = $"{widget.Name} (onLongPress)", Icon = widget.Icon });
                    }
                    else if (t == "textfield")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onChanged", WidgetType = widget.Type, DisplayName = $"{widget.Name} (onChanged)", Icon = widget.Icon });
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onSubmitted", WidgetType = widget.Type, DisplayName = $"{widget.Name} (onSubmitted)", Icon = widget.Icon });
                    }
                    else if (t == "checkbox" || t == "switch" || t == "slider")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onChanged", WidgetType = widget.Type, DisplayName = $"{widget.Name} (onChanged)", Icon = widget.Icon });
                    }
                }
            }

            ListEvents.ItemsSource = events;
        }

        private async void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSelectingEventProgrammatically) return;

            if (ListEvents.SelectedItem is EventListItem item)
            {
                await SwitchToEventWorkspaceAsync(item.WidgetId, item.EventType, item.WidgetType);
            }
        }

        private async Task SwitchToEventWorkspaceAsync(string widgetId, string eventType, string widgetType = null)
        {
            bool isOverview = string.IsNullOrEmpty(widgetId);
            string newContext = isOverview ? null : $"{widgetId}_{eventType}";
            
            if (_currentEventContext == newContext) return; // already here
            
            _autoSaveTimer?.Stop();

            if (_hasUnsavedChanges)
            {
                await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
            }
            
            _currentEventContext = newContext;
            
            if (EditorLoadingPanel != null && EditorLoadingPanel.Visibility == Visibility.Visible)
            {
                EditorLoadingPanel.Visibility = Visibility.Collapsed;
                BlocklyWebView.Visibility = Visibility.Visible;
            }
            
            if (_isWebViewReady && BlocklyWebView != null && BlocklyWebView.CoreWebView2 != null)
            {
                if (isOverview)
                {
                    await LoadSavedWorkspaceAsync(loadNativeUi: false);
                }
                else
                {
                    string workspaceFile = $"{_currentPageName}_{_currentEventContext}";
                    string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, workspaceFile);
                    
                    if (string.IsNullOrEmpty(widgetType) && NativeDesigner?.AllWidgetsFlat != null)
                    {
                        var matching = NativeDesigner.AllWidgetsFlat.FirstOrDefault(w => w.Id == widgetId);
                        if (matching != null) widgetType = matching.Type ?? "";
                    }

                    string safeWidgetId = JsonSerializer.Serialize(widgetId);
                    string safeEventType = JsonSerializer.Serialize(eventType);
                    string safeJson = JsonSerializer.Serialize(workspaceJson ?? "");
                    string safeWidgetType = JsonSerializer.Serialize(widgetType ?? "");
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadEventWorkspace({safeWidgetId}, {safeEventType}, {safeJson}, {safeWidgetType})");
                    _ = BlocklyWebView.CoreWebView2.ExecuteScriptAsync("if (window.resizeWorkspace) window.resizeWorkspace();");
                }

                _hasUnsavedChanges = false;
            }

            _autoSaveTimer?.Start();
        }
    }
}
