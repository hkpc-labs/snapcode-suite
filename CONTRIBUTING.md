# Contributing to Snapcode

Thank you for your interest in contributing to **Snapcode**! Snapcode is built and maintained by **hkpc labs** as an open-source visual Flutter application builder.

We welcome contributions from developers, designers, and documentation writers. This document provides clear guidelines for setting up your environment, submitting pull requests, and following our development standards.

---

## 📋 Table of Contents
1. [Code of Conduct](#code-of-conduct)
2. [Development Environment Setup](#development-environment-setup)
3. [Repository Layout](#repository-layout)
4. [Contribution Workflow](#contribution-workflow)
5. [Commit Message Conventions](#commit-message-conventions)
6. [Coding Standards](#coding-standards)
7. [Testing & Verification](#testing--verification)
8. [Submitting Pull Requests](#submitting-pull-requests)

---

## 🤝 Code of Conduct

We are committed to providing a welcoming, inclusive, and harassment-free experience for everyone. Please be respectful, constructive, and collaborative in all discussions and pull requests.

---

## 💻 Development Environment Setup

### Prerequisites
- **Operating System:** Windows 10 (Build 19041+) or Windows 11 (x64)
- **IDE:** [Visual Studio 2022 Community / Professional](https://visualstudio.microsoft.com/)
  - Workload: **.NET Desktop Development**
  - Component: **Windows App SDK (WinUI 3)** C# Templates
- **SDK:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Flutter:** [Flutter SDK](https://flutter.dev/docs/get-started/install/windows) (Version 3.19+ recommended)
- **Inno Setup:** (Optional, for building full installers) [Inno Setup 6+](https://jrsoftware.org/isinfo.php)

### Cloning and Opening
```bash
git clone https://github.com/hkpc-labs/snapcode.git
cd snapcode
```

1. Open **Visual Studio 2022**.
2. Select **Open a project or solution**.
3. Choose `src/Snapcode.slnx` or `src/Snapcode.csproj`.
4. Set the active solution platform to **x64**.
5. Press **F5** to build and run in Debug mode.

---

## 📁 Repository Layout

```
├── Docs/                     # Architecture specifications & system verification reports
├── helpers/                  # Auxiliary development notes
├── installer/
│   ├── Snapcode-Installer.iss # Inno Setup installer script
│   └── ThinInstaller/        # Standalone lightweight WPF Thin Installer
├── releases/                 # Production distribution folder & checksums
├── scripts/
│   ├── tests/                # Automated verification and end-to-end test scripts
│   ├── tools/                # Maintenance, reporting, and diagram generation tools
│   ├── patches/              # Codebase maintenance patches
│   ├── Build-Release.ps1     # 1-click pipeline for generating all release packages
│   └── Install-Snapcode.ps1  # Automated local silent installer
├── snapshots/                # Architecture milestone snapshots
└── src/                      # WinUI 3 Core Application Source
    ├── Assets/               # Icons, fonts, and BlockEditor (Google Blockly) HTML/JS
    ├── Models/               # Data contracts, diagnostics, and project models
    ├── Properties/           # Publish profiles and launch settings
    ├── Services/             # Flutter compiler, device manager, and code preservation
    ├── ViewModels/           # MVVM view models
    └── Views/                # UI designers, editor pages, and dialogs
```

---

## 🔄 Contribution Workflow

1. **Fork and Branch:**
   Create a descriptive branch for your work:
   ```bash
   git checkout -b feat/add-audio-block
   # or
   git checkout -b fix/editor-tab-crash
   ```
2. **Make Focused Changes:**
   Keep changes modular and directly relevant to the issue or feature.
3. **Verify Locally:**
   Run builds and automated verification scripts before committing (see below).
4. **Push and Open a Pull Request:**
   Push your branch and open a PR targeting the `master` branch.

---

## 📝 Commit Message Conventions

We adhere to the [Conventional Commits](https://www.conventionalcommits.org/) specification:

| Prefix | Description | Example |
| :--- | :--- | :--- |
| `feat:` | A new feature or capability | `feat: add lists and collections category to blockly toolbox` |
| `fix:` | A bug fix | `fix: resolve block canvas blank screen on direct file navigation` |
| `docs:` | Documentation changes | `docs: update contributing and release notes` |
| `refactor:` | Code restructuring without behavior changes | `refactor: rename 'vs files' folder to standard 'src' directory` |
| `test:` | Adding or updating automated tests | `test: add comprehensive end-to-end verification script` |
| `chore:` | Build scripts, dependencies, or maintenance | `chore: update .gitignore to exclude webview2 caches` |

---

## 🎨 Coding Standards

### C# & WinUI 3
- Enable nullable reference types (`<Nullable>enable</Nullable>`).
- Use asynchronous APIs with proper cancellation token support.
- **WinUI Constraint:** Never assign `double.NaN` to `MinWidth` or `MinHeight` in XAML or code-behind (use `0.0` or explicit bounds to prevent WinRT crashes).
- Ensure all resources in `Assets/` are registered with appropriate Build Actions (`Content` with `Copy to Output Directory: PreserveNewest`).

### Blockly & Web Integration
- Follow ES6+ standards in `src/Assets/BlockEditor/index.html`.
- Maintain decoupled WebView2 bridge communications via JSON message payloads (`chrome.webview.postMessage`).
- Ensure block definitions have corresponding Dart generator functions in `dartGenerator.forBlock`.

---

## 🧪 Testing & Verification

Before submitting any Pull Request, ensure that:

1. **Solution Compiles Cleanly:**
   ```powershell
   dotnet build src/Snapcode.csproj -c Release -p:Platform=x64
   ```
   *Must exit with 0 errors.*

2. **Automated Verification Passes:**
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts\tests\run_comprehensive_verification.ps1
   ```

3. **Release Packaging Succeeds (if modifying installer/build pipeline):**
   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts\Build-Release.ps1
   ```

---

## 🚀 Submitting Pull Requests

- Provide a concise title summarizing the change.
- Describe the motivation, problem solved, and any breaking changes.
- Attach screenshots or recording clips for UI modifications.
- Reference any related GitHub issues (`Fixes #123`).

Thank you for building the future of visual Flutter app development with **hkpc labs**!
