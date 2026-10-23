using System;
using Microsoft.UI.Xaml;

namespace Snapcode.Views
{
    public sealed partial class DebugLogsWindow : Window
    {
        public DebugLogsWindow()
        {
            this.InitializeComponent();
            this.Title = "SnapCode - Debug Console & Live Logs";

            ConsoleControl.CloseRequested += (s, e) =>
            {
                this.Close();
            };
        }
    }
}
