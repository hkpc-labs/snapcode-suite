import os
import sys
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patches as patches

# Output directory for diagrams
scratch_dir = r"C:\Users\hardi\.gemini\antigravity\brain\6b093260-b6f3-43b6-a1c5-46235362356f\scratch"
os.makedirs(scratch_dir, exist_ok=True)

# -----------------------------------------------------------------------------
# Figure 1: End-to-End System Architecture Diagram
# -----------------------------------------------------------------------------
def create_figure1():
    fig, ax = plt.subplots(figsize=(12, 7), dpi=300)
    ax.set_xlim(0, 12)
    ax.set_ylim(0, 7)
    ax.axis('off')

    # Background canvas
    fig.patch.set_facecolor('#F8FAFC')
    ax.set_facecolor('#F8FAFC')

    # Title
    ax.text(6, 6.6, "Snapcode: Hybrid Code Generation & Event Orchestration Architecture",
            ha='center', va='center', fontsize=14, fontweight='bold', color='#0F172A', fontname='Segoe UI')
    ax.text(6, 6.25, "Coexistence of Visual Canvas, Block-Based Logic, and Pro-Code Manual Dart Editing",
            ha='center', va='center', fontsize=9.5, color='#475569', fontname='Segoe UI')

    def draw_box(x, y, w, h, bg, border, title, desc, tag=""):
        p = patches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.08,rounding_size=0.15",
                                   ec=border, fc=bg, lw=1.8)
        ax.add_patch(p)
        ax.text(x + w/2, y + h - 0.28, title, ha='center', va='center', fontsize=10.5, fontweight='bold', color='#0F172A', fontname='Segoe UI')
        if tag:
            tbox = patches.FancyBboxPatch((x + w - 1.25, y + h - 0.38), 1.15, 0.26, boxstyle="round,pad=0.03,rounding_size=0.06",
                                          ec=border, fc=border, lw=0.5)
            ax.add_patch(tbox)
            ax.text(x + w - 0.675, y + h - 0.25, tag, ha='center', va='center', fontsize=7, fontweight='bold', color='#FFFFFF', fontname='Segoe UI')
        ax.text(x + w/2, y + (h - 0.35)/2, desc, ha='center', va='center', fontsize=8, color='#334155', fontname='Segoe UI', wrap=True)

    # 1. UI Designer Block
    draw_box(0.5, 3.8, 3.2, 2.0, '#EFF6FF', '#2563EB', "1. UI View Designer", 
             "• WinUI 3 Drag & Drop Canvas\n• Hierarchical Widget Tree\n• Live Property Inspector\n• Exports: UI JSON State Tree", "FRONTEND")

    # 2. Event Manager (Blockly)
    draw_box(0.5, 1.2, 3.2, 2.0, '#F5F3FF', '#7C3AED', "2. Event Manager (Blockly)", 
             "• Google Blockly WebView2 Host\n• Widget-Bound Event Workspaces\n• Lifecycle Triggers (init/dispose)\n• dartGenerator.workspaceToCode()", "LOGIC")

    # 3. Middle Orchestrator: EditorPage & Compiler
    draw_box(4.4, 2.2, 3.4, 3.6, '#ECFDF5', '#059669', "3. C# Orchestration Core", 
             "EditorPage.xaml.cs\n\n[FlutterCompilerService]\n• Compiles UI JSON -> Widget build()\n• Sizing, margins, layout containers\n\n[WebView2 IPC Bridge]\n• Receives block-generated Dart code\n• Synchronizes widget IDs\n\n[Save Coordinator]\n• Dual-state (.snapcode JSONs)\n• Triggers safe Dart generation", "CORE")

    # 4. Preservation Engine
    draw_box(8.5, 3.8, 3.0, 2.0, '#FEF3C7', '#D97706', "4. Preservation Service", 
             "UserCodePreservationService\n\n• Scans existing main.dart\n• Extracts 6 Marker Blocks\n• Untagged import discovery\n• Orphaned code failsafe", "SECURITY")

    # 5. Output / Code Editor
    draw_box(8.5, 1.2, 3.0, 2.0, '#F1F5F9', '#475569', "5. Pro-Code Editor & Disk", 
             "Monaco Editor & Project Explorer\n\n• flutter/lib/main.dart\n• Non-destructive round-trip\n• Custom files (.dart models/apis)\n• Zero crash on visual reload", "PERSIST")

    def draw_arrow(x1, y1, x2, y2, color='#64748B', label=""):
        ax.annotate('', xy=(x2, y2), xytext=(x1, y1),
                    arrowprops=dict(arrowstyle="-|>", color=color, lw=2, mutation_scale=15))
        if label:
            mx, my = (x1 + x2)/2, (y1 + y2)/2
            ax.text(mx, my + 0.15, label, ha='center', va='center', fontsize=7.5, fontweight='bold', color=color, fontname='Segoe UI',
                    bbox=dict(boxstyle='square,pad=0.15', fc='#FFFFFF', ec='none', alpha=0.85))

    # Connectors
    draw_arrow(3.7, 4.8, 4.4, 4.8, '#2563EB', "UI JSON")
    draw_arrow(3.7, 2.2, 4.4, 2.6, '#7C3AED', "IPC Code")
    draw_arrow(7.8, 4.8, 8.5, 4.8, '#059669', "Merge Req")
    draw_arrow(9.0, 3.8, 9.0, 3.2, '#D97706', "Preserved")
    draw_arrow(7.8, 2.5, 8.5, 2.2, '#059669', "Write Disk")

    plt.tight_layout()
    p1 = os.path.join(scratch_dir, "fig1_system_architecture.png")
    plt.savefig(p1, bbox_inches='tight')
    plt.close()
    print("Created Figure 1:", p1)

