@echo off
cd /d "%~dp0"
echo Starting Snapcode 1-Click Installer (Publisher: hkpc labs)...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\scripts\Install-Snapcode.ps1"
pause