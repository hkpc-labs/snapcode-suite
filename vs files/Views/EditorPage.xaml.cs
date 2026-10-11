using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Snapcode.Services;
using System;
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

        public EditorPage()
        {
            this.InitializeComponent();
            if (NativeDesigner != null)
            {
                NativeDesigner.WidgetTreeFlatJsonChanged += NativeDesigner_WidgetTreeFlatJsonChanged;
                NativeDesigner.UIStateChanged += NativeDesigner_UIStateChanged;
                NativeDesigner.OpenEventWorkspaceRequested += NativeDesigner_OpenEventWorkspaceRequested;
            }
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
                TxtSaveStatus.Text = "Unsaved changes";
            });
        }

        private async void NativeDesigner_WidgetTreeFlatJsonChanged(object sender, string jsonList)
        {
            if (_isWebViewReady && BlocklyWebView != null && BlocklyWebView.CoreWebView2 != null)
            {
                try 
                {
                    string safeJson = JsonSerializer.Serialize(jsonList);
                    await BlocklyWebView.CoreWebView2.ExecuteScriptAsync($"window.updateWidgetIds({safeJson})");
                }
                catch { }
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
                    
                    if (project.Platforms != null)
                    {
                        ComboItemWeb.Visibility = project.Platforms.Contains("web", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                        ComboItemAndroid.Visibility = project.Platforms.Contains("android", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                        ComboItemWindows.Visibility = project.Platforms.Contains("windows", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                        
                        if (ComboTarget.SelectedItem is ComboBoxItem currentItem && currentItem.Visibility == Visibility.Collapsed)
                        {
                            if (ComboItemWeb.Visibility == Visibility.Visible) ComboTarget.SelectedItem = ComboItemWeb;
                            else if (ComboItemAndroid.Visibility == Visibility.Visible) ComboTarget.SelectedItem = ComboItemAndroid;
                            else if (ComboItemWindows.Visibility == Visibility.Visible) ComboTarget.SelectedItem = ComboItemWindows;
                        }
                    }
                }
                else
                {
                    // Legacy project (no .snapcode folder)
                    TxtProjectName.Text = _projectName;
                    TxtProjectOrg.Text = "";
                }

                // Populate Pages
                var pages = ProjectService.GetProjectPages(projectPath);
                _isSwitchingPage = true;
                ListPages.Items.Clear();
                foreach (var p in pages)
                {
                    ListPages.Items.Add(p);
                }
                _currentPageName = "main";
                ListPages.SelectedItem = _currentPageName;
                _isSwitchingPage = false;

                await InitializeWebViewAsync();
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

                // Load the Blockly HTML
                string htmlPath = Path.Combine(AppContext.BaseDirectory, "Assets", "BlockEditor", "index.html");
                if (File.Exists(htmlPath))
                {
                    // Read and modify HTML to set the correct initial theme
                    string html = await File.ReadAllTextAsync(htmlPath);
                    html = html.Replace("initWorkspace('dark')", $"initWorkspace('{themeMode}')");
                    html = html.Replace("class=\"dark\"", $"class=\"{themeMode}\"");

                    BlocklyWebView.CoreWebView2.NavigateToString(html);
                }
                else
                {
                    // Fallback: Show error
                    EditorLoadingPanel.Visibility = Visibility.Collapsed;
                    BlocklyWebView.Visibility = Visibility.Visible;
                }
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

                            // Load saved workspace if it exists
                            await LoadSavedWorkspaceAsync();
                            
                            // Load custom blocks from Sketchware JSON
                            await LoadSketchwareBlocksAsync();
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

        private async Task LoadSavedWorkspaceAsync()
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
                    TxtSaveStatus.Text = "Loaded";
                }
                
                // Load Native UI state
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
            await SaveWorkspaceAsync(_currentPageName, _currentEventContext);
        }

        private async Task SaveWorkspaceAsync(string overridePageName = null, string overrideEventContext = null)
        {
            if (!_isWebViewReady || string.IsNullOrEmpty(_projectPath)) return;

            string pageToSave = overridePageName ?? _currentPageName;
            string contextToSave = overrideEventContext ?? _currentEventContext;

            await _saveLock.WaitAsync();
            try
            {
                string workspaceJson = await BlocklyWebView.CoreWebView2.ExecuteScriptAsync("window.getWorkspace()");

                if (workspaceJson.StartsWith("\"") && workspaceJson.EndsWith("\""))
                {
                    workspaceJson = JsonSerializer.Deserialize<string>(workspaceJson) ?? "{}";
                }

                string workspaceFile = contextToSave != null ? $"{pageToSave}_{contextToSave}" : pageToSave;
                await ProjectService.SavePageWorkspaceAsync(_projectPath, workspaceFile, workspaceJson);

                if (NativeDesigner != null)
                {
                    string uiJson = NativeDesigner.GetUIJson();
                    if (!string.IsNullOrEmpty(uiJson) && uiJson != "null")
                    {
                        if (uiJson.StartsWith("\"") && uiJson.EndsWith("\""))
                        {
                            uiJson = JsonSerializer.Deserialize<string>(uiJson) ?? "{}";
                        }
                        await ProjectService.SavePageUIAsync(_projectPath, pageToSave, uiJson);
                    }
                }

                if (!string.IsNullOrWhiteSpace(_lastGeneratedCode) && pageToSave == "main")
                {
                    string flutterLibPath = Path.Combine(_projectPath, ProjectService.FlutterFolderName, "lib");
                    if (Directory.Exists(flutterLibPath))
                    {
                        await File.WriteAllTextAsync(Path.Combine(flutterLibPath, "main.dart"), _lastGeneratedCode);
                    }
                    string legacyLibPath = Path.Combine(_projectPath, "lib");
                    if (Directory.Exists(legacyLibPath))
                    {
                        await File.WriteAllTextAsync(Path.Combine(legacyLibPath, "main.dart"), _lastGeneratedCode);
                    }
                }

                _hasUnsavedChanges = false;
                StatusSaveIcon.Glyph = "\uE73E";
                TxtSaveStatus.Text = $"Saved at {DateTime.Now:HH:mm:ss}";

                await ProjectService.AppendBuildLogAsync(_projectPath, $"Workspace {pageToSave} saved successfully.");
            }
            catch (Exception ex)
            {
                StatusSaveIcon.Glyph = "\uEA39";
                TxtSaveStatus.Text = $"Save failed: {ex.Message}";
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
                await SaveWorkspaceAsync();
                var selectedTarget = (ComboTarget.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Web";
                var selectedDevice = (ComboDevice?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Default Device";
                
                if (TxtTargetInfo != null) TxtTargetInfo.Text = $"Target: {selectedTarget}";

                var dialog = new ContentDialog
                {
                    Title = "Start Debugging",
                    Content = $"Launching on {selectedDevice} ({selectedTarget}).\n\nHot Reload and live emulator launch will be fully integrated into the 'Run' tab in an upcoming Flutter Compilation update.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EditorPage] BtnRun_Click failed: {ex.Message}");
            }
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

        private void ComboTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboTarget?.SelectedItem is ComboBoxItem item)
            {
                if (TxtTargetInfo != null)
                {
                    TxtTargetInfo.Text = $"Target: {item.Content}";
                }

                if (ComboDevice != null)
                {
                    ComboDevice.Items.Clear();
                    string tag = item.Tag?.ToString() ?? "";
                    
                    if (tag == "android")
                    {
                        // In a real implementation, we would run `flutter devices` or `emulator -list-avds`
                        ComboDevice.Items.Add(new ComboBoxItem { Content = "Pixel_4_API_30", Tag = "emulator-5554" });
                        ComboDevice.Items.Add(new ComboBoxItem { Content = "No other devices", Tag = "none", IsEnabled = false });
                    }
                    else if (tag == "web")
                    {
                        ComboDevice.Items.Add(new ComboBoxItem { Content = "Chrome", Tag = "chrome" });
                        ComboDevice.Items.Add(new ComboBoxItem { Content = "Edge", Tag = "edge" });
                    }
                    else if (tag == "windows")
                    {
                        ComboDevice.Items.Add(new ComboBoxItem { Content = "Windows (Desktop)", Tag = "windows" });
                    }
                    
                    if (ComboDevice.Items.Count > 0)
                        ComboDevice.SelectedIndex = 0;
                }
            }
        }

        private async void EditorTabs_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString() ?? "";

                if (tag == "event" && !_isProgrammaticTabChange)
                {
                    _currentEventContext = null;
                    PopulateEventList();
                    await LoadSavedWorkspaceAsync();
                }
                else if (tag == "event" && _isProgrammaticTabChange)
                {
                    PopulateEventList();
                }
                
                if (tag == "view" || tag == "event" || tag == "component")
                {
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
            var dialog = new ContentDialog
            {
                Title = "Page Configuration",
                Content = "Page configuration properties will appear here (Coming Soon).",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
                RequestedTheme = (App.MainWindow?.Content as FrameworkElement)?.RequestedTheme ?? ElementTheme.Default
            };
            await dialog.ShowAsync();
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
                                NativeDesigner.SetPageName(_currentPageName);
                                NativeDesigner.LoadUIJson(uiJson);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[EditorPage] Failed to inject UI state: {ex.Message}");
                        }

                        TxtSaveStatus.Text = $"Loaded {pageName}";
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
                    if (t == "button" || t == "image")
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
