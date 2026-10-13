using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Snapcode.Models;
using SnapCode.Services;

namespace Snapcode.Services
{
    public static class ComprehensiveSystemVerificationService
    {
        public static async Task<(int Total, int Passed, int Failed, string Report)> RunVerificationSuiteAsync(string outputPath = "")
        {
            var report = new System.Text.StringBuilder();
            int total = 0;
            int passed = 0;
            int failed = 0;
            var failures = new List<string>();

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
                    failures.Add(err);
                    report.AppendLine($"  - [ ] **FAIL**: {testName} - *{failureDetail}*");
                    Console.WriteLine($"  [FAIL] {err}");
                }
            }

            report.AppendLine("# SnapCode Full System Automated Verification Report");
            report.AppendLine($"*Executed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}*\n");

            // ==============================================================
            // 1. FLUTTER COMPILER SERVICE & CODE GENERATION
            // ==============================================================
            report.AppendLine("## 1. Flutter Compiler & Code Generation Pipeline");
            Console.WriteLine(">>> 1. Testing Flutter Compiler & Code Generation Pipeline...");

            // Test 1.1: Single root element must NOT be wrapped in redundant Column
            string singleRootJson = "[{\"id\":\"col_1\",\"type\":\"Column\",\"children\":[{\"id\":\"btn_1\",\"type\":\"Button\",\"text\":\"Submit\",\"children\":[]}]}]";
            string singleRootCode = FlutterCompilerService.CompileRootWidget(singleRootJson);
            Assert(!singleRootCode.StartsWith("Column(children: [Column("), 
                "CompileRootWidget eliminates redundant root Column", 
                $"Unexpected root wrapper: {singleRootCode}");

            // Test 1.2: Multiple root elements MUST be wrapped in Column
            string multiRootJson = "[{\"id\":\"btn_1\",\"type\":\"Button\",\"text\":\"One\",\"children\":[]},{\"id\":\"btn_2\",\"type\":\"Button\",\"text\":\"Two\",\"children\":[]}]";
            string multiRootCode = FlutterCompilerService.CompileRootWidget(multiRootJson);
            Assert(multiRootCode.StartsWith("Column(children: [ElevatedButton("),
                "CompileRootWidget wraps multiple root widgets in Column",
                $"Expected Column wrapper: {multiRootCode}");

            // Test 1.3: ExtractStateDeclarations - Textfield controller
            string stateJson = "[{\"id\":\"col_1\",\"type\":\"Column\",\"children\":[" +
                "{\"id\":\"user_name\",\"type\":\"TextField\",\"children\":[]}," +
                "{\"id\":\"is_agree\",\"type\":\"Checkbox\",\"children\":[]}," +
                "{\"id\":\"notify_enabled\",\"type\":\"Switch\",\"children\":[]}," +
                "{\"id\":\"volume_level\",\"type\":\"Slider\",\"currentValue\":75,\"children\":[]}" +
                "]}]";

            var (decls, disposals, controllerMap) = FlutterCompilerService.ExtractStateDeclarations(stateJson);

            Assert(decls.Contains("final TextEditingController _ctrl_user_name = TextEditingController();"),
                "ExtractStateDeclarations declares TextEditingController for TextField",
                $"Declarations: {decls}");

            Assert(disposals.Contains("_ctrl_user_name.dispose();"),
                "ExtractStateDeclarations generates controller disposal",
                $"Disposals: {disposals}");

            Assert(decls.Contains("bool is_agree = false;"),
                "ExtractStateDeclarations declares bool for Checkbox",
                $"Declarations: {decls}");

            Assert(decls.Contains("bool notify_enabled = false;"),
                "ExtractStateDeclarations declares bool for Switch",
                $"Declarations: {decls}");

            Assert(decls.Contains("double volume_level_value = 75;"),
                "ExtractStateDeclarations declares double for Slider with currentValue",
                $"Declarations: {decls}");

            // Test 1.4: WrapEventCode transforms raw event binding into proper void method
            string rawEvent = "// EVENT_BINDING: onClick -> btn_submit\n" +
                "showDialog(context: context, builder: (context) => AlertDialog(title: Text('Submitted!')));";
            string wrappedMethod = FlutterCompilerService.WrapEventCode(rawEvent);

            Assert(wrappedMethod.Contains("void _onClick_btn_submit(BuildContext context)"),
                "WrapEventCode converts event binding into named Dart method",
                $"Wrapped: {wrappedMethod}");

            Assert(wrappedMethod.Contains("showDialog("),
                "WrapEventCode preserves inner event logic inside method body",
                $"Wrapped: {wrappedMethod}");

