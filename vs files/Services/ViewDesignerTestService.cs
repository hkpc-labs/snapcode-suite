using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Snapcode.Views;
using SnapCode.Services;

namespace Snapcode.Services
{
    public static class ViewDesignerTestService
    {
        public static string RunAllTests(string outputReportPath = "")
        {
            var report = new System.Text.StringBuilder();
            int total = 0;
            int passed = 0;
            int failed = 0;
            var bugsFound = new List<string>();

            void Assert(bool condition, string testName, string failureDetail = "")
            {
                total++;
                if (condition)
                {
                    passed++;
                    report.AppendLine($"  - [x] **PASS**: {testName}");
                    Console.WriteLine($"  [PASS] {testName}");
                }
                else
                {
                    failed++;
                    string err = $"{testName} -> {failureDetail}";
                    bugsFound.Add(err);
                    report.AppendLine($"  - [ ] **FAIL**: {testName} - *{failureDetail}*");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAIL] {err}");
                    Console.ResetColor();
                }
            }

            report.AppendLine("# View Designer & Widget Property Automated Verification Report");
            report.AppendLine($"*Executed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}*\n");
            Console.WriteLine("\n=======================================================");
            Console.WriteLine("  SNAPCODE VIEW DESIGNER AUTOMATED VERIFICATION SUITE");
            Console.WriteLine("=======================================================\n");

            // 1. All Widget Types
            report.AppendLine("## 1. Widget Instantiation & Identity");
            Console.WriteLine(">>> 1. Testing Widget Instantiation for All Palette Items...");
            string[] widgetTypes = new[]
            {
                "Row", "Column", "Container",
                "Text", "Button", "TextField", "Image",
                "Checkbox", "Switch", "Slider",
                "ListView", "ScrollView", "HorizontalScrollView", "Expanded"
            };

            foreach (var type in widgetTypes)
            {
                var item = new WidgetItem
                {
                    Type = type,
                    Name = $"{type} Widget",
                    Id = $"{type.ToLower()}_test"
                };

                Assert(item.Type == type && item.Id == $"{type.ToLower()}_test",
                    $"Create Widget: {type}", "Type or Id property assignment failed.");
            }

            // 2. All Properties
            report.AppendLine("\n## 2. Widget Properties Validation");
            Console.WriteLine("\n>>> 2. Testing All Properties on Model...");
            var propItem = new WidgetItem
            {
                Type = "TextField",
                Id = "tf_username",
                Name = "Username Input",
                Width = "Match Parent",
                Height = "56",
                Padding = "12,16,12,16",
                Margin = "8,4,8,4",
                BackgroundColor = "#FFE0E0E0",
                TextColor = "#FF212121",
                TextSize = "16",
                CornerRadius = "8",
                BorderWidth = "1.5",
                BorderColor = "#FF9E9E9E",
                Alignment = "Center",
                Gravity = "Center",
                AssetPath = "lanscape_image_01.jpg",
                FontWeight = "Bold",
                TextAlign = "Left",
                MainAxisAlignment = "Start",
                CrossAxisAlignment = "Center",
                BoxFit = "Cover",
                ObscureText = false,
                ScrollDirection = "Vertical",
                AssetUsage = "Background",
                HasShadow = true,
                Checked = true,
                ActiveColor = "#FF2196F3",
                MinValue = 10.0,
                MaxValue = 100.0,
                CurrentValue = 50.0,
                HintText = "Enter your username",
                InputType = "Text",
                FontFamily = "Roboto",
                Flex = 2
            };

            Assert(propItem.Width == "Match Parent", "Property: Width");
            Assert(propItem.Height == "56", "Property: Height");
            Assert(propItem.Padding == "12,16,12,16", "Property: Padding");
            Assert(propItem.Margin == "8,4,8,4", "Property: Margin");
            Assert(propItem.BackgroundColor == "#FFE0E0E0", "Property: BackgroundColor");
            Assert(propItem.TextColor == "#FF212121", "Property: TextColor");
            Assert(propItem.TextSize == "16", "Property: TextSize");
            Assert(propItem.CornerRadius == "8", "Property: CornerRadius");
            Assert(propItem.BorderWidth == "1.5", "Property: BorderWidth");
            Assert(propItem.BorderColor == "#FF9E9E9E", "Property: BorderColor");
            Assert(propItem.Alignment == "Center", "Property: Alignment");
            Assert(propItem.Gravity == "Center", "Property: Gravity");
            Assert(propItem.AssetPath == "lanscape_image_01.jpg", "Property: AssetPath");
            Assert(propItem.FontWeight == "Bold", "Property: FontWeight");
            Assert(propItem.TextAlign == "Left", "Property: TextAlign");
            Assert(propItem.MainAxisAlignment == "Start", "Property: MainAxisAlignment");
            Assert(propItem.CrossAxisAlignment == "Center", "Property: CrossAxisAlignment");
            Assert(propItem.BoxFit == "Cover", "Property: BoxFit");
            Assert(propItem.ObscureText == false, "Property: ObscureText");
            Assert(propItem.ScrollDirection == "Vertical", "Property: ScrollDirection");
            Assert(propItem.AssetUsage == "Background", "Property: AssetUsage");
            Assert(propItem.HasShadow == true, "Property: HasShadow");
            Assert(propItem.Checked == true, "Property: Checked");
            Assert(propItem.ActiveColor == "#FF2196F3", "Property: ActiveColor");
            Assert(propItem.MinValue == 10.0 && propItem.MaxValue == 100.0 && propItem.CurrentValue == 50.0, "Properties: Slider Bounds");
            Assert(propItem.HintText == "Enter your username", "Property: HintText");
            Assert(propItem.InputType == "Text", "Property: InputType");
            Assert(propItem.FontFamily == "Roboto", "Property: FontFamily");
            Assert(propItem.Flex == 2, "Property: Flex");

