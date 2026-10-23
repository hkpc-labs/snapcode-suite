# Snapcode v0.1.0 Release Notes

**Release Date:** October 23, 2026  
**Publisher:** hkpc labs  
**Tag:** `v0.1.0`  
**Target:** Windows 10 / 11 (x64)

---

## Overview

We are pleased to announce the release preview of **Snapcode v0.1.0**, a visual, block-driven Flutter application builder designed for rapid cross-platform app development on Windows.

Snapcode merges intuitive drag-and-drop UI composition with visual logic blocks powered by Google Blockly, automatically compiling real-time AST-level Flutter Dart code with full bi-directional code preservation.

---

## Key Highlights & Features in v0.1.0

### 1. Visual Drag-and-Drop Native UI Designer
- **Interactive Canvas:** Visual layout editor with intuitive drag-and-drop tree hierarchy.
- **Flutter Widget Support:** Buttons, Containers, Rows, Columns, Text, TextFields, Images, Cards, and dynamic Grid layouts.
- **Live Properties Panel:** Real-time property inspector supporting colors, dimensions, margins, paddings, typography, and flex alignments.
- **Responsive Layout Geometry:** Compiles visual nodes into clean, idiomatic Flutter widget trees (`build(BuildContext context)`).

### 2. Visual Event Logic (Google Blockly)
- **High-Performance WebView2 Bridge:** Integrated Google Blockly engine with dynamic theme-aware styling and responsive `ResizeObserver` canvas resizing.
- **Direct Protocol Integration:** Replaced virtual host mappings with direct, secure local `file:///` protocols for zero-delay workspace loading.
- **Rich Visual Logic Toolbox:**
  - **Lists & Collections:** `List.filled()`, `list[index]` getter/setter, 2D matrix win-patterns, list fullness checks (`!list.contains('')`).
  - **Logic & Control Flow:** `for each` loops, compound `AND`/`OR` boolean operators, ternary toggles (`currentPlayer == 'X' ? 'O' : 'X'`), and custom function callers (`_makeMove(i)`).
  - **Dynamic State Declarations:** Strongly typed state variables (`List<String>`, `String`, `int`, `bool`) that automatically declare inside `_MainPageState`.
  - **Interactive Dialogs & Modals:** `showDialog` with barrier control, alert content, and customizable callback actions.
  - **Dynamic Button Text Bindings:** Visual blocks to dynamically update individual cell buttons (`_btnCell0Text` through `_btnCell8Text`) via automatic `setState()` triggers.
- **Zero Raw-Code Requirement:** Full games (such as interactive Tic-Tac-Toe with turns, winning detection, and reset dialogs) can now be authored entirely with visual blocks without raw Dart injection blocks.

### 3. Bi-Directional Code Preservation Service
- **Intelligent Marker Tokenization:** User code written in custom methods (`CUSTOM_METHODS`), imports (`CUSTOM_IMPORTS`), and lifecycle hooks (`INIT_STATE`, `DISPOSE`) is automatically preserved when visual UI is regenerated.
- **Orphan Block Archival:** Deprecated visual blocks or removed widget events are safely archived rather than lost.

### 4. Integrated Flutter Toolchain & Device Runner
- **Live Device Discovery:** Automatically detects connected Android devices, running emulators, and Windows desktop targets.
- **Log Streaming & Hot Reload:** Real-time log streaming output in an embedded diagnostic console with hot-reload triggers.

---

## Release Artifacts & Distribution

All release packages are built with self-contained runtimes (no separate .NET runtime installation required).

| File Name | Description | Size | SHA-256 Checksum |
| :--- | :--- | :--- | :--- |
| `Snapcode-Setup-v0.1.0.exe` | Full Windows Setup Wizard (Inno Setup) | ~51 MB | `0B907CCD5C66BB1367E77AAB7163CD77A12A0806BBC363863C6E47617B7FBFDB` |
| `Snapcode-ThinInstaller.exe` | Lightweight GUI Thin Installer | ~324 KB | `EAB1D4BD8F73E72164D308CB609E92D683D6F1490E22608D82FD91F74FB3FF11` |
| `Snapcode-v0.1.0-win-x64.zip` | Standalone Portable ZIP Package | ~71 MB | `E40FB12A0CE530C9B77E0417949E9AA075506DC3DFFFA440ADA9F6E1FBE4C3C8` |
| `SHA256SUMS.txt` | Cryptographic Checksum File | ~1 KB | Verified across all assets |

---

## System Requirements

- **Operating System:** Windows 10 (Build 19041+) or Windows 11 (x64)
- **Runtime:** Included (Self-Contained .NET 8 + WinUI 3 Windows App SDK 1.5)
- **WebView2:** Microsoft Edge WebView2 Runtime (Pre-installed on Windows 10/11)
- **Development Toolchain (Optional for app running):**
  - Flutter SDK 3.19+
  - Android Studio / Android SDK (for mobile emulation)

---

## Installation Instructions

### Option 1: Standard Setup Wizard (Recommended)
1. Download **`Snapcode-Setup-v0.1.0.exe`**.
2. Run the executable and follow the setup wizard.
3. Launch **Snapcode** from your Desktop or Start Menu.

### Option 2: Portable Zip
1. Download **`Snapcode-v0.1.0-win-x64.zip`**.
2. Extract the archive to any folder (e.g. `C:\Tools\Snapcode`).
3. Run `Snapcode.exe`.

### Option 3: Thin Installer
1. Download **`Snapcode-ThinInstaller.exe`**.
2. Run the lightweight installer to install Snapcode into your `%LOCALAPPDATA%\Programs\Snapcode` folder with zero admin prompts.

---

## Roadmap (v0.2.0)
- Visual asset management dialog with image bundling into `pubspec.yaml`
- REST API / HTTP client visual block categories
- Multi-page navigation router blocks (`Navigator.push`, `Navigator.pop`)
- Web and macOS target export pipelines

---

*Snapcode is published and maintained by **hkpc labs**.*
