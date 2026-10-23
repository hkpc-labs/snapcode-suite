using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Snapcode.ViewModels
{
    public class ProjectExplorerViewModel : INotifyPropertyChanged
    {
        private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
        {
            ".git",
            ".dart_tool",
            "build",
            ".gradle",
            ".idea",
            ".vscode",
            ".snapcode",
            ".system_generated",
            "bin",
            "obj"
        };

        private static readonly HashSet<string> ExcludedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe",
            ".dll",
            ".pdb",
            ".obj",
            ".tmp"
        };

        private string? _projectRootPath;
        private string? _flutterRootPath;
        private FileItemViewModel? _selectedItem;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<string>? OpenFileRequested;

        public ObservableCollection<FileItemViewModel> RootItems { get; } = new();

        public string? ProjectRootPath
        {
            get => _projectRootPath;
            private set
            {
                if (_projectRootPath != value)
                {
                    _projectRootPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? FlutterRootPath
        {
            get => _flutterRootPath;
            private set
            {
                if (_flutterRootPath != value)
                {
                    _flutterRootPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public FileItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    if (_selectedItem != null) _selectedItem.IsSelected = false;
                    _selectedItem = value;
                    if (_selectedItem != null) _selectedItem.IsSelected = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public void LoadProject(string projectRoot)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                return;

            ProjectRootPath = projectRoot;
            string flutterSubdir = Path.Combine(projectRoot, "flutter");
            FlutterRootPath = Directory.Exists(flutterSubdir) ? flutterSubdir : projectRoot;

            Refresh();
        }

        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(FlutterRootPath) || !Directory.Exists(FlutterRootPath))
                return;

            IsLoading = true;
            try
            {
                var expandedPaths = GetExpandedPaths(RootItems);

                RootItems.Clear();
                var rootDirItem = BuildDirectoryNode(FlutterRootPath, expandedPaths);
                if (rootDirItem != null)
                {
                    // Root is always expanded initially
                    rootDirItem.IsExpanded = true;
                    RootItems.Add(rootDirItem);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private HashSet<string> GetExpandedPaths(IEnumerable<FileItemViewModel> items)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                if (item.IsExpanded)
                {
                    paths.Add(item.FullPath);
                }
                foreach (var childPath in GetExpandedPaths(item.Children))
                {
                    paths.Add(childPath);
                }
            }
            return paths;
        }

        private FileItemViewModel? BuildDirectoryNode(string dirPath, HashSet<string> expandedPaths)
        {
            try
            {
                var dirInfo = new DirectoryInfo(dirPath);
                if (ExcludedDirectories.Contains(dirInfo.Name))
                    return null;

                var node = new FileItemViewModel(dirPath, isDirectory: true)
                {
                    IsExpanded = expandedPaths.Contains(dirPath)
                };

                // Add directories first
                var subDirs = dirInfo.GetDirectories()
                    .Where(d => !ExcludedDirectories.Contains(d.Name))
                    .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var subDir in subDirs)
                {
                    var childNode = BuildDirectoryNode(subDir.FullName, expandedPaths);
                    if (childNode != null)
                    {
                        node.Children.Add(childNode);
                    }
                }

                // Add files
                var files = dirInfo.GetFiles()
                    .Where(f => !ExcludedFileExtensions.Contains(f.Extension))
                    .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var file in files)
                {
                    node.Children.Add(new FileItemViewModel(file.FullName, isDirectory: false));
                }

                return node;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectExplorer] Error reading directory '{dirPath}': {ex.Message}");
                return null;
            }
        }

        public async Task<FileItemViewModel?> CreateFileAsync(FileItemViewModel? parent, string fileName)
        {
            string targetDir = GetTargetDirectory(parent);
            if (!Directory.Exists(targetDir)) return null;

            string targetPath = Path.Combine(targetDir, fileName.Trim());
            if (File.Exists(targetPath))
            {
                throw new InvalidOperationException($"File '{fileName}' already exists.");
            }

            // Provide smart initial template
            string initialContent = GetInitialContentForFile(fileName);
            await File.WriteAllTextAsync(targetPath, initialContent);

            var parentNode = FindNodeByPath(RootItems, targetDir);
            var newItem = new FileItemViewModel(targetPath, isDirectory: false);

            if (parentNode != null)
            {
                parentNode.IsExpanded = true;
                // Insert sorted
                int index = 0;
                while (index < parentNode.Children.Count &&
                       (parentNode.Children[index].IsDirectory || string.Compare(parentNode.Children[index].Name, newItem.Name, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    index++;
                }
                parentNode.Children.Insert(index, newItem);
            }
            else
            {
                RootItems.Add(newItem);
            }

            SelectedItem = newItem;
            RequestOpenFile(targetPath);
            return newItem;
        }

        public FileItemViewModel? CreateFolder(FileItemViewModel? parent, string folderName)
        {
            string targetDir = GetTargetDirectory(parent);
            if (!Directory.Exists(targetDir)) return null;

            string newDirPath = Path.Combine(targetDir, folderName.Trim());
            if (Directory.Exists(newDirPath))
            {
                throw new InvalidOperationException($"Directory '{folderName}' already exists.");
            }

            Directory.CreateDirectory(newDirPath);

            var parentNode = FindNodeByPath(RootItems, targetDir);
            var newItem = new FileItemViewModel(newDirPath, isDirectory: true) { IsExpanded = true };

            if (parentNode != null)
            {
                parentNode.IsExpanded = true;
                // Insert sorted among directories
                int index = 0;
                while (index < parentNode.Children.Count &&
                       parentNode.Children[index].IsDirectory &&
                       string.Compare(parentNode.Children[index].Name, newItem.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    index++;
                }
                parentNode.Children.Insert(index, newItem);
            }
            else
            {
                RootItems.Add(newItem);
            }

            SelectedItem = newItem;
            return newItem;
        }

        public bool RenameItem(FileItemViewModel item, string newName)
        {
            if (item == null || string.IsNullOrWhiteSpace(newName)) return false;
            string trimmed = newName.Trim();
            if (string.Equals(item.Name, trimmed, StringComparison.OrdinalIgnoreCase)) return true;

            string parentDir = Path.GetDirectoryName(item.FullPath) ?? "";
            string newPath = Path.Combine(parentDir, trimmed);

            try
            {
                if (item.IsDirectory)
                {
                    if (Directory.Exists(newPath)) return false;
                    Directory.Move(item.FullPath, newPath);
                }
                else
                {
                    if (File.Exists(newPath)) return false;
                    File.Move(item.FullPath, newPath);
                }

                item.FullPath = newPath;
                item.Name = trimmed;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectExplorer] Rename failed: {ex.Message}");
                return false;
            }
        }

        public bool DeleteItem(FileItemViewModel item)
        {
            if (item == null) return false;

            try
            {
                if (item.IsDirectory)
                {
                    if (Directory.Exists(item.FullPath))
                    {
                        Directory.Delete(item.FullPath, recursive: true);
                    }
                }
                else
                {
                    if (File.Exists(item.FullPath))
                    {
                        File.Delete(item.FullPath);
                    }
                }

                RemoveNodeFromTree(RootItems, item);
                if (SelectedItem == item) SelectedItem = null;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectExplorer] Delete failed: {ex.Message}");
                return false;
            }
        }

        private bool RemoveNodeFromTree(IList<FileItemViewModel> list, FileItemViewModel target)
        {
            if (list.Remove(target)) return true;
            foreach (var item in list)
            {
                if (RemoveNodeFromTree(item.Children, target)) return true;
            }
            return false;
        }

        public FileItemViewModel? FindNodeByPath(IEnumerable<FileItemViewModel> items, string path)
        {
            foreach (var item in items)
            {
                if (string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase))
                    return item;

                var child = FindNodeByPath(item.Children, path);
                if (child != null) return child;
            }
            return null;
        }

        private string GetTargetDirectory(FileItemViewModel? item)
        {
            if (item != null)
            {
                if (item.IsDirectory) return item.FullPath;
                return Path.GetDirectoryName(item.FullPath) ?? FlutterRootPath ?? ProjectRootPath ?? "";
            }
            return FlutterRootPath ?? ProjectRootPath ?? "";
        }

        public void RequestOpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                OpenFileRequested?.Invoke(this, filePath);
            }
        }

        private static string GetInitialContentForFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            return ext switch
            {
                ".dart" => $@"// Flutter Dart source: {fileName}
import 'package:flutter/material.dart';

// <!-- USER_CODE_START: CLASS_MEMBERS -->
// Custom models, services, or widgets
// <!-- USER_CODE_END: CLASS_MEMBERS -->
",
                ".json" => "{\n  \n}\n",
                ".yaml" or ".yml" => "# Configuration\n",
                ".md" => $"# {nameWithoutExt}\n\n",
                _ => ""
            };
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
