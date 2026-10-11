using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private static async Task<string> GetAdbPathAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (!string.IsNullOrEmpty(config.AndroidSdkPath))
            {
                string adbPath = Path.Combine(config.AndroidSdkPath, "platform-tools", "adb.exe");
                if (File.Exists(adbPath)) return adbPath;
            }
            return "adb"; // Fallback to PATH
        }

        private static async Task<string> GetEmulatorPathAsync()
        {
            var config = await ConfigService.LoadAsync();
            if (!string.IsNullOrEmpty(config.AndroidSdkPath))
            {
                string emuPath = Path.Combine(config.AndroidSdkPath, "emulator", "emulator.exe");
                if (File.Exists(emuPath)) return emuPath;
            }
            return "emulator"; // Fallback to PATH
        }

        public static async Task<List<DeviceInfo>> GetRunningDevicesAsync()
        {
            var devices = new List<DeviceInfo>();
            try
            {
                string adbPath = await GetAdbPathAsync();
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = "devices -l",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var line in lines.Skip(1)) // Skip "List of devices attached"
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && parts[1] == "device")
                        {
                            string id = parts[0];
                            string name = id;
                            
                            // Try to extract model name
                            var modelPart = parts.FirstOrDefault(p => p.StartsWith("model:"));
                            if (modelPart != null)
                            {
                                name = modelPart.Substring(6).Replace("_", " ");
                            }

                            devices.Add(new DeviceInfo
                            {
                                Id = id,
                                Name = name,
                                IsEmulator = id.StartsWith("emulator-"),
                                IsRunning = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get running devices: {ex.Message}");
            }
            return devices;
        }

        public static async Task<List<DeviceInfo>> GetInstalledAvdsAsync()
        {
            var avds = new List<DeviceInfo>();
            try
            {
                string emulatorPath = await GetEmulatorPathAsync();
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = emulatorPath,
                    Arguments = "-list-avds",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (line.StartsWith("INFO") || line.StartsWith("WARNING")) continue;
                        
                        avds.Add(new DeviceInfo
                        {
                            Id = line,
                            Name = line.Replace("_", " "),
                            IsEmulator = true,
                            IsRunning = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to list AVDs: {ex.Message}");
            }
            return avds;
        }

        public static async Task LaunchAvdAsync(string avdName)
        {
            try
            {
                string emulatorPath = await GetEmulatorPathAsync();
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = emulatorPath,
                    Arguments = $"-avd \"{avdName}\" -netdelay none -netspeed full",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch AVD: {ex.Message}");
                throw;
            }
        }
    }
}
