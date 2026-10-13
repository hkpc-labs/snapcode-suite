using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Snapcode.Models;

namespace Snapcode.Services
{
    public enum ExecutionState
    {
        Idle,
        Starting,
        Building,
        Running,
        Reloading,
        Restarting,
        Stopping
    }

    public class FlutterExecutionService
    {
        private static FlutterExecutionService? _instance;
        public static FlutterExecutionService Instance => _instance ??= new FlutterExecutionService();

        private Process? _flutterProcess;
        private StreamWriter? _stdinWriter;
        private CancellationTokenSource? _runCts;
        private DispatcherQueue? _dispatcherQueue;

        public ExecutionState CurrentState { get; private set; } = ExecutionState.Idle;
        public bool IsRunning => _flutterProcess != null && !_flutterProcess.HasExited && CurrentState != ExecutionState.Idle;

        public ObservableCollection<DebugLogEntry> Logs { get; } = new();

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public string? DevToolsUrl { get; private set; }
        public string? AppUrl { get; private set; }

        public event EventHandler<ExecutionState>? StateChanged;
        public event EventHandler<DebugLogEntry>? LogReceived;
        public event EventHandler<string>? DevToolsUrlDiscovered;

        private static readonly Regex DevToolsRegex = new Regex(@"The Flutter DevTools debugger and profiler.*is available at:\s*(http://\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ObservatoryRegex = new Regex(@"An Observatory debugger.*is available at:\s*(http://\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WebAppUrlRegex = new Regex(@"Serving at\s*(http://localhost:\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Initialize(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public async Task<bool> StartRunAsync(string projectPath, FlutterDevice device)
        {
            if (IsRunning)
            {
                await StopAsync();
            }

            SetState(ExecutionState.Starting);
            AddLog("==================================================", LogEntryType.System);
            AddLog($"Starting Flutter Application on {device.DisplayName}...", LogEntryType.System);
            AddLog("==================================================", LogEntryType.System);

            // Locate flutter directory inside project
            string flutterDir = Path.Combine(projectPath, ProjectService.FlutterFolderName);
            if (!Directory.Exists(flutterDir))
            {
                flutterDir = projectPath;
            }

            // Sync assets from SnapcodeAssets to flutter/assets and ensure pubspec.yaml includes them
            AssetService.SyncAssetsToFlutterProject(projectPath);

            // Ensure target platform files (windows/, android/, web/) exist in project
            if (device.Platform == DevicePlatform.Windows && !Directory.Exists(Path.Combine(flutterDir, "windows")))
            {
                AddLog("Configuring Windows desktop platform support in project ('flutter create --platforms windows .')...", LogEntryType.System);
                await EnsurePlatformSupportAsync(flutterDir, "windows");
            }
            else if (device.Platform == DevicePlatform.Android && !Directory.Exists(Path.Combine(flutterDir, "android")))
            {
                AddLog("Configuring Android platform support in project ('flutter create --platforms android .')...", LogEntryType.System);
                await EnsurePlatformSupportAsync(flutterDir, "android");
            }
            else if (device.Platform == DevicePlatform.Web && !Directory.Exists(Path.Combine(flutterDir, "web")))
            {
                AddLog("Configuring Web platform support in project ('flutter create --platforms web .')...", LogEntryType.System);
                await EnsurePlatformSupportAsync(flutterDir, "web");
            }

            // Check if device is an offline Android emulator and boot it if needed
            if (device.Platform == DevicePlatform.Android && device.IsEmulator && !device.IsRunning)
            {
                AddLog($"Launching Android Emulator '{device.Name}' in background...", LogEntryType.System);
                bool launched = await DeviceService.LaunchAvdAsync(device.Id);
                if (launched)
                {
                    AddLog("Waiting for Android emulator to boot and register with ADB...", LogEntryType.Info);
                    // Give emulator a short window to boot
                    await Task.Delay(4000);
                }
            }

            try
            {
                string flutterExe = await DeviceService.GetFlutterExePathAsync();
                string arguments = $"run -d \"{device.Id}\"";

                AddLog($"Executing: {flutterExe} {arguments} in {flutterDir}", LogEntryType.System);

                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = arguments,
                    WorkingDirectory = flutterDir,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                _runCts = new CancellationTokenSource();
                _flutterProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                _flutterProcess.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        ProcessOutputLine(e.Data, isErrorStream: false);
                    }
                };

                _flutterProcess.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        ProcessOutputLine(e.Data, isErrorStream: true);
                    }
                };

