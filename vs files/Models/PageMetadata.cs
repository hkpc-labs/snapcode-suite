using System.Text.Json.Serialization;

namespace Snapcode.Models
{
    public class PageMetadata
    {
        public string Name { get; set; } = "NewPage";
        public string Type { get; set; } = "Page"; // "Page", "DialogFragment", or "CustomView"
        public bool IsToolbarHidden { get; set; }
        public bool IsStatusBarHidden { get; set; }
        public bool HasFAB { get; set; }
        public bool HasDrawer { get; set; }
    }
}
