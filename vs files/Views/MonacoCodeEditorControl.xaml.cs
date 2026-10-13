using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Snapcode.Services;

namespace Snapcode.Views
{
    public class EditorTabModel : INotifyPropertyChanged
    {
        private string _filePath;
        private string _fileName;
        private bool _isDirty;
        private string _cachedContent = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditorTabModel(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileName(filePath);
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    _fileName = Path.GetFileName(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(IconGlyph));
                    OnPropertyChanged(nameof(IconBrush));
                }
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DirtyVisibility));
                }
            }
        }

        public string CachedContent
        {
            get => _cachedContent;
            set => _cachedContent = value;
        }

        public Visibility DirtyVisibility => _isDirty ? Visibility.Visible : Visibility.Collapsed;

        public string IconGlyph
        {
            get
            {
                string ext = Path.GetExtension(FilePath).ToLowerInvariant();
                return ext switch
                {
                    ".dart" => "\uE790",
                    ".yaml" or ".yml" => "\uE7B8",
                    ".json" => "\uE943",
                    ".md" => "\uE8A5",
                    ".png" or ".jpg" or ".jpeg" => "\uEB9F",
                    _ => "\uE8A5"
                };
            }
        }

        public SolidColorBrush IconBrush
        {
            get
            {
                string ext = Path.GetExtension(FilePath).ToLowerInvariant();
                return ext switch
                {
                    ".dart" => new SolidColorBrush(ColorHelper.FromArgb(255, 0, 180, 171)),
                    ".yaml" or ".yml" => new SolidColorBrush(ColorHelper.FromArgb(255, 203, 56, 55)),
                    ".json" => new SolidColorBrush(ColorHelper.FromArgb(255, 203, 203, 65)),
                    ".md" => new SolidColorBrush(ColorHelper.FromArgb(255, 81, 154, 186)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public sealed partial class MonacoCodeEditorControl : UserControl
    {
        private bool _isWebViewReady = false;
        private bool _isNativeFallbackActive = false;
        private EditorTabModel? _activeTab = null;
        private bool _isProgrammaticTabSwitch = false;

        public ObservableCollection<EditorTabModel> Tabs { get; } = new();

        public bool HasUnsavedChanges => Tabs.Any(t => t.IsDirty);

        public event EventHandler<string>? FileSaved;

        public MonacoCodeEditorControl()
        {
            this.InitializeComponent();
            TabListView.ItemsSource = Tabs;
            this.Loaded += MonacoCodeEditorControl_Loaded;
        }

        private async void MonacoCodeEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeEditorAsync();
        }

        public async Task InitializeEditorAsync()
        {
            if (_isWebViewReady || _isNativeFallbackActive) return;

            try
            {
                await MonacoWebView.EnsureCoreWebView2Async();
                MonacoWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                string htmlPath = Path.Combine(AppContext.BaseDirectory, "Assets", "MonacoEditor", "index.html");
                if (File.Exists(htmlPath))
                {
                    MonacoWebView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
                }
                else
                {
                    ActivateNativeFallback("Assets/MonacoEditor/index.html not found.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MonacoEditor] Failed to initialize WebView2: {ex.Message}");
                ActivateNativeFallback(ex.Message);
            }
        }

        private void ActivateNativeFallback(string reason)
        {
            _isNativeFallbackActive = true;
            EditorLoadingPanel.Visibility = Visibility.Collapsed;
            MonacoWebView.Visibility = Visibility.Collapsed;
            NativeFallbackTextBox.Visibility = Visibility.Visible;
            TxtEngineMode.Text = "Native Editor";
            TxtEditorStatus.Text = $"Using Native Editor ({reason})";
        }

        private void CoreWebView2_WebMessageReceived(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                string jsonString = args.WebMessageAsJson;
                string type = "";
                string mode = "Monaco";
                string content = "";

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("type", out var tProp))
                    {
                        type = tProp.GetString() ?? "";
                    }
                    if (root.TryGetProperty("mode", out var mProp))
                    {
                        mode = mProp.GetString() ?? "Monaco";
                    }
                    if (root.TryGetProperty("content", out var cProp))
                    {
                        content = cProp.GetString() ?? "";
                    }
                }

                switch (type)
                {
                    case "editorReady":
                        _isWebViewReady = true;
                        string capturedMode = mode;
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            EditorLoadingPanel.Visibility = Visibility.Collapsed;
                            MonacoWebView.Visibility = Visibility.Visible;
                            TxtEngineMode.Text = capturedMode == "monaco" ? "Monaco (VS Code)" : "Offline Code Editor";
                            TxtEditorStatus.Text = "Ready";

                            // If a file was waiting to be displayed
                            if (_activeTab != null)
                            {
                                SendActiveTabToEditor();
                            }
                        });
                        break;

                    case "contentChanged":
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            if (_activeTab != null)
                            {
                                _activeTab.IsDirty = true;
                                StatusSaveIcon.Glyph = "\uEA3B";
                                TxtEditorStatus.Text = "Unsaved changes";
                            }
                        });
                        break;

                    case "saveRequested":
                        string capturedContent = content;
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SaveActiveTabAsync(capturedContent);
                        });
                        break;

                    case "contentSaved":
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            if (_activeTab != null)
                            {
                                _activeTab.IsDirty = false;
                                StatusSaveIcon.Glyph = "\uE73E";
                                TxtEditorStatus.Text = $"Saved at {DateTime.Now:HH:mm:ss}";
                            }
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MonacoEditor] WebMessage error: {ex.Message}");
            }
        }

        public async Task OpenFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;

            // Ensure editor initialized
            if (!_isWebViewReady && !_isNativeFallbackActive)
            {
                await InitializeEditorAsync();
            }

            // Check if already open
            var existing = Tabs.FirstOrDefault(t => string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (!existing.IsDirty && File.Exists(filePath))
                {
                    try
                    {
                        existing.CachedContent = await File.ReadAllTextAsync(filePath);
                    }
                    catch { }
                }
                _isProgrammaticTabSwitch = true;
                TabListView.SelectedItem = existing;
                _isProgrammaticTabSwitch = false;
                _activeTab = existing;
                UpdateBreadcrumb(existing.FilePath);
                SendActiveTabToEditor();
                return;
            }

            // Read from disk
            string fileContent = await File.ReadAllTextAsync(filePath);
            var newTab = new EditorTabModel(filePath)
            {
                CachedContent = fileContent,
                IsDirty = false
            };

            Tabs.Add(newTab);
            _isProgrammaticTabSwitch = true;
            TabListView.SelectedItem = newTab;
            _isProgrammaticTabSwitch = false;

            _activeTab = newTab;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            UpdateBreadcrumb(filePath);
            SendActiveTabToEditor();
        }

        private async void TabListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isProgrammaticTabSwitch) return;

            if (TabListView.SelectedItem is EditorTabModel tab)
            {
                // Cache content of outgoing tab if available
                if (_activeTab != null && _activeTab != tab)
                {
                    await CacheCurrentEditorContentAsync(_activeTab);
                }

                _activeTab = tab;
                UpdateBreadcrumb(tab.FilePath);
                SendActiveTabToEditor();
            }
            else if (Tabs.Count == 0)
            {
                _activeTab = null;
                EmptyStatePanel.Visibility = Visibility.Visible;
                TxtBreadcrumb.Text = "No file open";
            }
        }

        private void SendActiveTabToEditor()
        {
            if (_activeTab == null) return;

            string ext = Path.GetExtension(_activeTab.FilePath).ToLowerInvariant();
            TxtLanguageMode.Text = ext switch
            {
                ".dart" => "Dart",
                ".json" => "JSON",
                ".yaml" or ".yml" => "YAML",
                ".md" => "Markdown",
                _ => "Plain Text"
            };

            if (_isWebViewReady)
            {
                string safeName = JsonSerializer.Serialize(_activeTab.FileName);
                string safeContent = JsonSerializer.Serialize(_activeTab.CachedContent);
                string safeLang = JsonSerializer.Serialize(TxtLanguageMode.Text.ToLowerInvariant());

                _ = MonacoWebView.CoreWebView2.ExecuteScriptAsync($"window.openFile({safeName}, {safeContent}, {safeLang})");
            }
            else if (_isNativeFallbackActive)
            {
                NativeFallbackTextBox.Text = _activeTab.CachedContent;
            }
        }

        private async Task CacheCurrentEditorContentAsync(EditorTabModel tab)
        {
            if (tab == null) return;

            if (_isWebViewReady)
            {
                try
                {
                    string contentJson = await MonacoWebView.CoreWebView2.ExecuteScriptAsync("window.getContent()");
                    if (contentJson.StartsWith("\"") && contentJson.EndsWith("\""))
                    {
                        tab.CachedContent = JsonSerializer.Deserialize<string>(contentJson) ?? "";
                    }
                    else
                    {
                        tab.CachedContent = contentJson;
                    }
                }
                catch { }
            }
            else if (_isNativeFallbackActive)
            {
                tab.CachedContent = NativeFallbackTextBox.Text;
            }
        }

        public async Task SaveCurrentFileAsync()
        {
            if (_activeTab == null) return;

            string content = "";
            if (_isWebViewReady)
            {
                try
                {
                    string contentJson = await MonacoWebView.CoreWebView2.ExecuteScriptAsync("window.getContent()");
                    if (contentJson.StartsWith("\"") && contentJson.EndsWith("\""))
                    {
                        content = JsonSerializer.Deserialize<string>(contentJson) ?? "";
                    }
                    else
                    {
                        content = contentJson;
                    }
                }
                catch
                {
                    content = _activeTab.CachedContent;
                }
            }
            else if (_isNativeFallbackActive)
            {
                content = NativeFallbackTextBox.Text;
            }

            await SaveActiveTabAsync(content);
        }

        private async Task SaveActiveTabAsync(string content)
        {
            if (_activeTab == null) return;

            try
            {
                string filePath = _activeTab.FilePath;
                string ext = Path.GetExtension(filePath).ToLowerInvariant();

                if (ext == ".dart")
                {
                    // Non-destructive preservation save
                    await UserCodePreservationService.SaveDartFileWithPreservationAsync(filePath, content);
                }
                else
                {
                    await File.WriteAllTextAsync(filePath, content);
                }

                _activeTab.CachedContent = content;
                _activeTab.IsDirty = false;
                StatusSaveIcon.Glyph = "\uE73E";
                TxtEditorStatus.Text = $"Saved at {DateTime.Now:HH:mm:ss}";

                if (_isWebViewReady)
                {
                    await MonacoWebView.CoreWebView2.ExecuteScriptAsync("window.markSaved()");
                }

                FileSaved?.Invoke(this, filePath);
            }
            catch (Exception ex)
            {
                StatusSaveIcon.Glyph = "\uEA39";
                TxtEditorStatus.Text = $"Save error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[MonacoEditor] Save error: {ex.Message}");
            }
        }

        public async Task SaveAllFilesAsync()
        {
            foreach (var tab in Tabs.Where(t => t.IsDirty).ToList())
            {
                if (tab == _activeTab)
                {
                    await SaveCurrentFileAsync();
                }
                else
                {
                    string ext = Path.GetExtension(tab.FilePath).ToLowerInvariant();
                    if (ext == ".dart")
                    {
                        await UserCodePreservationService.SaveDartFileWithPreservationAsync(tab.FilePath, tab.CachedContent);
                    }
                    else
                    {
                        await File.WriteAllTextAsync(tab.FilePath, tab.CachedContent);
                    }
                    tab.IsDirty = false;
                }
            }
        }

        private async void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filePath)
            {
                var tab = Tabs.FirstOrDefault(t => string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
                if (tab != null)
                {
                    await CloseTabAsync(tab);
                }
            }
        }

        public async Task CloseTabAsync(EditorTabModel tab)
        {
            if (tab.IsDirty)
            {
                var dialog = new ContentDialog
                {
                    Title = "Unsaved Changes",
                    Content = $"Do you want to save changes to '{tab.FileName}' before closing?",
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Don't Save",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await CacheCurrentEditorContentAsync(tab);
                    await SaveActiveTabAsync(tab.CachedContent);
                }
                else if (result == ContentDialogResult.None)
                {
                    return; // Cancelled
                }
            }

            int index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            if (_activeTab == tab)
            {
                if (Tabs.Count > 0)
                {
                    int nextIndex = Math.Min(index, Tabs.Count - 1);
                    TabListView.SelectedItem = Tabs[nextIndex];
                }
                else
                {
                    _activeTab = null;
                    EmptyStatePanel.Visibility = Visibility.Visible;
                    TxtBreadcrumb.Text = "No file open";
                    TxtEditorStatus.Text = "Ready";
                }
            }
        }

        private async void BtnSaveTab_Click(object sender, RoutedEventArgs e)
        {
            await SaveCurrentFileAsync();
        }

        private async void BtnCloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            var tabList = Tabs.ToList();
            foreach (var tab in tabList)
            {
                await CloseTabAsync(tab);
            }
        }

        private void NativeFallbackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_activeTab != null)
            {
                _activeTab.IsDirty = true;
                _activeTab.CachedContent = NativeFallbackTextBox.Text;
                StatusSaveIcon.Glyph = "\uEA3B";
                TxtEditorStatus.Text = "Unsaved changes";
            }
        }

        private async void NativeFallbackTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.S)
            {
                var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                if ((ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
                {
                    e.Handled = true;
                    await SaveCurrentFileAsync();
                }
            }
        }

        private void UpdateBreadcrumb(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath) ?? "";
            string name = Path.GetFileName(filePath);
            TxtBreadcrumb.Text = $"{dir} > {name}";
        }

        public void SetTheme(string themeMode)
        {
            if (_isWebViewReady)
            {
                string safe = JsonSerializer.Serialize(themeMode);
                _ = MonacoWebView.CoreWebView2.ExecuteScriptAsync($"window.setTheme({safe})");
            }
        }
    }
}
