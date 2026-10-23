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
            Assert(dartCode.Contains("_controllers['tf_email']") || dartCode.Contains("_ctrlTfEmail"), "Compiler Bug 82: TextField controller generated");

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

            // Event Wiring & Dialog Trigger Tests
            string eventJson = @"[{""type"":""Button"",""id"":""btn_dialog"",""text"":""Open Dialog""},{""type"":""Row"",""id"":""row_header"",""children"":[]}]";
            string rawEvents = @"// EVENT_BINDING: onClick -> btn_dialog
showDialog(context: context, builder: (ctx) => AlertDialog(title: Text('Test'), content: Text('Msg')));
// EVENT_BINDING: onClick -> row_header
print('Row tapped');";

            string fullDart = FlutterCompilerService.GenerateCompletePageDart("main", eventJson, rawEvents);
            Assert(fullDart.Contains("void onClickBtnDialog(BuildContext context)"), "Event: Generates onClickBtnDialog with context");
            Assert(fullDart.Contains("showDialog("), "Event: Event body contains showDialog");
            Assert(fullDart.Contains("onClickBtnDialog(context);"), "Event: Button onPressed calls onClickBtnDialog(context)");
            Assert(fullDart.Contains("onClickRowHeader(context)"), "Event: Row wrapped in GestureDetector calls onClickRowHeader");

            // Dynamic Widget State & Reactive Property Wiring Tests
            string testUiJson = @"[
              {""type"":""Text"",""id"":""txt_title"",""text"":""Initial Title""},
              {""type"":""Button"",""id"":""btn_action"",""text"":""Action""},
              {""type"":""TextField"",""id"":""tf_input"",""hintText"":""Old Hint""},
              {""type"":""Image"",""id"":""img_avatar"",""assetPath"":""sample.png""},
              {""type"":""Checkbox"",""id"":""cb_agree""},
              {""type"":""Switch"",""id"":""sw_theme""},
              {""type"":""Slider"",""id"":""sl_volume""}
            ]";
            string testEventCode = @"// EVENT_BINDING: onClick -> btn_action
