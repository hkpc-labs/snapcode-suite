using System;
using System.Text.Json.Serialization;

namespace Snapcode.Models
{
    /// <summary>
    /// Metadata for a visual page in Snapcode (.snapcode/pages/{name}_meta.json).
    /// Stores comprehensive layout, AppBar, orientation, and scaffold configuration.
    /// </summary>
    public class PageMetadata
    {
        // General Page Identification
        public string Name { get; set; } = "NewPage";
        public string Title { get; set; } = "";
        public string Type { get; set; } = "Page"; // "Page", "DialogFragment", "CustomView", or "Drawer"
        public string RoutePath { get; set; } = "";

        [JsonIgnore]
        public bool IsDrawer => string.Equals(Type, "Drawer", StringComparison.OrdinalIgnoreCase) || 
                                (Name != null && Name.EndsWith("_drawer", StringComparison.OrdinalIgnoreCase));

        // AppBar / Toolbar Properties
        public bool ShowAppBar { get; set; } = true;

        [JsonIgnore]
        public bool IsToolbarHidden
        {
            get => !ShowAppBar;
            set => ShowAppBar = !value;
        }

        public string AppBarTitle { get; set; } = "";
        public string AppBarBackgroundColor { get; set; } = "#2196F3";
        public string AppBarForegroundColor { get; set; } = "#FFFFFF";
        public double AppBarElevation { get; set; } = 4.0;
        public bool AppBarCenterTitle { get; set; } = false;
        public string AppBarLeadingIcon { get; set; } = "back"; // "back", "menu", "none"
        public bool AppBarShowActions { get; set; } = false;

        // Device Orientation
        // "Portrait" (locks to portrait), "Landscape" (locks to landscape), "Both" (auto-rotate)
        public string Orientation { get; set; } = "Portrait";

        // Scaffold / Body Properties
        public string ScaffoldBackgroundColor { get; set; } = "#FFFFFF";
        public bool SafeArea { get; set; } = true;
        public bool ResizeToAvoidBottomInset { get; set; } = true;
        public bool IsStatusBarHidden { get; set; } = false;
        public string StatusBarStyle { get; set; } = "Default"; // "Default", "Light", "Dark"

        // Floating Action Button (FAB)
        public bool HasFAB { get; set; } = false;
        public string FabIcon { get; set; } = "add"; // "add", "edit", "share", "check", "star", "camera"
        public string FabBackgroundColor { get; set; } = "#2196F3";
        public string FabTooltip { get; set; } = "Action";

        // Navigation Components
        public bool HasDrawer { get; set; } = false;
        public bool HasBottomNavigationBar { get; set; } = false;
    }
}