                _flutterProcess.Exited += (s, e) =>
                {
                    AddLog($"Application process terminated (Exit code: {_flutterProcess?.ExitCode ?? 0})", LogEntryType.System);
                    SetState(ExecutionState.Idle);
                    _stdinWriter = null;
                };

                bool started = _flutterProcess.Start();
                if (!started)
                {
                    AddLog("Failed to start Flutter process.", LogEntryType.Error);
                    SetState(ExecutionState.Idle);
                    return false;
                }

                _stdinWriter = _flutterProcess.StandardInput;
                _flutterProcess.BeginOutputReadLine();
                _flutterProcess.BeginErrorReadLine();

                SetState(ExecutionState.Building);
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Exception while launching Flutter: {ex.Message}", LogEntryType.Error);
                SetState(ExecutionState.Idle);
                return false;
            }
        }

        private void ProcessOutputLine(string rawLine, bool isErrorStream)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) return;

            string line = rawLine.Trim();

            // Detect DevTools / Web URLs
            var devToolsMatch = DevToolsRegex.Match(line);
            if (devToolsMatch.Success)
            {
                DevToolsUrl = devToolsMatch.Groups[1].Value;
                DevToolsUrlDiscovered?.Invoke(this, DevToolsUrl);
            }
            var obsMatch = ObservatoryRegex.Match(line);
            if (obsMatch.Success && string.IsNullOrEmpty(DevToolsUrl))
            {
                DevToolsUrl = obsMatch.Groups[1].Value;
                DevToolsUrlDiscovered?.Invoke(this, DevToolsUrl);
            }
            var webMatch = WebAppUrlRegex.Match(line);
            if (webMatch.Success)
            {
                AppUrl = webMatch.Groups[1].Value;
            }

            // Detect State transitions
            if (line.Contains("Flutter run key commands", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Syncing files to device", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Application finished", StringComparison.OrdinalIgnoreCase))
            {
                if (CurrentState != ExecutionState.Running)
                {
                    SetState(ExecutionState.Running);
                }
            }
            else if (line.Contains("Performing hot reload", StringComparison.OrdinalIgnoreCase))
            {
                SetState(ExecutionState.Reloading);
            }
            else if (line.Contains("Performing hot restart", StringComparison.OrdinalIgnoreCase))
            {
                SetState(ExecutionState.Restarting);
            }
            else if (line.Contains("Reloaded", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Restarted application", StringComparison.OrdinalIgnoreCase))
            {
                SetState(ExecutionState.Running);
            }

            // Determine entry type
            LogEntryType type = LogEntryType.Info;

            // 1. First, check if the line is explicitly a Warning (even if emitted to stderr by Java/Gradle/Dart)
            if (line.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("WARN:", StringComparison.OrdinalIgnoreCase) ||
                line.Contains(" warning:", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("WARNING: ", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("deprecated", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Note:", StringComparison.OrdinalIgnoreCase))
            {
                type = LogEntryType.Warning;
                WarningCount++;
            }
            // 2. Check if it's a Reload or Success
            else if (line.Contains("Hot reload performed", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Reloaded ", StringComparison.OrdinalIgnoreCase))
            {
                type = LogEntryType.Reload;
            }
            else if (line.Contains("Built ", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Restarted application", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Syncing files to device", StringComparison.OrdinalIgnoreCase))
            {
                type = LogEntryType.Success;
            }
            // 3. Check if it's an Error
            else if (line.StartsWith("Error:", StringComparison.OrdinalIgnoreCase) ||
                     line.StartsWith("Exception:", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("FAILURE: Build failed", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Dart compiler error", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("Target file \"lib/main.dart\" not found", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("error MSB", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("fatal error", StringComparison.OrdinalIgnoreCase) ||
                     (isErrorStream && (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                                        line.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
                                        line.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                                        line.Contains("failure", StringComparison.OrdinalIgnoreCase) ||
                                        line.Contains("crash", StringComparison.OrdinalIgnoreCase))))
            {
                type = LogEntryType.Error;
                ErrorCount++;
            }
            else
            {
                type = LogEntryType.Info;
            }

            AddLog(line, type);
        }

        public async Task<bool> HotReloadAsync()
        {
            if (!IsRunning || _stdinWriter == null)
            {
                AddLog("Cannot hot reload: application is not currently running.", LogEntryType.Warning);
                return false;
            }

            try
            {
                SetState(ExecutionState.Reloading);
                AddLog("⚡ Triggering Hot Reload (r)...", LogEntryType.Reload);
                await _stdinWriter.WriteLineAsync("r");
                await _stdinWriter.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Hot reload failed: {ex.Message}", LogEntryType.Error);
                return false;
            }
        }

        public async Task<bool> HotRestartAsync()
        {
            if (!IsRunning || _stdinWriter == null)
            {
                AddLog("Cannot hot restart: application is not currently running.", LogEntryType.Warning);
                return false;
            }

            try
            {
                SetState(ExecutionState.Restarting);
                AddLog("🔄 Triggering Hot Restart (R)...", LogEntryType.Reload);
                await _stdinWriter.WriteLineAsync("R");
                await _stdinWriter.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Hot restart failed: {ex.Message}", LogEntryType.Error);
                return false;
            }
        }

        public async Task<bool> SendCommandAsync(string command)
        {
            if (!IsRunning || _stdinWriter == null || string.IsNullOrWhiteSpace(command))
                return false;

            try
            {
                await _stdinWriter.WriteLineAsync(command.Trim());
                await _stdinWriter.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Failed to send command '{command}': {ex.Message}", LogEntryType.Error);
                return false;
            }
        }

        public async Task StopAsync()
        {
            if (!IsRunning && _flutterProcess == null) return;

            SetState(ExecutionState.Stopping);
            AddLog("Stopping application...", LogEntryType.System);

            try
            {
                if (_stdinWriter != null)
                {
                    await _stdinWriter.WriteLineAsync("q");
                    await _stdinWriter.FlushAsync();
                }

                // Wait up to 3 seconds for graceful exit
                if (_flutterProcess != null && !_flutterProcess.HasExited)
                {
                    using var cts = new CancellationTokenSource(3000);
                    try
                    {
                        await _flutterProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (!_flutterProcess.HasExited)
                        {
                            _flutterProcess.Kill(entireProcessTree: true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlutterExecutionService] Error stopping process: {ex.Message}");
            }
            finally
            {
                _flutterProcess = null;
                _stdinWriter = null;
                SetState(ExecutionState.Idle);
            }
        }

        public void ClearLogs()
        {
            void Clear()
            {
                Logs.Clear();
                ErrorCount = 0;
                WarningCount = 0;
            }

            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(Clear);
            }
            else
            {
                Clear();
            }
        }

        private void AddLog(string message, LogEntryType type)
        {
            var entry = new DebugLogEntry
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            };

            void Append()
            {
                Logs.Add(entry);
                // Keep history reasonable
                if (Logs.Count > 2000)
                {
                    Logs.RemoveAt(0);
                }
                LogReceived?.Invoke(this, entry);
            }

            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(Append);
            }
            else
            {
                Append();
            }
        }

        private void SetState(ExecutionState state)
        {
            CurrentState = state;
            void Fire() => StateChanged?.Invoke(this, state);

            if (_dispatcherQueue != null)
            {
                _dispatcherQueue.TryEnqueue(Fire);
            }
            else
            {
                Fire();
            }
        }

        public static async Task EnsurePlatformSupportAsync(string flutterDir, string platform)
        {
            try
            {
                string flutterExe = await DeviceService.GetFlutterExePathAsync();
                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = $"create --platforms {platform} .",
                    WorkingDirectory = flutterDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlutterExecutionService] EnsurePlatformSupportAsync error: {ex.Message}");
            }
        }
    }
}
