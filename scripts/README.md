# Snapcode Scripts & Automation

This directory organizes all project automation, installer pipelines, test suites, and maintenance tools.

## Core Build & Install Scripts

| Script | Purpose |
| :--- | :--- |
| `Build-Release.ps1` | Orchestrates the full production release build pipeline (self-contained app, portable ZIP, thin installer, and Inno Setup Windows installer wizard). |
| `Install-Snapcode.ps1` | Native PowerShell installer that installs Snapcode to `%LOCALAPPDATA%\Programs\Snapcode`, registers desktop & start menu shortcuts, and adds Windows Add/Remove programs entry. |

## Subdirectories

- **`tests/`**: Automated verification test suites, E2E live runner, UI launch testers, block coverage scripts, and window capture utilities.
- **`tools/`**: Documentation generators (Word docs, technical reports) and architecture diagram generators.
- **`patches/`**: Historical migration, block repair, and code patching utilities.