setState(() {
  _txtTitleText = 'Updated Title';
  _txtTitleTextColor = Color(0xFFE91E63);
  _txtTitleFontSize = 24.0;
  _btnActionBgColor = Color(0xFF4CAF50);
  _btnActionVisible = false;
  _btnActionEnabled = false;
  _tfInputHint = 'New Hint';
  _imgAvatarAsset = 'assets/new.png';
  _cbAgreeChecked = true;
  _swThemeChecked = true;
  _slVolumeValue = 75.0;
});";
            string testCompleteDart = FlutterCompilerService.GenerateCompletePageDart("main", testUiJson, testEventCode);
            Assert(testCompleteDart.Contains("String? _txtTitleText;"), "State Var: _txtTitleText declared");
            Assert(testCompleteDart.Contains("Color? _txtTitleTextColor;"), "State Var: _txtTitleTextColor declared");
            Assert(testCompleteDart.Contains("double? _txtTitleFontSize;"), "State Var: _txtTitleFontSize declared");
            Assert(testCompleteDart.Contains("bool _btnActionVisible = true;"), "State Var: _btnActionVisible declared");
            Assert(testCompleteDart.Contains("bool _btnActionEnabled = true;"), "State Var: _btnActionEnabled declared");
            Assert(testCompleteDart.Contains("_txtTitleText ?? \"Initial Title\""), "Widget Wiring: Text dynamically uses _txtTitleText");
            Assert(testCompleteDart.Contains("Visibility(visible: _btnActionVisible"), "Widget Wiring: Button wrapped in Visibility");
            Assert(testCompleteDart.Contains("IgnorePointer(ignoring: !(_btnActionEnabled)"), "Widget Wiring: Button wrapped in IgnorePointer");
            Assert(testCompleteDart.Contains("_tfInputHint ?? \"Old Hint\""), "Widget Wiring: TextField dynamically uses _tfInputHint");

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

            // 10.6 Deserialization Flex Safety (Legacy Project Healing)
            using var legacyButtonDoc = JsonDocument.Parse("{\"type\":\"Button\",\"name\":\"button_0382\",\"flex\":1,\"height\":\"150dp\"}");
            var healedButton = NativeUIDesigner.DeserializeItem(legacyButtonDoc.RootElement);
            Assert(healedButton.Flex == 0, "Deserialization: Non-Expanded widget with legacy 'flex: 1' heals to Flex = 0");
            Assert(healedButton.Height == "150dp", "Deserialization: Button preserves Height '150dp'");

            using var legacyExpDoc = JsonDocument.Parse("{\"type\":\"Expanded\",\"name\":\"Expanded\",\"flex\":3}");
            var deserializedExp = NativeUIDesigner.DeserializeItem(legacyExpDoc.RootElement);
            Assert(deserializedExp.Flex == 3, "Deserialization: Expanded widget preserves explicit flex factor = 3");

            using var legacyExpDefaultDoc = JsonDocument.Parse("{\"type\":\"Expanded\",\"name\":\"Expanded\"}");
            var deserializedExpDef = NativeUIDesigner.DeserializeItem(legacyExpDefaultDoc.RootElement);
            Assert(deserializedExpDef.Flex == 1, "Deserialization: Expanded widget without flex property defaults to Flex = 1");

            // 10.7 Selector Geometry & Dimension Reflection (User Screenshot Reproduction)
            var buttonItem = new WidgetItem { Type = "Button", Id = "button_0382", Name = "Button", Height = "150dp", Width = "Auto" };
            var btnHDim = AlignmentHelper.ParseDimension(buttonItem.Height, 640.0);
            var btnWDim = AlignmentHelper.ParseDimension(buttonItem.Width, 360.0);
            Assert(btnHDim.HasExplicitSize && btnHDim.Value == 150.0, "Selector Geometry: Button Height '150dp' parses to exact 150dp");
            Assert(!btnWDim.HasExplicitSize && !btnWDim.IsMatchParent, "Selector Geometry: Button Width 'Auto' identified as Auto");

            string btnDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(buttonItem) }));
            Assert(btnDart.Contains("SizedBox(") && btnDart.Contains("height: 150"), "Compiler: Button with 150dp height generates SizedBox(height: 150.0)");

            // 10.8 Margin & Selector Border Reflection (User Margin Screenshot Reproduction)
            Assert(AlignmentHelper.ToFlutterEdgeInsetsCode("8") == "const EdgeInsets.all(8)", "EdgeInsets: Single value '8' -> const EdgeInsets.all(8)");
            Assert(AlignmentHelper.ToFlutterEdgeInsetsCode("8, 16") == "const EdgeInsets.symmetric(horizontal: 8, vertical: 16)", "EdgeInsets: Symmetric '8, 16' -> const EdgeInsets.symmetric");
            Assert(AlignmentHelper.ToFlutterEdgeInsetsCode("4, 8, 12, 16") == "const EdgeInsets.fromLTRB(4, 8, 12, 16)", "EdgeInsets: 4-way '4, 8, 12, 16' -> const EdgeInsets.fromLTRB");

            var marginText = new WidgetItem { Type = "Text", Id = "text_5f04", Name = "Text", Height = "100dp", Margin = "8" };
            string marginTextDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(marginText) }));
            Assert(marginTextDart.Contains("Padding(padding: const EdgeInsets.all(8)") && marginTextDart.Contains("height: 100"),
                "Compiler: Text with Margin '8' and Height '100dp' wrapped in Padding and SizedBox");

            var marginContainer = new WidgetItem { Type = "Container", Id = "cnt_margin", Margin = "8", Width = "200dp", Height = "100dp" };
            string marginContainerDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(marginContainer) }));
            Assert(marginContainerDart.Contains("margin: const EdgeInsets.all(8)"),
                "Compiler: Container with Margin '8' emits native container margin");

            // 10.9 Color, Border, CornerRadius Reflection & Compilation Suite
            var colRed = AlignmentHelper.ParseColor("Red");
            Assert(colRed.A == 255 && colRed.R == 244 && colRed.G == 67 && colRed.B == 54, "Color: Named 'Red' parses to Material Red #F44336");

            var colHex6 = AlignmentHelper.ParseColor("#2196F3");
            Assert(colHex6.A == 255 && colHex6.R == 33 && colHex6.G == 150 && colHex6.B == 243, "Color: 6-char '#2196F3' parses to full opacity Material Blue");

            var colZeroAlphaHealed = AlignmentHelper.ParseColor("#002196F3");
            Assert(colZeroAlphaHealed.A == 255 && colZeroAlphaHealed.R == 33, "Color: 0-Alpha bug '#002196F3' auto-heals to A = 255");

            Assert(AlignmentHelper.ToFlutterColorCode("Material Blue") == "Colors.blue", "Color Code: 'Material Blue' -> Colors.blue");
            Assert(AlignmentHelper.ToFlutterColorCode("#2196F3") == "const Color(0xFF2196F3)", "Color Code: '#2196F3' -> const Color(0xFF2196F3)");

            string boxDec = AlignmentHelper.ToFlutterBoxDecorationCode("#2196F3", "#000000", "2", "12", true);
            Assert(boxDec.Contains("color: const Color(0xFF2196F3)") && boxDec.Contains("borderRadius: BorderRadius.circular(12)") &&
                   boxDec.Contains("border: Border.all(color: const Color(0xFF000000), width: 2)") && boxDec.Contains("boxShadow: const [BoxShadow("),
                   "BoxDecoration: Emits color, borderRadius, border, and boxShadow");

            var styledContainer = new WidgetItem
            {
                Type = "Container",
                Id = "cnt_styled",
                BackgroundColor = "#2196F3",
                CornerRadius = "12",
                BorderColor = "#000000",
                BorderWidth = "2",
                HasShadow = true
            };
            string cntDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledContainer) }));
            Assert(cntDart.Contains("decoration: BoxDecoration(") && cntDart.Contains("borderRadius: BorderRadius.circular(12)"),
                "Compiler: Container emits decoration with color, radius, border, and shadow");

            var styledText = new WidgetItem { Type = "Text", Id = "txt_styled", Name = "Styled Label", TextColor = "#F44336", TextSize = "18", FontWeight = "Bold" };
            string txtDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledText) }));
            Assert(txtDart.Contains("style: TextStyle(") && txtDart.Contains("color: const Color(0xFFF44336)") && txtDart.Contains("fontSize: 18") && txtDart.Contains("FontWeight.bold"),
                "Compiler: Text emits TextStyle with color, fontSize, and fontWeight");

            var styledButton = new WidgetItem { Type = "Button", Id = "btn_styled", Name = "Click Me", BackgroundColor = "#4CAF50", CornerRadius = "24", BorderWidth = "2", BorderColor = "#FFFFFF" };
            string btnStyledDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledButton) }));
            Assert(btnStyledDart.Contains("style: ElevatedButton.styleFrom(") && btnStyledDart.Contains("backgroundColor: const Color(0xFF4CAF50)") &&
                   btnStyledDart.Contains("borderRadius: BorderRadius.circular(24)") && btnStyledDart.Contains("width: 2"),
                   "Compiler: Button emits ElevatedButton.styleFrom with background, rounded corners, and border side");

            var styledTextField = new WidgetItem { Type = "TextField", Id = "tf_styled", HintText = "Enter email", BackgroundColor = "#F5F5F5", CornerRadius = "10", BorderWidth = "1.5" };
            string tfDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledTextField) }));
            Assert(tfDart.Contains("decoration: InputDecoration(") && tfDart.Contains("filled: true") && tfDart.Contains("OutlineInputBorder(borderRadius: BorderRadius.circular(10)"),
                "Compiler: TextField emits InputDecoration with fill color and outline border radius");

            var styledCheckbox = new WidgetItem { Type = "Checkbox", Id = "cb_styled", ActiveColor = "#E91E63" };
            string cbDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledCheckbox) }));
            Assert(cbDart.Contains("activeColor: const Color(0xFFE91E63)"), "Compiler: Checkbox emits activeColor");

            var styledSwitch = new WidgetItem { Type = "Switch", Id = "sw_styled", ActiveColor = "#009688" };
            string swDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledSwitch) }));
            Assert(swDart.Contains("activeColor: const Color(0xFF009688)"), "Compiler: Switch emits activeColor");

            var styledSlider = new WidgetItem { Type = "Slider", Id = "sl_styled", ActiveColor = "#FF9800" };
            string slDart = FlutterCompilerService.GenerateBuildMethod(JsonSerializer.Serialize(new List<object> { SerializeItem(styledSlider) }));
            Assert(slDart.Contains("activeColor: const Color(0xFFFF9800)"), "Compiler: Slider emits activeColor");

            // 13. Tab Switching & Auto-Save State Preservation Suite
            report.AppendLine("\n## 13. Tab Switching, Auto-Save & State Preservation");
            Console.WriteLine(">>> 13. Testing Tab Switching, Auto-Save & State Preservation...");

            // Test 1: Property change notification triggers state change
            var columnWidget = new WidgetItem { Type = "Column", Id = "col_test", BackgroundColor = "Transparent" };
            bool propertyChangedFired = false;
            columnWidget.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WidgetItem.BackgroundColor)) propertyChangedFired = true;
            };
            columnWidget.BackgroundColor = "#FF5722"; // Material Deep Orange
            Assert(propertyChangedFired, "Auto-Save: BackgroundColor change triggers PropertyChanged event");

            // Test 2: In-memory DOM state preservation across tab toggles
            var canvasWidgets = new ObservableCollection<WidgetItem> { columnWidget };
            var childText = new WidgetItem { Type = "Text", Id = "txt_inside_col", Name = "Hello Snapcode" };
            columnWidget.Children.Add(childText);
            
            // Serialize in-memory tree state before any tab switch
            string inMemoryJson = JsonSerializer.Serialize(canvasWidgets.Select(SerializeItem).ToList());
            Assert(inMemoryJson.Contains("#FF5722") && inMemoryJson.Contains("txt_inside_col"), "State: In-memory widget tree contains modified background and children");

            // Test 3: Tab switch does not reset in-memory tree when loadNativeUi is false
            // Simulating switching from View -> Event -> View: the in-memory canvasWidgets must retain col_test's new background
            var retainedColumn = FindWidgetById(canvasWidgets, "col_test");
            Assert(retainedColumn != null && retainedColumn.BackgroundColor == "#FF5722", "Tab Switch: In-memory designer retains UI modifications without disk overwrite");

            // Test 4: Save & Reload parity
            using var reloadedDoc = JsonDocument.Parse(inMemoryJson);
            var reloadedItems = new ObservableCollection<WidgetItem>();
            foreach (var elem in reloadedDoc.RootElement.EnumerateArray())
            {
                reloadedItems.Add(NativeUIDesigner.DeserializeItem(elem));
            }
            var reloadedCol = FindWidgetById(reloadedItems, "col_test");
            Assert(reloadedCol != null && reloadedCol.BackgroundColor == "#FF5722" && reloadedCol.Children.Count == 1, "Save/Reload: Deserialized tree preserves modified column color and child count");
            Assert(reloadedCol?.Children[0].Name == "Hello Snapcode", "Save/Reload: Nested child properties are fully preserved");

            // 14. Advanced Code Editor, User Code Preservation & File Isolation
            report.AppendLine("\n## 14. Advanced Code Editor, Code Preservation & File Isolation");
            Console.WriteLine(">>> 14. Testing User Code Preservation, Marker Parsing & Project File Classification...");

            // 14.1 Extraction of user code blocks
            string sampleExistingCode = @"import 'package:flutter/material.dart';
