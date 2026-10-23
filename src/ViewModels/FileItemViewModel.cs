using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Snapcode.ViewModels
{
    public class FileItemViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _fullPath;
        private bool _isDirectory;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isDirty;
        private bool _isEditing;

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileItemViewModel(string fullPath, bool isDirectory)
        {
            _fullPath = fullPath;
            _isDirectory = isDirectory;
            _name = Path.GetFileName(fullPath);
            if (string.IsNullOrEmpty(_name) && isDirectory)
            {
                _name = fullPath;
            }
            Children = new ObservableCollection<FileItemViewModel>();
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Extension));
                    OnPropertyChanged(nameof(IconGlyph));
                    OnPropertyChanged(nameof(IconColorHex));
                }
            }
        }

        public bool IsDirectory
        {
            get => _isDirectory;
            set
            {
                if (_isDirectory != value)
                {
                    _isDirectory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconGlyph));
                    OnPropertyChanged(nameof(IconColorHex));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconGlyph));
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DirtyVisibility));
                }
            }
        }

        public Microsoft.UI.Xaml.Visibility DirtyVisibility =>
            _isDirty ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<FileItemViewModel> Children { get; }

        public string Extension => IsDirectory ? string.Empty : Path.GetExtension(FullPath).ToLowerInvariant();

        public string IconGlyph
        {
            get
            {
                if (IsDirectory)
                {
                    return IsExpanded ? "\uED43" : "\uED25"; // Folder Open vs Folder Closed
                }

                return Extension switch
                {
                    ".dart" => "\uE790",    // Code / Page
                    ".yaml" or ".yml" => "\uE7B8", // Settings / Document
                    ".json" => "\uE943",    // Code data
                    ".md" => "\uE8A5",      // Text document
                    ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".svg" or ".ico" => "\uEB9F", // Image
                    ".html" or ".htm" => "\uE774", // Web / Globe
                    ".xml" => "\uE7C3",
                    ".gradle" => "\uE90F",  // Build
                    ".lock" => "\uE72E",    // Lock
                    _ => "\uE8A5"           // Default file
                };
            }
        }

        public string IconColorHex
        {
            get
            {
                if (IsDirectory)
                {
                    return "#E5A93C"; // VS Code Folder Amber
                }

                return Extension switch
                {
                    ".dart" => "#00B4AB",    // Flutter Teal / Sky Blue
                    ".yaml" or ".yml" => "#CB3837", // Red/Coral
                    ".json" => "#CBCB41",    // Yellow
                    ".md" => "#519ABA",      // Blue
                    ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".svg" or ".ico" => "#4CAF50", // Green
                    ".html" or ".htm" => "#E34F26", // Orange
                    ".xml" => "#FF9800",
                    ".gradle" => "#02303A",
                    _ => "#9E9E9E"           // Gray
                };
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
