using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Snapcode.Models;
using Snapcode.Services;

namespace Snapcode.Views
{
    public sealed partial class EmulatorManagerDialog : ContentDialog
    {
        private readonly ObservableCollection<FlutterDevice> _emulators = new();

        public EmulatorManagerDialog()
        {
            this.InitializeComponent();
            ListEmulators.ItemsSource = _emulators;

            Loaded += async (s, e) =>
            {
                await LoadEmulatorsAsync();
            };
        }

        private async Task LoadEmulatorsAsync()
        {
            ProgressScan.Visibility = Visibility.Visible;
            TxtStatusFeedback.Text = "Scanning for Android Virtual Devices...";
            _emulators.Clear();

            try
            {
                var avds = await DeviceService.GetInstalledAndroidAvdsAsync();
                var running = await DeviceService.GetRunningFlutterDevicesAsync();

                foreach (var avd in avds)
                {
                    // Check if already running
                    bool isOnline = running.Any(r => r.Platform == DevicePlatform.Android && 
                        (r.Name.Contains(avd.Name, StringComparison.OrdinalIgnoreCase) || r.Id.Contains(avd.Id, StringComparison.OrdinalIgnoreCase)));
                    avd.IsRunning = isOnline;
                    _emulators.Add(avd);
                }

                if (_emulators.Count == 0)
                {
                    TxtEmptyNotice.Visibility = Visibility.Visible;
                    TxtStatusFeedback.Text = "No emulators found.";
                }
                else
                {
                    TxtEmptyNotice.Visibility = Visibility.Collapsed;
                    TxtStatusFeedback.Text = $"Found {_emulators.Count} Android Virtual Device(s).";
                }
            }
            catch (Exception ex)
            {
                TxtStatusFeedback.Text = $"Error scanning emulators: {ex.Message}";
            }
            finally
            {
                ProgressScan.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadEmulatorsAsync();
        }

        private async void BtnLaunchItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string avdId)
            {
                btn.IsEnabled = false;
                TxtStatusFeedback.Text = $"Launching emulator '{avdId}'...";
                bool launched = await DeviceService.LaunchAvdAsync(avdId);
                if (launched)
                {
                    TxtStatusFeedback.Text = $"Emulator '{avdId}' launch command issued. It may take a minute to boot.";
                }
                else
                {
                    TxtStatusFeedback.Text = $"Failed to launch emulator '{avdId}'. Check Android SDK installation.";
                }

                await Task.Delay(2000);
                btn.IsEnabled = true;
                await LoadEmulatorsAsync();
            }
        }
    }
}