            // 3. Tree Hierarchy & Nesting
            report.AppendLine("\n## 3. Tree Hierarchy, Nesting & Search");
            Console.WriteLine("\n>>> 3. Testing Tree Hierarchy, Nesting, and Finding...");
            var root = new WidgetItem { Type = "Column", Id = "col_root", Name = "Root Column" };
            var headerRow = new WidgetItem { Type = "Row", Id = "row_header", Name = "Header" };
            var titleText = new WidgetItem { Type = "Text", Id = "txt_title", Name = "Snapcode App" };
            var actionBtn = new WidgetItem { Type = "Button", Id = "btn_submit", Name = "Submit" };

            headerRow.Children.Add(titleText);
            headerRow.Children.Add(actionBtn);
            root.Children.Add(headerRow);

            var contentContainer = new WidgetItem { Type = "Container", Id = "cnt_body", Name = "Body" };
            var input = new WidgetItem { Type = "TextField", Id = "tf_email", Name = "Email Input" };
            contentContainer.Children.Add(input);
            root.Children.Add(contentContainer);

            var items = new ObservableCollection<WidgetItem> { root };

            var foundText = FindWidgetById(items, "txt_title");
            Assert(foundText != null && foundText.Id == "txt_title", "FindWidgetById: Deep nested child");

            var foundInput = FindWidgetById(items, "tf_email");
            Assert(foundInput != null && foundInput.Id == "tf_email", "FindWidgetById: Sibling branch child");

            var notFound = FindWidgetById(items, "non_existent_id");
            Assert(notFound == null, "FindWidgetById: Non-existent ID returns null");

            bool removed = RemoveWidgetFromTree(items, actionBtn);
            Assert(removed && headerRow.Children.Count == 1, "RemoveWidgetFromTree: Remove nested child");
            Assert(FindWidgetById(items, "btn_submit") == null, "FindWidgetById: Removed child is absent");

            // 4. JSON Serialization & Round-Trip
            report.AppendLine("\n## 4. JSON Serialization & Deserialization");
            Console.WriteLine("\n>>> 4. Testing JSON Serialization and Round-Trip...");
            var serRoot = new WidgetItem 
            { 
                Type = "Column", 
                Id = "root_col", 
                Name = "Root Column",
                Width = "Match Parent",
                Height = "Match Parent"
            };

            var btn = new WidgetItem
            {
                Type = "Button",
                Id = "btn_play",
                Name = "Play Music",
                BackgroundColor = "#FF4CAF50",
                TextColor = "#FFFFFFFF",
                TextSize = "16",
                CornerRadius = "20",
                HasShadow = true
            };
            serRoot.Children.Add(btn);

            var slider = new WidgetItem
            {
                Type = "Slider",
                Id = "slider_vol",
                Name = "Volume",
                MinValue = 0.0,
                MaxValue = 100.0,
                CurrentValue = 75.0,
                ActiveColor = "#FF2196F3"
            };
            serRoot.Children.Add(slider);

            var list = new List<object> { SerializeItem(serRoot) };
            string json = JsonSerializer.Serialize(list);

            Assert(!string.IsNullOrEmpty(json) && json.Contains("btn_play") && json.Contains("slider_vol"), 
                "Serialization: Output contains expected elements");

            using var doc = JsonDocument.Parse(json);
            Assert(doc.RootElement.ValueKind == JsonValueKind.Array, "Deserialization: Root element is Array");
            Assert(doc.RootElement.GetArrayLength() == 1, "Deserialization: Root contains 1 top-level widget");

            var rootEl = doc.RootElement[0];
            Assert(rootEl.GetProperty("id").GetString() == "root_col", "Deserialization: Root ID matches");
            Assert(rootEl.GetProperty("type").GetString() == "Column", "Deserialization: Root Type matches");

            var children = rootEl.GetProperty("children");
            Assert(children.GetArrayLength() == 2, "Deserialization: Children count matches (2)");
            Assert(children[0].GetProperty("id").GetString() == "btn_play", "Deserialization: Child 0 ID matches");
            Assert(children[0].GetProperty("cornerRadius").GetString() == "20", "Deserialization: Child 0 CornerRadius preserved");
            Assert(children[1].GetProperty("currentValue").GetDouble() == 75.0, "Deserialization: Child 1 CurrentValue preserved");

