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
    }
}