// <!-- USER_CODE_START: IMPORTS -->
import 'package:http/http.dart' as http;
import 'package:provider/provider.dart';
// <!-- USER_CODE_END: IMPORTS -->

class MyApp extends StatefulWidget {
  // <!-- USER_CODE_START: CLASS_MEMBERS -->
  int customCounter = 42;
  String? authToken;
  // <!-- USER_CODE_END: CLASS_MEMBERS -->

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  // <!-- USER_CODE_START: CUSTOM_METHODS -->
  Future<void> fetchRemoteData() async {
    final response = await http.get(Uri.parse('https://api.example.com'));
  }
  // <!-- USER_CODE_END: CUSTOM_METHODS -->

  @override
  Widget build(BuildContext context) {
    return Container();
  }
}";

            var extracted = UserCodePreservationService.ExtractUserCodeBlocks(sampleExistingCode);
            Assert(extracted.ContainsKey(UserCodePreservationService.TagImports), "Preservation: Successfully extracted user IMPORTS block");
            Assert(extracted[UserCodePreservationService.TagImports].Contains("package:http/http.dart"), "Preservation: Extracted imports contain http package");
            Assert(extracted.ContainsKey(UserCodePreservationService.TagClassMembers), "Preservation: Successfully extracted CLASS_MEMBERS block");
            Assert(extracted[UserCodePreservationService.TagClassMembers].Contains("customCounter = 42"), "Preservation: Extracted class members contain custom counter variable");
            Assert(extracted.ContainsKey(UserCodePreservationService.TagCustomMethods), "Preservation: Successfully extracted CUSTOM_METHODS block");
            Assert(extracted[UserCodePreservationService.TagCustomMethods].Contains("fetchRemoteData"), "Preservation: Extracted custom method block contains fetchRemoteData");

            // 14.2 Code Merge into newly regenerated Flutter template
            string freshlyGeneratedCode = @"import 'package:flutter/material.dart';