# -----------------------------------------------------------------------------
# Figure 2: Code Generation Pipeline Flowchart
# -----------------------------------------------------------------------------
def create_figure2():
    fig, ax = plt.subplots(figsize=(12, 7.5), dpi=300)
    ax.set_xlim(0, 12)
    ax.set_ylim(0, 7.5)
    ax.axis('off')

    fig.patch.set_facecolor('#F8FAFC')
    ax.set_facecolor('#F8FAFC')

    ax.text(6, 7.1, "Two-Tier Code Generation & Compilation Pipeline",
            ha='center', va='center', fontsize=14, fontweight='bold', color='#0F172A', fontname='Segoe UI')
    ax.text(6, 6.75, "Parallel compilation of visual structure and declarative logic into a cohesive Flutter application",
            ha='center', va='center', fontsize=9.5, color='#475569', fontname='Segoe UI')

    def draw_step(x, y, w, h, bg, border, title, lines):
        p = patches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.06,rounding_size=0.12",
                                   ec=border, fc=bg, lw=1.5)
        ax.add_patch(p)
        ax.text(x + w/2, y + h - 0.22, title, ha='center', va='center', fontsize=9.5, fontweight='bold', color='#0F172A', fontname='Segoe UI')
        ax.text(x + w/2, y + (h - 0.28)/2, lines, ha='center', va='center', fontsize=7.8, color='#334155', fontname='Segoe UI')

    # Stream A: UI
    ax.text(2.8, 6.2, "STREAM A: Visual UI Generation", fontsize=11, fontweight='bold', color='#1D4ED8', fontname='Segoe UI', ha='center')
    draw_step(0.8, 4.8, 4.0, 1.1, '#DBEAFE', '#2563EB', "1. DesignerNode Tree (XAML)", "Visual properties: width, height, colors,\nborders, padding, gravity, child arrays")
    draw_step(0.8, 3.4, 4.0, 1.1, '#EFF6FF', '#3B82F6', "2. UI JSON Serialization", "NativeDesigner.GetUIJson() produces\nhierarchical JSON AST of full layout")
    draw_step(0.8, 2.0, 4.0, 1.1, '#E0F2FE', '#0284C7', "3. FlutterCompilerService", "GenerateBuildMethod() maps nodes to:\nRow, Column, Container, Text, Button...")

    # Stream B: Events
    ax.text(9.2, 6.2, "STREAM B: Event Blocks Generation", fontsize=11, fontweight='bold', color='#6D28D9', fontname='Segoe UI', ha='center')
    draw_step(7.2, 4.8, 4.0, 1.1, '#EDE9FE', '#7C3AED', "1. Blockly Event Workspace", "Isolated workspaces for widget triggers\n(button_1_onClick, page_initState)")
    draw_step(7.2, 3.4, 4.0, 1.1, '#F5F3FF', '#8B5CF6', "2. dartGenerator Execution", "dartGenerator.workspaceToCode()\ngenerates stateful Dart logic")
    draw_step(7.2, 2.0, 4.0, 1.1, '#FAF5FF', '#A855F7', "3. WebView2 IPC Bridge", "window.chrome.webview.postMessage()\nTransfers code to EditorPage._lastGeneratedCode")

    # Merger Box
    draw_step(3.5, 0.4, 5.0, 1.2, '#ECFDF5', '#059669', "4. Assembly & User Code Preservation", 
              "UserCodePreservationService merges build() + event handlers\nPreserves custom imports, class variables & orphan code\nWrites final code to flutter/lib/main.dart")

    def draw_arr(x1, y1, x2, y2, color='#64748B'):
        ax.annotate('', xy=(x2, y2), xytext=(x1, y1),
                    arrowprops=dict(arrowstyle="-|>", color=color, lw=1.8, mutation_scale=12))

    draw_arr(2.8, 4.8, 2.8, 4.5, '#2563EB')
    draw_arr(2.8, 3.4, 2.8, 3.1, '#3B82F6')
    draw_arr(2.8, 2.0, 4.8, 1.6, '#0284C7')

    draw_arr(9.2, 4.8, 9.2, 4.5, '#7C3AED')
    draw_arr(9.2, 3.4, 9.2, 3.1, '#8B5CF6')
    draw_arr(9.2, 2.0, 7.2, 1.6, '#A855F7')

    plt.tight_layout()
    p2 = os.path.join(scratch_dir, "fig2_code_generation_pipeline.png")
    plt.savefig(p2, bbox_inches='tight')
    plt.close()
    print("Created Figure 2:", p2)