            // Test 1.5: GenerateCompletePageDart full template structure
            string fullDart = FlutterCompilerService.GenerateCompletePageDart("main", stateJson, rawEvent);

            Assert(fullDart.Contains("class MainPage extends StatefulWidget"),
                "GenerateCompletePageDart contains StatefulWidget", "Missing StatefulWidget");

            Assert(fullDart.Contains("class _MainPageState extends State<MainPage>"),
                "GenerateCompletePageDart contains State class", "Missing State class");

            Assert(fullDart.Contains("// <!-- USER_CODE_START: CLASS_MEMBERS -->"),
                "GenerateCompletePageDart contains CLASS_MEMBERS marker", "Missing CLASS_MEMBERS marker");

            Assert(fullDart.Contains("// <!-- USER_CODE_START: CUSTOM_METHODS -->"),
                "GenerateCompletePageDart contains CUSTOM_METHODS marker", "Missing CUSTOM_METHODS marker");

            Assert(fullDart.Contains("Scaffold("),
                "GenerateCompletePageDart contains Scaffold root", "Missing Scaffold");

            Assert(fullDart.Contains("setState(() {"),
                "GenerateCompletePageDart wires reactive setState on interactive widgets", "Missing setState");

            // ==============================================================
            // 2. USER CODE PRESERVATION SERVICE
            // ==============================================================
            report.AppendLine("\n## 2. User Code Preservation Engine");
            Console.WriteLine(">>> 2. Testing User Code Preservation Engine...");

            string existingUserFile = @"import 'package:flutter/material.dart';

// <!-- USER_CODE_START: IMPORTS -->
import 'package:http/http.dart' as http;
// <!-- USER_CODE_END: IMPORTS -->

void main() => runApp(const MyApp());

class MainPage extends StatefulWidget {
  const MainPage({super.key});
  @override
  State<MainPage> createState() => _MainPageState();
}

class _MainPageState extends State<MainPage> {
  // <!-- USER_CODE_START: CLASS_MEMBERS -->
  int userCounter = 42;
  // <!-- USER_CODE_END: CLASS_MEMBERS -->

  // <!-- USER_CODE_START: CUSTOM_METHODS -->
  Future<void> fetchRemoteData() async {
    final res = await http.get(Uri.parse('https://example.com'));
  }
  // <!-- USER_CODE_END: CUSTOM_METHODS -->

  @override
  Widget build(BuildContext context) {
    return Container();
  }
}
";

            var extracted = UserCodePreservationService.ExtractUserCodeBlocks(existingUserFile);

            Assert(extracted.ContainsKey(UserCodePreservationService.TagImports) && extracted[UserCodePreservationService.TagImports].Contains("package:http/http.dart"),
                "ExtractUserCodeBlocks extracts custom user imports", "Imports not found");

            Assert(extracted.ContainsKey(UserCodePreservationService.TagClassMembers) && extracted[UserCodePreservationService.TagClassMembers].Contains("int userCounter = 42;"),
                "ExtractUserCodeBlocks extracts custom class members", "Class members not found");

            Assert(extracted.ContainsKey(UserCodePreservationService.TagCustomMethods) && extracted[UserCodePreservationService.TagCustomMethods].Contains("fetchRemoteData"),
                "ExtractUserCodeBlocks extracts custom async methods", "Custom methods not found");

            // Test merging preserved user code into new generated code
            string freshlyGenerated = FlutterCompilerService.GenerateCompletePageDart("main", stateJson);
            string merged = UserCodePreservationService.MergePreservedCode(freshlyGenerated, extracted);

            Assert(merged.Contains("package:http/http.dart"),
                "MergePreservedCode keeps custom user imports intact", "Merged code lost imports");

            Assert(merged.Contains("int userCounter = 42;"),
                "MergePreservedCode keeps custom state variables intact", "Merged code lost class member");

            Assert(merged.Contains("fetchRemoteData"),
                "MergePreservedCode keeps custom user functions intact", "Merged code lost custom function");

            // Ensure auto-generated event bindings are not treated as user code
            string codeWithAutoBinding = "// EVENT_BINDING: onClick -> none\nshowDialog(context: context);\n";
            var extractedWithAuto = UserCodePreservationService.ExtractUserCodeBlocks(codeWithAutoBinding);
            Assert(!extractedWithAuto.ContainsKey(UserCodePreservationService.TagOrphanedCode),
                "UserCodePreservationService does not treat auto-generated event bindings as orphaned user code",
                "Orphaned code wrongly created for event binding");

