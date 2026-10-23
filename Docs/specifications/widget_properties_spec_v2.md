# Comprehensive Widget Properties Specification

This document defines the bulletproof property mapping and visual behavior for every widget supported in the Native UI Designer. It acts as the absolute source of truth for both the UI Designer implementation and the eventual Flutter Compiler Engine.

## 1. General Properties (Base Properties)
These apply to almost all widgets (with minor exceptions like `Expanded` which ignores explicit Width/Height).

| Property | Type | Default | Description |
|---|---|---|---|
| **ID** | String | Auto-generated (e.g., `button_1`) | Unique identifier for code generation. |
| **Name/Text** | String | Widget Type Name | The textual content (for Text, Button, Checkbox) or descriptive name. |
| **Width** | String | Auto | Numeric pixel value, `Auto`, or `Match Parent`. |
| **Height** | String | Auto | Numeric pixel value, `Auto`, or `Match Parent`. |
| **Padding** | String (L,T,R,B) | 8 | Inner spacing. Default 8px. |
| **Margin** | String (L,T,R,B) | 0 | Outer spacing. |
| **BackgroundColor** | Color (Hex/Name) | Transparent | Fills the bounding box of the widget. |
| **BorderColor** | Color (Hex/Name) | Transparent | Color of the widget's border. |
| **BorderWidth** | Numeric | 0 | Thickness of the widget's border. |
| **CornerRadius** | Numeric | 0 | Rounds the corners of the widget/border. |
| **DropShadow** | Boolean | False | Applies a Z-axis elevation shadow. |
| **Alignment** | Enum | TopLeft | Aligns the widget within its parent. |
| **Gravity** | Enum | TopLeft | Aligns the widget's children within itself. |

---

## 2. Widget-Specific Properties

### A. Layout Widgets (Row, Column)
*Relies on General Properties for styling.*
| Property | Type | Options | Description |
|---|---|---|---|
| **MainAxisAlignment** | Enum | Start, Center, End, SpaceBetween | Distributes children along the primary axis. |
| **CrossAxisAlignment** | Enum | Start, Center, End, Stretch | Aligns children along the secondary axis. |

### B. Typography Widgets (Text, Button, TextField)
| Property | Type | Options | Description |
|---|---|---|---|
| **TextColor** | Color | #000000 | Color of the text. |
| **TextSize** | Numeric | 14 | Font size in logical pixels. |
| **FontWeight** | Enum | Normal, Bold | Thickness of the font. |
| **TextAlign** | Enum | Left, Center, Right | Justification of the text. |
| **FontFamily** | Asset Picker | *File Path* | Opens the Asset Manager to select a custom `.ttf` or `.otf` font file. |

*Additional for TextField:*
| Property | Type | Options | Description |
|---|---|---|---|
| **ObscureText** | Boolean | True, False | Hides input (for passwords). |
| **HintText** | String | "" | Placeholder text when empty. |
| **InputType** | Enum | Text, Number, Email, Phone | Keyboard type for the input field. |

### C. Media Widgets (Image)
| Property | Type | Options | Description |
|---|---|---|---|
| **AssetPath** | Asset Picker | *File Path* | Opens Asset Manager to choose an image file. |
| **AssetUsage** | Enum | Image, Background | Tells the compiler how to layer this asset. |
| **BoxFit** | Enum | Contain, Cover, Fill | How the image scales to fit its bounds. |

### D. Boolean Input Widgets (Checkbox, Switch)
| Property | Type | Options | Description |
|---|---|---|---|
| **Checked** | Boolean | False | Default checked state. |
| **ActiveColor** | Color | #2196F3 (Material Blue) | The color of the widget when it is turned "On". |

### E. Value Input Widgets (Slider)
| Property | Type | Options | Description |
|---|---|---|---|
| **MinValue** | Numeric | 0 | The minimum bound. |
| **MaxValue** | Numeric | 100 | The maximum bound. |
| **CurrentValue**| Numeric | 50 | The default starting value. |
| **ActiveColor** | Color | #2196F3 | The color of the filled track. |

### F. Scroll & List Widgets (ListView, ScrollView, H-ScrollView)
| Property | Type | Options | Description |
|---|---|---|---|
| **ScrollDirection**| Enum | Vertical, Horizontal | Determines the axis of scrolling (specifically for ListView, as the ScrollViews are hardcoded). |

### G. Flexibility Widgets (Expanded)
| Property | Type | Options | Description |
|---|---|---|---|
| **Flex** | Numeric | 1 | The ratio of remaining space the widget should consume. |

---

## 3. Action Plan for Implementation
1. **Model Updates (`WidgetItem`):** Add `Checked`, `ActiveColor`, `MinValue`, `MaxValue`, `CurrentValue`, `HintText`, `InputType`, `FontFamily`, `Flex`.
2. **UI Designer XAML Updates:** Build corresponding `StackPanels` for Checkbox/Switch (Toggle panel), Slider (Value panel), TextField (Input panel expanded), Typography (FontFamily Asset picker), and Expanded (Flex panel).
3. **Mock Renderer Updates:** Update `DesignerNodeControl.xaml.cs` to visually reflect these properties in the WinUI 3 mock canvas (e.g. coloring the Slider track, applying custom fonts, setting CheckBox states).
4. **State Persistence:** Ensure all these new properties are serialized and deserialized to JSON.