# -----------------------------------------------------------------------------
# Figure 3: Marker-Based Preservation & Merge Sequence
# -----------------------------------------------------------------------------
def create_figure3():
    fig, ax = plt.subplots(figsize=(12, 6.5), dpi=300)
    ax.set_xlim(0, 12)
    ax.set_ylim(0, 6.5)
    ax.axis('off')

    fig.patch.set_facecolor('#F8FAFC')
    ax.set_facecolor('#F8FAFC')

    ax.text(6, 6.1, "Marker-Based Non-Destructive Code Merge Engine",
            ha='center', va='center', fontsize=14, fontweight='bold', color='#0F172A', fontname='Segoe UI')
    ax.text(6, 5.75, "How UserCodePreservationService protects user modifications across re-saves",
            ha='center', va='center', fontsize=9.5, color='#475569', fontname='Segoe UI')

    def draw_marker_node(x, y, w, h, bg, border, title, desc):
        p = patches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.06,rounding_size=0.12",
                                   ec=border, fc=bg, lw=1.5)
        ax.add_patch(p)
        ax.text(x + w/2, y + h - 0.22, title, ha='center', va='center', fontsize=9, fontweight='bold', color='#0F172A', fontname='Segoe UI')
        ax.text(x + w/2, y + (h - 0.26)/2, desc, ha='center', va='center', fontsize=7.5, color='#334155', fontname='Segoe UI')

    # Step 1: Read Disk
    draw_marker_node(0.5, 2.4, 2.4, 2.8, '#FFFBEB', '#D97706', "Step 1: Read File",
                     "Check if main.dart exists\non disk.\n\nExtract content\nvia File.ReadAllText\nor ReadAllTextAsync.\n\nHandles uncreated files\ngracefully.")

    # Step 2: Extract Markers
    draw_marker_node(3.3, 2.4, 2.6, 2.8, '#FEF3C7', '#B45309', "Step 2: Parse Markers",
                     "Regex scans for blocks:\n• IMPORTS\n• CLASS_MEMBERS\n• INIT_STATE\n• DISPOSE\n• CUSTOM_METHODS\n\nHeuristic scan detects\nuntagged third-party imports.")

    # Step 3: Template Injection
    draw_marker_node(6.3, 2.4, 2.6, 2.8, '#ECFDF5', '#059669', "Step 3: Merge Content",
                     "Ensures preservation\nmarkers on new template.\n\nInjects user code into\nmatching tags.\n\nPreserves user edits in\nmethods & state variables.")

    # Step 4: Orphan Sweep & Write
    draw_marker_node(9.3, 2.4, 2.2, 2.8, '#EFF6FF', '#2563EB', "Step 4: Orphan Failsafe",
                     "If widget was renamed or\ndeleted in UI designer:\n\nIts code is moved to\nORPHANED_CODE marker\nrather than erased!\n\nAtomic write to disk.")

    def draw_narr(x1, y1, x2, y2):
        ax.annotate('', xy=(x2, y2), xytext=(x1, y1),
                    arrowprops=dict(arrowstyle="-|>", color='#059669', lw=2, mutation_scale=14))

    draw_narr(2.9, 3.8, 3.3, 3.8)
    draw_narr(5.9, 3.8, 6.3, 3.8)
    draw_narr(8.9, 3.8, 9.3, 3.8)

    # Bottom summary badge
    sum_box = patches.FancyBboxPatch((1.5, 0.5), 9.0, 1.3, boxstyle="round,pad=0.08,rounding_size=0.15",
                                     ec='#64748B', fc='#FFFFFF', lw=1.2)
    ax.add_patch(sum_box)
    ax.text(6.0, 1.4, "Guaranteed Preservation Invariant", fontsize=10, fontweight='bold', color='#0F172A', fontname='Segoe UI', ha='center')
    ax.text(6.0, 0.9, "Visual UI edits update only widget tree declarations; user logic, variables, and imported packages remain 100% intact.",
            fontsize=8.5, color='#475569', fontname='Segoe UI', ha='center')

    plt.tight_layout()
    p3 = os.path.join(scratch_dir, "fig3_marker_merge_sequence.png")
    plt.savefig(p3, bbox_inches='tight')
    plt.close()
    print("Created Figure 3:", p3)