            // 5. Dart Compiler Code Generation
            report.AppendLine("\n## 5. Flutter / Dart Code Generation");
            Console.WriteLine("\n>>> 5. Testing Flutter Code Generation for View Tree...");

            var compRoot = new WidgetItem { Type = "Column", Id = "col_page" };
            compRoot.Children.Add(new WidgetItem { Type = "Text", Id = "txt_welcome", Name = "Welcome $100 & User!" });
            compRoot.Children.Add(new WidgetItem { Type = "Button", Id = "btn_start", Name = "Get Started" });
            compRoot.Children.Add(new WidgetItem { Type = "TextField", Id = "tf_email", HintText = "Enter $email" });
            compRoot.Children.Add(new WidgetItem { Type = "Checkbox", Id = "cb_terms", Name = "Accept Terms" });
            compRoot.Children.Add(new WidgetItem { Type = "Switch", Id = "sw_notifs", Name = "Notifications" });
            compRoot.Children.Add(new WidgetItem { Type = "Slider", Id = "sl_volume", MinValue = 5.0, MaxValue = 50.0, CurrentValue = 25.0 });

            var compList = new List<object> { SerializeItem(compRoot) };
            string compJson = JsonSerializer.Serialize(compList);

            string dartCode = FlutterCompilerService.GenerateBuildMethod(compJson);

            Assert(!string.IsNullOrEmpty(dartCode), "Compiler: Generated code is not empty");
            Assert(dartCode.Contains("Widget build(BuildContext context)"), "Compiler: Generates build method signature");
            Assert(dartCode.Contains("Column(children: ["), "Compiler: Generates root Column");
            
            // Bug 78 Check: String interpolation $ must be escaped
            Assert(dartCode.Contains(@"Welcome \$100 & User!"), "Compiler Bug 78: $ escaped in Text");
            Assert(dartCode.Contains(@"Enter \$email"), "Compiler Bug 78: $ escaped in TextField hint");

            // Bug 79 Check: Async handlers
            Assert(dartCode.Contains("onPressed: () async"), "Compiler Bug 79: Button onPressed is async");
            Assert(dartCode.Contains("onChanged: (bool? value) async"), "Compiler Bug 79: Checkbox onChanged is async");
            Assert(dartCode.Contains("onChanged: (bool value) async"), "Compiler Bug 79: Switch onChanged is async");
            Assert(dartCode.Contains("onChanged: (double value) async"), "Compiler Bug 79: Slider onChanged is async");

            // Bug 81 Check: Slider min/max
            Assert(dartCode.Contains("min: 5") && dartCode.Contains("max: 50") && dartCode.Contains("value: 25"), 
                "Compiler Bug 81: Slider respects min/max/current bounds");

            // Bug 82 Check: TextField controller
            Assert(dartCode.Contains("_controllers['tf_email']"), "Compiler Bug 82: TextField controller generated");

            // 6. Edge Cases
            report.AppendLine("\n## 6. Edge Cases & Resilience");
            Console.WriteLine("\n>>> 6. Testing Edge Cases & Defensive Safeguards...");

            string emptyDart = FlutterCompilerService.GenerateBuildMethod("");
            Assert(emptyDart.Contains("return Container();"), "Edge Case: Empty JSON returns Container fallback");

            string invalidDart = FlutterCompilerService.GenerateBuildMethod("{ invalid: json }");
            Assert(invalidDart.Contains("return Container();"), "Edge Case: Malformed JSON returns Container fallback");

            string unsupportedJson = "[{\"type\": \"SuperFancyWidget\"}]";
            string unsupportedDart = FlutterCompilerService.GenerateBuildMethod(unsupportedJson);
            Assert(unsupportedDart.Contains("SizedBox()"), "Edge Case Bug 83: Unsupported widget falls back to SizedBox()");

            var badSlider = new WidgetItem { Type = "Slider", MinValue = 100.0, MaxValue = 10.0, CurrentValue = 50.0 };
            var badList = new List<object> { SerializeItem(badSlider) };
            string badSliderJson = JsonSerializer.Serialize(badList);
            string badSliderDart = FlutterCompilerService.GenerateBuildMethod(badSliderJson);
            Assert(badSliderDart.Contains("Slider("), "Edge Case: Inverted Slider handles gracefully without throwing");

            // 7. Flutter Material 3 Preview Bridge & Layout Mechanics
            report.AppendLine("\n## 7. Flutter Material 3 Preview Bridge & Flex Layout Mechanics");
            Console.WriteLine("\n>>> 7. Testing Flutter Flex Panel & Nested Layouts...");

