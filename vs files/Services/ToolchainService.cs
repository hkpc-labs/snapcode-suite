using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using Snapcode.Models;

namespace Snapcode.Services
{
    public class ToolchainService
    {
        public static List<DiagnosticItem> CreateDiagnosticItems()
        {
            return new List<DiagnosticItem>
            {
                new DiagnosticItem
                {
                    Id = "git",
                    DisplayName = "Git",
                    Description = "Version control system required by Flutter for package management.",
                    IconGlyph = "\uF08C",
                    Requirement = ToolRequirement.Critical,
                    CanBrowse = true,
                    PrimaryFix = FixAction.InstallViaWinget,
                    WingetPackageId = "Git.Git",
                    DownloadUrl = "https://git-scm.com/download/win"
                },
                new DiagnosticItem
                {
                    Id = "flutter",
                    DisplayName = "Flutter SDK",
                    Description = "Core framework for compiling visual blocks into cross-platform applications.",
                    IconGlyph = "\uE943",
                    Requirement = ToolRequirement.Critical,
                    CanBrowse = true,
                    PrimaryFix = FixAction.AddToPath,
                    DownloadUrl = "https://docs.flutter.dev/get-started/install/windows"
                },
                new DiagnosticItem
                {
                    Id = "webview2",
                    DisplayName = "Edge WebView2 Runtime",
                    Description = "Required to render the Blockly visual editor canvas inside the app.",
                    IconGlyph = "\uE774",
                    Requirement = ToolRequirement.Critical,
                    PrimaryFix = FixAction.InstallViaWinget,
                    WingetPackageId = "Microsoft.EdgeWebView2Runtime",
                    DownloadUrl = "https://developer.microsoft.com/en-us/microsoft-edge/webview2/"
                },
                new DiagnosticItem
                {
                    Id = "dev_mode",
                    DisplayName = "Windows Developer Mode",
                    Description = "Allows Flutter to create filesystem symlinks without admin elevation.",
                    IconGlyph = "\uE7EF",
                    Requirement = ToolRequirement.WindowsTarget,
                    PrimaryFix = FixAction.OpenSystemSettings
                },
                new DiagnosticItem
                {
                    Id = "vs_cpp",
                    DisplayName = "Visual Studio C++ Build Tools",
                    Description = "Required by the Flutter engine to compile native Windows desktop executables.",
                    IconGlyph = "\uEC7A",
                    Requirement = ToolRequirement.WindowsTarget,
                    CanBrowse = true,
                    PrimaryFix = FixAction.DownloadExternal,
                    DownloadUrl = "https://visualstudio.microsoft.com/downloads/"
                },
                new DiagnosticItem
                {
                    Id = "android_sdk",
                    DisplayName = "Android SDK",
                    Description = "Build tools and platform APIs for compiling Android APKs.",
                    IconGlyph = "\uE1C9",
                    Requirement = ToolRequirement.AndroidTarget,
                    CanBrowse = true,
                    PrimaryFix = FixAction.SetEnvironmentVariable,
                    EnvironmentVariableName = "ANDROID_HOME",
                    DownloadUrl = "https://developer.android.com/studio"
                },
                new DiagnosticItem
                {
                    Id = "java_jdk",
                    DisplayName = "Java Development Kit (JDK)",
                    Description = "Required by Gradle to compile Android applications.",
                    IconGlyph = "\uE7F8",
                    Requirement = ToolRequirement.AndroidTarget,
                    CanBrowse = true,
                    PrimaryFix = FixAction.SetEnvironmentVariable,
                    EnvironmentVariableName = "JAVA_HOME",
                    DownloadUrl = "https://adoptium.net/"
                }
            };
        }

        public static async Task RunAllChecksAsync(List<DiagnosticItem> items)
        {
            foreach (var item in items)
            {
                item.Status = DiagnosticStatus.Scanning;
                item.StatusMessage = "Scanning...";
            }

            var tasks = items.Select(ValidateItemAsync);
            await Task.WhenAll(tasks);
        }

        public static async Task ValidateItemAsync(DiagnosticItem item)
        {
            item.IsValidating = true;
            item.Status = DiagnosticStatus.Scanning;
            item.StatusMessage = "Validating...";

            try
            {
                switch (item.Id)
                {
                    case "git": await CheckGitAsync(item); break;
                    case "flutter": await CheckFlutterAsync(item); break;
                    case "webview2": await CheckWebView2Async(item); break;
                    case "dev_mode": await CheckDevModeAsync(item); break;
                    case "vs_cpp": await CheckVsCppAsync(item); break;
                    case "android_sdk": await CheckAndroidSdkAsync(item); break;
                    case "java_jdk": await CheckJavaAsync(item); break;
                }
            }
            finally
            {
                item.IsValidating = false;
            }
        }