# -----------------------------------------------------------------------------
# Figure 4: Storage & Project Hierarchy Layout
# -----------------------------------------------------------------------------
def create_figure4():
    fig, ax = plt.subplots(figsize=(12, 6.5), dpi=300)
    ax.set_xlim(0, 12)
    ax.set_ylim(0, 6.5)
    ax.axis('off')

    fig.patch.set_facecolor('#F8FAFC')
    ax.set_facecolor('#F8FAFC')

    ax.text(6, 6.1, "Snapcode Project Storage & File Isolation Architecture",
            ha='center', va='center', fontsize=14, fontweight='bold', color='#0F172A', fontname='Segoe UI')
    ax.text(6, 5.75, "Physical disk layout separating visual project metadata from pure Flutter Dart code",
            ha='center', va='center', fontsize=9.5, color='#475569', fontname='Segoe UI')

    def draw_dir_box(x, y, w, h, bg, border, title, items):
        p = patches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.06,rounding_size=0.12",
                                   ec=border, fc=bg, lw=1.5)
        ax.add_patch(p)
        ax.text(x + w/2, y + h - 0.25, title, ha='center', va='center', fontsize=9.5, fontweight='bold', color='#0F172A', fontname='Segoe UI')
        ax.text(x + 0.2, y + (h - 0.35)/2, items, ha='left', va='center', fontsize=7.8, color='#334155', fontname='Segoe UI')

    # Box 1: .snapcode internal folder
    draw_dir_box(0.6, 1.2, 3.4, 4.0, '#FEF2F2', '#DC2626', ".snapcode/ (Visual Metadata)",
                 "• project.json (Project settings)\n• pages/\n   |-- main_ui.json (Visual tree)\n   |-- main_meta.json (Page config)\n   |-- main_button_1_onClick.json\n   +-- page2_ui.json\n\n* Isolated from Flutter build tools\n* Powers WinUI 3 re-hydration")

    # Box 2: flutter/lib/ Flutter Project
    draw_dir_box(4.3, 1.2, 3.4, 4.0, '#EFF6FF', '#2563EB', "flutter/lib/ (Managed Pages)",
                 "• main.dart (Preserved page)\n• page2.dart (Generated page)\n\n* Contains preservation markers\n* Visual designer re-generates build()\n* User custom code preserved in tags\n* Monitored by Flutter toolchain")

    # Box 3: Custom Developer Files
    draw_dir_box(8.0, 1.2, 3.4, 4.0, '#F0FDF4', '#16A34A', "flutter/lib/... (Custom Code)",
                 "• services/api_client.dart\n• models/user_profile.dart\n• widgets/custom_card.dart\n\n* Classified: IsVisualPage == false\n* NEVER shown in UI Designer\n* Edited via Monaco Code Editor\n* 100% immune to UI generator")

    plt.tight_layout()
    p4 = os.path.join(scratch_dir, "fig4_project_storage_hierarchy.png")
    plt.savefig(p4, bbox_inches='tight')
    plt.close()
    print("Created Figure 4:", p4)

if __name__ == "__main__":
    create_figure1()
    create_figure2()
    create_figure3()
    create_figure4()
    print("All diagrams generated successfully.")
