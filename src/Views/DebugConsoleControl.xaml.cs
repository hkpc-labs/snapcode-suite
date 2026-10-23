using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Snapcode.Models;
using Snapcode.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace Snapcode.Views
{
    public sealed partial class DebugConsoleControl : UserControl
    {
        private readonly ObservableCollection<DebugLogEntry> _filteredLogs = new();
        private bool _isControlLoaded = false;
        public event EventHandler? CloseRequested;
        public event EventHandler? PopOutRequested;

        public DebugConsoleControl()
        {
            try
            {
                this.InitializeComponent();
                ListLogs.ItemsSource = _filteredLogs;

                Loaded += DebugConsoleControl_Loaded;
                Unloaded += DebugConsoleControl_Unloaded;
            }
            catch (Exception ex)
            {
                try
                {
                    string wsCrashLog = @"c:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\app_crash.log";
                    System.IO.File.AppendAllText(wsCrashLog, $"[{DateTime.Now}] DEBUG_CONSOLE_CTOR EXCEPTION: {ex}\r\nInner: {ex.InnerException}\r\n\r\n");
                }
                catch { }
                throw;
            }
        }

        private void DebugConsoleControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isControlLoaded = true;
            var service = FlutterExecutionService.Instance;
            service.Initialize(this.DispatcherQueue);

            service.LogReceived += Service_LogReceived;
            service.StateChanged += Service_StateChanged;
            service.DevToolsUrlDiscovered += Service_DevToolsUrlDiscovered;

            UpdateStateUI(service.CurrentState);
            RefreshFilter();
        }

        private void DebugConsoleControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isControlLoaded = false;
            var service = FlutterExecutionService.Instance;
            service.LogReceived -= Service_LogReceived;
            service.StateChanged -= Service_StateChanged;
            service.DevToolsUrlDiscovered -= Service_DevToolsUrlDiscovered;
        }

        private void Service_LogReceived(object? sender, DebugLogEntry entry)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                TxtErrorCount.Text = $"{FlutterExecutionService.Instance.ErrorCount} Errors";
                TxtWarningCount.Text = $"{FlutterExecutionService.Instance.WarningCount} Warnings";

                if (MatchesFilter(entry))
                {
                    _filteredLogs.Add(entry);
                    if (ToggleAutoScroll.IsChecked == true && _filteredLogs.Count > 0)
                    {
                        ListLogs.ScrollIntoView(_filteredLogs[^1]);
                    }
                }
            });
        }

        private void Service_StateChanged(object? sender, ExecutionState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateStateUI(state);
            });
        }

        private void Service_DevToolsUrlDiscovered(object? sender, string url)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (!string.IsNullOrEmpty(url))
                {
                    BtnDevTools.Visibility = Visibility.Visible;
                }
            });
        }

        private void UpdateStateUI(ExecutionState state)
        {
            if (!_isControlLoaded || BtnHotReload == null) return;

            bool isRunning = state != ExecutionState.Idle;

            BtnHotReload.IsEnabled = isRunning && state != ExecutionState.Starting && state != ExecutionState.Building;
            BtnHotRestart.IsEnabled = isRunning && state != ExecutionState.Starting && state != ExecutionState.Building;
            BtnStop.IsEnabled = isRunning;
            TxtCommand.IsEnabled = isRunning;
            BtnSendCommand.IsEnabled = isRunning;

            switch (state)
            {
                case ExecutionState.Idle:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 158, 158, 158));
                    TxtStatus.Text = "Idle";
                    BtnDevTools.Visibility = Visibility.Collapsed;
                    break;
                case ExecutionState.Starting:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 179, 0));
                    TxtStatus.Text = "Starting...";
                    break;
                case ExecutionState.Building:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 179, 0));
                    TxtStatus.Text = "Building...";
                    break;
                case ExecutionState.Running:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80));
                    TxtStatus.Text = "Running";
                    break;
                case ExecutionState.Reloading:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 188, 212));
                    TxtStatus.Text = "Hot Reloading...";
                    break;
                case ExecutionState.Restarting:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 152, 0));
                    TxtStatus.Text = "Hot Restarting...";
                    break;
                case ExecutionState.Stopping:
                    DotStatus.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54));
                    TxtStatus.Text = "Stopping...";
                    break;
            }
        }

        private bool MatchesFilter(DebugLogEntry entry)
        {
            if (ComboFilter == null) return true;
            string filterTag = (ComboFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "all";
            string searchText = TxtSearch?.Text?.Trim() ?? "";

            bool typeMatches = filterTag switch
            {
                "errors" => entry.Type == LogEntryType.Error,
                "warnings" => entry.Type == LogEntryType.Warning || entry.Type == LogEntryType.Error,
                "reloads" => entry.Type == LogEntryType.Reload || entry.Type == LogEntryType.Success,
                _ => true
            };

            if (!typeMatches) return false;

            if (!string.IsNullOrEmpty(searchText))
            {
                return entry.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private void RefreshFilter()
        {
            if (!_isControlLoaded || ListLogs == null) return;

            _filteredLogs.Clear();
            var allLogs = FlutterExecutionService.Instance.Logs;
            foreach (var log in allLogs)
            {
                if (MatchesFilter(log))
                {
                    _filteredLogs.Add(log);
                }
            }

            if (ToggleAutoScroll?.IsChecked == true && _filteredLogs.Count > 0)
            {
                ListLogs.ScrollIntoView(_filteredLogs[^1]);
            }
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isControlLoaded) return;
            RefreshFilter();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isControlLoaded) return;
            RefreshFilter();
        }

        private async void BtnHotReload_Click(object sender, RoutedEventArgs e)
        {
            await FlutterExecutionService.Instance.HotReloadAsync();
        }

        private async void BtnHotRestart_Click(object sender, RoutedEventArgs e)
        {
            await FlutterExecutionService.Instance.HotRestartAsync();
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            await FlutterExecutionService.Instance.StopAsync();
        }

        private async void BtnDevTools_Click(object sender, RoutedEventArgs e)
        {
            string? url = FlutterExecutionService.Instance.DevToolsUrl ?? FlutterExecutionService.Instance.AppUrl;
            if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                await Launcher.LaunchUriAsync(uri);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            FlutterExecutionService.Instance.ClearLogs();
            _filteredLogs.Clear();
            TxtErrorCount.Text = "0 Errors";
            TxtWarningCount.Text = "0 Warnings";
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var log in _filteredLogs)
            {
                sb.AppendLine($"[{log.FormattedTime}] [{log.Type}] {log.Message}");
            }

            var package = new DataPackage();
            package.SetText(sb.ToString());
            Clipboard.SetContent(package);
        }

        private void BtnPopOut_Click(object sender, RoutedEventArgs e)
        {
            PopOutRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private async void BtnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            string cmd = TxtCommand.Text;
            if (!string.IsNullOrWhiteSpace(cmd))
            {
                TxtCommand.Text = "";
                await FlutterExecutionService.Instance.SendCommandAsync(cmd);
            }
        }

        private async void TxtCommand_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string cmd = TxtCommand.Text;
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    TxtCommand.Text = "";
                    await FlutterExecutionService.Instance.SendCommandAsync(cmd);
                }
                e.Handled = true;
            }
        }
    }
}
