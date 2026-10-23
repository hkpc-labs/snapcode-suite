# SnapCode — Project Snapshot v0.1

**Snapshot Tag**: `v0.1`  
**Date**: October 11, 2026  
**State**: Production-ready initial version of the SnapCode IDE, View Designer, and Compiler  

---

## 1. Milestone Overview
Version 0.1 establishes the unified visual IDE bridging Flutter application development with a native Windows WinUI 3 interface and Blockly block-based logic programming.

### Key Achievements:
- **Flutter Material 3 View Designer**:
  - Full support for all 14 core Flutter widgets: `Row`, `Column`, `Container`, `Text`, `Button`, `TextField`, `Image`, `Checkbox`, `Switch`, `Slider`, `ListView`, `ScrollView`, `HorizontalScrollView`, and `Expanded`.
  - Custom `FlutterFlexPanel` delivering sub-pixel parity with Flutter's Flex layout algorithm:
    - `MainAxisAlignment` (Start, Center, End, SpaceBetween, SpaceAround, SpaceEvenly).
    - `CrossAxisAlignment` (Center by default, Start, End, Stretch).
    - Proportional flex allocation for `Expanded(flex: n)` children.
    - Deep nested hierarchy support (Row in Column, Column in Row, nested Expanded widgets).
- **Zero-Flicker Live Reactivity**:
  - In-place mock view re-rendering on property edits without wiping the canvas or destroying node controls.
  - Complete selection retention and active focus stability.
- **Bi-Directional Compiler & Workspace**:
  - Real-time Dart code generator (`FlutterCompilerService`) outputting idiomatic Flutter widgets and asynchronous event handlers.
  - Blockly WebView2 integration with custom blocks mapping.
  - Project lifecycle and asset management (`SnapcodeAssets/`).
- **Automated Verification**:
  - Built-in test suite (`ViewDesignerTestService`) passing **87/87 tests (100%)**.

---

## 2. Included Directories & Assets
- `src/`: Complete C# / WinUI 3 desktop client application, view designer controls, test services, dialogs, and project configurations.
- `helpers/`: Custom blocks database, conversion scripts, and palette schemas.
- `Docs/`: Project architecture, block specification guides, and engineering documentation.
- `snapcode-compiler/`: Standalone compilation tools and Flutter AST processors.
- `snapcode-frontend/`: Web-based Blockly visual programming workspace assets and scripts.
- `snapcode-workers/`: Background task processors and execution services.
- `Docs/reports/view_designer_test_report.md`: Automated test execution report validating all widgets, properties, and edge cases.