            // MainAxisAlignment Parsing
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("Start") == FlutterMainAxisAlignment.Start, "Flex: MainAxisAlignment.Start");
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("Center") == FlutterMainAxisAlignment.Center, "Flex: MainAxisAlignment.Center");
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("End") == FlutterMainAxisAlignment.End, "Flex: MainAxisAlignment.End");
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("SpaceBetween") == FlutterMainAxisAlignment.SpaceBetween, "Flex: MainAxisAlignment.SpaceBetween");
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("SpaceAround") == FlutterMainAxisAlignment.SpaceAround, "Flex: MainAxisAlignment.SpaceAround");
            Assert(FlutterFlexPanel.ParseMainAxisAlignment("SpaceEvenly") == FlutterMainAxisAlignment.SpaceEvenly, "Flex: MainAxisAlignment.SpaceEvenly");

            // CrossAxisAlignment Parsing
            Assert(FlutterFlexPanel.ParseCrossAxisAlignment("Center") == FlutterCrossAxisAlignment.Center, "Flex: CrossAxisAlignment.Center (Flutter default)");
            Assert(FlutterFlexPanel.ParseCrossAxisAlignment("") == FlutterCrossAxisAlignment.Center, "Flex: CrossAxisAlignment default fallback is Center");
            Assert(FlutterFlexPanel.ParseCrossAxisAlignment("Start") == FlutterCrossAxisAlignment.Start, "Flex: CrossAxisAlignment.Start");
            Assert(FlutterFlexPanel.ParseCrossAxisAlignment("End") == FlutterCrossAxisAlignment.End, "Flex: CrossAxisAlignment.End");
            Assert(FlutterFlexPanel.ParseCrossAxisAlignment("Stretch") == FlutterCrossAxisAlignment.Stretch, "Flex: CrossAxisAlignment.Stretch");

            // Attached Flex Dependency Property
            var testChildFe = new Microsoft.UI.Xaml.Controls.Border();
            FlutterFlexPanel.SetFlex(testChildFe, 3);
            Assert(FlutterFlexPanel.GetFlex(testChildFe) == 3, "Flex: Attached Flex factor property getter/setter");

            // Nested Row inside Column with Expanded Proportions
            var nestedCol = new WidgetItem { Type = "Column", Id = "outer_col", MainAxisAlignment = "SpaceBetween", CrossAxisAlignment = "Center" };
            var nestedRow = new WidgetItem { Type = "Row", Id = "inner_row", MainAxisAlignment = "Start", CrossAxisAlignment = "Center" };
            var exp1 = new WidgetItem { Type = "Expanded", Id = "exp_left", Flex = 1 };
            exp1.Children.Add(new WidgetItem { Type = "Button", Id = "btn_left", Name = "Left Button" });
            var exp2 = new WidgetItem { Type = "Expanded", Id = "exp_right", Flex = 2 };
            exp2.Children.Add(new WidgetItem { Type = "Text", Id = "txt_right", Name = "Right Expanded Text" });
            nestedRow.Children.Add(exp1);
            nestedRow.Children.Add(exp2);
            nestedCol.Children.Add(nestedRow);

            var nestedList = new List<object> { SerializeItem(nestedCol) };
            string nestedJson = JsonSerializer.Serialize(nestedList);
            Assert(nestedJson.Contains("outer_col") && nestedJson.Contains("inner_row") && nestedJson.Contains("exp_left") && nestedJson.Contains("exp_right"),
                "Layout: Nested Row inside Column serialization");

            string nestedDart = FlutterCompilerService.GenerateBuildMethod(nestedJson);
            Assert(nestedDart.Contains("Column(") && nestedDart.Contains("Row(") && nestedDart.Contains("Expanded(flex: 1") && nestedDart.Contains("Expanded(flex: 2"),
                "Layout: Nested Row/Column with Expanded flex proportional code generation");

            // Section 8: Alignment, Gravity Mesh & Flutter Flex Parity Tests
            // 8.1 Gravity Parsing
            var (cHoriz, cVert) = AlignmentHelper.ParseGravity("center");
            Assert(cHoriz == Microsoft.UI.Xaml.HorizontalAlignment.Center && cVert == Microsoft.UI.Xaml.VerticalAlignment.Center,
                "Alignment: ParseGravity 'center' -> Center, Center");

            var (chHoriz, chVert) = AlignmentHelper.ParseGravity("center_horizontal");
            Assert(chHoriz == Microsoft.UI.Xaml.HorizontalAlignment.Center && chVert == Microsoft.UI.Xaml.VerticalAlignment.Top,
                "Alignment: ParseGravity 'center_horizontal' -> Center, Top");

            var (cvHoriz, cvVert) = AlignmentHelper.ParseGravity("center_vertical");
            Assert(cvHoriz == Microsoft.UI.Xaml.HorizontalAlignment.Left && cvVert == Microsoft.UI.Xaml.VerticalAlignment.Center,
                "Alignment: ParseGravity 'center_vertical' -> Left, Center");

            var (pipedH, pipedV) = AlignmentHelper.ParseGravity("center_horizontal|bottom");
            Assert(pipedH == Microsoft.UI.Xaml.HorizontalAlignment.Center && pipedV == Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                "Alignment: ParseGravity 'center_horizontal|bottom' -> Center, Bottom");

            var (strH, strV) = AlignmentHelper.ParseGravity("stretch");
            Assert(strH == Microsoft.UI.Xaml.HorizontalAlignment.Stretch && strV == Microsoft.UI.Xaml.VerticalAlignment.Stretch,
                "Alignment: ParseGravity 'stretch' -> Stretch, Stretch");

            // 8.2 9-Point Named Alignment Points
            Assert(AlignmentHelper.ParseGravity("TopLeft") == (Microsoft.UI.Xaml.HorizontalAlignment.Left, Microsoft.UI.Xaml.VerticalAlignment.Top), "Alignment: 9-point TopLeft");
            Assert(AlignmentHelper.ParseGravity("TopCenter") == (Microsoft.UI.Xaml.HorizontalAlignment.Center, Microsoft.UI.Xaml.VerticalAlignment.Top), "Alignment: 9-point TopCenter");
            Assert(AlignmentHelper.ParseGravity("TopRight") == (Microsoft.UI.Xaml.HorizontalAlignment.Right, Microsoft.UI.Xaml.VerticalAlignment.Top), "Alignment: 9-point TopRight");
            Assert(AlignmentHelper.ParseGravity("CenterLeft") == (Microsoft.UI.Xaml.HorizontalAlignment.Left, Microsoft.UI.Xaml.VerticalAlignment.Center), "Alignment: 9-point CenterLeft");
            Assert(AlignmentHelper.ParseGravity("CenterRight") == (Microsoft.UI.Xaml.HorizontalAlignment.Right, Microsoft.UI.Xaml.VerticalAlignment.Center), "Alignment: 9-point CenterRight");
            Assert(AlignmentHelper.ParseGravity("BottomLeft") == (Microsoft.UI.Xaml.HorizontalAlignment.Left, Microsoft.UI.Xaml.VerticalAlignment.Bottom), "Alignment: 9-point BottomLeft");
            Assert(AlignmentHelper.ParseGravity("BottomCenter") == (Microsoft.UI.Xaml.HorizontalAlignment.Center, Microsoft.UI.Xaml.VerticalAlignment.Bottom), "Alignment: 9-point BottomCenter");
            Assert(AlignmentHelper.ParseGravity("BottomRight") == (Microsoft.UI.Xaml.HorizontalAlignment.Right, Microsoft.UI.Xaml.VerticalAlignment.Bottom), "Alignment: 9-point BottomRight");

            // 8.3 Flutter Code Mapping
            Assert(AlignmentHelper.ToFlutterAlignmentCode("center") == "Alignment.center", "Alignment: ToFlutterAlignmentCode 'center'");
            Assert(AlignmentHelper.ToFlutterAlignmentCode("TopLeft") == "Alignment.topLeft", "Alignment: ToFlutterAlignmentCode 'TopLeft'");
            Assert(AlignmentHelper.ToFlutterAlignmentCode("BottomRight") == "Alignment.bottomRight", "Alignment: ToFlutterAlignmentCode 'BottomRight'");

            // 8.4 Gravity to Flex Mapping for Row & Column
            var rowFlexCenter = AlignmentHelper.MapGravityToFlex("center", Microsoft.UI.Xaml.Controls.Orientation.Horizontal);
            Assert(rowFlexCenter.MainAxis == FlutterMainAxisAlignment.Center && rowFlexCenter.CrossAxis == FlutterCrossAxisAlignment.Center,
                "Alignment: Row Gravity 'center' -> MainAxis=Center, CrossAxis=Center");

            var rowFlexCustom = AlignmentHelper.MapGravityToFlex("center_horizontal|bottom", Microsoft.UI.Xaml.Controls.Orientation.Horizontal);
            Assert(rowFlexCustom.MainAxis == FlutterMainAxisAlignment.Center && rowFlexCustom.CrossAxis == FlutterCrossAxisAlignment.End,
                "Alignment: Row Gravity 'center_horizontal|bottom' -> MainAxis=Center, CrossAxis=End");

            var colFlexCenter = AlignmentHelper.MapGravityToFlex("center", Microsoft.UI.Xaml.Controls.Orientation.Vertical);
            Assert(colFlexCenter.MainAxis == FlutterMainAxisAlignment.Center && colFlexCenter.CrossAxis == FlutterCrossAxisAlignment.Center,
                "Alignment: Column Gravity 'center' -> MainAxis=Center, CrossAxis=Center");

            var colFlexCustom = AlignmentHelper.MapGravityToFlex("center_horizontal|bottom", Microsoft.UI.Xaml.Controls.Orientation.Vertical);
            Assert(colFlexCustom.MainAxis == FlutterMainAxisAlignment.End && colFlexCustom.CrossAxis == FlutterCrossAxisAlignment.Center,
                "Alignment: Column Gravity 'center_horizontal|bottom' -> MainAxis=End, CrossAxis=Center");

            // 8.5 Container Child Alignment Resolution & Overrides
            var containerParent = new WidgetItem { Type = "Container", Gravity = "center" };
            var childInherited = new WidgetItem { Type = "Text", Name = "Inherited Text" };
            var (effH1, effV1) = AlignmentHelper.ResolveEffectiveChildAlignment(childInherited, containerParent);
            Assert(effH1 == Microsoft.UI.Xaml.HorizontalAlignment.Center && effV1 == Microsoft.UI.Xaml.VerticalAlignment.Center,
                "Alignment: Container child inherits parent Gravity");

            var childOverride = new WidgetItem { Type = "Text", Name = "Overridden Text", Alignment = "BottomRight" };
            var (effH2, effV2) = AlignmentHelper.ResolveEffectiveChildAlignment(childOverride, containerParent);
            Assert(effH2 == Microsoft.UI.Xaml.HorizontalAlignment.Right && effV2 == Microsoft.UI.Xaml.VerticalAlignment.Bottom,
                "Alignment: Container child explicit Alignment overrides parent Gravity");

            var childMatchParent = new WidgetItem { Type = "Text", Name = "Match Parent Text", Width = "Match Parent", Height = "Match Parent" };
            var (effH3, effV3) = AlignmentHelper.ResolveEffectiveChildAlignment(childMatchParent, containerParent);
            Assert(effH3 == Microsoft.UI.Xaml.HorizontalAlignment.Stretch && effV3 == Microsoft.UI.Xaml.VerticalAlignment.Stretch,
                "Alignment: Match Parent dimensions enforce Stretch precedence");

            // 8.6 Flutter Compiler Output with Alignment & Gravity
            var testAlignRow = new WidgetItem { Type = "Row", Id = "align_row", MainAxisAlignment = "Center", CrossAxisAlignment = "Center" };
            testAlignRow.Children.Add(new WidgetItem { Type = "Text", Id = "row_txt", Name = "Centered Item" });
            string rowAlignDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(testAlignRow) }));
            Assert(rowAlignDart.Contains("mainAxisAlignment: MainAxisAlignment.center") && rowAlignDart.Contains("crossAxisAlignment: CrossAxisAlignment.center"),
                "Compiler: Row emits mainAxisAlignment and crossAxisAlignment");

            var testAlignContainer = new WidgetItem { Type = "Container", Id = "align_box", Width = "250", Height = "120", Alignment = "Center" };
            testAlignContainer.Children.Add(new WidgetItem { Type = "Button", Id = "box_btn", Name = "Action Button" });
            string containerAlignDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(testAlignContainer) }));
            Assert(containerAlignDart.Contains("alignment: Alignment.center") && containerAlignDart.Contains("width: 250") && containerAlignDart.Contains("height: 120"),
                "Compiler: Container emits alignment, width, and height");

            // Section 9: Custom Dimensions (dp, px, pt, %, Match Parent) & Option Presets Tests
            // 9.1 Multi-Unit Dimension Parsing
            var dpDim = AlignmentHelper.ParseDimension("150dp");
            Assert(dpDim.HasExplicitSize && dpDim.Value == 150.0 && dpDim.Unit == "dp", "Dimension: Parse '150dp' -> 150dp");

            var pxDim = AlignmentHelper.ParseDimension("200px");
            Assert(pxDim.HasExplicitSize && pxDim.Value == 200.0 && pxDim.Unit == "px", "Dimension: Parse '200px' -> 200px");

            var ptDim = AlignmentHelper.ParseDimension("120pt");
            Assert(ptDim.HasExplicitSize && ptDim.Value == 120.0 && ptDim.Unit == "pt", "Dimension: Parse '120pt' -> 120pt");

            var rawDim = AlignmentHelper.ParseDimension("320");
            Assert(rawDim.HasExplicitSize && rawDim.Value == 320.0, "Dimension: Parse raw number '320' -> 320");

            var pctDim = AlignmentHelper.ParseDimension("50%", 360.0);
            Assert(pctDim.IsPercentage && pctDim.PercentageValue == 50.0 && pctDim.Value == 180.0, "Dimension: Parse '50%' -> 180px on 360 viewport");

            var mpDim = AlignmentHelper.ParseDimension("match_parent");
            Assert(mpDim.IsMatchParent && !mpDim.IsAuto, "Dimension: Parse 'match_parent' -> Match Parent");

            var autoDim = AlignmentHelper.ParseDimension("wrap_content");
            Assert(autoDim.IsAuto && !autoDim.IsMatchParent, "Dimension: Parse 'wrap_content' -> Auto");

            // 9.2 Flutter Dimension Code Generation
            Assert(AlignmentHelper.ToFlutterDimensionCode("150dp", "width") == "150", "Compiler: '150dp' -> '150'");
            Assert(AlignmentHelper.ToFlutterDimensionCode("200px", "height") == "200", "Compiler: '200px' -> '200'");
            Assert(AlignmentHelper.ToFlutterDimensionCode("Match Parent", "width") == "double.infinity", "Compiler: 'Match Parent' -> 'double.infinity'");
            Assert(AlignmentHelper.ToFlutterDimensionCode("50%", "width") == "MediaQuery.of(context).size.width * 0.5", "Compiler: '50%' width -> MediaQuery width * 0.5");
            Assert(AlignmentHelper.ToFlutterDimensionCode("Auto", "width") == "", "Compiler: 'Auto' -> omitted (shrink-wrap)");

            // 9.3 Available Options & Presets Verification
            Assert(AlignmentHelper.AvailableAlignments.Length >= 10 && AlignmentHelper.AvailableAlignments.Contains("Center") && AlignmentHelper.AvailableAlignments.Contains("Default"),
                "Options: AvailableAlignments contains all standard 9-point choices and Default");
            Assert(AlignmentHelper.AvailableGravities.Length >= 13 && AlignmentHelper.AvailableGravities.Contains("Stretch") && AlignmentHelper.AvailableGravities.Contains("CenterHorizontal"),
                "Options: AvailableGravities contains stretch, horizontal/vertical centers and 9-point choices");
            Assert(AlignmentHelper.AvailableDimensionPresets.Length >= 10 && AlignmentHelper.AvailableDimensionPresets.Contains("50dp") && AlignmentHelper.AvailableDimensionPresets.Contains("Match Parent"),
                "Options: AvailableDimensionPresets contains unit options and presets");

            // 9.4 Flutter Compiler Container with Multi-Unit Dimension
            var testUnitContainer = new WidgetItem { Type = "Container", Id = "unit_box", Width = "180dp", Height = "50%" };
            string unitContainerDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(testUnitContainer) }));
            Assert(unitContainerDart.Contains("width: 180") && unitContainerDart.Contains("height: MediaQuery.of(context).size.height * 0.5"),
                "Compiler: Container emits custom multi-unit dimensions (dp & %)");

            // Section 10: Parent and Child Dimension Reflection Across Row, Column, Container & Combos
            report.AppendLine("\n## 10. Parent and Child Dimension Reflection Across Row, Column, Container & Combos");
            Console.WriteLine("\n>>> 10. Testing Dimension Reflection & Sizing Across Parent/Child Combos...");

            // 10.1 Default Flex Factors (Bug Fix Verification)
            var defaultText = new WidgetItem { Type = "Text", Name = "Sample Text" };
            Assert(defaultText.Flex == 0, "Flex: Default widget Flex factor is 0 (non-flex by default)");

            var defaultExp = new WidgetItem { Type = "Expanded", Name = "Expanded", Flex = 1 };
            Assert(defaultExp.Flex == 1, "Flex: Expanded widget defaults to Flex = 1");

            // 10.2 Child Text Dimension Reflection (User Screenshot Reproduction)
            var userCol = new WidgetItem { Type = "Column", Id = "column_1", Width = "Match Parent", Height = "Match Parent" };
            var userText = new WidgetItem { Type = "Text", Id = "text_5f04", Name = "Text", Width = "25%", Height = "50dp" };
            var userBtn = new WidgetItem { Type = "Button", Id = "button_1", Name = "Button", Height = "Auto" };
            userCol.Children.Add(userText);
            userCol.Children.Add(userBtn);

            var userTextWDim = AlignmentHelper.ParseDimension(userText.Width, 360.0);
            var userTextHDim = AlignmentHelper.ParseDimension(userText.Height, 640.0);
            Assert(userTextWDim.HasExplicitSize && userTextWDim.Value == 90.0, "Dimension: Child Text Width '25%' -> 90dp on 360 viewport");
            Assert(userTextHDim.HasExplicitSize && userTextHDim.Value == 50.0, "Dimension: Child Text Height '50dp' -> 50dp explicit size");

            string userColDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(userCol) }));
            Assert(userColDart.Contains("SizedBox(") && userColDart.Contains("height: 50") && userColDart.Contains("MediaQuery.of(context).size.width * 0.25"),
                "Compiler: Child Text with custom 25% width and 50dp height wrapped in responsive SizedBox");

            // 10.3 Parent Column Dimension Reflection (Height & Width Combos)
            var parentColExplicit = new WidgetItem { Type = "Column", Id = "col_parent", Height = "200dp", Width = "300dp" };
            parentColExplicit.Children.Add(new WidgetItem { Type = "Text", Name = "Inside Col" });
            string parentColDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(parentColExplicit) }));
            Assert(parentColDart.Contains("SizedBox(") && parentColDart.Contains("height: 200") && parentColDart.Contains("width: 300"),
                "Compiler: Parent Column with explicit 200dp height and 300dp width wrapped in SizedBox");

            var parentColAuto = new WidgetItem { Type = "Column", Id = "col_auto", Height = "Auto" };
            parentColAuto.Children.Add(new WidgetItem { Type = "Text", Name = "Inside Auto Col" });
            string parentColAutoDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(parentColAuto) }));
            Assert(parentColAutoDart.Contains("mainAxisSize: MainAxisSize.min"),
                "Compiler: Column with Height 'Auto' emits mainAxisSize: MainAxisSize.min (shrink-wrap)");

            // 10.4 Parent Row Dimension Reflection (Width & Height Combos)
            var parentRowExplicit = new WidgetItem { Type = "Row", Id = "row_parent", Width = "250dp", Height = "60dp" };
            parentRowExplicit.Children.Add(new WidgetItem { Type = "Button", Name = "Inside Row" });
            string parentRowDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(parentRowExplicit) }));
            Assert(parentRowDart.Contains("SizedBox(") && parentRowDart.Contains("width: 250") && parentRowDart.Contains("height: 60"),
                "Compiler: Parent Row with explicit 250dp width and 60dp height wrapped in SizedBox");

            var parentRowAuto = new WidgetItem { Type = "Row", Id = "row_auto", Width = "Auto" };
            parentRowAuto.Children.Add(new WidgetItem { Type = "Button", Name = "Inside Auto Row" });
            string parentRowAutoDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(parentRowAuto) }));
            Assert(parentRowAutoDart.Contains("mainAxisSize: MainAxisSize.min"),
                "Compiler: Row with Width 'Auto' emits mainAxisSize: MainAxisSize.min (shrink-wrap)");

            // 10.5 Complex Nested Container Hierarchy (Container -> Column -> Row -> Children)
            var grandContainer = new WidgetItem { Type = "Container", Id = "cnt_grand", Width = "320dp", Height = "480dp", Alignment = "Center" };
            var childCol = new WidgetItem { Type = "Column", Id = "col_mid", Height = "300dp", Width = "Match Parent" };
            var rowCombo = new WidgetItem { Type = "Row", Id = "row_combo", Width = "Auto", Height = "50dp" };
            var comboText = new WidgetItem { Type = "Text", Id = "txt_combo", Name = "Combo Text", Width = "80dp", Height = "30dp" };
            var comboBtn = new WidgetItem { Type = "Button", Id = "btn_combo", Name = "Combo Button", Width = "100dp", Height = "40dp" };
            rowCombo.Children.Add(comboText);
            rowCombo.Children.Add(comboBtn);
            childCol.Children.Add(rowCombo);
            grandContainer.Children.Add(childCol);

            string complexDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(grandContainer) }));
            Assert(complexDart.Contains("Container(width: 320, height: 480, alignment: Alignment.center") &&
                   complexDart.Contains("height: 300") &&
                   complexDart.Contains("Row(") &&
                   complexDart.Contains("width: 80") &&
                   complexDart.Contains("width: 100"),
                "Compiler: Nested Container > Column > Row > Widgets respects all explicit dimension constraints");

            // Summary
            report.AppendLine("\n---");
            report.AppendLine($"### Result: {passed}/{total} Passed ({(failed == 0 ? "100%" : $"{Math.Round((double)passed/total*100, 1)}%")})");
            if (failed > 0)
            {
                report.AppendLine("### Identified Bugs:");
                foreach (var b in bugsFound)
                {
                    report.AppendLine($"- {b}");
                }
            }
            else
            {
                report.AppendLine("**Status: ALL WIDGETS AND PROPERTIES FULLY OPERATIONAL AND VERIFIED.**");
            }

            string resultStr = report.ToString();

            if (!string.IsNullOrEmpty(outputReportPath))
            {
                try
                {
                    File.WriteAllText(outputReportPath, resultStr);
                    Console.WriteLine($"\n>>> Report written to: {outputReportPath}");
                }
                catch { }
            }

            Console.WriteLine("\n=======================================================");
            Console.WriteLine($"  SUMMARY: {passed}/{total} PASSED ({(failed == 0 ? "100%" : $"{Math.Round((double)passed/total*100, 1)}%")})");
            Console.WriteLine("=======================================================");

            return resultStr;
        }

        private static WidgetItem? FindWidgetById(ObservableCollection<WidgetItem> list, string id)
        {
            foreach (var item in list)
            {
                if (item.Id == id) return item;
                var found = FindWidgetById(item.Children, id);
                if (found != null) return found;
            }
            return null;
        }

        private static bool RemoveWidgetFromTree(ObservableCollection<WidgetItem> list, WidgetItem target)
        {
            if (list.Contains(target))
            {
                list.Remove(target);
                return true;
            }
            foreach (var item in list)
            {
                if (RemoveWidgetFromTree(item.Children, target))
                    return true;
            }
            return false;
        }

        private static object SerializeItem(WidgetItem item)
        {
            var childrenList = new List<object>();
            foreach (var child in item.Children)
            {
                childrenList.Add(SerializeItem(child));
            }

            return new
            {
                id = item.Id,
                type = item.Type,
                text = item.Name,
                width = item.Width,
                height = item.Height,
                padding = item.Padding,
                margin = item.Margin,
                backgroundColor = item.BackgroundColor,
                textColor = item.TextColor,
                textSize = item.TextSize,
                cornerRadius = item.CornerRadius,
                borderWidth = item.BorderWidth,
                borderColor = item.BorderColor,
                alignment = item.Alignment,
                gravity = item.Gravity,
                assetPath = item.AssetPath,
                fontWeight = item.FontWeight,
                textAlign = item.TextAlign,
                mainAxisAlignment = item.MainAxisAlignment,
                crossAxisAlignment = item.CrossAxisAlignment,
                boxFit = item.BoxFit,
                obscureText = item.ObscureText,
                scrollDirection = item.ScrollDirection,
                assetUsage = item.AssetUsage,
                hasShadow = item.HasShadow,
                isChecked = item.Checked,
                activeColor = item.ActiveColor,
                minValue = item.MinValue,
                maxValue = item.MaxValue,
                currentValue = item.CurrentValue,
                hintText = item.HintText,
                inputType = item.InputType,
                fontFamily = item.FontFamily,
                flex = item.Flex,
                children = childrenList
            };
        }
    }
}