        private static async Task CheckGitAsync(DiagnosticItem item)
        {
            string gitExe = "git";
            if (!string.IsNullOrWhiteSpace(item.EffectivePath))
            {
                gitExe = Path.Combine(item.EffectivePath, "bin", "git.exe");
                if (!File.Exists(gitExe)) gitExe = Path.Combine(item.EffectivePath, "cmd", "git.exe");
            }

            var (exitCode, stdout, _) = await RunProcessAsync(gitExe, "--version");
            if (exitCode == 0)
            {
                item.DetectedPath = item.EffectivePath;
                item.DetectedVersion = stdout.Trim().Replace("git version ", "");
                item.Status = DiagnosticStatus.Found;
                item.StatusMessage = $"Found v{item.DetectedVersion}";
            }
            else
            {
                item.Status = DiagnosticStatus.Error;
                item.StatusMessage = "Git not found.";
                item.RemediationHint = "Provide a valid Git installation path or install it.";
            }
        }

        private static async Task CheckFlutterAsync(DiagnosticItem item)
        {
            string flutterExe = "flutter";
            string sdkRoot = item.EffectivePath;

            if (!string.IsNullOrWhiteSpace(item.EffectivePath))
            {
                 flutterExe = Path.Combine(item.EffectivePath, "bin", "flutter.bat");
            }

            var (exitCode, stdout, stderr) = await RunProcessAsync(flutterExe, "--version --machine", 30000);
            
            if (exitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
            {
                try
                {
                    using var doc = JsonDocument.Parse(stdout);
                    item.DetectedVersion = doc.RootElement.GetProperty("frameworkVersion").GetString() ?? "Unknown";
                    
                    if (string.IsNullOrWhiteSpace(item.EffectivePath))
                    {
                         var (whereCode, whereOut, _) = await RunProcessAsync("where.exe", "flutter");
                         if (whereCode == 0 && !string.IsNullOrWhiteSpace(whereOut))
                         {
                             string path = whereOut.Split('\n', StringSplitOptions.RemoveEmptyEntries).First().Trim();
                             item.DetectedPath = Path.GetDirectoryName(Path.GetDirectoryName(path)) ?? path;
                         }
                    }

                    item.Status = DiagnosticStatus.Found;
                    item.StatusMessage = $"Found v{item.DetectedVersion}";
                }
                catch
                {
                     item.Status = DiagnosticStatus.Found;
                     item.StatusMessage = "Found Flutter SDK";
                }
            }
            else
            {
                item.Status = DiagnosticStatus.Error;
                item.StatusMessage = "Flutter SDK not found or invalid.";
                item.RemediationHint = "Provide a valid Flutter SDK path.";
            }
        }

        private static async Task CheckWebView2Async(DiagnosticItem item)
        {
            await Task.Run(() =>
            {
                string[] registryPaths = new[]
                {
                    @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}",
                    @"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"
                };

                foreach (var regPath in registryPaths)
                {
                    using var key = Registry.LocalMachine.OpenSubKey(regPath);
                    if (key != null)
                    {
                        string? version = key.GetValue("pv")?.ToString();
                        if (!string.IsNullOrEmpty(version) && version != "0.0.0.0")
                        {
                            item.DetectedVersion = version;
                            item.DetectedPath = "Evergreen Runtime";
                            item.Status = DiagnosticStatus.Found;
                            item.StatusMessage = $"Found v{version}";
                            return;
                        }
                    }
                }
                item.Status = DiagnosticStatus.Error;
                item.StatusMessage = "WebView2 Runtime not found.";
                item.RemediationHint = "Please download and install the WebView2 Evergreen Runtime.";
            });
        }

