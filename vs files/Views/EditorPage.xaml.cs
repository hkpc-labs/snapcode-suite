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
        private bool _isSwitchingPage = false;
        private string _currentEventContext = null;
        private bool _isProgrammaticTabChange = false;
        private DispatcherTimer _autoSaveTimer;
        private DebugLogsWindow? _standaloneDebugWindow;
        private List<FlutterDevice> _discoveredDevices = new();

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
            // Build the context string
            _currentEventContext = $"{args.WidgetId}_{args.EventType}";

            // Switch the tab to the "event" pane
            foreach(var item in EditorTabs.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "event")
                {
                    _isProgrammaticTabChange = true;
                    EditorTabs.SelectedItem = navItem;
                    _isProgrammaticTabChange = false;
                    break;
                }
            }

            // Load the contextual event workspace
            if (_isWebViewReady && BlocklyWebView != null && BlocklyWebView.CoreWebView2 != null)
            {
                string workspaceFile = $"{_currentPageName}_{_currentEventContext}";
                string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, workspaceFile);
                
                string safeWidgetId = JsonSerializer.Serialize(args.WidgetId);
                string safeEventType = JsonSerializer.Serialize(args.EventType);
                string safeJson = JsonSerializer.Serialize(workspaceJson ?? "");
                await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadEventWorkspace({safeWidgetId}, {safeEventType}, {safeJson})");
            }
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

                // Immediately load Native UI state for the current page so widgets render on canvas instantly
                try
                {
                    string uiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
                    if (NativeDesigner != null)
                    {
                        var pageMeta = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName);
                        NativeDesigner.ApplyPageMetadata(pageMeta);
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
                // Initialize WebView2
                await BlocklyWebView.EnsureCoreWebView2Async();
                
                // Set up message handler for JS → C# communication
                BlocklyWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // Navigation completed handler for error catching
                BlocklyWebView.CoreWebView2.NavigationCompleted += (s, navArgs) =>
                {
                    if (!navArgs.IsSuccess)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BlockEditor] Navigation failed: {navArgs.WebErrorStatus}");
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            BlocklyWebView.Visibility = Visibility.Visible;
                        });
                    }
                };

                // Determine the theme
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

                if (MonacoEditor != null)
                {
                    MonacoEditor.SetTheme(themeMode);
                }

                // Load the Blockly HTML via Virtual Host Name Mapping (ensures secure, offline HTTPS context for scripts and media)
                string blockEditorDir = Path.Combine(AppContext.BaseDirectory, "Assets", "BlockEditor");
                if (Directory.Exists(blockEditorDir))
                {
                    try
                    {
                        BlocklyWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                            "blockeditor.snapcode",
                            blockEditorDir,
                            Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

                        BlocklyWebView.CoreWebView2.Navigate($"https://blockeditor.snapcode/index.html?theme={themeMode}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BlockEditor] Virtual host mapping failed: {ex.Message}. Falling back to file navigation.");
                        string indexPath = Path.Combine(blockEditorDir, "index.html");
                        BlocklyWebView.CoreWebView2.Navigate(new Uri(indexPath).AbsoluteUri);
                    }
                }
                else
                {
                    // Fallback: Show error
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

                if (workspaceJson != "{}" && !string.IsNullOrWhiteSpace(workspaceJson))
                {
                    string safeJson = JsonSerializer.Serialize(workspaceJson);
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadWorkspace({safeJson})");
                    DispatcherQueue?.TryEnqueue(() =>
                    {
                        TxtSaveStatus.Text = "Loaded";
                    });
                }
                
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

        private async Task SaveWorkspaceAsync(string overridePageName = null, string overrideEventContext = null)
        {
            if (string.IsNullOrEmpty(_projectPath)) return;

            string pageToSave = overridePageName ?? _currentPageName;
            string contextToSave = overrideEventContext ?? _currentEventContext;

            await _saveLock.WaitAsync();
            try
            {
                string currentActiveCode = null;
                if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null)
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
                            if (!File.Exists(targetEventDart))
                            {
                                string jsonContent = await File.ReadAllTextAsync(jsonFile);
                                if (!string.IsNullOrWhiteSpace(jsonContent) && jsonContent != "{}")
                                {
                                    if (_isWebViewReady && BlocklyWebView?.CoreWebView2 != null)
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed compiling pending event files: {ex.Message}");
                }

                string savedUiJson = null;
                if (NativeDesigner != null)
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

                    string fileName = pageToSave.Equals("main", StringComparison.OrdinalIgnoreCase) 
                        ? "main.dart" 
                        : $"{pageToSave.ToLowerInvariant()}.dart";

                    string flutterLibPath = Path.Combine(_projectPath, ProjectService.FlutterFolderName, "lib");
                    if (Directory.Exists(flutterLibPath))
                    {
                        string dartFile = Path.Combine(flutterLibPath, fileName);
                        await UserCodePreservationService.SaveDartFileWithPreservationAsync(dartFile, completeDartSource);
                    }

                    string legacyLibPath = Path.Combine(_projectPath, "lib");
                    if (Directory.Exists(legacyLibPath))
                    {
                        string dartFile = Path.Combine(legacyLibPath, fileName);
                        await UserCodePreservationService.SaveDartFileWithPreservationAsync(dartFile, completeDartSource);
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
            string activeTarget = target ?? GetSelectedTarget();
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
                if (ComboDevice == null) return;

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
                    ComboDevice.Items.Add(new ComboBoxItem { Content = "No devices detected", Tag = null, IsEnabled = false });
                    ComboDevice.SelectedIndex = 0;
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

                // Flush auto-save and compile page before changing tabs so no edits are lost
                _autoSaveTimer.Stop();
                await SaveWorkspaceAsync(_currentPageName, _currentEventContext);

                if (tag == "event" && !_isProgrammaticTabChange)
                {
                    _currentEventContext = null;
                    PopulateEventList();
                    await LoadSavedWorkspaceAsync(loadNativeUi: false);
                }
                else if (tag == "event" && _isProgrammaticTabChange)
                {
                    PopulateEventList();
                }

                if (tag == "event")
                {
                    await PushWidgetListToBlocklyAsync();
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
                    await ProjectService.SavePageMetadataAsync(_projectPath, _currentPageName, updated);

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
                string oldPageName = _currentPageName;
                string oldEventContext = _currentEventContext;
                
                _currentPageName = pageName;
                _currentEventContext = null;

                await SaveWorkspaceAsync(oldPageName, oldEventContext);
                
                if (_isWebViewReady)
                {
                    try
                    {
                        string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, _currentPageName);
                        
                        var metadata = await ProjectService.LoadPageMetadataAsync(_projectPath, _currentPageName);
                        string pageType = metadata?.Type?.ToLower() ?? "page";
                        if (pageType == "custom view") pageType = "custom_view";
                        
                        await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.setWorkspaceContext('{pageType}')");

                        if (workspaceJson != "{}" && !string.IsNullOrWhiteSpace(workspaceJson))
                        {
                            string safeJson = JsonSerializer.Serialize(workspaceJson);
                            await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadWorkspace({safeJson})");
                        }
                        else
                        {
                            await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.clearWorkspace()");
                        }

                        try
                        {
                            string uiJson = await ProjectService.LoadPageUIAsync(_projectPath, _currentPageName);
                            if (NativeDesigner != null)
                            {
                                NativeDesigner.ApplyPageMetadata(metadata);
                                NativeDesigner.SetPageName(_currentPageName);
                                NativeDesigner.LoadUIJson(uiJson);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to inject UI state: {ex.Message}");
                        }

                        DispatcherQueue?.TryEnqueue(() =>
                        {
                            TxtSaveStatus.Text = $"Loaded {pageName}";
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to switch page: {ex.Message}");
                    }
                }
            }
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

                    _isSwitchingPage = true;
                    if (!ListPages.Items.Contains(newPageName))
                    {
                        ListPages.Items.Add(newPageName);
                    }
                    _isSwitchingPage = false;
                    
                    ListPages.SelectedItem = newPageName;
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
            public string DisplayName { get; set; }
            public string Icon { get; set; }
        }

        private void PopulateEventList()
        {
            var events = new System.Collections.ObjectModel.ObservableCollection<EventListItem>();
            
            // Flutter Lifecycle Events
            events.Add(new EventListItem { WidgetId = "page", EventType = "initState", DisplayName = "Page: initState (on Create)", Icon = "\uE81E" });
            events.Add(new EventListItem { WidgetId = "page", EventType = "build", DisplayName = "Page: build (on Render)", Icon = "\uE81E" });
            events.Add(new EventListItem { WidgetId = "page", EventType = "dispose", DisplayName = "Page: dispose (on Destroy)", Icon = "\uE81E" });

            if (NativeDesigner != null && NativeDesigner.AllWidgetsFlat != null)
            {
                foreach (var widget in NativeDesigner.AllWidgetsFlat)
                {
                    string t = widget.Type?.ToLower() ?? "";
                    if (t == "button" || t == "image" || t == "column" || t == "row" || t == "container" || t == "card" || t == "text" || t == "textview")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onClick", DisplayName = $"{widget.Name} (onTap / onClick)", Icon = widget.Icon });
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onLongPress", DisplayName = $"{widget.Name} (onLongPress)", Icon = widget.Icon });
                    }
                    else if (t == "textfield")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onChanged", DisplayName = $"{widget.Name} (onChanged)", Icon = widget.Icon });
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onSubmitted", DisplayName = $"{widget.Name} (onSubmitted)", Icon = widget.Icon });
                    }
                    else if (t == "checkbox" || t == "switch" || t == "slider")
                    {
                        events.Add(new EventListItem { WidgetId = widget.Id, EventType = "onChanged", DisplayName = $"{widget.Name} (onChanged)", Icon = widget.Icon });
                    }
                }
            }

            ListEvents.ItemsSource = events;
        }

        private async void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListEvents.SelectedItem is EventListItem item)
            {
                await SwitchToEventWorkspaceAsync(item.WidgetId, item.EventType);
            }
        }

        private async Task SwitchToEventWorkspaceAsync(string widgetId, string eventType)
        {
            string newContext = $"{widgetId}_{eventType}";
            
            if (_currentEventContext == newContext) return; // already here
            
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
                string workspaceFile = $"{_currentPageName}_{_currentEventContext}";
                string workspaceJson = await ProjectService.LoadPageWorkspaceAsync(_projectPath, workspaceFile);
                
                string safeWidgetId = JsonSerializer.Serialize(widgetId);
                string safeEventType = JsonSerializer.Serialize(eventType);
                string safeJson = JsonSerializer.Serialize(workspaceJson ?? "");
                await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.loadEventWorkspace({safeWidgetId}, {safeEventType}, {safeJson})");
            }
        }
    }
}
