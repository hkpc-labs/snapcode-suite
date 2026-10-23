# Snapcode Automated App Runner & Verification Script
# Launches the app into newapp, connects to WebView2 DevTools, captures window screenshots, and validates UI state

param(
    [switch]$NoExit,
    [int]$WaitSeconds = 8
)

$ErrorActionPreference = "Stop"

$appExe = Resolve-Path ".\src\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\Snapcode.exe"
if (-not (Test-Path $appExe)) {
    Write-Error "Snapcode.exe not found at $appExe. Run dotnet build -p:Platform=x64 first."
}

$projectPath = Resolve-Path "$HOME\Documents\Snapcode\repos\newapp"
Write-Host ">>> Launching Snapcode with project: $projectPath" -ForegroundColor Cyan
Write-Host ">>> Enabling WebView2 remote debugging on port 9222..." -ForegroundColor Cyan

# Set WebView2 remote debugging port
$env:WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS = "--remote-debugging-port=9222"

$proc = Start-Process -FilePath $appExe -ArgumentList "--project `"$projectPath`"" -PassThru

try {
    Write-Host ">>> Waiting for app window to initialize ($WaitSeconds seconds)..." -ForegroundColor Yellow
    Start-Sleep -Seconds $WaitSeconds

    # Load Windows UI Automation and Drawing
    [System.Reflection.Assembly]::LoadWithPartialName("UIAutomationClient") | Out-Null
    [System.Reflection.Assembly]::LoadWithPartialName("UIAutomationTypes") | Out-Null
    Add-Type -AssemblyName System.Drawing
    Add-Type -AssemblyName System.Windows.Forms

    # Find the main window
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $appWindow = $null

    # Try by process ID first
    $snapcodeProcs = Get-Process -Name "Snapcode*" -ErrorAction SilentlyContinue
    foreach ($p in $snapcodeProcs) {
        $cond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty,
            $p.Id
        )
        $w = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
        if ($w) { $appWindow = $w; break }
    }

    # If not found by process, search all top-level windows
    if (-not $appWindow) {
        $allWindows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($w in $allWindows) {
            if ($w.Current.Name -like "*SnapCode*" -or $w.Current.Name -like "*newapp*") {
                $appWindow = $w
                break
            }
        }
    }

    if ($appWindow) {
        $windowName = $appWindow.Current.Name
        Write-Host ">>> Successfully located Snapcode Window: '$windowName'" -ForegroundColor Green
        
        # Capture Screenshot of the Running Window
        $rect = $appWindow.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            $bmp = New-Object System.Drawing.Bitmap ([int]$rect.Width), ([int]$rect.Height)
            $graphics = [System.Drawing.Graphics]::FromImage($bmp)
            $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, (New-Object System.Drawing.Size([int]$rect.Width, [int]$rect.Height)))
            
            $screenshotPath = ".\helpers\app_screenshot.png"
            $bmp.Save($screenshotPath, [System.Drawing.Imaging.ImageFormat]::Png)
            $graphics.Dispose()
            $bmp.Dispose()
            Write-Host ">>> Window screenshot saved to: $screenshotPath" -ForegroundColor Green
        }
    } else {
        Write-Warning "Could not find main window by AutomationElement. The app may still be loading."
    }

    # Query WebView2 CDP endpoints
    Write-Host ">>> Querying WebView2 Chrome DevTools Protocol at http://localhost:9222/json..." -ForegroundColor Cyan
    try {
        $endpoints = Invoke-RestMethod -Uri "http://localhost:9222/json" -TimeoutSec 3 -ErrorAction Stop
        Write-Host ">>> Connected to WebView2 successfully! Found $($endpoints.Count) target(s):" -ForegroundColor Green
        foreach ($ep in $endpoints) {
            Write-Host "    - Title: $($ep.title)" -ForegroundColor Gray
            Write-Host "      URL:   $($ep.url)" -ForegroundColor Gray
            Write-Host "      WS:    $($ep.webSocketDebuggerUrl)" -ForegroundColor Gray
        }
    } catch {
        Write-Host ">>> WebView2 CDP not yet responding on port 9222." -ForegroundColor Yellow
    }

} finally {
    if (-not $NoExit) {
        Write-Host ">>> Stopping test processes..." -ForegroundColor Cyan
        Get-Process -Name "Snapcode*" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host ">>> App left running as requested." -ForegroundColor Green
    }
}