        private static async Task CheckDevModeAsync(DiagnosticItem item)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
                    if (key != null)
                    {
                        int? devMode = key.GetValue("AllowDevelopmentWithoutDevLicense") as int?;
                        if (devMode == 1)
                        {
                            item.Status = DiagnosticStatus.Found;
                            item.StatusMessage = "Enabled";
                            item.DetectedPath = "Enabled";
                            return;
                        }
                    }
                    item.Status = DiagnosticStatus.Warning;
                    item.StatusMessage = "Disabled";
                    item.RemediationHint = "Enable Developer Mode in Windows Settings.";
                }
                catch
                {
                    item.Status = DiagnosticStatus.Warning;
                    item.StatusMessage = "Could not verify.";
                }
            });
        }

        private static async Task CheckVsCppAsync(DiagnosticItem item)
        {
            // If custom path provided, assume it's correct for now (complex to validate deeply without vswhere)
            if (!string.IsNullOrWhiteSpace(item.EffectivePath))
            {
                 item.Status = DiagnosticStatus.Found;
                 item.StatusMessage = "Custom path provided.";
                 return;
            }

            string vsWherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");

            if (!File.Exists(vsWherePath))
            {
                item.Status = DiagnosticStatus.Warning;
                item.StatusMessage = "Not found.";
                item.RemediationHint = "Install Visual Studio with C++ workload.";
                return;
            }

            var (exitCode, stdout, _) = await RunProcessAsync(vsWherePath, "-requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath", 10000);
            if (exitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
            {
                item.DetectedPath = stdout.Trim().Split('\n').First().Trim();
                item.Status = DiagnosticStatus.Found;
                item.StatusMessage = "Found Visual Studio C++ Tools.";
            }
            else
            {
                item.Status = DiagnosticStatus.Warning;
                item.StatusMessage = "C++ workload missing.";
                item.RemediationHint = "Modify Visual Studio installation to include Desktop development with C++.";
            }
        }

        private static async Task CheckAndroidSdkAsync(DiagnosticItem item)
        {
            await Task.Run(() =>
            {
                string sdkRoot = item.EffectivePath;
                if (string.IsNullOrWhiteSpace(sdkRoot))
                {
                    sdkRoot = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "";
                }

                if (!string.IsNullOrWhiteSpace(sdkRoot) && Directory.Exists(sdkRoot))
                {
                    if (Directory.Exists(Path.Combine(sdkRoot, "platform-tools")))
                    {
                        item.DetectedPath = sdkRoot;
                        item.Status = DiagnosticStatus.Found;
                        item.StatusMessage = "Valid Android SDK found.";
                        return;
                    }
                }
                
                item.Status = DiagnosticStatus.Warning;
                item.StatusMessage = "Invalid or missing SDK.";
                item.RemediationHint = "Provide a valid Android SDK path.";
            });
        }

        private static async Task CheckJavaAsync(DiagnosticItem item)
        {
            await Task.Run(async () =>
            {
                string javacExe = "javac";
                if (!string.IsNullOrWhiteSpace(item.EffectivePath))
                {
                    javacExe = Path.Combine(item.EffectivePath, "bin", "javac.exe");
                }
                else
                {
                    string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
                    if (!string.IsNullOrEmpty(javaHome))
                    {
                         javacExe = Path.Combine(javaHome, "bin", "javac.exe");
                    }
                }

                var (exitCode, stdout, stderr) = await RunProcessAsync(javacExe, "-version");
                if (exitCode == 0)
                {
                     string version = (stdout + stderr).Trim().Replace("javac ", "");
                     item.DetectedVersion = version;
                     item.Status = DiagnosticStatus.Found;
                     item.StatusMessage = $"Found JDK {version}";
                }
                else
                {
                     item.Status = DiagnosticStatus.Warning;
                     item.StatusMessage = "JDK not found.";
                     item.RemediationHint = "Provide a valid JDK path.";
                }
            });
        }


        // Utilities
        public static bool AddToUserPath(string directory)
        {
            try
            {
                string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                if (currentPath.Split(';').Any(p => p.Trim().Equals(directory, StringComparison.OrdinalIgnoreCase)))
                    return true;

                string newPath = currentPath.TrimEnd(';') + ";" + directory;
                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                return true;
            }
            catch { return false; }
        }

        public static bool SetEnvironmentVariable(string name, string value)
        {
            try
            {
                Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
                return true;
            }
            catch { return false; }
        }

        public static void OpenDeveloperModeSettings()
        {
            Process.Start(new ProcessStartInfo { FileName = "ms-settings:developers", UseShellExecute = true });
        }

        public static void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        /// <summary>
        /// Installs a package silently via winget.
        /// </summary>
        public static async Task<bool> InstallViaWingetAsync(string packageId)
        {
            try
            {
                var (exitCode, _, _) = await RunProcessAsync("winget", $"install --id {packageId} -e --accept-package-agreements --accept-source-agreements --silent", 300000); // 5 minute timeout
                return exitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<(int exitCode, string stdout, string stderr)> RunProcessAsync(
            string fileName, string arguments, int timeoutMs = 15000)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();

                var completed = await Task.WhenAny(process.WaitForExitAsync(), Task.Delay(timeoutMs));

                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    return (-1, "", "Process timed out");
                }

                return (process.ExitCode, await stdoutTask, await stderrTask);
            }
            catch (Exception ex)
            {
                return (-1, "", ex.Message);
            }
        }
    }
}
