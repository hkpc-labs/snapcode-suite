# Snapcode Releases

This folder contains generated production releases and installation packages for Snapcode.

## Generated Release Artifacts

- **`Snapcode-Setup-v<version>.exe`**: Standard Windows Setup Wizard installer (Inno Setup) with Start Menu and Desktop shortcuts.
- **`Snapcode-ThinInstaller.exe`**: Ultra-fast, lightweight GUI thin installer.
- **`Snapcode-v<version>-win-x64.zip`**: Zero-install portable package.
- **`Install-Snapcode.cmd`**: 1-click native batch installer script.

## Rebuilding Releases

To build all release packages from source, run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Build-Release.ps1
```
