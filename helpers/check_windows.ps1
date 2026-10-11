Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$root = [System.Windows.Automation.AutomationElement]::RootElement
$windows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
foreach ($w in $windows) {
    if ($w.Current.Name) {
        Write-Host "Window: $($w.Current.Name) | Class: $($w.Current.ClassName) | PID: $($w.Current.ProcessId)"
    }
}
