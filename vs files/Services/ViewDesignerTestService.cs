using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

            // Two-Way Property Reactivity Check
            bool propChangedFired = false;
            string changedPropName = "";
            var reactiveWidget = new WidgetItem { Type = "Button", Name = "Original Name" };
            reactiveWidget.PropertyChanged += (s, e) => { propChangedFired = true; changedPropName = e.PropertyName ?? ""; };
            reactiveWidget.Name = "Updated Name";
            Assert(propChangedFired && changedPropName == "Name" && reactiveWidget.Name == "Updated Name",
                "Reactivity: PropertyChanged event fires with updated value for live reflection");

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
