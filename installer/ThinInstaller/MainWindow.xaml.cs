using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Snapcode.ThinInstaller
{
    public partial class MainWindow : Window
    {
        private bool _isFinished = false;

        public MainWindow()
        {
            InitializeComponent();
            string defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Snapcode");
            TxtPath.Text = defaultPath;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Snapcode Installation Folder",
                InitialDirectory = TxtPath.Text
            };
            if (dialog.ShowDialog() == true)
            {
                TxtPath.Text = dialog.FolderName;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (_isFinished)
            {
                if (ChkLaunch.IsChecked == true)
                {
                    string exePath = Path.Combine(TxtPath.Text, "Snapcode.exe");
                    if (File.Exists(exePath))
                    {
                        Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                    }
                }
                Close();
                return;
            }

            string targetDir = TxtPath.Text.Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Please enter a valid installation folder.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnInstall.IsEnabled = false;
            BtnBrowse.IsEnabled = false;
            TxtPath.IsEnabled = false;
            ProgBar.Visibility = Visibility.Visible;
            ProgBar.IsIndeterminate = true;

            try
            {
                await Task.Run(() => PerformInstall(targetDir));

                ProgBar.IsIndeterminate = false;
                ProgBar.Value = 100;
                TxtStatus.Text = "Installation completed successfully!";
                TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(56, 189, 248));
                BtnInstall.Content = "Finish";
                BtnInstall.IsEnabled = true;
                BtnCancel.Visibility = Visibility.Collapsed;
                _isFinished = true;
            }
            catch (Exception ex)
            {
                ProgBar.Visibility = Visibility.Collapsed;
                TxtStatus.Text = "Installation failed: " + ex.Message;
                TxtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 63, 94));
                BtnInstall.IsEnabled = true;
                BtnBrowse.IsEnabled = true;
                TxtPath.IsEnabled = true;
                MessageBox.Show("Failed to install: " + ex.Message, "Installation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PerformInstall(string targetDir)
        {
            // Stop any running Snapcode process
            foreach (var proc in Process.GetProcessesByName("Snapcode"))
            {
                try { proc.Kill(); proc.WaitForExit(2000); } catch { }
            }

            UpdateStatus("Preparing destination directory...");
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Find source files
            string baseDir = AppContext.BaseDirectory;
            string? zipFile = FindReleaseZip(baseDir);
            string? publishDir = FindPublishDir(baseDir);

            if (zipFile != null && File.Exists(zipFile))
            {
                UpdateStatus("Extracting application files from " + Path.GetFileName(zipFile) + "...");
                ZipFile.ExtractToDirectory(zipFile, targetDir, overwriteFiles: true);
            }
            else if (publishDir != null && Directory.Exists(publishDir))
            {
                UpdateStatus("Copying application files...");
                CopyDirectory(publishDir, targetDir);
            }
            else
            {
                throw new FileNotFoundException("Could not locate Snapcode release payload. Please ensure the release ZIP or publish folder is present.");
            }

            string exePath = Path.Combine(targetDir, "Snapcode.exe");
            string iconPath = Path.Combine(targetDir, "Assets", "app.ico");

            // Shortcuts
            UpdateStatus("Creating desktop and start menu shortcuts...");
            if (ChkDesktop.IsChecked == true)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string lnk = Path.Combine(desktopPath, "Snapcode.lnk");
                if (File.Exists(lnk)) { try { File.Delete(lnk); } catch { } }
                CreateShortcut(lnk, exePath, targetDir, iconPath);
            }

            if (ChkStartMenu.IsChecked == true)
            {
                string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string lnk = Path.Combine(programsPath, "Snapcode.lnk");
                if (File.Exists(lnk)) { try { File.Delete(lnk); } catch { } }
                CreateShortcut(lnk, exePath, targetDir, iconPath);
            }

            // Create uninstaller script
            UpdateStatus("Registering with Windows Settings...");
            string uninstallScriptPath = Path.Combine(targetDir, "Uninstall.ps1");
            string uninstallContent = $@"
$ErrorActionPreference = 'SilentlyContinue'
Stop-Process -Name Snapcode -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Remove-Item -Path '{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Snapcode.lnk")}' -Force
Remove-Item -Path '{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Snapcode.lnk")}' -Force
Remove-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Snapcode' -Recurse -Force
Remove-Item -Path '{targetDir}' -Recurse -Force
";
            File.WriteAllText(uninstallScriptPath, uninstallContent);

            // Register in HKCU Uninstall
            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Snapcode"))
            {
                if (key != null)
                {
                    key.SetValue("DisplayName", "Snapcode");
                    key.SetValue("DisplayVersion", "0.1.0");
                    key.SetValue("Publisher", "hkpc labs");
                    key.SetValue("DisplayIcon", iconPath);
                    key.SetValue("InstallLocation", targetDir);
                    key.SetValue("UninstallString", $"powershell.exe -ExecutionPolicy Bypass -File \"{uninstallScriptPath}\"");
                    key.SetValue("EstimatedSize", 74200, RegistryValueKind.DWord);
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }
            }
        }

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() => TxtStatus.Text = message);
        }

        private static string? FindReleaseZip(string startDir)
        {
            string[] candidates = new[]
            {
                Path.Combine(startDir, "Snapcode-v0.1.0-win-x64.zip"),
                Path.Combine(startDir, "Snapcode-v1.0.0-win-x64.zip"),
                Path.Combine(startDir, "..", "Snapcode-v0.1.0-win-x64.zip"),
                Path.Combine(startDir, "..", "releases", "Snapcode-v0.1.0-win-x64.zip"),
                Path.Combine(startDir, "..", "..", "releases", "Snapcode-v0.1.0-win-x64.zip"),
                @"C:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\releases\Snapcode-v0.1.0-win-x64.zip"
            };

            foreach (var cand in candidates)
            {
                if (File.Exists(cand)) return Path.GetFullPath(cand);
            }
            return null;
        }

        private static string? FindPublishDir(string startDir)
        {
            string[] candidates = new[]
            {
                Path.Combine(startDir, "publish"),
                Path.Combine(startDir, "..", "publish"),
                @"C:\Users\hardi\Documents\After Mids III-I\Software Engineering Project\vs files\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
            };

            foreach (var cand in candidates)
            {
                if (Directory.Exists(cand) && File.Exists(Path.Combine(cand, "Snapcode.exe")))
                {
                    return Path.GetFullPath(cand);
                }
            }
            return null;
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(sourceDir, dir);
                Directory.CreateDirectory(Path.Combine(targetDir, rel));
            }

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(sourceDir, file);
                File.Copy(file, Path.Combine(targetDir, rel), true);
            }
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string iconPath)
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDir;
            if (File.Exists(iconPath))
            {
                shortcut.IconLocation = iconPath + ",0";
            }
            shortcut.Description = "Snapcode Visual Flutter App Builder (hkpc labs)";
            shortcut.Save();
        }
    }
}
