using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Snapcode.Services
{
    public class AssetInfo
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Extension { get; set; } = "";
        public string Type { get; set; } = ""; // Image, Audio, Video, Font, Other
        public long SizeBytes { get; set; }
    }

    public static class AssetService
    {
        public static string GetProjectAssetsFolder(string projectRoot)
        {
            string path = Path.Combine(projectRoot, "SnapcodeAssets");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static List<AssetInfo> GetAssets(string projectRoot)
        {
            var assets = new List<AssetInfo>();
            string folder = GetProjectAssetsFolder(projectRoot);
            
            foreach (var file in Directory.GetFiles(folder))
            {
                var info = new FileInfo(file);
                string ext = info.Extension.ToLower();
                string type = "Other";

                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp") type = "Image";
                else if (ext == ".mp3" || ext == ".wav" || ext == ".ogg") type = "Audio";
                else if (ext == ".mp4" || ext == ".avi" || ext == ".mkv") type = "Video";
                else if (ext == ".ttf" || ext == ".otf") type = "Font";

                assets.Add(new AssetInfo
                {
                    FileName = info.Name,
                    FilePath = info.FullName,
                    Extension = ext,
                    Type = type,
                    SizeBytes = info.Length
                });
            }

            return assets.OrderBy(a => a.Type).ThenBy(a => a.FileName).ToList();
        }

        public static async Task<AssetInfo> ImportAssetAsync(string projectRoot, string sourceFilePath)
        {
            string folder = GetProjectAssetsFolder(projectRoot);
            string fileName = Path.GetFileName(sourceFilePath);
            string destPath = Path.Combine(folder, fileName);

            // Handle duplicates
            int count = 1;
            while (File.Exists(destPath))
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);
                destPath = Path.Combine(folder, $"{nameWithoutExt}_{count++}{ext}");
            }

            await Task.Run(() => File.Copy(sourceFilePath, destPath));
            
            // Return just the file name as a relative path representation
            return GetAssets(projectRoot).First(a => a.FilePath == destPath);
        }

        public static void DeleteAsset(string projectRoot, string fileName)
        {
            string folder = GetProjectAssetsFolder(projectRoot);
            string destPath = Path.Combine(folder, fileName);
            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }
        }

        public static void SyncAssetsToFlutterProject(string projectRoot)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                    return;

                string snapcodeAssetsDir = Path.Combine(projectRoot, "SnapcodeAssets");
                string flutterDir = Path.Combine(projectRoot, ProjectService.FlutterFolderName);
                if (!Directory.Exists(flutterDir))
                {
                    flutterDir = projectRoot;
                }

                string flutterAssetsDir = Path.Combine(flutterDir, "assets");

                // 1. Ensure SnapcodeAssets directory exists
                if (!Directory.Exists(snapcodeAssetsDir))
                {
                    Directory.CreateDirectory(snapcodeAssetsDir);
                }

                // 2. Always ensure flutter/assets exists on disk with a placeholder if empty.
                // Flutter's Windows build system (flutter_assemble / CMake) throws a fatal error:
                // "unable to find directory entry in pubspec.yaml: .../flutter/assets/"
                // if pubspec.yaml registers `assets: - assets/` and the directory does not exist on disk.
                if (!Directory.Exists(flutterAssetsDir))
                {
                    Directory.CreateDirectory(flutterAssetsDir);
                }
                string gitkeepFile = Path.Combine(flutterAssetsDir, ".gitkeep");
                if (Directory.GetFiles(flutterAssetsDir).Length == 0 && !File.Exists(gitkeepFile))
                {
                    File.WriteAllText(gitkeepFile, "");
                }

                // 3. Copy all assets from SnapcodeAssets to flutter/assets
                if (Directory.Exists(snapcodeAssetsDir))
                {
                    var files = Directory.GetFiles(snapcodeAssetsDir);
                    if (files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            string dest = Path.Combine(flutterAssetsDir, Path.GetFileName(file));
                            if (!File.Exists(dest) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(dest))
                            {
                                File.Copy(file, dest, overwrite: true);
                            }
                        }
                    }
                }

                // 4. Ensure pubspec.yaml registers the assets directory
                string pubspecPath = Path.Combine(flutterDir, "pubspec.yaml");
                if (File.Exists(pubspecPath))
                {
                    string content = File.ReadAllText(pubspecPath);
                    if (!content.Contains("assets:") || !content.Contains("- assets/"))
                    {
                        if (content.Contains("uses-material-design: true"))
                        {
                            string updated = content.Replace("uses-material-design: true", "uses-material-design: true\n\n  assets:\n    - assets/");
                            File.WriteAllText(pubspecPath, updated);
                        }
                        else if (content.Contains("flutter:"))
                        {
                            string updated = content.Replace("flutter:", "flutter:\n  assets:\n    - assets/");
                            File.WriteAllText(pubspecPath, updated);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AssetService] SyncAssetsToFlutterProject failed: {ex.Message}");
            }
        }
    }
}
