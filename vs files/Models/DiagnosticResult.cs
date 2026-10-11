using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Snapcode.Models
{
    /// <summary>
    /// Severity level for a diagnostic check.
    /// </summary>
    public enum DiagnosticStatus
    {
        /// <summary>Check has not yet been executed.</summary>
        Pending,
        /// <summary>Currently running the check.</summary>
        Scanning,
        /// <summary>Tool found and verified successfully.</summary>
        Found,
        /// <summary>Tool is missing or misconfigured but not strictly required for all targets.</summary>
        Warning,
        /// <summary>Tool is missing and is required for the app to function.</summary>
        Error
    }

    /// <summary>
    /// Categorizes which compilation target a tool is required for.
    /// </summary>
    public enum ToolRequirement
    {
        /// <summary>Required for all targets (Git, Flutter, WebView2).</summary>
        Critical,
        /// <summary>Required only when targeting Windows desktop builds.</summary>
        WindowsTarget,
        /// <summary>Required only when targeting Android builds.</summary>
        AndroidTarget,
        /// <summary>Optional but enhances the experience.</summary>
        Optional
    }

    /// <summary>
    /// The type of auto-fix action available for a failed diagnostic item.
    /// </summary>
    public enum FixAction
    {
        None,
        AddToPath,
        SetEnvironmentVariable,
        OpenSystemSettings,
        AcceptLicenses,
        DownloadExternal,
        InstallViaWinget
    }

    /// <summary>
    /// Represents the result of a single diagnostic check for one tool/component.
    /// Implements INotifyPropertyChanged so WinUI 3 data-bound UI elements update live
    /// as each check completes asynchronously.
    /// </summary>
    public class DiagnosticItem : INotifyPropertyChanged
    {
        private DiagnosticStatus _status = DiagnosticStatus.Pending;
        private string _detectedPath = "";
        private string _customPath = "";
        private string _detectedVersion = "";
        private string _statusMessage = "Waiting for scan...";
        private string _remediationHint = "";
        private bool _isValidating = false;

        /// <summary>Unique identifier for this check (e.g. "flutter", "git", "android_sdk").</summary>
        public string Id { get; set; } = "";

        /// <summary>Human-readable display name (e.g. "Flutter SDK").</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>Short description of why this tool is needed.</summary>
        public string Description { get; set; } = "";

        /// <summary>Icon glyph from Segoe Fluent Icons font.</summary>
        public string IconGlyph { get; set; } = "\uE835";

        /// <summary>Whether this tool is critical, target-specific, or optional.</summary>
        public ToolRequirement Requirement { get; set; } = ToolRequirement.Critical;

        /// <summary>Whether this item supports browsing for a custom folder path.</summary>
        public bool CanBrowse { get; set; } = false;

        /// <summary>The primary auto-fix action type for this item.</summary>
        public FixAction PrimaryFix { get; set; } = FixAction.None;

        /// <summary>URL to open for downloading the tool (if applicable).</summary>
        public string DownloadUrl { get; set; } = "";

        /// <summary>Winget package ID for one-click installation (e.g. "Git.Git").</summary>
        public string WingetPackageId { get; set; } = "";

        /// <summary>Name of the environment variable this item configures (e.g. "ANDROID_HOME").</summary>
        public string EnvironmentVariableName { get; set; } = "";

        /// <summary>Current status of this diagnostic check.</summary>
        public DiagnosticStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        /// <summary>Filesystem path where the tool was auto-detected (empty if not found).</summary>
        public string DetectedPath
        {
            get => _detectedPath;
            set { _detectedPath = value; OnPropertyChanged(); }
        }

        /// <summary>User-overridden custom path. Takes priority over DetectedPath when non-empty.</summary>
        public string CustomPath
        {
            get => _customPath;
            set { _customPath = value; OnPropertyChanged(); }
        }

        /// <summary>Returns the effective path: CustomPath if set, otherwise DetectedPath.</summary>
        public string EffectivePath =>
            !string.IsNullOrWhiteSpace(CustomPath) ? CustomPath : DetectedPath;

        /// <summary>Detected version string (e.g. "3.22.2").</summary>
        public string DetectedVersion
        {
            get => _detectedVersion;
            set { _detectedVersion = value; OnPropertyChanged(); }
        }

        /// <summary>Human-readable status message displayed in the UI.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        /// <summary>Hint text shown when the check fails, telling the user how to fix it.</summary>
        public string RemediationHint
        {
            get => _remediationHint;
            set { _remediationHint = value; OnPropertyChanged(); }
        }

        /// <summary>Whether a per-item validation is currently running.</summary>
        public bool IsValidating
        {
            get => _isValidating;
            set { _isValidating = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Aggregated report from a full diagnostic scan.
    /// </summary>
    public class DiagnosticReport
    {
        public List<DiagnosticItem> Items { get; set; } = new();
        public DateTime ScanTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>True if all Critical items have Status == Found.</summary>
        public bool AllCriticalsPassed =>
            Items.TrueForAll(item =>
                item.Requirement != ToolRequirement.Critical || item.Status == DiagnosticStatus.Found);

        /// <summary>True if every single item (including optional) passed.</summary>
        public bool AllPassed =>
            Items.TrueForAll(item => item.Status == DiagnosticStatus.Found);

        /// <summary>Count of items that passed.</summary>
        public int PassedCount =>
            Items.FindAll(item => item.Status == DiagnosticStatus.Found).Count;

        /// <summary>Count of items that failed (Error or Warning).</summary>
        public int FailedCount =>
            Items.FindAll(item => item.Status == DiagnosticStatus.Error || item.Status == DiagnosticStatus.Warning).Count;
    }
}