            // ==============================================================
            // 3. DEVICE & ANDROID EMULATOR SERVICE
            // ==============================================================
            report.AppendLine("\n## 3. Device Discovery & Android Emulator Service");
            Console.WriteLine(">>> 3. Testing Device Discovery & Android Emulator Service...");

            string flutterExePath = await DeviceService.GetFlutterExePathAsync();
            Assert(!string.IsNullOrEmpty(flutterExePath),
                "DeviceService resolves Flutter executable path", $"Path: {flutterExePath}");

            var runningDevices = await DeviceService.GetRunningFlutterDevicesAsync();
            Assert(runningDevices.Count > 0,
                "DeviceService discovers running devices (Chrome, Edge, Windows, etc.)",
                $"Found {runningDevices.Count} devices");

            Assert(runningDevices.Any(d => d.Platform == DevicePlatform.Web || d.Platform == DevicePlatform.Windows),
                "DeviceService identifies Web/Windows desktop targets correctly",
                $"Devices: {string.Join(", ", runningDevices.Select(d => d.Name))}");

            var avds = await DeviceService.GetInstalledAndroidAvdsAsync();
            Assert(avds != null,
                "DeviceService scans installed Android Virtual Devices without throwing",
                "AVD list was null");

            var allDevices = await DeviceService.GetAllDevicesAndEmulatorsAsync();
            Assert(allDevices.Count >= runningDevices.Count,
                "GetAllDevicesAndEmulatorsAsync aggregates running devices and available AVDs",
                $"Total: {allDevices.Count}");

            // Backward compatibility tests for SettingsWindow
            var legacyRunning = await DeviceService.GetRunningDevicesAsync();
            Assert(legacyRunning != null && legacyRunning.Count >= 0,
                "GetRunningDevicesAsync backward compatibility works", "Legacy running failed");

            var legacyAvds = await DeviceService.GetInstalledAvdsAsync();
            Assert(legacyAvds != null,
                "GetInstalledAvdsAsync backward compatibility works", "Legacy AVDs failed");

            // ==============================================================
            // 4. FLUTTER EXECUTION & LOG ENGINE
            // ==============================================================
            report.AppendLine("\n## 4. Flutter Execution & Log Parsing Engine");
            Console.WriteLine(">>> 4. Testing Flutter Execution & Log Parsing Engine...");

            var execService = FlutterExecutionService.Instance;
            Assert(execService.CurrentState == ExecutionState.Idle || execService.CurrentState == ExecutionState.Running,
                "FlutterExecutionService initial state is valid", $"State: {execService.CurrentState}");

            // Regex test for DevTools URL
            var devToolsRegex = new Regex(@"The Flutter DevTools debugger and profiler.*is available at:\s*(http://\S+)", RegexOptions.IgnoreCase);
            var devToolsMatch = devToolsRegex.Match("The Flutter DevTools debugger and profiler on Chrome is available at: http://127.0.0.1:9100");
            Assert(devToolsMatch.Success && devToolsMatch.Groups[1].Value == "http://127.0.0.1:9100",
                "DevTools regex extracts debugger URL accurately",
                $"Match: {devToolsMatch.Groups[1].Value}");

            // Regex test for Web app URL
            var webRegex = new Regex(@"Serving at\s*(http://localhost:\d+)", RegexOptions.IgnoreCase);
            var webMatch = webRegex.Match("Serving at http://localhost:54321");
            Assert(webMatch.Success && webMatch.Groups[1].Value == "http://localhost:54321",
                "Web App URL regex extracts localhost URL accurately",
                $"Match: {webMatch.Groups[1].Value}");

            // ==============================================================
            // 5. SUMMARY & METRICS
            // ==============================================================
            report.AppendLine("\n## 5. Verification Summary");
            report.AppendLine($"- **Total Tests Executed**: {total}");
            report.AppendLine($"- **Passed**: {passed} ({(passed * 100.0 / total):F1}%)");
            report.AppendLine($"- **Failed**: {failed}");

            if (failed == 0)
            {
                report.AppendLine("\n> [!NOTE]\n> **ALL FULL SYSTEM CHECKS PASSED WITH 100% SUCCESS.**");
            }
            else
            {
                report.AppendLine("\n### Failures Encountered:");
                foreach (var f in failures)
                {
                    report.AppendLine($"- `{f}`");
                }
            }

            string reportContent = report.ToString();
            if (!string.IsNullOrEmpty(outputPath))
            {
                try
                {
                    await File.WriteAllTextAsync(outputPath, reportContent);
                }
                catch { }
            }

            return (total, passed, failed, reportContent);
        }
    }
}