class MyApp extends StatefulWidget {
  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(body: Center(child: Text('Regenerated')));
  }
}";

            string mergedCode = UserCodePreservationService.MergeUserCodeIntoGenerated(freshlyGeneratedCode, extracted);
            Assert(mergedCode.Contains("package:http/http.dart"), "Preservation: Merged code preserves user custom imports");
            Assert(mergedCode.Contains("customCounter = 42"), "Preservation: Merged code preserves user class members");
            Assert(mergedCode.Contains("fetchRemoteData"), "Preservation: Merged code preserves user custom methods");
            Assert(mergedCode.Contains("Regenerated"), "Preservation: Merged code retains newly generated visual designer code");

            // 14.3 Non-destructive file preservation test on disk
            string testTempDir = Path.Combine(Path.GetTempPath(), "snapcode_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testTempDir);
            try
            {
                string testDartFile = Path.Combine(testTempDir, "main.dart");
                File.WriteAllText(testDartFile, sampleExistingCode);

                // Simulate visual designer regeneration saving with preservation
                string updatedGenerated = @"import 'package:flutter/material.dart';
class MyApp extends StatefulWidget {
  @override
  State<MyApp> createState() => _MyAppState();
}
class _MyAppState extends State<MyApp> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(body: Text('Brand new button added'));
  }
}";

                UserCodePreservationService.SaveDartFileWithPreservation(testDartFile, updatedGenerated);
                string diskContent = File.ReadAllText(testDartFile);

                Assert(diskContent.Contains("package:http/http.dart"), "Preservation on Disk: User custom imports survived designer regeneration");
                Assert(diskContent.Contains("customCounter = 42"), "Preservation on Disk: User class members survived designer regeneration");
                Assert(diskContent.Contains("fetchRemoteData"), "Preservation on Disk: User custom methods survived designer regeneration");
                Assert(diskContent.Contains("Brand new button added"), "Preservation on Disk: Visual designer modifications accurately injected");

                // 14.4 Untagged custom import heuristic fallback
                string untaggedSource = @"import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class PageOne extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container();
}";
                var untaggedBlocks = UserCodePreservationService.ExtractUserCodeBlocks(untaggedSource);
                Assert(untaggedBlocks.ContainsKey(UserCodePreservationService.TagImports) &&
                       untaggedBlocks[UserCodePreservationService.TagImports].Contains("shared_preferences"),
                       "Preservation: Untagged custom imports automatically detected and salvaged");

                // 14.5 Project File Classification & Isolation
                string dummyProjectRoot = Path.Combine(testTempDir, "sample_project");
                string pagesDir = Path.Combine(dummyProjectRoot, ".snapcode", "pages");
                Directory.CreateDirectory(pagesDir);

                // Create visual page metadata for "login_page"
                File.WriteAllText(Path.Combine(pagesDir, "login_page_meta.json"), "{\"Name\": \"login_page\"}");
                File.WriteAllText(Path.Combine(pagesDir, "login_page_ui.json"), "{}");

                // Create pure custom Dart files in lib/
                string libDir = Path.Combine(dummyProjectRoot, "flutter", "lib");
                string servicesDir = Path.Combine(libDir, "services");
                Directory.CreateDirectory(servicesDir);
                string authServicePath = Path.Combine(servicesDir, "auth_service.dart");
                File.WriteAllText(authServicePath, "// Custom Auth Service");

                Assert(ProjectFileClassifier.IsVisualPage(dummyProjectRoot, "main"), "Classifier: 'main' is recognized as visual page");
                Assert(ProjectFileClassifier.IsVisualPage(dummyProjectRoot, "login_page"), "Classifier: 'login_page' is recognized as visual page");
                Assert(!ProjectFileClassifier.IsVisualPage(dummyProjectRoot, "auth_service"), "Classifier: 'auth_service' is NOT a visual page (custom only)");

                Assert(ProjectFileClassifier.ClassifyFile(dummyProjectRoot, "lib/main.dart") == ProjectFileType.VisualPage,
                       "Classifier: main.dart classified as VisualPage");
                Assert(ProjectFileClassifier.ClassifyFile(dummyProjectRoot, authServicePath) == ProjectFileType.PureDartFile,
                       "Classifier: auth_service.dart classified as PureDartFile (hidden from UI Designer)");

                var visualPages = ProjectFileClassifier.GetVisualPages(dummyProjectRoot);
                Assert(visualPages.Contains("main") && visualPages.Contains("login_page"), "Classifier: GetVisualPages lists all visual pages");
                Assert(!visualPages.Contains("auth_service"), "Classifier: GetVisualPages strictly excludes pure custom Dart files");

                // 14.6 ProjectExplorer File Item Icons
                var dartItem = new Snapcode.ViewModels.FileItemViewModel(authServicePath, isDirectory: false);
                Assert(dartItem.IconGlyph == "\uE790", "Explorer: Dart file displays code glyph \uE790");
                Assert(dartItem.IconColorHex == "#00B4AB", "Explorer: Dart file displays Flutter teal color");

                var yamlItem = new Snapcode.ViewModels.FileItemViewModel("pubspec.yaml", isDirectory: false);
                Assert(yamlItem.IconGlyph == "\uE7B8", "Explorer: YAML file displays settings glyph \uE7B8");

                var folderItem = new Snapcode.ViewModels.FileItemViewModel("services", isDirectory: true);
                Assert(folderItem.IconGlyph == "\uED25", "Explorer: Closed folder displays folder glyph \uED25");
                folderItem.IsExpanded = true;
                Assert(folderItem.IconGlyph == "\uED43", "Explorer: Opened folder displays folder glyph \uED43");

                // 14.7 Full Page Code Generation (UI Designer + Blockly Integration)
                string sampleUiJson = "[{\"id\":\"col_1\",\"type\":\"Column\",\"children\":[{\"id\":\"t_1\",\"type\":\"Text\",\"text\":\"Hello World\"},{\"id\":\"b_1\",\"type\":\"Button\",\"text\":\"Submit\"}]}]";
                string sampleEventCode = "// EVENT_BINDING: onClick -> b_1\nshowDialog(context: context, builder: (_) => const AlertDialog(title: Text('Clicked!')));";
                string fullMainDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("main", sampleUiJson, sampleEventCode);

                Assert(fullMainDart.Contains("void main() {"), "FullPageCodegen: main.dart contains entrypoint main()");
                Assert(fullMainDart.Contains("class MyApp extends StatelessWidget"), "FullPageCodegen: main.dart contains MyApp widget");
                Assert(fullMainDart.Contains("class MainPage extends StatefulWidget"), "FullPageCodegen: main.dart contains MainPage stateful widget");
                Assert(fullMainDart.Contains("Text(\"Hello World\""), "FullPageCodegen: main.dart contains visual Text widget from canvas");
                Assert(fullMainDart.Contains("ElevatedButton("), "FullPageCodegen: main.dart contains visual Button widget from canvas");
                Assert(fullMainDart.Contains("showDialog(context: context"), "FullPageCodegen: main.dart contains event block logic inside state class");

                string fullAuxDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("profile", sampleUiJson, null);
                Assert(fullAuxDart.Contains("class ProfilePage extends StatefulWidget"), "FullPageCodegen: Auxiliary page generates ProfilePage stateful widget");
                Assert(!fullAuxDart.Contains("void main()"), "FullPageCodegen: Auxiliary page does not have main()");
                Assert(fullAuxDart.Contains("Text(\"Hello World\""), "FullPageCodegen: Auxiliary page contains visual widgets");
            }
            finally
            {
                try { Directory.Delete(testTempDir, true); } catch { }
            }

            // 15. End-to-End Block System, Target Widgets & Multi-Event Aggregation
            report.AppendLine("\n## 15. End-to-End Block System, Target Widgets & Multi-Event Aggregation");
            Console.WriteLine(">>> 15. Testing End-to-End Block System, Target Widgets & Multi-Event Aggregation...");

            string multiEventUiJson = @"[
                {""id"":""column_1"",""type"":""Column"",""children"":[
                    {""id"":""image_ae35"",""type"":""Image"",""assetPath"":""lanscape_image_01.jpg""},
                    {""id"":""text_f2c6"",""type"":""Text"",""text"":""Original Text""},
                    {""id"":""row_431c"",""type"":""Row"",""children"":[
                        {""id"":""textfield_b3ef"",""type"":""TextField"",""hintText"":""Input""}
                    ]},
                    {""id"":""checkbox_1ac2"",""type"":""Checkbox""},
                    {""id"":""switch_8bb0"",""type"":""Switch""},
                    {""id"":""slider_b124"",""type"":""Slider"",""minValue"":0,""maxValue"":100,""currentValue"":50},
                    {""id"":""button_09ac"",""type"":""Button"",""text"":""Click Me""}
                ]}
            ]";

            string testMultiEvents = @"// EVENT_BINDING: onClick -> column_1
