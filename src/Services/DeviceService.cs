using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Snapcode.Models;

namespace Snapcode.Services
{
    public class DeviceInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsEmulator { get; set; }
        public bool IsRunning { get; set; }
    }

    public static class DeviceService
    {
        public static async Task<List<DeviceInfo>> GetRunningDevicesAsync()
        {
            var flutterDevs = await GetRunningFlutterDevicesAsync();
            return flutterDevs.Select(d => new DeviceInfo
            {
                Id = d.Id,
                Name = d.Name,
                IsEmulator = d.IsEmulator,
                IsRunning = d.IsRunning
            }).ToList();
        }

        public static async Task<List<DeviceInfo>> GetInstalledAvdsAsync()
        {
            var avds = await GetInstalledAndroidAvdsAsync();
            return avds.Select(d => new DeviceInfo
            {
                Id = d.Id,
                Name = d.Name,
                IsEmulator = true,
                IsRunning = d.IsRunning
            }).ToList();
        }
        public static async Task<string> GetAdbPathAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (!string.IsNullOrEmpty(config.AndroidSdkPath))
            {
                string adbPath = Path.Combine(config.AndroidSdkPath, "platform-tools", "adb.exe");
                if (File.Exists(adbPath)) return adbPath;
            }

            // Check standard LocalAppData Android SDK location
            string localAppDataSdk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "platform-tools", "adb.exe");
            if (File.Exists(localAppDataSdk)) return localAppDataSdk;

            string envHome = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "";
            if (!string.IsNullOrEmpty(envHome))
            {
                string envAdb = Path.Combine(envHome, "platform-tools", "adb.exe");
                if (File.Exists(envAdb)) return envAdb;
            }

            return "adb"; // Fallback to PATH
        }

        public static async Task<string> GetEmulatorPathAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (!string.IsNullOrEmpty(config.AndroidSdkPath))
            {
                string emuPath = Path.Combine(config.AndroidSdkPath, "emulator", "emulator.exe");
                if (File.Exists(emuPath)) return emuPath;
            }

            string localAppDataEmu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk", "emulator", "emulator.exe");
            if (File.Exists(localAppDataEmu)) return localAppDataEmu;

            string envHome = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "";
            if (!string.IsNullOrEmpty(envHome))
            {
                string envEmu = Path.Combine(envHome, "emulator", "emulator.exe");
                if (File.Exists(envEmu)) return envEmu;
            }

            return "emulator"; // Fallback to PATH
        }

        public static async Task<string> GetFlutterExePathAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (!string.IsNullOrEmpty(config.FlutterSdkPath))
            {
                string batPath = Path.Combine(config.FlutterSdkPath, "bin", "flutter.bat");
                if (File.Exists(batPath)) return batPath;
            }
            return "flutter";
        }

        /// <summary>
        /// Queries connected devices across all platforms (Chrome, Edge, Windows Desktop, Running Android phones/emulators)
        /// using 'flutter devices --machine'.
        /// </summary>
        public static async Task<List<FlutterDevice>> GetRunningFlutterDevicesAsync()
        {
            var devices = new List<FlutterDevice>();
            try
            {
                string flutterExe = await GetFlutterExePathAsync();
                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = "devices --machine",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        using var doc = JsonDocument.Parse(output);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in doc.RootElement.EnumerateArray())
                            {
                                string id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                                string name = item.TryGetProperty("name", out var nProp) ? nProp.GetString() ?? id : id;
                                string targetPlatform = item.TryGetProperty("targetPlatform", out var tpProp) ? tpProp.GetString() ?? "" : "";
                                bool isEmu = item.TryGetProperty("emulator", out var emuProp) && emuProp.GetBoolean();
                                string sdk = item.TryGetProperty("sdk", out var sProp) ? sProp.GetString() ?? "" : "";

                                DevicePlatform platform = DevicePlatform.Unknown;
                                if (targetPlatform.Contains("web", StringComparison.OrdinalIgnoreCase))
                                    platform = DevicePlatform.Web;
                                else if (targetPlatform.Contains("android", StringComparison.OrdinalIgnoreCase))
                                    platform = DevicePlatform.Android;
                                else if (targetPlatform.Contains("windows", StringComparison.OrdinalIgnoreCase))
                                    platform = DevicePlatform.Windows;

                                devices.Add(new FlutterDevice
                                {
                                    Id = id,
                                    Name = name,
                                    Platform = platform,
                                    IsEmulator = isEmu,
                                    IsRunning = true,
                                    Sdk = sdk,
                                    RawTargetPlatform = targetPlatform
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceService] GetRunningFlutterDevicesAsync failed: {ex.Message}");
            }

            // Ensure Windows desktop target is always available on Windows OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!devices.Any(d => d.Platform == DevicePlatform.Windows || string.Equals(d.Id, "windows", StringComparison.OrdinalIgnoreCase)))
                {
                    devices.Add(new FlutterDevice
                    {
                        Id = "windows",
                        Name = "Windows (Desktop)",
                        Platform = DevicePlatform.Windows,
                        IsRunning = true,
                        RawTargetPlatform = "windows-x64"
                    });
                    // Enable windows desktop in flutter config in background if it wasn't enabled
                    _ = EnsureWindowsDesktopConfigAsync();
                }
            }

            // Ensure standard web browser targets are always available
            if (!devices.Any(d => d.Platform == DevicePlatform.Web))
            {
                devices.Add(new FlutterDevice { Id = "chrome", Name = "Chrome", Platform = DevicePlatform.Web, IsRunning = true });
                devices.Add(new FlutterDevice { Id = "edge", Name = "Edge", Platform = DevicePlatform.Web, IsRunning = true });
            }

            return devices;
        }

        /// <summary>
        /// Ensures 'flutter config --enable-windows-desktop' has been executed.
        /// </summary>
        public static async Task EnsureWindowsDesktopConfigAsync()
        {
            try
            {
                string flutterExe = await GetFlutterExePathAsync();
                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = "config --enable-windows-desktop",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(startInfo);
                if (p != null)
                {
                    await p.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceService] EnsureWindowsDesktopConfigAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Discovers all installed Android Virtual Devices (AVDs).
        /// </summary>
        public static async Task<List<FlutterDevice>> GetInstalledAndroidAvdsAsync()
        {
            var avds = new List<FlutterDevice>();
            try
            {
                string emulatorPath = await GetEmulatorPathAsync();
                var startInfo = new ProcessStartInfo
                {
                    FileName = emulatorPath,
                    Arguments = "-list-avds",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("INFO", StringComparison.OrdinalIgnoreCase) || 
                            line.StartsWith("WARNING", StringComparison.OrdinalIgnoreCase) ||
                            line.StartsWith("error", StringComparison.OrdinalIgnoreCase))
                            continue;

                        avds.Add(new FlutterDevice
                        {
                            Id = line,
                            Name = line.Replace("_", " "),
                            Platform = DevicePlatform.Android,
                            IsEmulator = true,
                            IsRunning = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeviceService] GetInstalledAndroidAvdsAsync failed: {ex.Message}");
            }

            // Fallback check with 'flutter emulators'
            if (avds.Count == 0)
            {
                try
                {
                    string flutterExe = await GetFlutterExePathAsync();
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = flutterExe,
                        Arguments = "emulators",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var p = Process.Start(startInfo);
                    if (p != null)
                    {
                        string outText = await p.StandardOutput.ReadToEndAsync();
                        await p.WaitForExitAsync();
                        var lines = outText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var l in lines)
                        {
                            if (l.Contains("•") && !l.Contains("Id           •"))
                            {
                                var parts = l.Split('•').Select(x => x.Trim()).ToArray();
                                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[0]))
                                {
                                    avds.Add(new FlutterDevice
                                    {
                                        Id = parts[0],
                                        Name = parts.Length > 1 ? parts[1] : parts[0],
                                        Platform = DevicePlatform.Android,
                                        IsEmulator = true,
                                        IsRunning = false
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return avds;
        }

        /// <summary>
        /// Gets all devices and emulators, merging running devices with offline AVDs.
        /// </summary>
        public static async Task<List<FlutterDevice>> GetAllDevicesAndEmulatorsAsync()
        {
            var runningDevices = await GetRunningFlutterDevicesAsync();
            var installedAvds = await GetInstalledAndroidAvdsAsync();

            var result = new List<FlutterDevice>(runningDevices);

            foreach (var avd in installedAvds)
            {
                // Check if this AVD is already in runningDevices (e.g., emulator-5554 is matching this AVD)
                bool alreadyRunning = runningDevices.Any(d => d.Platform == DevicePlatform.Android && 
                    (d.Name.Equals(avd.Name, StringComparison.OrdinalIgnoreCase) || d.Id.Equals(avd.Id, StringComparison.OrdinalIgnoreCase)));
                
                if (!alreadyRunning)
                {
                    result.Add(avd);
                }
            }

            return result;
        }

        /// <summary>
        /// Launches an Android emulator AVD in the background.
        /// </summary>
        public static async Task<bool> LaunchAvdAsync(string avdId)
        {
            try
            {
                // Try launching via flutter emulators --launch <id> first
                string flutterExe = await GetFlutterExePathAsync();
                var startInfo = new ProcessStartInfo
                {
                    FileName = flutterExe,
                    Arguments = $"emulators --launch \"{avdId}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var p = Process.Start(startInfo);
                if (p != null)
                {
                    return true;
                }
            }
            catch
            {
                // Fallback to direct emulator executable
                try
                {
                    string emulatorPath = await GetEmulatorPathAsync();
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = emulatorPath,
                        Arguments = $"-avd \"{avdId}\" -netdelay none -netspeed full",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeviceService] Failed to launch AVD: {ex.Message}");
                }
            }

            return false;
        }
    }
}
