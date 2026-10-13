using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Snapcode.Models
{
    public enum LogEntryType
    {
        Info,
        Warning,
        Error,
        Success,
        Reload,
        System
    }

    [Microsoft.UI.Xaml.Data.Bindable]
    public class DebugLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Message { get; set; } = "";
        public LogEntryType Type { get; set; } = LogEntryType.Info;

        public string FormattedTime => Timestamp.ToString("HH:mm:ss");

        public string IconGlyph => Type switch
        {
            LogEntryType.Error => "\uEA39",    // Error badge
            LogEntryType.Warning => "\uE7BA",  // Warning triangle
            LogEntryType.Success => "\uE73E",  // Checkmark
            LogEntryType.Reload => "\uE72C",   // Refresh/Lightning
            LogEntryType.System => "\uE713",   // Gear
            _ => "\uE946"                      // Info circle
        };

        public SolidColorBrush ColorBrush
        {
            get
            {
                return Type switch
                {
                    LogEntryType.Error => new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)),    // Red
                    LogEntryType.Warning => new SolidColorBrush(Color.FromArgb(255, 255, 179, 0)),  // Amber
                    LogEntryType.Success => new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)),   // Green
                    LogEntryType.Reload => new SolidColorBrush(Color.FromArgb(255, 0, 188, 212)),    // Cyan
                    LogEntryType.System => new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)), // Muted
                    _ => new SolidColorBrush(Color.FromArgb(255, 224, 224, 224))                    // Standard Light Gray
                };
            }
        }
    }
}
