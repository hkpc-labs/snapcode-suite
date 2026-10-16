using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.System;

namespace Snapcode.Services
{
    public class ShortcutCommand : System.ComponentModel.INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "General"; // "General", "View Designer", "Flutter"
        public string Description { get; set; } = "";
        public string Scope { get; set; } = "Global"; // "Global", "Designer", "Editor"

        private VirtualKey _key;
        public VirtualKey Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged(nameof(Key));
                    OnPropertyChanged(nameof(DisplayText));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        private VirtualKeyModifiers _modifiers;
        public VirtualKeyModifiers Modifiers
        {
            get => _modifiers;
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    OnPropertyChanged(nameof(Modifiers));
                    OnPropertyChanged(nameof(DisplayText));
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        public VirtualKey DefaultKey { get; set; }
        public VirtualKeyModifiers DefaultModifiers { get; set; }

        public string DisplayText => ShortcutManager.FormatShortcut(Key, Modifiers);
        public bool IsModified => Key != DefaultKey || Modifiers != DefaultModifiers;

        [JsonIgnore]
        public Action? ActionHandler { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public void ResetToDefault()
        {
            Key = DefaultKey;
            Modifiers = DefaultModifiers;
        }
    }

    public class ShortcutConfigDto
    {
        public string Id { get; set; } = "";
        public int Key { get; set; }
        public int Modifiers { get; set; }
    }

    public class ShortcutManager
    {
        private static ShortcutManager? _instance;
        public static ShortcutManager Instance => _instance ??= new ShortcutManager();

        public ObservableCollection<ShortcutCommand> Commands { get; } = new();

        private static readonly string ConfigDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapCode");

        private static readonly string ShortcutsFilePath =
            Path.Combine(ConfigDirectory, "shortcuts.json");

        public event EventHandler<string>? ShortcutExecuted;

        public ShortcutManager()
        {
            InitializeDefaultCommands();
            _ = LoadAsync();
        }

        private void InitializeDefaultCommands()
        {
            Commands.Clear();

            // General / File
            RegisterCommand(new ShortcutCommand
            {
                Id = "file.save",
                Name = "Save Project / Page",
                Category = "General",
                Description = "Saves all open files, designer changes, and project workspace",
                Scope = "Global",
                DefaultKey = VirtualKey.S,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.S,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "general.settings",
                Name = "Open Settings",
                Category = "General",
                Description = "Opens the Snapcode Settings configuration window",
                Scope = "Global",
                DefaultKey = (VirtualKey)188, // Comma
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = (VirtualKey)188,
                Modifiers = VirtualKeyModifiers.Control
            });

            // View Designer
            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.undo",
                Name = "Undo Designer Action",
                Category = "View Designer",
                Description = "Reverts the last widget change, addition, or property edit",
                Scope = "Designer",
                DefaultKey = VirtualKey.Z,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.Z,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.redo",
                Name = "Redo Designer Action",
                Category = "View Designer",
                Description = "Reapplies a previously undone designer change",
                Scope = "Designer",
                DefaultKey = VirtualKey.Y,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.Y,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.rotate",
                Name = "Rotate Canvas Orientation",
                Category = "View Designer",
                Description = "Toggles canvas frame between Portrait and Landscape",
                Scope = "Designer",
                DefaultKey = VirtualKey.R,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.R,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.dual_preview",
                Name = "Toggle Dual Preview",
                Category = "View Designer",
                Description = "Toggles simultaneous side-by-side portrait & landscape live preview",
                Scope = "Designer",
                DefaultKey = VirtualKey.D,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.D,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.mobile",
                Name = "Quick Mobile View",
                Category = "View Designer",
                Description = "Instantly previews standard mobile phone resolution",
                Scope = "Designer",
                DefaultKey = VirtualKey.Number1,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.Number1,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.tablet",
                Name = "Quick Tablet View",
                Category = "View Designer",
                Description = "Instantly previews standard tablet resolution",
                Scope = "Designer",
                DefaultKey = VirtualKey.Number2,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.Number2,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.desktop",
                Name = "Quick Desktop View",
                Category = "View Designer",
                Description = "Instantly previews desktop/laptop screen resolution",
                Scope = "Designer",
                DefaultKey = VirtualKey.Number3,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.Number3,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "designer.delete",
                Name = "Delete Selected Widget",
                Category = "View Designer",
                Description = "Removes the currently selected widget from canvas",
                Scope = "Designer",
                DefaultKey = VirtualKey.Delete,
                DefaultModifiers = VirtualKeyModifiers.None,
                Key = VirtualKey.Delete,
                Modifiers = VirtualKeyModifiers.None
            });

            // Flutter Commands
            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.run",
                Name = "Start / Debug App",
                Category = "Flutter",
                Description = "Compiles and starts Flutter application on target device",
                Scope = "Global",
                DefaultKey = VirtualKey.F5,
                DefaultModifiers = VirtualKeyModifiers.None,
                Key = VirtualKey.F5,
                Modifiers = VirtualKeyModifiers.None
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.hot_reload",
                Name = "Flutter Hot Reload",
                Category = "Flutter",
                Description = "Quickly reloads UI and state changes in running Flutter app",
                Scope = "Global",
                DefaultKey = VirtualKey.F5,
                DefaultModifiers = VirtualKeyModifiers.Control,
                Key = VirtualKey.F5,
                Modifiers = VirtualKeyModifiers.Control
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.hot_restart",
                Name = "Flutter Hot Restart",
                Category = "Flutter",
                Description = "Restarts Flutter application state and rebuilds widget tree",
                Scope = "Global",
                DefaultKey = VirtualKey.F5,
                DefaultModifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift,
                Key = VirtualKey.F5,
                Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.stop",
                Name = "Stop Running App",
                Category = "Flutter",
                Description = "Terminates the currently active Flutter debugging session",
                Scope = "Global",
                DefaultKey = VirtualKey.F5,
                DefaultModifiers = VirtualKeyModifiers.Shift,
                Key = VirtualKey.F5,
                Modifiers = VirtualKeyModifiers.Shift
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.build_apk",
                Name = "Build Android APK",
                Category = "Flutter",
                Description = "Triggers Flutter Android APK build task",
                Scope = "Global",
                DefaultKey = VirtualKey.B,
                DefaultModifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift,
                Key = VirtualKey.B,
                Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift
            });

            RegisterCommand(new ShortcutCommand
            {
                Id = "flutter.clean",
                Name = "Clear Build Cache",
                Category = "Flutter",
                Description = "Executes flutter clean to clear build caches",
                Scope = "Global",
                DefaultKey = VirtualKey.K,
                DefaultModifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift,
                Key = VirtualKey.K,
                Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift
            });
        }

        private void RegisterCommand(ShortcutCommand command)
        {
            Commands.Add(command);
        }

        public void BindAction(string commandId, Action handler)
        {
            var cmd = Commands.FirstOrDefault(c => c.Id == commandId);
            if (cmd != null)
            {
                cmd.ActionHandler = handler;
            }
        }

        public bool TryExecute(VirtualKey key, VirtualKeyModifiers modifiers, string? currentScope = null)
        {
            var match = Commands.FirstOrDefault(c => 
                c.Key == key && 
                c.Modifiers == modifiers &&
                (c.Scope == "Global" || currentScope == null || c.Scope == currentScope));

            if (match != null)
            {
                if (match.ActionHandler != null)
                {
                    match.ActionHandler.Invoke();
                    ShortcutExecuted?.Invoke(this, match.Id);
                    return true;
                }
                else
                {
                    ShortcutExecuted?.Invoke(this, match.Id);
                    return true;
                }
            }

            return false;
        }

        public static VirtualKeyModifiers GetCurrentModifiers()
        {
            VirtualKeyModifiers mods = VirtualKeyModifiers.None;

            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            if ((ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                mods |= VirtualKeyModifiers.Control;
            }

            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            if ((shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                mods |= VirtualKeyModifiers.Shift;
            }

            var altState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);
            if ((altState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                mods |= VirtualKeyModifiers.Menu;
            }

            var winState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows);
            var winRightState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.RightWindows);
            if ((winState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down ||
                (winRightState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                mods |= VirtualKeyModifiers.Windows;
            }

            return mods;
        }

        public static bool IsWindowsSystemReserved(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            // Windows key combinations are strictly OS reserved
            if ((modifiers & VirtualKeyModifiers.Windows) != 0) return true;

            // Alt+F4 (Close window)
            if (modifiers == VirtualKeyModifiers.Menu && key == VirtualKey.F4) return true;

            // Alt+Tab (Switch app)
            if (modifiers == VirtualKeyModifiers.Menu && key == VirtualKey.Tab) return true;

            // Alt+Space (System menu)
            if (modifiers == VirtualKeyModifiers.Menu && key == VirtualKey.Space) return true;

            // Alt+Escape (Switch directly)
            if (modifiers == VirtualKeyModifiers.Menu && key == VirtualKey.Escape) return true;

            // Ctrl+Alt+Del
            if (modifiers == (VirtualKeyModifiers.Control | VirtualKeyModifiers.Menu) && key == VirtualKey.Delete) return true;

            // Ctrl+Shift+Esc (Task Manager)
            if (modifiers == (VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift) && key == VirtualKey.Escape) return true;

            // F1 alone (Windows Help)
            if (modifiers == VirtualKeyModifiers.None && key == VirtualKey.F1) return true;

            return false;
        }

        public static bool IsStandardTextEditingShortcut(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            if (modifiers == VirtualKeyModifiers.Control)
            {
                return key == VirtualKey.C || key == VirtualKey.V || key == VirtualKey.X || key == VirtualKey.A;
            }
            return false;
        }

        public (bool isValid, string errorMessage) ValidateShortcut(string targetCommandId, VirtualKey key, VirtualKeyModifiers modifiers)
        {
            if (key == VirtualKey.None)
            {
                return (false, "Please select a valid key.");
            }

            if (IsWindowsSystemReserved(key, modifiers))
            {
                return (false, "This key combination is reserved by the Windows operating system.");
            }

            if (IsStandardTextEditingShortcut(key, modifiers))
            {
                return (false, "Ctrl+C, Ctrl+V, Ctrl+X, and Ctrl+A are reserved for text editing.");
            }

            // Check if already in use by another command in same or overlapping scope
            var targetCmd = Commands.FirstOrDefault(c => c.Id == targetCommandId);
            var conflict = Commands.FirstOrDefault(c => 
                c.Id != targetCommandId && 
                c.Key == key && 
                c.Modifiers == modifiers &&
                (c.Scope == "Global" || targetCmd == null || targetCmd.Scope == "Global" || c.Scope == targetCmd.Scope));

            if (conflict != null)
            {
                return (false, $"Already assigned to '{conflict.Name}' ({conflict.Category}).");
            }

            return (true, string.Empty);
        }

        public static string FormatShortcut(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            var parts = new List<string>();
            if ((modifiers & VirtualKeyModifiers.Control) != 0) parts.Add("Ctrl");
            if ((modifiers & VirtualKeyModifiers.Menu) != 0) parts.Add("Alt");
            if ((modifiers & VirtualKeyModifiers.Shift) != 0) parts.Add("Shift");
            if ((modifiers & VirtualKeyModifiers.Windows) != 0) parts.Add("Win");

            string keyName = key switch
            {
                VirtualKey.Number0 => "0",
                VirtualKey.Number1 => "1",
                VirtualKey.Number2 => "2",
                VirtualKey.Number3 => "3",
                VirtualKey.Number4 => "4",
                VirtualKey.Number5 => "5",
                VirtualKey.Number6 => "6",
                VirtualKey.Number7 => "7",
                VirtualKey.Number8 => "8",
                VirtualKey.Number9 => "9",
                (VirtualKey)188 => ",", // Comma
                (VirtualKey)190 => ".", // Period
                (VirtualKey)186 => ";", // Semicolon
                VirtualKey.Delete => "Del",
                VirtualKey.Back => "Backspace",
                _ => key.ToString()
            };

            parts.Add(keyName);
            return string.Join("+", parts);
        }

        public async Task SaveAsync()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }

                var dtos = Commands.Select(c => new ShortcutConfigDto
                {
                    Id = c.Id,
                    Key = (int)c.Key,
                    Modifiers = (int)c.Modifiers
                }).ToList();

                string json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(ShortcutsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save shortcuts: {ex.Message}");
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(ShortcutsFilePath)) return;

                string json = await File.ReadAllTextAsync(ShortcutsFilePath);
                var dtos = JsonSerializer.Deserialize<List<ShortcutConfigDto>>(json);
                if (dtos != null)
                {
                    foreach (var dto in dtos)
                    {
                        var cmd = Commands.FirstOrDefault(c => c.Id == dto.Id);
                        if (cmd != null)
                        {
                            cmd.Key = (VirtualKey)dto.Key;
                            cmd.Modifiers = (VirtualKeyModifiers)dto.Modifiers;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load shortcuts: {ex.Message}");
            }
        }

        public void ResetAllToDefaults()
        {
            foreach (var cmd in Commands)
            {
                cmd.ResetToDefault();
            }
        }
    }
}
