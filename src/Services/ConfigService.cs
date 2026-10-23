using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Snapcode.Services
{
    /// <summary>
    /// Persisted application configuration stored in %APPDATA%\SnapCode\config.json.
    /// Contains all validated toolchain paths and first-run state.
    /// </summary>
    public class AppConfig
    {
        /// <summary>Whether the initial setup wizard has been completed at least once.</summary>
        public bool IsFirstRunComplete { get; set; } = false;

        /// <summary>Absolute path to the Flutter SDK root directory (e.g. "C:\src\flutter").</summary>
        public string FlutterSdkPath { get; set; } = "";

        /// <summary>Absolute path to the Git executable (e.g. "C:\Program Files\Git\cmd\git.exe").</summary>
        public string GitPath { get; set; } = "";

        /// <summary>Absolute path to Android SDK root (e.g. "%LOCALAPPDATA%\Android\Sdk").</summary>
        public string AndroidSdkPath { get; set; } = "";

        /// <summary>Absolute path to JDK home directory.</summary>
        public string JavaHomePath { get; set; } = "";

        /// <summary>Absolute path to the Visual Studio installation with C++ workload.</summary>
        public string VisualStudioPath { get; set; } = "";

        /// <summary>Default compilation target: "windows", "android", or "web".</summary>
        public string DefaultTarget { get; set; } = "web";

        /// <summary>Whether Windows Developer Mode was confirmed enabled.</summary>
        public bool WindowsDevModeEnabled { get; set; } = false;

        /// <summary>Last time the diagnostic scan was run successfully.</summary>
        public DateTime LastDiagnosticScan { get; set; } = DateTime.MinValue;

        /// <summary>List of recent project paths.</summary>
        public System.Collections.Generic.List<string> RecentProjects { get; set; } = new();

        /// <summary>Application Theme preference (System Default, Light, Dark).</summary>
        public string AppTheme { get; set; } = "System Default";

        /// <summary>Editor Font preference.</summary>
        public string EditorFont { get; set; } = "Consolas";
    }

    /// <summary>
    /// Handles reading and writing the SnapCode configuration file to disk.
    /// Config is stored at: %APPDATA%\SnapCode\config.json
    /// </summary>
    public static class ConfigService
    {
        private static readonly string ConfigDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapCode");

        private static readonly string ConfigFilePath =
            Path.Combine(ConfigDirectory, "config.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Loads the config from disk. If the file doesn't exist, returns a fresh default config.
        /// </summary>
        public static async Task<AppConfig> LoadAsync()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new AppConfig();
                }

                string json = await File.ReadAllTextAsync(ConfigFilePath);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigService] Failed to load config: {ex.Message}");
                return new AppConfig();
            }
        }

        /// <summary>
        /// Saves the config to disk. Creates the directory if it doesn't exist.
        /// </summary>
        public static async Task SaveAsync(AppConfig config)
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }

                string json = JsonSerializer.Serialize(config, JsonOptions);
                await File.WriteAllTextAsync(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigService] Failed to save config: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the config file, forcing the wizard to run again on next launch.
        /// </summary>
        public static void Reset()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    File.Delete(ConfigFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigService] Failed to reset config: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the path to the config directory for display purposes.
        /// </summary>
        public static string GetConfigDirectory() => ConfigDirectory;
    }
}
