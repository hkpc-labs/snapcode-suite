<#
.SYNOPSIS
    One-click native Windows Installer for Snapcode.
    Publisher: hkpc labs
    Version: 0.1.0
#>

param(
    [string]$InstallDir = "$env:LOCALAPPDATA\Programs\Snapcode"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$PublishDir = Join-Path $ProjectRoot "vs files\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"

if (!(Test-Path $PublishDir)) {
    Write-Host "[*] Release publish build not found. Running dotnet publish..." -ForegroundColor Cyan
    $publishProc = Start-Process dotnet -ArgumentList "publish `"$ProjectRoot\vs files\Snapcode.csproj`" -c Release -p:Platform=x64" -Wait -NoNewWindow -PassThru
    if ($publishProc.ExitCode -ne 0) {
        Write-Error "Failed to build Snapcode Release package."
        exit 1
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "       Installing Snapcode v0.1.0         " -ForegroundColor Cyan
Write-Host "            Publisher: hkpc labs          " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Stop any running Snapcode processes
$running = Get-Process Snapcode -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "[!] Closing running instance of Snapcode..." -ForegroundColor Yellow
    Stop-Process -Name Snapcode -Force
    Start-Sleep -Seconds 1
}

# 2. Copy files to target install directory
Write-Host "[1/4] Copying files to $InstallDir..." -ForegroundColor Green
if (!(Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}
Copy-Item -Path "$PublishDir\*" -Destination $InstallDir -Recurse -Force

$exePath = Join-Path $InstallDir "Snapcode.exe"
$iconPath = Join-Path $InstallDir "Assets\app.ico"

# 3. Create Windows Shortcuts (Desktop + Start Menu)
Write-Host "[2/4] Creating Desktop and Start Menu shortcuts with icon..." -ForegroundColor Green
$WshShell = New-Object -ComObject WScript.Shell

# Desktop Shortcut
$desktopPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)
$desktopShortcutPath = Join-Path $desktopPath "Snapcode.lnk"
if (Test-Path $desktopShortcutPath) { Remove-Item $desktopShortcutPath -Force }
$shortcut = $WshShell.CreateShortcut($desktopShortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $InstallDir
$shortcut.IconLocation = "$iconPath,0"
$shortcut.Description = "Snapcode Visual Flutter App Builder (hkpc labs)"
$shortcut.Save()

# Start Menu Shortcut
$programsPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::Programs)
$startMenuShortcutPath = Join-Path $programsPath "Snapcode.lnk"
if (Test-Path $startMenuShortcutPath) { Remove-Item $startMenuShortcutPath -Force }
$shortcut2 = $WshShell.CreateShortcut($startMenuShortcutPath)
$shortcut2.TargetPath = $exePath
$shortcut2.WorkingDirectory = $InstallDir
$shortcut2.IconLocation = "$iconPath,0"
$shortcut2.Description = "Snapcode Visual Flutter App Builder (hkpc labs)"
$shortcut2.Save()

# 4. Create Uninstaller script
Write-Host "[3/4] Creating uninstaller..." -ForegroundColor Green
$uninstallScriptPath = Join-Path $InstallDir "Uninstall.ps1"
$uninstallScriptContent = @"
`$ErrorActionPreference = 'SilentlyContinue'
Write-Host 'Uninstalling Snapcode...' -ForegroundColor Yellow
Stop-Process -Name Snapcode -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# Remove shortcuts
Remove-Item -Path '$desktopShortcutPath' -Force
Remove-Item -Path '$startMenuShortcutPath' -Force

# Remove Registry entry
Remove-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Snapcode' -Recurse -Force

# Remove installation directory
Remove-Item -Path '$InstallDir' -Recurse -Force

Write-Host 'Snapcode was successfully uninstalled.' -ForegroundColor Green
Start-Sleep -Seconds 2
"@
[System.IO.File]::WriteAllText($uninstallScriptPath, $uninstallScriptContent)

# 5. Register in Windows Settings (Add or Remove Programs)
Write-Host "[4/4] Registering in Windows Add or Remove Programs..." -ForegroundColor Green
$regKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Snapcode"
if (!(Test-Path $regKey)) {
    New-Item -Path $regKey -Force | Out-Null
}
Set-ItemProperty -Path $regKey -Name "DisplayName" -Value "Snapcode"
Set-ItemProperty -Path $regKey -Name "DisplayVersion" -Value "0.1.0"
Set-ItemProperty -Path $regKey -Name "Publisher" -Value "hkpc labs"
Set-ItemProperty -Path $regKey -Name "DisplayIcon" -Value $iconPath
Set-ItemProperty -Path $regKey -Name "InstallLocation" -Value $InstallDir
Set-ItemProperty -Path $regKey -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$uninstallScriptPath`""
Set-ItemProperty -Path $regKey -Name "EstimatedSize" -Value 74200
Set-ItemProperty -Path $regKey -Name "NoModify" -Value 1
Set-ItemProperty -Path $regKey -Name "NoRepair" -Value 1

Write-Host "`n[SUCCESS] Snapcode v0.1.0 is installed!" -ForegroundColor Green
Write-Host " -> App Location: $exePath"
Write-Host " -> Publisher: hkpc labs"
Write-Host " -> Icon: $iconPath"
Write-Host " -> Desktop & Start Menu shortcuts created."
Write-Host " -> Appears in Windows Settings > Installed Apps."
