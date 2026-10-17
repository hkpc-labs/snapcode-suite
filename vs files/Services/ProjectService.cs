using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Snapcode.Services
{
    /// <summary>
    /// Represents the metadata for a SnapCode project, stored in .snapcode/project.json.
    /// </summary>
    public class SnapcodeProject
    {
        public string Name { get; set; } = "";
        public string Organization { get; set; } = "";
        public string Platforms { get; set; } = "";
        public bool UseKotlin { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Manages the .snapcode/ project metadata folder structure.
    /// Handles creation, loading, and saving of project state including Blockly workspace.
    /// </summary>
    public static class ProjectService
    {
        private const string MetaFolderName = ".snapcode";
        private const string ProjectFileName = "project.json";
        private const string WorkspaceFileName = "workspace.json";
        private const string AssetsFolderName = "assets";
        private const string LogsFolderName = "logs";
        public const string FlutterFolderName = "flutter";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Returns the path to the .snapcode metadata folder for a project.
        /// </summary>
        public static string GetMetaPath(string projectRoot) =>
            Path.Combine(projectRoot, MetaFolderName);

        /// <summary>
        /// Returns the path to the flutter/ subdirectory for a project.
        /// </summary>
        public static string GetFlutterPath(string projectRoot) =>
            Path.Combine(projectRoot, FlutterFolderName);

        /// <summary>
        /// Initializes the .snapcode/ folder structure for a new project.
        /// Creates: .snapcode/project.json, .snapcode/assets/, .snapcode/logs/
        /// </summary>
        public static async Task InitializeProjectAsync(string projectRoot, string name, string org, string platforms, bool useKotlin)
        {
            string metaPath = GetMetaPath(projectRoot);
            Directory.CreateDirectory(metaPath);
            Directory.CreateDirectory(Path.Combine(metaPath, AssetsFolderName));
            Directory.CreateDirectory(Path.Combine(metaPath, LogsFolderName));

            // Hide the .snapcode folder on Windows for cleanliness
            try
            {
                var dirInfo = new DirectoryInfo(metaPath);
                dirInfo.Attributes |= FileAttributes.Hidden;
            }
            catch { /* Non-critical, ignore */ }

            var project = new SnapcodeProject
            {
                Name = name,
                Organization = org,
                Platforms = platforms,
                UseKotlin = useKotlin,
                CreatedAt = DateTime.UtcNow,
                LastOpenedAt = DateTime.UtcNow
            };

            string json = JsonSerializer.Serialize(project, JsonOptions);
            await File.WriteAllTextAsync(Path.Combine(metaPath, ProjectFileName), json);

            // Create an empty workspace file
            await File.WriteAllTextAsync(Path.Combine(metaPath, WorkspaceFileName), "{}");
        }

        /// <summary>
        /// Loads the project metadata from .snapcode/project.json.
        /// Returns null if the project is not a valid SnapCode project.
        /// </summary>
        public static async Task<SnapcodeProject?> LoadProjectAsync(string projectRoot)
        {
            string projectFile = Path.Combine(GetMetaPath(projectRoot), ProjectFileName);
            if (!File.Exists(projectFile)) return null;

            try
            {
                string json = await File.ReadAllTextAsync(projectFile);
                var project = JsonSerializer.Deserialize<SnapcodeProject>(json, JsonOptions);

                // Update last opened timestamp
                if (project != null)
                {
                    project.LastOpenedAt = DateTime.UtcNow;
                    string updatedJson = JsonSerializer.Serialize(project, JsonOptions);
                    await File.WriteAllTextAsync(projectFile, updatedJson);
                }

                return project;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load project: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves the Blockly workspace JSON state to .snapcode/workspace.json.
        /// </summary>
        public static async Task SaveWorkspaceAsync(string projectRoot, string blocklyJson)
        {
            try
            {
                string metaPath = GetMetaPath(projectRoot);
                Directory.CreateDirectory(metaPath); // Ensure exists
                await File.WriteAllTextAsync(Path.Combine(metaPath, WorkspaceFileName), blocklyJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to save workspace: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads the Blockly workspace JSON state from .snapcode/workspace.json.
        /// Returns "{}" if not found.
        /// </summary>
        public static async Task<string> LoadWorkspaceAsync(string projectRoot)
        {
            try
            {
                string wsFile = Path.Combine(GetMetaPath(projectRoot), WorkspaceFileName);
                if (!File.Exists(wsFile)) return "{}";
                return await File.ReadAllTextAsync(wsFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load workspace: {ex.Message}");
                return "{}";
            }
        }

        public static string GetPagesDirectory(string projectRoot)
        {
            string pagesDir = Path.Combine(GetMetaPath(projectRoot), "pages");
            Directory.CreateDirectory(pagesDir);
            return pagesDir;
        }

        public static async Task SavePageWorkspaceAsync(string projectRoot, string pageName, string blocklyJson)
        {
            try
            {
                if (string.Equals(pageName, "main", StringComparison.OrdinalIgnoreCase))
                {
                    await SaveWorkspaceAsync(projectRoot, blocklyJson);
                    return;
                }

                string pagesDir = GetPagesDirectory(projectRoot);
                string pageFile = Path.Combine(pagesDir, $"{pageName}.json");
                await File.WriteAllTextAsync(pageFile, blocklyJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to save page workspace: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> LoadPageWorkspaceAsync(string projectRoot, string pageName)
        {
            try
            {
                if (string.Equals(pageName, "main", StringComparison.OrdinalIgnoreCase))
                {
                    return await LoadWorkspaceAsync(projectRoot);
                }

                string pagesDir = GetPagesDirectory(projectRoot);
                string pageFile = Path.Combine(pagesDir, $"{pageName}.json");
                if (!File.Exists(pageFile)) return "{}";
                return await File.ReadAllTextAsync(pageFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load page workspace: {ex.Message}");
                return "{}";
            }
        }

        public static async Task SavePageUIAsync(string projectRoot, string pageName, string uiJson)
        {
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string pageFile = Path.Combine(pagesDir, $"{pageName}_ui.json");
                await File.WriteAllTextAsync(pageFile, uiJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to save page UI: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> LoadPageUIAsync(string projectRoot, string pageName)
        {
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string pageFile = Path.Combine(pagesDir, $"{pageName}_ui.json");
                if (!File.Exists(pageFile)) return "{}";
                return await File.ReadAllTextAsync(pageFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load page UI: {ex.Message}");
                return "{}";
            }
        }

        public static async Task SavePageEventCodeAsync(string projectRoot, string workspaceFile, string dartCode)
        {
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string fileName = workspaceFile.EndsWith(".event.dart", StringComparison.OrdinalIgnoreCase)
                    ? Path.GetFileName(workspaceFile)
                    : $"{Path.GetFileNameWithoutExtension(workspaceFile)}.event.dart";
                string filePath = Path.Combine(pagesDir, fileName);
                await File.WriteAllTextAsync(filePath, dartCode ?? string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to save page event code: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> LoadAllPageEventCodeAsync(string projectRoot, string pageName, string? currentActiveCode = null, string? currentContext = null)
        {
            try
            {
                var eventCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string pagesDir = GetPagesDirectory(projectRoot);

                if (Directory.Exists(pagesDir))
                {
                    string prefix = $"{pageName}_";
                    string exactName = $"{pageName}.event.dart";

                    var matchingFiles = Directory.GetFiles(pagesDir, "*.event.dart")
                        .Select(f => new FileInfo(f))
                        .Where(fi => string.Equals(fi.Name, exactName, StringComparison.OrdinalIgnoreCase) ||
                                     fi.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(fi => fi.LastWriteTimeUtc)
                        .ToList();

                    foreach (var fi in matchingFiles)
                    {
                        try
                        {
                            string code = await File.ReadAllTextAsync(fi.FullName);
                            if (!string.IsNullOrWhiteSpace(code))
                            {
                                eventCodes[fi.Name] = code;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to read event file {fi.Name}: {ex.Message}");
                        }
                    }
                }

                // If current workspace has active code, register/override its entry so it is always fresh and last in enumeration order
                if (!string.IsNullOrWhiteSpace(currentActiveCode))
                {
                    string currentKey = !string.IsNullOrWhiteSpace(currentContext)
                        ? $"{pageName}_{currentContext}.event.dart"
                        : $"{pageName}.event.dart";
                    eventCodes.Remove(currentKey);
                    eventCodes[currentKey] = currentActiveCode;
                }

                var parts = new List<string>();
                foreach (var kvp in eventCodes)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Value))
                    {
                        parts.Add(kvp.Value.Trim());
                    }
                }

                return string.Join("\n\n", parts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load all page event code: {ex.Message}");
                return currentActiveCode ?? string.Empty;
            }
        }

        public static List<string> GetProjectPages(string projectRoot)
        {
            var pages = new List<string> { "main" };
            try
            {
                string pagesDir = Path.Combine(GetMetaPath(projectRoot), "pages");
                if (Directory.Exists(pagesDir))
                {
                    foreach (var file in Directory.GetFiles(pagesDir, "*.json"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        
                        // Ignore internal state files
                        if (name.EndsWith("_meta", StringComparison.OrdinalIgnoreCase)) continue;
                        if (name.EndsWith("_ui", StringComparison.OrdinalIgnoreCase)) continue;
                        if (name.Contains("_on", StringComparison.OrdinalIgnoreCase) && name.Contains("_")) continue; // Simple heuristic to avoid event workspaces
                        
                        if (!pages.Contains(name, StringComparer.OrdinalIgnoreCase))
                        {
                            pages.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to list project pages: {ex.Message}");
            }
            return pages;
        }

        public static async Task SavePageMetadataAsync(string projectRoot, string pageName, Snapcode.Models.PageMetadata metadata)
        {
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string metaFile = Path.Combine(pagesDir, $"{pageName}_meta.json");
                string json = JsonSerializer.Serialize(metadata, JsonOptions);
                await File.WriteAllTextAsync(metaFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to save page metadata: {ex.Message}");
                throw;
            }
        }

        public static async Task<Snapcode.Models.PageMetadata?> LoadPageMetadataAsync(string projectRoot, string pageName)
        {
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string metaFile = Path.Combine(pagesDir, $"{pageName}_meta.json");
                if (!File.Exists(metaFile)) return null;
                string json = await File.ReadAllTextAsync(metaFile);
                return JsonSerializer.Deserialize<Snapcode.Models.PageMetadata>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] Failed to load page metadata: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ensures a visual drawer companion file structure exists for a page that has HasDrawer enabled.
        /// Creates: .snapcode/pages/{parentPageName}_drawer_meta.json, _ui.json, .json, and compiles lib/models/drawers/{parentPageName}_drawer.dart.
        /// </summary>
        public static async Task<string> EnsureDrawerForPageAsync(string projectRoot, string parentPageName)
        {
            string drawerName = $"{parentPageName}_drawer";
            try
            {
                string pagesDir = GetPagesDirectory(projectRoot);
                string metaFile = Path.Combine(pagesDir, $"{drawerName}_meta.json");

                if (!File.Exists(metaFile))
                {
                    var drawerMeta = new Snapcode.Models.PageMetadata
                    {
                        Name = drawerName,
                        Title = $"{parentPageName} Navigation Drawer",
                        Type = "Drawer",
                        ShowAppBar = false,
                        SafeArea = true,
                        Orientation = "Portrait"
                    };
                    await SavePageMetadataAsync(projectRoot, drawerName, drawerMeta);

                    // Default drawer UI: A Column with a header Container and List items
                    string defaultDrawerUi = @"[
  {
    ""id"": ""drawer_header_container"",
    ""type"": ""Container"",
    ""height"": 140.0,
    ""color"": ""#2196F3"",
    ""padding"": 16.0,
    ""children"": [
      {
        ""id"": ""drawer_header_title"",
        ""type"": ""Text"",
        ""text"": ""App Navigation"",
        ""fontSize"": 20.0,
        ""textColor"": ""#FFFFFF"",
        ""isBold"": true
      }
    ]
  },
  {
    ""id"": ""drawer_btn_home"",
    ""type"": ""Button"",
    ""text"": ""Home"",
    ""backgroundColor"": ""#2196F3"",
    ""textColor"": ""#FFFFFF""
  },
  {
    ""id"": ""drawer_btn_settings"",
    ""type"": ""Button"",
    ""text"": ""Settings"",
    ""backgroundColor"": ""#2196F3"",
    ""textColor"": ""#FFFFFF""
  }
]";
                    await SavePageUIAsync(projectRoot, drawerName, defaultDrawerUi);
                    await SavePageWorkspaceAsync(projectRoot, drawerName, "{}");
                }

                // Compile/ensure the dart file in models/drawers/{drawerName}.dart
                var meta = await LoadPageMetadataAsync(projectRoot, drawerName);
                string uiJson = await LoadPageUIAsync(projectRoot, drawerName);
                string eventCode = await LoadAllPageEventCodeAsync(projectRoot, drawerName);

                string drawerDartSource = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart(
                    drawerName,
                    !string.IsNullOrWhiteSpace(uiJson) ? uiJson : "[]",
                    eventCode,
                    meta
                );

                string fileName = $"{drawerName.ToLowerInvariant()}.dart";

                string flutterLibPath = Path.Combine(projectRoot, FlutterFolderName, "lib", "models", "drawers");
                Directory.CreateDirectory(flutterLibPath);
                string flutterDartFile = Path.Combine(flutterLibPath, fileName);
                await UserCodePreservationService.SaveDartFileWithPreservationAsync(flutterDartFile, drawerDartSource);

                string legacyLibBase = Path.Combine(projectRoot, "lib");
                if (Directory.Exists(legacyLibBase))
                {
                    string legacyLibPath = Path.Combine(legacyLibBase, "models", "drawers");
                    Directory.CreateDirectory(legacyLibPath);
                    string legacyDartFile = Path.Combine(legacyLibPath, fileName);
                    await UserCodePreservationService.SaveDartFileWithPreservationAsync(legacyDartFile, drawerDartSource);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectService] EnsureDrawerForPageAsync failed: {ex.Message}");
            }
            return drawerName;
        }

        /// <summary>
        /// Appends a log entry to .snapcode/logs/build.log.
        /// </summary>
        public static async Task AppendBuildLogAsync(string projectRoot, string logEntry)
        {
            try
            {
                string logDir = Path.Combine(GetMetaPath(projectRoot), LogsFolderName);
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "build.log");
                await File.AppendAllTextAsync(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logEntry}\n");
            }
            catch { /* Non-critical */ }
        }

        /// <summary>
        /// Checks if a directory is a valid SnapCode project (has .snapcode/project.json).
        /// </summary>
        public static bool IsSnapcodeProject(string path)
        {
            return File.Exists(Path.Combine(path, MetaFolderName, ProjectFileName));
        }

        /// <summary>
        /// Legacy check: Returns true if the path contains a pubspec.yaml
        /// (for projects created before the new folder structure).
        /// </summary>
        public static bool IsLegacyFlutterProject(string path)
        {
            return File.Exists(Path.Combine(path, "pubspec.yaml"));
        }
    }
}
