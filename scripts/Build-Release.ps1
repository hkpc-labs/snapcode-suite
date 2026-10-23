<#
.SYNOPSIS
    Builds all final production Release packages for Snapcode.
    Publisher: hkpc labs
    Version: 0.1.0
    Creates:
    1. Standalone self-contained release directory (bin\Release\...\publish)
    2. Portable Zip package (releases\Snapcode-v0.1.0-win-x64.zip)
    3. Lightweight GUI Thin Installer (releases\Snapcode-ThinInstaller.exe)
    4. Setup Wizard Installer (.exe) via Inno Setup (releases\Snapcode-Setup-v0.1.0.exe)
    5. 1-Click Batch Installer wrapper (releases\Install-Snapcode.cmd)
#>

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$Csproj = Join-Path $ProjectRoot "vs files\Snapcode.csproj"
$ThinInstallerCsproj = Join-Path $ProjectRoot "installer\ThinInstaller\Snapcode-ThinInstaller.csproj"
$ReleasesDir = Join-Path $ProjectRoot "releases"
$PublishDir = Join-Path $ProjectRoot "vs files\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     Building Snapcode Release v0.1.0     " -ForegroundColor Cyan
Write-Host "            Publisher: hkpc labs          " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Ensure output release directory exists
if (!(Test-Path $ReleasesDir)) {
    New-Item -ItemType Directory -Path $ReleasesDir -Force | Out-Null
}

# Clean old release packages in releases dir to avoid clutter
Get-ChildItem -Path $ReleasesDir -Filter "Snapcode-v*.zip" | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path $ReleasesDir -Filter "Snapcode-Setup-*.exe" | Remove-Item -Force -ErrorAction SilentlyContinue

# 2. Clean old publish directory to ensure fresh compilation
if (Test-Path $PublishDir) {
    Write-Host "[*] Cleaning previous publish directory..." -ForegroundColor Gray
    Remove-Item -Path $PublishDir -Recurse -Force -ErrorAction SilentlyContinue
}

# 3. Run dotnet publish for main app
Write-Host "[1/5] Compiling and publishing self-contained Snapcode Release..." -ForegroundColor Green
$publishProc = Start-Process dotnet -ArgumentList "publish `"$Csproj`" -c Release -p:Platform=x64" -Wait -NoNewWindow -PassThru
if ($publishProc.ExitCode -ne 0) {
    Write-Error "dotnet publish failed with exit code $($publishProc.ExitCode)"
    exit 1
}

# Verify Assets in PublishDir
$srcAssets = Join-Path $ProjectRoot "vs files\Assets"
$dstAssets = Join-Path $PublishDir "Assets"
if (Test-Path $srcAssets) {
    Copy-Item -Path "$srcAssets\*" -Destination $dstAssets -Recurse -Force
}

# 4. Create portable zip package
Write-Host "[2/5] Creating portable ZIP release package..." -ForegroundColor Green
$zipPath = Join-Path $ReleasesDir "Snapcode-v0.1.0-win-x64.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$PublishDir\*" -DestinationPath $zipPath -Force

$zipSizeMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "  -> Created $zipPath ($zipSizeMB MB)" -ForegroundColor Yellow

# 5. Compile Thin Installer
Write-Host "[3/5] Building lightweight GUI Thin Installer..." -ForegroundColor Green
$thinProc = Start-Process dotnet -ArgumentList "publish `"$ThinInstallerCsproj`" -c Release -r win-x64 -o `"$ReleasesDir`" /p:PublishSingleFile=true /p:SelfContained=false" -Wait -NoNewWindow -PassThru
if ($thinProc.ExitCode -eq 0) {
    $pdb = Join-Path $ReleasesDir "Snapcode-ThinInstaller.pdb"
    if (Test-Path $pdb) { Remove-Item $pdb -Force }
    $thinSizeKB = [math]::Round((Get-Item (Join-Path $ReleasesDir "Snapcode-ThinInstaller.exe")).Length / 1KB, 1)
    Write-Host "  -> Thin Installer created: Snapcode-ThinInstaller.exe ($thinSizeKB KB)" -ForegroundColor Yellow
} else {
    Write-Host "  -> Warning: Failed to build Thin Installer." -ForegroundColor Yellow
}

# 6. Create 1-click batch installer wrapper in releases directory
Write-Host "[4/5] Creating 1-click batch installer script..." -ForegroundColor Green
$cmdPath = Join-Path $ReleasesDir "Install-Snapcode.cmd"
$cmdContent = @"
@echo off
cd /d "%~dp0"
echo Starting Snapcode 1-Click Installer (Publisher: hkpc labs)...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\scripts\Install-Snapcode.ps1"
pause
"@
[System.IO.File]::WriteAllText($cmdPath, $cmdContent)
Write-Host "  -> Created $cmdPath" -ForegroundColor Yellow

# 7. Compile Inno Setup installer if available
Write-Host "[5/5] Compiling Inno Setup wizard installer (iscc.exe)..." -ForegroundColor Green
$cmd = Get-Command iscc.exe -ErrorAction SilentlyContinue
$isccPath = if ($cmd) { $cmd.Source } else { $null }
if (!$isccPath) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
    )
    foreach ($cand in $candidates) {
        if (Test-Path $cand) { $isccPath = $cand; break }
    }
}

if ($isccPath) {
    Write-Host "  -> Found Inno Setup at $isccPath" -ForegroundColor Cyan
    $issFile = Join-Path $ProjectRoot "installer\Snapcode-Installer.iss"
    Start-Process $isccPath -ArgumentList "`"$issFile`"" -Wait -NoNewWindow
    Write-Host "  -> Setup Wizard Installer created in $ReleasesDir!" -ForegroundColor Yellow
} else {
    Write-Host "  -> Note: Inno Setup not detected on PATH." -ForegroundColor Gray
}

# 8. Automatically refresh the user's currently installed local app copy if it exists
$localInstall = "$env:LOCALAPPDATA\Programs\Snapcode"
if (Test-Path $localInstall) {
    Write-Host "`n[*] Updating existing local installation in $localInstall with latest build..." -ForegroundColor Cyan
    try {
        Get-Process Snapcode -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 1
        Copy-Item -Path "$PublishDir\*" -Destination $localInstall -Recurse -Force
        Write-Host "  -> Local installation updated to latest build successfully!" -ForegroundColor Green
    } catch {
        Write-Host "  -> Could not auto-update local install: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " [SUCCESS] All release assets generated successfully!   " -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host "Directory: $ReleasesDir" -ForegroundColor Cyan
Get-ChildItem -Path $ReleasesDir | Select-Object Name, @{Name="Size (MB)";Expression={[math]::Round($_.Length / 1MB, 2)}} | Format-Table -AutoSize
