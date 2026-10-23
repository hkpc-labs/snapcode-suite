using System;

namespace Snapcode.Models
{
    public enum DevicePlatform
    {
        Web,
        Android,
        Windows,
        Unknown
    }

    public class FlutterDevice
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DevicePlatform Platform { get; set; } = DevicePlatform.Web;
        public bool IsEmulator { get; set; }
        public bool IsRunning { get; set; }
        public string Sdk { get; set; } = "";
        public string RawTargetPlatform { get; set; } = "";

        public string DisplayName
        {
            get
            {
                string status = IsEmulator && !IsRunning ? " [Offline AVD]" : (IsRunning ? " [Online]" : "");
                return $"{Name}{status}";
            }
        }

        public string IconGlyph
        {
            get
            {
                return Platform switch
                {
                    DevicePlatform.Web => "\uE774",      // Globe
                    DevicePlatform.Android => "\uE8EA",  // CellPhone
                    DevicePlatform.Windows => "\uE7F4",  // PC
                    _ => "\uE7F8"                        // Device
                };
            }
        }

        public override string ToString() => DisplayName;
    }
}