setState(() {
  _textF2c6Text = 'Column Clicked!';
});

// EVENT_BINDING: onClick -> button_09ac
showDialog(
  context: context,
  builder: (ctx) => AlertDialog(
    title: const Text('Alert'),
    content: const Text('Button Clicked!'),
    actions: [
      TextButton(onPressed: () => Navigator.of(ctx).pop(), child: const Text('OK'))
    ]
  )
);

// EVENT_BINDING: onLongPress -> button_09ac
ScaffoldMessenger.of(context).showSnackBar(
  const SnackBar(content: Text('Button Long Pressed!'))
);

// EVENT_BINDING: onChanged(double value) -> slider_b124
setState(() {
  _sliderB124Value = value;
});

// EVENT_BINDING: onClick -> row_431c
setState(() {
  _row431cBgColor = const Color(0xFF4CAF50);
  _column1Padding = 24.0;
  _column1BgColor = const Color(0xFFE0E0E0);
  _column1Alignment = MainAxisAlignment.center;
  _textfieldB3efEnabled = false;
  _button09acBgColor = const Color(0xFFFF5722);
  _ctrlCustomInput.text = 'AutoDeclared';
});";

            var multiPageMeta = new Snapcode.Models.PageMetadata
            {
                Title = "Hardik Prajapati",
                ShowAppBar = true,
                HasDrawer = true,
                Orientation = "Portrait"
            };

            string multiDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("main", multiEventUiJson, testMultiEvents, multiPageMeta);

            // Verify HitTestBehavior.opaque on Column GestureDetector
            Assert(multiDart.Contains("GestureDetector(behavior: HitTestBehavior.opaque, onTap: () => onClickColumn1(context)"),
                "E2E Blocks: Column is wrapped in GestureDetector with HitTestBehavior.opaque");

            // Verify onClickColumn1 method
            Assert(multiDart.Contains("void onClickColumn1(BuildContext context)"),
                "E2E Blocks: onClickColumn1 method generated with BuildContext context parameter");
            Assert(multiDart.Contains("_textF2c6Text = 'Column Clicked!';"),
                "E2E Blocks: onClickColumn1 sets _textF2c6Text");

            // Verify onClickButton09ac with showDialog
            Assert(multiDart.Contains("void onClickButton09ac(BuildContext context)"),
                "E2E Blocks: onClickButton09ac method generated with BuildContext context parameter");
            Assert(multiDart.Contains("showDialog(") && multiDart.Contains("AlertDialog("),
                "E2E Blocks: onClickButton09ac correctly invokes showDialog with AlertDialog");

            // Verify onLongPressButton09ac with ScaffoldMessenger
            Assert(multiDart.Contains("void onLongPressButton09ac(BuildContext context)"),
                "E2E Blocks: onLongPressButton09ac method generated with BuildContext context parameter");
            Assert(multiDart.Contains("ScaffoldMessenger.of(context).showSnackBar("),
                "E2E Blocks: onLongPressButton09ac invokes ScaffoldMessenger.of(context).showSnackBar");

            // Verify Button widget wires both onClick and onLongPress
            Assert(multiDart.Contains("onClickButton09ac(context);"),
                "E2E Blocks: ElevatedButton onPressed calls onClickButton09ac(context)");
            Assert(multiDart.Contains("onLongPressButton09ac(context);"),
                "E2E Blocks: ElevatedButton onLongPress calls onLongPressButton09ac(context)");

            // Verify Slider onChanged callback
            Assert(multiDart.Contains("onChangedSliderB124(context, value)"),
                "E2E Blocks: Slider onChanged calls onChangedSliderB124(context, value)");

            // Verify dynamic state variables declared
            Assert(multiDart.Contains("String? _textF2c6Text;"),
                "E2E Blocks: Dynamic state variable _textF2c6Text declared");
            Assert(multiDart.Contains("Color? _row431cBgColor;"),
                "E2E Blocks: Dynamic state variable _row431cBgColor declared");
            Assert(multiDart.Contains("double? _column1Padding;"),
                "E2E Blocks: Dynamic state variable _column1Padding declared");
            Assert(multiDart.Contains("Color? _column1BgColor;"),
                "E2E Blocks: Dynamic state variable _column1BgColor declared");
            Assert(multiDart.Contains("MainAxisAlignment? _column1Alignment;"),
                "E2E Blocks: MainAxisAlignment state variable _column1Alignment declared");
            Assert(multiDart.Contains("mainAxisAlignment: _column1Alignment ?? MainAxisAlignment.start"),
                "E2E Blocks: Column uses dynamic _column1Alignment");
            Assert(multiDart.Contains("bool _textfieldB3efEnabled = true;"),
                "E2E Blocks: Dynamic state variable _textfieldB3efEnabled declared");
            Assert(multiDart.Contains("Color? _button09acBgColor;"),
                "E2E Blocks: Dynamic state variable _button09acBgColor declared");
            Assert(multiDart.Contains("final TextEditingController _ctrlCustomInput = TextEditingController();"),
                "E2E Blocks: TextEditingController _ctrlCustomInput auto-declared");
            Assert(multiDart.Contains("_ctrlCustomInput.dispose();"),
                "E2E Blocks: TextEditingController _ctrlCustomInput auto-disposed");

            // Verify Text widget reactive binding
            Assert(multiDart.Contains("_textF2c6Text ?? \"Original Text\""),
                "E2E Blocks: Text widget dynamically uses _textF2c6Text");

            // Verify services.dart imported
            Assert(multiDart.Contains("import 'package:flutter/services.dart';"),
                "E2E Blocks: Services import included for Portrait orientation and system features");

            // Verify companion drawer imported and wired in Scaffold
            Assert(multiDart.Contains("import 'models/drawers/main_drawer.dart';"),
                "Scaffold Drawer: models/drawers/main_drawer.dart auto-imported in main page");
            Assert(multiDart.Contains("drawer: const MainDrawer(),"),
                "Scaffold Drawer: drawer widget wired with const MainDrawer()");

            // Test Drawer component compilation (Type = "Drawer")
            var drawerMeta = new Snapcode.Models.PageMetadata
            {
                Name = "main_drawer",
                Title = "Main Navigation Drawer",
                Type = "Drawer",
                ShowAppBar = false
            };
            string drawerDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("main_drawer", multiEventUiJson, testMultiEvents, drawerMeta);
            Assert(drawerDart.Contains("class MainDrawer extends StatefulWidget"), "Drawer Architecture: MainDrawer class generated");
            Assert(drawerDart.Contains("return Drawer("), "Drawer Architecture: build method returns Drawer widget");
            Assert(!drawerDart.Contains("return Scaffold("), "Drawer Architecture: Drawer does not contain Scaffold");

            // Test FAB event generation
            var fabPageMeta = new Snapcode.Models.PageMetadata
            {
                Name = "main",
                HasFAB = true,
                FabIcon = "add"
            };
            string fabEventCode = "// EVENT_BINDING: onClick -> fab\nprint('FAB Clicked!');";
            string fabDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("main", "[]", fabEventCode, fabPageMeta);
            Assert(fabDart.Contains("FloatingActionButton("), "FAB: FloatingActionButton generated in Scaffold");
            Assert(fabDart.Contains("onClickFab(context);"), "FAB: FloatingActionButton onPressed calls onClickFab(context)");
            Assert(fabDart.Contains("void onClickFab(BuildContext context)"), "FAB: onClickFab event method generated");

            // Test dynamic page navigation import
            string navEventCode = "// EVENT_BINDING: onClick -> btn_nav\nNavigator.of(context).push(MaterialPageRoute(builder: (context) => const SettingsPage()));";
            string navDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart("main", "[]", navEventCode);
            Assert(navDart.Contains("import 'settings_page.dart';"), "Dynamic Page Navigation: settings_page.dart auto-imported from route block");

            // 15.3 Template System: Tic-Tac-Toe Template Verification
            string tempTemplateDir = Path.Combine(Path.GetTempPath(), "snapcode_test_tictactoe_" + Guid.NewGuid().ToString("N"));
            try
            {
                TemplateService.CreateTicTacToeProjectAsync(tempTemplateDir, "test_tictactoe").GetAwaiter().GetResult();

                string tttMeta = Path.Combine(tempTemplateDir, ".snapcode", "pages", "main_meta.json");
                string tttUi = Path.Combine(tempTemplateDir, ".snapcode", "pages", "main_ui.json");
                string tttMainDart = Path.Combine(tempTemplateDir, "flutter", "lib", "main.dart");
                string tttResetEvent = Path.Combine(tempTemplateDir, ".snapcode", "pages", "main_btn_reset_onClick.json");
                string tttCell0Event = Path.Combine(tempTemplateDir, ".snapcode", "pages", "main_btn_cell_0_onClick.json");
                string tttWorkspace = Path.Combine(tempTemplateDir, ".snapcode", "workspace.json");

                Assert(File.Exists(tttMeta), "Template: main_meta.json exists");
                Assert(File.Exists(tttUi), "Template: main_ui.json exists");
                Assert(File.Exists(tttMainDart), "Template: flutter/lib/main.dart compiled and exists");
                Assert(File.Exists(tttResetEvent), "Template: Reset button event workspace exists");
                Assert(File.Exists(tttCell0Event), "Template: Cell 0 event workspace exists");
                Assert(File.Exists(tttWorkspace), "Template: Overview workspace.json exists");

                if (File.Exists(tttUi))
                {
                    string uiContent = File.ReadAllText(tttUi);
                    Assert(uiContent.Contains("btn_cell_0") && uiContent.Contains("btn_cell_8"), "Template: UI contains 9 grid cell buttons");
                    Assert(uiContent.Contains("txt_status") && uiContent.Contains("txt_score_x"), "Template: UI contains scoreboard & status indicators");
                    Assert(uiContent.Contains("btn_reset"), "Template: UI contains restart button");
                }

                if (File.Exists(tttMainDart))
                {
                    string dartContent = File.ReadAllText(tttMainDart);
                    Assert(dartContent.Contains("class MainPage extends StatefulWidget"), "Template: main.dart has MainPage stateful widget");
                    Assert(dartContent.Contains("_makeMove"), "Template: main.dart contains _makeMove game logic");
                    Assert(dartContent.Contains("_checkWinner"), "Template: main.dart contains _checkWinner detection");
                    Assert(dartContent.Contains("onClickBtnCell0"), "Template: main.dart wires cell 0 click event");
                    Assert(dartContent.Contains("onClickBtnReset"), "Template: main.dart wires reset button click event");
                }
            }
            finally
            {
                if (Directory.Exists(tempTemplateDir))
                {
                    try { Directory.Delete(tempTemplateDir, true); } catch { }
                }
            }

            // 15.2 Real Project Verification & Disk Sync
            string userProjectDir = @"C:\Users\hardi\Documents\Snapcode\repos\new_project_test";
            if (Directory.Exists(userProjectDir))
            {
                string userMainUi = Path.Combine(userProjectDir, ".snapcode", "pages", "main_ui.json");
                string userMainDart = Path.Combine(userProjectDir, "flutter", "lib", "main.dart");
                if (File.Exists(userMainUi))
                {
                    string loadedUi = File.ReadAllText(userMainUi);
                    string pagesDir = Path.Combine(userProjectDir, ".snapcode", "pages");
                    var eventFiles = Directory.GetFiles(pagesDir, "main_*.event.dart");
                    var loadedEventsList = new List<string>();
                    foreach (var ef in eventFiles)
                    {
                        loadedEventsList.Add(File.ReadAllText(ef));
                    }
                    string loadedEvents = string.Join("\n\n", loadedEventsList);
                    Snapcode.Models.PageMetadata? loadedMeta = null;
                    string metaFile = Path.Combine(pagesDir, "main_meta.json");
                    if (File.Exists(metaFile))
                    {
                        string metaJson = File.ReadAllText(metaFile);
                        loadedMeta = JsonSerializer.Deserialize<Snapcode.Models.PageMetadata>(metaJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }

                    string compiledUserDart = SnapCode.Services.FlutterCompilerService.GenerateCompletePageDart(
                        "main",
                        loadedUi,
                        loadedEvents,
                        loadedMeta
                    );

                    Assert(!string.IsNullOrWhiteSpace(compiledUserDart),
                        "User Project: Compiled real project main.dart without errors");
                    Assert(compiledUserDart.Contains("onClickButton09ac(context)"),
                        "User Project: main.dart wires Button onClick to onClickButton09ac");
                    Assert(compiledUserDart.Contains("onClickColumn1(context)"),
                        "User Project: main.dart wires Column onTap to onClickColumn1");

                    // Write compiled source to disk
                    File.WriteAllText(userMainDart, compiledUserDart, System.Text.Encoding.UTF8);
                    Assert(File.Exists(userMainDart) && new FileInfo(userMainDart).Length > 100,
                        "User Project: main.dart written to disk successfully");
                }
            }

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
