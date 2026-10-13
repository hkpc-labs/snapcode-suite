using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Snapcode.Views;
using Snapcode.Models;

namespace SnapCode.Services
{
    public class FlutterCompilerService
    {
        public static (string declarations, string disposals, string controllerMapEntries) ExtractStateDeclarations(string uiJson)
        {
            if (string.IsNullOrWhiteSpace(uiJson) || uiJson == "{}" || uiJson == "null" || uiJson == "[]")
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(uiJson);
                var declarations = new List<string>();
                var disposals = new List<string>();
                var mapEntries = new List<string>();
                var seenIds = new HashSet<string>(StringComparer.Ordinal);

                TraverseWidgetTree(doc.RootElement, node =>
                {
                    if (!node.TryGetProperty("type", out var typeProp)) return;
                    string type = typeProp.GetString() ?? string.Empty;
                    string id = node.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;

                    if (string.IsNullOrWhiteSpace(id)) return;
                    if (!seenIds.Add(id)) return;

                    string pascalId = ToPascalCase(id);
                    string camelId = ToCamelCase(id);

                    switch (type)
                    {
                        case "TextField":
                            declarations.Add($"final TextEditingController _ctrl{pascalId} = TextEditingController();");
                            disposals.Add($"_ctrl{pascalId}.dispose();");
                            mapEntries.Add($"'{id}': _ctrl{pascalId}");
                            break;

                        case "Checkbox":
                            declarations.Add($"bool {camelId} = false;");
                            break;

                        case "Switch":
                            declarations.Add($"bool {camelId} = false;");
                            break;

                        case "Slider":
                            double minVal = node.TryGetProperty("minValue", out var minP) && minP.TryGetDouble(out var dMin) ? dMin : 0.0;
                            double curVal = minVal;
                            if (node.TryGetProperty("currentValue", out var curP))
                            {
                                if (curP.ValueKind == JsonValueKind.Number && curP.TryGetDouble(out var dVal))
                                    curVal = dVal;
                                else if (curP.ValueKind == JsonValueKind.String && double.TryParse(curP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sVal))
                                    curVal = sVal;
                            }
                            string curValStr = curVal % 1 == 0
                                ? curVal.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                                : curVal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            declarations.Add($"double {camelId}Value = {curValStr};");
                            break;
                    }
                });

                return (
                    string.Join("\n", declarations),
                    string.Join("\n", disposals),
                    string.Join(", ", mapEntries)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlutterCompilerService] Error extracting state declarations: {ex.Message}");
                return (string.Empty, string.Empty, string.Empty);
            }
        }

        public static string WrapEventCode(string rawEventCode)
        {
            if (string.IsNullOrWhiteSpace(rawEventCode))
            {
                return string.Empty;
            }

            var lines = rawEventCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new System.Text.StringBuilder();

            string currentEventType = "";
            string currentTarget = "";
            var currentBodyLines = new List<string>();
            bool inBlock = false;
            var usedMethodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void EmitMethod(string eventType, string target, List<string> body)
            {
                int start = 0;
                while (start < body.Count && string.IsNullOrWhiteSpace(body[start])) start++;
                int end = body.Count - 1;
                while (end >= start && string.IsNullOrWhiteSpace(body[end])) end--;

                if (start > end) return;

                string cleanEvt = eventType;
                if (cleanEvt.Contains('('))
                {
                    cleanEvt = cleanEvt.Substring(0, cleanEvt.IndexOf('(')).Trim();
                }
                if (cleanEvt.StartsWith("on", StringComparison.OrdinalIgnoreCase) && cleanEvt.Length > 2)
                {
                    cleanEvt = cleanEvt.Substring(2);
                }
                cleanEvt = ToPascalCase(cleanEvt);
                if (string.IsNullOrWhiteSpace(cleanEvt)) cleanEvt = "Event";

                string cleanTgt = ToPascalCase(target);
                if (string.IsNullOrWhiteSpace(cleanTgt)) cleanTgt = "None";

                string methodName = $"on{cleanEvt}{cleanTgt}";
                string baseName = methodName;
                int suffix = 2;
                while (usedMethodNames.Contains(methodName))
                {
                    methodName = $"{baseName}{suffix++}";
                }
                usedMethodNames.Add(methodName);

                bool isAsync = false;
                for (int i = start; i <= end; i++)
                {
                    if (Regex.IsMatch(body[i], @"\bawait\b"))
                    {
                        isAsync = true;
                        break;
                    }
                }

                string asyncModifier = isAsync ? " async" : "";
                sb.AppendLine($"  void {methodName}(BuildContext context){asyncModifier} {{");
                for (int i = start; i <= end; i++)
                {
                    string line = body[i];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine($"    {line.TrimStart()}");
                    }
                }
                sb.AppendLine("  }");
                sb.AppendLine();
            }

            foreach (var rawLine in lines)
            {
                string line = rawLine;
                var match = Regex.Match(line, @"^//\s*EVENT_BINDING:\s*(.+?)\s*->\s*(.+?)\s*$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (inBlock || currentBodyLines.Count > 0)
                    {
                        EmitMethod(currentEventType, currentTarget, currentBodyLines);
                        currentBodyLines.Clear();
                    }
                    currentEventType = match.Groups[1].Value.Trim();
                    currentTarget = match.Groups[2].Value.Trim();
                    inBlock = true;
                }
                else
                {
                    if (!inBlock)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            currentEventType = "Event";
                            currentTarget = "custom";
                            inBlock = true;
                            currentBodyLines.Add(line);
                        }
                    }
                    else
                    {
                        currentBodyLines.Add(line);
                    }
                }
            }

            if (currentBodyLines.Count > 0)
            {
                EmitMethod(currentEventType, currentTarget, currentBodyLines);
            }

            return sb.ToString().TrimEnd();
        }

        public static string CompileRootWidget(string uiJson)
        {
            if (string.IsNullOrWhiteSpace(uiJson) || uiJson == "{}" || uiJson == "null" || uiJson == "[]")
            {
                return "Container()";
            }

            using JsonDocument doc = JsonDocument.Parse(uiJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                int count = doc.RootElement.GetArrayLength();
                if (count == 0)
                {
                    return "Container()";
                }
                if (count == 1)
                {
                    return CompileRootElement(doc.RootElement[0]);
                }
                var children = new List<string>();
                foreach (var child in doc.RootElement.EnumerateArray())
                {
                    children.Add(CompileWidget(child));
                }
                return $"Column(children: [{string.Join(", ", children)}])";
            }
            else
            {
                return CompileRootElement(doc.RootElement);
            }
        }

        private static string CompileRootElement(JsonElement element)
        {
            if (element.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "Expanded")
            {
                // In Flutter, Expanded cannot be placed directly inside Scaffold/SafeArea without a Flex ancestor
                // If Expanded is placed as the root widget, unpack its child into a full-bleed container with Alignment
                if (element.TryGetProperty("children", out var chProp) && chProp.ValueKind == JsonValueKind.Array && chProp.GetArrayLength() > 0)
                {
                    var childNode = chProp[0];
                    string childAlign = "";
                    if (childNode.TryGetProperty("alignment", out var ap) && !string.IsNullOrWhiteSpace(ap.GetString()))
                        childAlign = ap.GetString()!;
                    else if (childNode.TryGetProperty("Alignment", out ap) && !string.IsNullOrWhiteSpace(ap.GetString()))
                        childAlign = ap.GetString()!;
                    else if (childNode.TryGetProperty("gravity", out var gp) && !string.IsNullOrWhiteSpace(gp.GetString()))
                        childAlign = gp.GetString()!;
                    else if (childNode.TryGetProperty("Gravity", out gp) && !string.IsNullOrWhiteSpace(gp.GetString()))
                        childAlign = gp.GetString()!;

                    bool matchParentW = (childNode.TryGetProperty("width", out var wp) || childNode.TryGetProperty("Width", out wp)) && string.Equals(wp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);
                    bool matchParentH = (childNode.TryGetProperty("height", out var hp) || childNode.TryGetProperty("Height", out hp)) && string.Equals(hp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);

                    string expandedBody;
                    if (matchParentW && matchParentH)
                    {
                        expandedBody = CompileWidget(childNode);
                    }
                    else
                    {
                        string alignCode = (!string.IsNullOrWhiteSpace(childAlign) && !childAlign.Equals("Default", StringComparison.OrdinalIgnoreCase))
                            ? AlignmentHelper.ToFlutterAlignmentCode(childAlign)
                            : "Alignment.center";

                        expandedBody = $"Align(alignment: {alignCode}, child: {CompileWidget(childNode)})";
                    }

                    // Check background color / decoration on the root Expanded widget!
                    string bgCol = "";
                    if (element.TryGetProperty("backgroundColor", out var bgP)) bgCol = bgP.GetString() ?? "";
                    else if (element.TryGetProperty("BackgroundColor", out bgP)) bgCol = bgP.GetString() ?? "";

                    string borderCol = "";
                    if (element.TryGetProperty("borderColor", out var bcP)) borderCol = bcP.GetString() ?? "";
                    else if (element.TryGetProperty("BorderColor", out bcP)) borderCol = bcP.GetString() ?? "";

                    string borderW = "";
                    if (element.TryGetProperty("borderWidth", out var bwP)) borderW = bwP.GetString() ?? "";
                    else if (element.TryGetProperty("BorderWidth", out bwP)) borderW = bwP.GetString() ?? "";

                    string cornerR = "";
                    if (element.TryGetProperty("cornerRadius", out var crP)) cornerR = crP.GetString() ?? "";
                    else if (element.TryGetProperty("CornerRadius", out crP)) cornerR = crP.GetString() ?? "";

                    bool hasShadow = (element.TryGetProperty("hasShadow", out var hsP) || element.TryGetProperty("HasShadow", out hsP)) && hsP.ValueKind == JsonValueKind.True;

                    string decCode = AlignmentHelper.ToFlutterBoxDecorationCode(bgCol, borderCol, borderW, cornerR, hasShadow);
                    if (!string.IsNullOrWhiteSpace(decCode))
                    {
                        return $"Container(width: double.infinity, height: double.infinity, decoration: {decCode}, child: {expandedBody})";
                    }

                    return $"SizedBox.expand(child: {expandedBody})";
                }
                return "SizedBox.expand()";
            }
            return CompileWidget(element);
        }

        public static string GenerateBuildMethod(string uiJson)
        {
            try
            {
                string rootWidgetCode = CompileRootWidget(uiJson);
                return $@"
Widget build(BuildContext context) {{
    return {rootWidgetCode};
}}".Trim();
            }
            catch (Exception ex)
            {
                return $@"// Error parsing JSON: {ex.Message}
Widget build(BuildContext context) {{
    return Container();
}}";
            }
        }

        public static string GenerateCompletePageDart(string pageName, string uiJson, string? eventCode = null, PageMetadata? metadata = null)
        {
            string rootWidget = "Container()";
            try
            {
                rootWidget = CompileRootWidget(uiJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlutterCompilerService] Error parsing UI JSON: {ex.Message}");
            }

            string pageTitle = string.IsNullOrWhiteSpace(pageName) ? "Home" : pageName;
            string effectiveTitle = !string.IsNullOrWhiteSpace(metadata?.AppBarTitle)
                ? metadata.AppBarTitle
                : (!string.IsNullOrWhiteSpace(metadata?.Title) ? metadata.Title : pageTitle);

            string className = string.Equals(pageName, "main", StringComparison.OrdinalIgnoreCase) 
                ? "MainPage" 
                : $"{ToPascalCase(pageName)}Page";

            var (declarations, disposals, _) = ExtractStateDeclarations(uiJson);

            string autoGeneratedDeclarations = "";
            if (!string.IsNullOrWhiteSpace(declarations))
            {
                var declLines = declarations.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                autoGeneratedDeclarations = $"  // Auto-generated state variables\n  {string.Join("\n  ", declLines)}\n\n";
            }
            else
            {
                autoGeneratedDeclarations = "  // Auto-generated state variables\n\n";
            }

            string autoGeneratedDisposals = "";
            if (!string.IsNullOrWhiteSpace(disposals))
            {
                var dispLines = disposals.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                autoGeneratedDisposals = $"    {string.Join("\n    ", dispLines)}\n";
            }

            string wrappedEventMethods = WrapEventCode(eventCode ?? "");
            string eventSection = "";
            if (!string.IsNullOrWhiteSpace(wrappedEventMethods))
            {
                eventSection = $"\n  // Auto-generated event handlers\n{wrappedEventMethods}\n";
            }

            // Orientation locks
            string orientationInit = "";
            string orientationDispose = "";
            bool needsServicesImport = false;
            if (metadata?.Orientation == "Landscape")
            {
                needsServicesImport = true;
                orientationInit = "    SystemChrome.setPreferredOrientations([DeviceOrientation.landscapeLeft, DeviceOrientation.landscapeRight]);\n";
                orientationDispose = "    SystemChrome.setPreferredOrientations(DeviceOrientation.values);\n";
            }
            else if (metadata?.Orientation == "Portrait")
            {
                needsServicesImport = true;
                orientationInit = "    SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp, DeviceOrientation.portraitDown]);\n";
                orientationDispose = "    SystemChrome.setPreferredOrientations(DeviceOrientation.values);\n";
            }

            string imports = "import 'package:flutter/material.dart';";
            if (needsServicesImport)
            {
                imports += "\nimport 'package:flutter/services.dart';";
            }

            // Build Scaffold
            bool showAppBar = metadata?.ShowAppBar ?? true;
            string appBarCode = "";
            if (showAppBar)
            {
                string appBarBg = !string.IsNullOrEmpty(metadata?.AppBarBackgroundColor) ? metadata.AppBarBackgroundColor : "#2196F3";
                string appBarFg = !string.IsNullOrEmpty(metadata?.AppBarForegroundColor) ? metadata.AppBarForegroundColor : "#FFFFFF";
                double elevation = metadata?.AppBarElevation ?? 4.0;
                bool centerTitle = metadata?.AppBarCenterTitle ?? false;
                string centerTitleStr = centerTitle ? "true" : "false";

                string leadingCode = "";
                if (metadata?.AppBarLeadingIcon == "menu")
                    leadingCode = "\n        leading: const Icon(Icons.menu),";
                else if (metadata?.AppBarLeadingIcon == "back")
                    leadingCode = "\n        leading: const BackButton(),";

                string actionsCode = "";
                if (metadata?.AppBarShowActions == true)
                    actionsCode = "\n        actions: const [IconButton(icon: Icon(Icons.search), onPressed: null), IconButton(icon: Icon(Icons.more_vert), onPressed: null)],";

                appBarCode = $@"appBar: AppBar(
        title: const Text('{effectiveTitle}'),
        backgroundColor: {ToFlutterColorCode(appBarBg)},
        foregroundColor: {ToFlutterColorCode(appBarFg)},
        elevation: {elevation.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)},
        centerTitle: {centerTitleStr},{leadingCode}{actionsCode}
      ),";
            }

            string scaffoldExtras = "";
            if (!string.IsNullOrEmpty(metadata?.ScaffoldBackgroundColor) && metadata.ScaffoldBackgroundColor != "#FFFFFF")
            {
                scaffoldExtras += $"\n      backgroundColor: {ToFlutterColorCode(metadata.ScaffoldBackgroundColor)},";
            }
            if (metadata != null && !metadata.ResizeToAvoidBottomInset)
            {
                scaffoldExtras += "\n      resizeToAvoidBottomInset: false,";
            }
            if (metadata?.HasDrawer == true)
            {
                scaffoldExtras += "\n      drawer: const Drawer(child: SafeArea(child: Column(children: [ListTile(leading: Icon(Icons.home), title: Text(\"Home\")), ListTile(leading: Icon(Icons.settings), title: Text(\"Settings\"))]))),";
            }
            if (metadata?.HasFAB == true)
            {
                string fabIcon = !string.IsNullOrEmpty(metadata.FabIcon) ? metadata.FabIcon : "add";
                string fabBg = !string.IsNullOrEmpty(metadata.FabBackgroundColor) ? metadata.FabBackgroundColor : "#2196F3";
                string fabTip = !string.IsNullOrEmpty(metadata.FabTooltip) ? metadata.FabTooltip : "Action";
                scaffoldExtras += $"\n      floatingActionButton: FloatingActionButton(onPressed: () {{}}, tooltip: '{fabTip}', backgroundColor: {ToFlutterColorCode(fabBg)}, child: const Icon(Icons.{fabIcon})),";
            }

            string bodyWidget = (metadata?.SafeArea ?? true)
                ? $"SafeArea(\n        child: {rootWidget},\n      )"
                : rootWidget;

            string scaffoldCode = "return Scaffold(";
            if (!string.IsNullOrEmpty(appBarCode))
            {
                scaffoldCode += $"\n      {appBarCode}";
            }
            if (!string.IsNullOrEmpty(scaffoldExtras))
            {
                scaffoldCode += scaffoldExtras;
            }
            scaffoldCode += $"\n      body: {bodyWidget},\n    );";

            if (string.Equals(pageName, "main", StringComparison.OrdinalIgnoreCase))
            {
                return $@"{imports}

// <!-- USER_CODE_START: IMPORTS -->
// <!-- USER_CODE_END: IMPORTS -->

void main() {{
  runApp(const MyApp());
}}

class MyApp extends StatelessWidget {{
  const MyApp({{super.key}});

  @override
  Widget build(BuildContext context) {{
    return MaterialApp(
      title: 'Snapcode App',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
        useMaterial3: true,
      ),
      home: const MainPage(),
    );
  }}
}}

class MainPage extends StatefulWidget {{
  const MainPage({{super.key}});

  @override
  State<MainPage> createState() => _MainPageState();
}}

class _MainPageState extends State<MainPage> {{
{autoGeneratedDeclarations}  // <!-- USER_CODE_START: CLASS_MEMBERS -->
  // <!-- USER_CODE_END: CLASS_MEMBERS -->

  @override
  void initState() {{
    super.initState();
{orientationInit}    // <!-- USER_CODE_START: INIT_STATE -->
    // <!-- USER_CODE_END: INIT_STATE -->
  }}

  @override
  void dispose() {{
{autoGeneratedDisposals}{orientationDispose}    // <!-- USER_CODE_START: DISPOSE -->
    // <!-- USER_CODE_END: DISPOSE -->
    super.dispose();
  }}
{eventSection}
  // <!-- USER_CODE_START: CUSTOM_METHODS -->
  // <!-- USER_CODE_END: CUSTOM_METHODS -->

  @override
  Widget build(BuildContext context) {{
    {scaffoldCode}
  }}
}}
";
            }
            else
            {
                return $@"{imports}

// <!-- USER_CODE_START: IMPORTS -->
// <!-- USER_CODE_END: IMPORTS -->

class {className} extends StatefulWidget {{
  const {className}({{super.key}});

  @override
  State<{className}> createState() => _{className}State();
}}

class _{className}State extends State<{className}> {{
{autoGeneratedDeclarations}  // <!-- USER_CODE_START: CLASS_MEMBERS -->
  // <!-- USER_CODE_END: CLASS_MEMBERS -->

  @override
  void initState() {{
    super.initState();
{orientationInit}    // <!-- USER_CODE_START: INIT_STATE -->
    // <!-- USER_CODE_END: INIT_STATE -->
  }}

  @override
  void dispose() {{
{autoGeneratedDisposals}{orientationDispose}    // <!-- USER_CODE_START: DISPOSE -->
    // <!-- USER_CODE_END: DISPOSE -->
    super.dispose();
  }}
{eventSection}
  // <!-- USER_CODE_START: CUSTOM_METHODS -->
  // <!-- USER_CODE_END: CUSTOM_METHODS -->

  @override
  Widget build(BuildContext context) {{
    {scaffoldCode}
  }}
}}
";
            }
        }

        public static string ToFlutterColorCode(string? colorStr)
        {
            return AlignmentHelper.ToFlutterColorCode(colorStr);
        }

        private static string SanitizeIdentifier(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "item";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }
            string result = sb.ToString();
            if (result.Length > 0 && char.IsDigit(result[0]))
            {
                result = "_" + result;
            }
            return result;
        }

        private static void TraverseWidgetTree(JsonElement element, Action<JsonElement> visitor)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    TraverseWidgetTree(item, visitor);
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                visitor(element);
                if (element.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var child in childrenProp.EnumerateArray())
                    {
                        TraverseWidgetTree(child, visitor);
                    }
                }
            }
        }

        private static string ToPascalCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "Page";
            var parts = str.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new System.Text.StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(part[0]));
                    if (part.Length > 1) sb.Append(part.Substring(1));
                }
            }
            return sb.Length > 0 ? sb.ToString() : "Page";
        }

        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "item";
            string pascal = ToPascalCase(str);
            if (string.IsNullOrEmpty(pascal)) return "item";
            return char.ToLowerInvariant(pascal[0]) + (pascal.Length > 1 ? pascal.Substring(1) : "");
        }

        private static string CompileWidget(JsonElement node)
        {
            if (!node.TryGetProperty("type", out var typeProp))
            {
                return "SizedBox()";
            }

            string type = typeProp.GetString() ?? string.Empty;
            string widgetCode = "SizedBox()";

            if (type == "Row" || type == "Column" || type == "Container" || 
                type == "ListView" || type == "ScrollView" || type == "HorizontalScrollView" || type == "Expanded")
            {
                widgetCode = CompileLayoutWidget(node);
            }
            else if (type == "Text" || type == "Button" || type == "TextField" || type == "Image" || 
                     type == "Checkbox" || type == "Switch" || type == "Slider")
            {
                widgetCode = CompileInteractiveWidget(node);
            }

            if ((node.TryGetProperty("AssetUsage", out var assetUsageProp) || node.TryGetProperty("assetUsage", out assetUsageProp)) 
                && string.Equals(assetUsageProp.GetString(), "Background", StringComparison.OrdinalIgnoreCase))
            {
                string assetSource = "";
                if (node.TryGetProperty("source", out var sourceProp)) assetSource = sourceProp.GetString() ?? "";
                else if (node.TryGetProperty("asset", out var assetProp)) assetSource = assetProp.GetString() ?? "";
                else if (node.TryGetProperty("image", out var imgProp)) assetSource = imgProp.GetString() ?? "";
                
                widgetCode = $"Container(decoration: BoxDecoration(image: DecorationImage(image: AssetImage(\"{assetSource}\"), fit: BoxFit.cover)), child: {widgetCode})";
            }

            if (type != "Container" && type != "Expanded" && 
                type != "Row" && type != "Column" && 
                type != "ScrollView" && type != "HorizontalScrollView" && type != "ListView")
            {
                bool includePadding = type != "Button" && type != "TextField";
                bool includeDecoration = type != "Button";
                widgetCode = WrapWithContainerIfNeeded(node, widgetCode, includePadding: includePadding, includeDecoration: includeDecoration);
            }

            return widgetCode;
        }

        private static string WrapWithContainerIfNeeded(JsonElement node, string innerWidget, bool includePadding = true, bool includeDecoration = true)
        {
            var containerProps = new List<string>();

            // Width (supports dp, px, %, Match Parent, etc.)
            string wStr = "";
            if (node.TryGetProperty("width", out var wProp) && !string.IsNullOrWhiteSpace(wProp.GetString()))
                wStr = wProp.GetString()!;
            else if (node.TryGetProperty("Width", out wProp) && !string.IsNullOrWhiteSpace(wProp.GetString()))
                wStr = wProp.GetString()!;

            if (!string.IsNullOrWhiteSpace(wStr))
            {
                string wCode = AlignmentHelper.ToFlutterDimensionCode(wStr, "width");
                if (!string.IsNullOrWhiteSpace(wCode)) containerProps.Add($"width: {wCode}");
            }

            // Height (supports dp, px, %, Match Parent, etc.)
            string hStr = "";
            if (node.TryGetProperty("height", out var hProp) && !string.IsNullOrWhiteSpace(hProp.GetString()))
                hStr = hProp.GetString()!;
            else if (node.TryGetProperty("Height", out hProp) && !string.IsNullOrWhiteSpace(hProp.GetString()))
                hStr = hProp.GetString()!;

            if (!string.IsNullOrWhiteSpace(hStr))
            {
                string hCode = AlignmentHelper.ToFlutterDimensionCode(hStr, "height");
                if (!string.IsNullOrWhiteSpace(hCode)) containerProps.Add($"height: {hCode}");
            }

            // Alignment
            string alignStr = "";
            if (node.TryGetProperty("alignment", out var aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                alignStr = aProp.GetString()!;
            else if (node.TryGetProperty("Alignment", out aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                alignStr = aProp.GetString()!;
            else if (node.TryGetProperty("gravity", out var gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                alignStr = gProp.GetString()!;
            else if (node.TryGetProperty("Gravity", out gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                alignStr = gProp.GetString()!;

            if (!string.IsNullOrWhiteSpace(alignStr) && 
                !alignStr.Equals("Default", StringComparison.OrdinalIgnoreCase) && 
                !alignStr.Equals("Inherit", StringComparison.OrdinalIgnoreCase) &&
                !alignStr.Equals("TopLeft", StringComparison.OrdinalIgnoreCase))
            {
                string flAlign = AlignmentHelper.ToFlutterAlignmentCode(alignStr);
                if (!string.IsNullOrWhiteSpace(flAlign) && flAlign != "Alignment.topLeft")
                {
                    containerProps.Add($"alignment: {flAlign}");
                }
            }

            // Padding (if handled at container level)
            string padStr = "";
            if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                padStr = pProp.GetString()!;
            else if (node.TryGetProperty("Padding", out pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                padStr = pProp.GetString()!;

            if (includePadding && !string.IsNullOrWhiteSpace(padStr))
            {
                string pVal = padStr.Trim();
                if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                {
                    string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                    if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                    {
                        containerProps.Add($"padding: {edgeInsetsCode}");
                    }
                }
            }

            // Margin
            string marginStr = "";
            if (node.TryGetProperty("margin", out var mProp) && !string.IsNullOrWhiteSpace(mProp.GetString()))
                marginStr = mProp.GetString()!;
            else if (node.TryGetProperty("Margin", out mProp) && !string.IsNullOrWhiteSpace(mProp.GetString()))
                marginStr = mProp.GetString()!;

            if (!string.IsNullOrWhiteSpace(marginStr))
            {
                string mVal = marginStr.Trim();
                if (!mVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !mVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                {
                    string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(mVal);
                    if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                    {
                        containerProps.Add($"margin: {edgeInsetsCode}");
                    }
                }
            }

            // Decoration (BoxDecoration: color, borderRadius, border, boxShadow)
            if (includeDecoration)
            {
                string bgCol = "";
                if (node.TryGetProperty("backgroundColor", out var bgP)) bgCol = bgP.GetString() ?? "";
                else if (node.TryGetProperty("BackgroundColor", out bgP)) bgCol = bgP.GetString() ?? "";

                string borderCol = "";
                if (node.TryGetProperty("borderColor", out var bcP)) borderCol = bcP.GetString() ?? "";
                else if (node.TryGetProperty("BorderColor", out bcP)) borderCol = bcP.GetString() ?? "";

                string borderW = "";
                if (node.TryGetProperty("borderWidth", out var bwP)) borderW = bwP.GetString() ?? "";
                else if (node.TryGetProperty("BorderWidth", out bwP)) borderW = bwP.GetString() ?? "";

                string cornerR = "";
                if (node.TryGetProperty("cornerRadius", out var crP)) cornerR = crP.GetString() ?? "";
                else if (node.TryGetProperty("CornerRadius", out crP)) cornerR = crP.GetString() ?? "";

                bool hasShadow = (node.TryGetProperty("hasShadow", out var hsP) || node.TryGetProperty("HasShadow", out hsP)) && hsP.ValueKind == JsonValueKind.True;

                string decorationCode = AlignmentHelper.ToFlutterBoxDecorationCode(bgCol, borderCol, borderW, cornerR, hasShadow);
                if (!string.IsNullOrWhiteSpace(decorationCode))
                {
                    containerProps.Add($"decoration: {decorationCode}");
                }
            }

            if (containerProps.Count > 0)
            {
                containerProps.Add($"child: {innerWidget}");
                return $"Container({string.Join(", ", containerProps)})";
            }

            return innerWidget;
        }

        private static string CompileLayoutWidget(JsonElement node)
        {
            string type = node.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty;

            switch (type)
            {
                case "Row":
                case "Column":
                {
                    var children = new List<string>();
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in childrenProp.EnumerateArray())
                        {
                            children.Add(CompileWidget(child));
                        }
                    }

                    var props = new List<string>();

                    // MainAxisAlignment
                    string mainAxis = "";
                    if (node.TryGetProperty("mainAxisAlignment", out var maProp) && !string.IsNullOrWhiteSpace(maProp.GetString()))
                    {
                        mainAxis = maProp.GetString()!;
                    }
                    else if (node.TryGetProperty("gravity", out var gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                    {
                        var (gm, _) = AlignmentHelper.MapGravityToFlex(gProp.GetString(), type == "Row" ? Orientation.Horizontal : Orientation.Vertical);
                        mainAxis = gm.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(mainAxis) && !mainAxis.Equals("Start", StringComparison.OrdinalIgnoreCase))
                    {
                        string maCamel = char.ToLowerInvariant(mainAxis[0]) + mainAxis.Substring(1);
                        props.Add($"mainAxisAlignment: MainAxisAlignment.{maCamel}");
                    }

                    // CrossAxisAlignment
                    string crossAxis = "";
                    if (node.TryGetProperty("crossAxisAlignment", out var caProp) && !string.IsNullOrWhiteSpace(caProp.GetString()))
                    {
                        crossAxis = caProp.GetString()!;
                    }
                    else if (node.TryGetProperty("gravity", out var gProp2) && !string.IsNullOrWhiteSpace(gProp2.GetString()))
                    {
                        var (_, gc) = AlignmentHelper.MapGravityToFlex(gProp2.GetString(), type == "Row" ? Orientation.Horizontal : Orientation.Vertical);
                        crossAxis = gc.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(crossAxis))
                    {
                        string caCamel = char.ToLowerInvariant(crossAxis[0]) + crossAxis.Substring(1);
                        props.Add($"crossAxisAlignment: CrossAxisAlignment.{caCamel}");
                    }

                    // MainAxisSize (when Auto is set, wrap content)
                    bool isAuto = type == "Column"
                        ? (node.TryGetProperty("height", out var hP) && string.Equals(hP.GetString(), "Auto", StringComparison.OrdinalIgnoreCase))
                        : (node.TryGetProperty("width", out var wP) && string.Equals(wP.GetString(), "Auto", StringComparison.OrdinalIgnoreCase));
                    if (isAuto)
                    {
                        props.Add("mainAxisSize: MainAxisSize.min");
                    }

                    props.Add($"children: [{string.Join(", ", children)}]");
                    string flexWidget = $"{type}({string.Join(", ", props)})";
                    return WrapWithContainerIfNeeded(node, flexWidget, includePadding: true);
                }

                case "ListView":
                {
                    var children = new List<string>();
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        foreach (var child in childrenProp.EnumerateArray())
                        {
                            children.Add(CompileWidget(child));
                        }
                    }
                    else
                    {
                        // Mock preview tiles when ListView has no children, mirroring WinUI 3 Designer
                        for (int i = 0; i < 4; i++)
                        {
                            children.Add($"ListTile(leading: const CircleAvatar(backgroundColor: Color(0xFFE3F2FD), child: Icon(Icons.person, color: Color(0xFF2196F3), size: 18)), title: Text(\"Item {i}\", style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 13)), subtitle: Text(\"Subtitle for item {i}\", style: const TextStyle(color: Color(0x99000000), fontSize: 11)), trailing: const Icon(Icons.chevron_right, size: 16, color: Color(0x66000000)))");
                        }
                    }

                    var lvProps = new List<string>();
                    string scrollDir = node.TryGetProperty("scrollDirection", out var sdProp) ? sdProp.GetString() ?? "" : "";
                    if (scrollDir.Equals("Horizontal", StringComparison.OrdinalIgnoreCase))
                    {
                        lvProps.Add("scrollDirection: Axis.horizontal");
                    }

                    // Padding for ListView
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                    {
                        string pVal = pProp.GetString()!.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode)) lvProps.Add($"padding: {edgeInsetsCode}");
                        }
                    }

                    lvProps.Add($"children: [{string.Join(", ", children)}]");
                    string listWidget = $"ListView({string.Join(", ", lvProps)})";
                    return WrapWithContainerIfNeeded(node, listWidget, includePadding: false);
                }

                case "Container":
                {
                    var props = new List<string>();

                    // Width (supports raw number, dp, px, pt, %, Match Parent)
                    string wStr = "";
                    if (node.TryGetProperty("width", out var wProp) && !string.IsNullOrWhiteSpace(wProp.GetString()))
                        wStr = wProp.GetString()!;
                    else if (node.TryGetProperty("Width", out wProp) && !string.IsNullOrWhiteSpace(wProp.GetString()))
                        wStr = wProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(wStr))
                    {
                        string wCode = AlignmentHelper.ToFlutterDimensionCode(wStr, "width");
                        if (!string.IsNullOrWhiteSpace(wCode)) props.Add($"width: {wCode}");
                    }

                    // Height (supports raw number, dp, px, pt, %, Match Parent)
                    string hStr = "";
                    if (node.TryGetProperty("height", out var hProp) && !string.IsNullOrWhiteSpace(hProp.GetString()))
                        hStr = hProp.GetString()!;
                    else if (node.TryGetProperty("Height", out hProp) && !string.IsNullOrWhiteSpace(hProp.GetString()))
                        hStr = hProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(hStr))
                    {
                        string hCode = AlignmentHelper.ToFlutterDimensionCode(hStr, "height");
                        if (!string.IsNullOrWhiteSpace(hCode)) props.Add($"height: {hCode}");
                    }

                    // Alignment
                    string alignStr = "";
                    if (node.TryGetProperty("alignment", out var aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                        alignStr = aProp.GetString()!;
                    else if (node.TryGetProperty("Alignment", out aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                        alignStr = aProp.GetString()!;
                    else if (node.TryGetProperty("gravity", out var gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                        alignStr = gProp.GetString()!;
                    else if (node.TryGetProperty("Gravity", out gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                        alignStr = gProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(alignStr) && !alignStr.Equals("Default", StringComparison.OrdinalIgnoreCase) && !alignStr.Equals("TopLeft", StringComparison.OrdinalIgnoreCase))
                    {
                        string flAlign = AlignmentHelper.ToFlutterAlignmentCode(alignStr);
                        props.Add($"alignment: {flAlign}");
                    }

                    // Padding
                    string padStr = "";
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                        padStr = pProp.GetString()!;
                    else if (node.TryGetProperty("Padding", out pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                        padStr = pProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(padStr))
                    {
                        string pVal = padStr.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                            {
                                props.Add($"padding: {edgeInsetsCode}");
                            }
                        }
                    }

                    // Margin
                    string marginStr = "";
                    if (node.TryGetProperty("margin", out var mProp) && !string.IsNullOrWhiteSpace(mProp.GetString()))
                        marginStr = mProp.GetString()!;
                    else if (node.TryGetProperty("Margin", out mProp) && !string.IsNullOrWhiteSpace(mProp.GetString()))
                        marginStr = mProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(marginStr))
                    {
                        string mVal = marginStr.Trim();
                        if (!mVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !mVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(mVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                            {
                                props.Add($"margin: {edgeInsetsCode}");
                            }
                        }
                    }

                    // Decoration (BoxDecoration: color, borderRadius, border, boxShadow)
                    string bgCol = "";
                    if (node.TryGetProperty("backgroundColor", out var bgP)) bgCol = bgP.GetString() ?? "";
                    else if (node.TryGetProperty("BackgroundColor", out bgP)) bgCol = bgP.GetString() ?? "";

                    string borderCol = "";
                    if (node.TryGetProperty("borderColor", out var bcP)) borderCol = bcP.GetString() ?? "";
                    else if (node.TryGetProperty("BorderColor", out bcP)) borderCol = bcP.GetString() ?? "";

                    string borderW = "";
                    if (node.TryGetProperty("borderWidth", out var bwP)) borderW = bwP.GetString() ?? "";
                    else if (node.TryGetProperty("BorderWidth", out bwP)) borderW = bwP.GetString() ?? "";

                    string cornerR = "";
                    if (node.TryGetProperty("cornerRadius", out var crP)) cornerR = crP.GetString() ?? "";
                    else if (node.TryGetProperty("CornerRadius", out crP)) cornerR = crP.GetString() ?? "";

                    bool hasShadow = (node.TryGetProperty("hasShadow", out var hsP) || node.TryGetProperty("HasShadow", out hsP)) && hsP.ValueKind == JsonValueKind.True;

                    string decorationCode = AlignmentHelper.ToFlutterBoxDecorationCode(bgCol, borderCol, borderW, cornerR, hasShadow);
                    if (!string.IsNullOrWhiteSpace(decorationCode))
                    {
                        props.Add($"decoration: {decorationCode}");
                    }

                    // Child
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        props.Add($"child: {CompileWidget(childrenProp[0])}");
                    }

                    return props.Count > 0 ? $"Container({string.Join(", ", props)})" : "Container()";
                }

                case "ScrollView":
                {
                    string childCode = "SizedBox()";
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        childCode = CompileWidget(childrenProp[0]);
                    }
                    var svProps = new List<string>();
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                    {
                        string pVal = pProp.GetString()!.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode)) svProps.Add($"padding: {edgeInsetsCode}");
                        }
                    }
                    svProps.Add($"child: {childCode}");
                    string scrollWidget = $"SingleChildScrollView({string.Join(", ", svProps)})";
                    return WrapWithContainerIfNeeded(node, scrollWidget, includePadding: false);
                }

                case "HorizontalScrollView":
                {
                    string childCode = "SizedBox()";
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        childCode = CompileWidget(childrenProp[0]);
                    }
                    var svProps = new List<string> { "scrollDirection: Axis.horizontal" };
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                    {
                        string pVal = pProp.GetString()!.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode)) svProps.Add($"padding: {edgeInsetsCode}");
                        }
                    }
                    svProps.Add($"child: {childCode}");
                    string scrollWidget = $"SingleChildScrollView({string.Join(", ", svProps)})";
                    return WrapWithContainerIfNeeded(node, scrollWidget, includePadding: false);
                }

                case "Expanded":
                {
                    int flexVal = 1;
                    if (node.TryGetProperty("flex", out var flexProp) && flexProp.TryGetInt32(out int parsedFlex))
                    {
                        flexVal = parsedFlex;
                    }
                    string flexParam = flexVal > 0 ? $"flex: {flexVal}, " : "";

                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        var childNode = childrenProp[0];
                        string childAlign = "";
                        if (childNode.TryGetProperty("alignment", out var ap) && !string.IsNullOrWhiteSpace(ap.GetString()))
                            childAlign = ap.GetString()!;
                        else if (childNode.TryGetProperty("Alignment", out ap) && !string.IsNullOrWhiteSpace(ap.GetString()))
                            childAlign = ap.GetString()!;
                        else if (childNode.TryGetProperty("gravity", out var gp) && !string.IsNullOrWhiteSpace(gp.GetString()))
                            childAlign = gp.GetString()!;
                        else if (childNode.TryGetProperty("Gravity", out gp) && !string.IsNullOrWhiteSpace(gp.GetString()))
                            childAlign = gp.GetString()!;

                        bool matchParentW = (childNode.TryGetProperty("width", out var wp) || childNode.TryGetProperty("Width", out wp)) && string.Equals(wp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);
                        bool matchParentH = (childNode.TryGetProperty("height", out var hp) || childNode.TryGetProperty("Height", out hp)) && string.Equals(hp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);

                        string expandedChild;
                        if (matchParentW && matchParentH)
                        {
                            expandedChild = CompileWidget(childNode);
                        }
                        else
                        {
                            string alignCode = (!string.IsNullOrWhiteSpace(childAlign) && !childAlign.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                ? AlignmentHelper.ToFlutterAlignmentCode(childAlign)
                                : "Alignment.center";

                            expandedChild = $"Align(alignment: {alignCode}, child: {CompileWidget(childNode)})";
                        }

                        // Check background color / decoration on the non-root Expanded widget
                        string bgCol = "";
                        if (node.TryGetProperty("backgroundColor", out var bgP)) bgCol = bgP.GetString() ?? "";
                        else if (node.TryGetProperty("BackgroundColor", out bgP)) bgCol = bgP.GetString() ?? "";

                        string borderCol = "";
                        if (node.TryGetProperty("borderColor", out var bcP)) borderCol = bcP.GetString() ?? "";
                        else if (node.TryGetProperty("BorderColor", out bcP)) borderCol = bcP.GetString() ?? "";

                        string borderW = "";
                        if (node.TryGetProperty("borderWidth", out var bwP)) borderW = bwP.GetString() ?? "";
                        else if (node.TryGetProperty("BorderWidth", out bwP)) borderW = bwP.GetString() ?? "";

                        string cornerR = "";
                        if (node.TryGetProperty("cornerRadius", out var crP)) cornerR = crP.GetString() ?? "";
                        else if (node.TryGetProperty("CornerRadius", out crP)) cornerR = crP.GetString() ?? "";

                        bool hasShadow = (node.TryGetProperty("hasShadow", out var hsP) || node.TryGetProperty("HasShadow", out hsP)) && hsP.ValueKind == JsonValueKind.True;

                        string decCode = AlignmentHelper.ToFlutterBoxDecorationCode(bgCol, borderCol, borderW, cornerR, hasShadow);
                        if (!string.IsNullOrWhiteSpace(decCode))
                        {
                            expandedChild = $"Container(decoration: {decCode}, child: {expandedChild})";
                        }

                        return $"Expanded({flexParam}child: {expandedChild})";
                    }
                    return $"Expanded({flexParam}child: SizedBox())";
                }

                default:
                    return "SizedBox()";
            }
        }

        private static string GetEventBinding(JsonElement node)
        {
            if (node.TryGetProperty("event", out var evProp)) return evProp.GetString() ?? "";
            if (node.TryGetProperty("action", out var acProp)) return acProp.GetString() ?? "";
            if (node.TryGetProperty("onPressed", out var opProp)) return opProp.GetString() ?? "";
            if (node.TryGetProperty("onChanged", out var ocProp)) return ocProp.GetString() ?? "";
            if (node.TryGetProperty("EVENT_BINDING", out var ebProp)) return ebProp.GetString() ?? "";
            return "";
        }

        private static string CompileInteractiveWidget(JsonElement node)
        {
            string type = node.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty;
            string evt = GetEventBinding(node);
            
            // Format event block if it exists so we don't just insert raw spaces
            string eventBlock = string.IsNullOrWhiteSpace(evt) ? "" : $"\n{evt}\n";

            switch (type)
            {
                case "Text":
                {
                    string textVal = node.TryGetProperty("text", out var tProp) ? tProp.GetString() ?? string.Empty : string.Empty;
                    textVal = textVal.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    
                    var styleProps = new List<string>();
                    if (node.TryGetProperty("textColor", out var tcProp) && !string.IsNullOrWhiteSpace(tcProp.GetString()))
                    {
                        string cCode = AlignmentHelper.ToFlutterColorCode(tcProp.GetString());
                        if (!string.IsNullOrWhiteSpace(cCode)) styleProps.Add($"color: {cCode}");
                    }
                    if (node.TryGetProperty("textSize", out var tsProp) && double.TryParse(tsProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double ts) && ts > 0)
                    {
                        string tsStr = ts % 1 == 0 ? ((int)ts).ToString() : ts.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        styleProps.Add($"fontSize: {tsStr}");
                    }
                    if (node.TryGetProperty("fontWeight", out var fwProp) && fwProp.GetString() == "Bold")
                    {
                        styleProps.Add("fontWeight: FontWeight.bold");
                    }
                    if (node.TryGetProperty("fontFamily", out var ffProp) && !string.IsNullOrWhiteSpace(ffProp.GetString()))
                    {
                        styleProps.Add($"fontFamily: \"{ffProp.GetString()}\"");
                    }

                    var textArgs = new List<string> { $"\"{textVal}\"" };
                    if (styleProps.Count > 0)
                    {
                        textArgs.Add($"style: TextStyle({string.Join(", ", styleProps)})");
                    }

                    if (node.TryGetProperty("textAlign", out var taProp) && !string.IsNullOrWhiteSpace(taProp.GetString()))
                    {
                        string ta = taProp.GetString()!.Trim().ToLowerInvariant();
                        if (ta == "center") textArgs.Add("textAlign: TextAlign.center");
                        else if (ta == "right") textArgs.Add("textAlign: TextAlign.right");
                        else if (ta == "justify") textArgs.Add("textAlign: TextAlign.justify");
                        else if (ta == "start" || ta == "left") textArgs.Add("textAlign: TextAlign.left");
                    }

                    string textWidget = $"Text({string.Join(", ", textArgs)})";
                    return textWidget;
                }

                case "Button":
                {
                    string text = node.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? string.Empty : string.Empty;
                    if (string.IsNullOrEmpty(text) && node.TryGetProperty("name", out var nProp))
                    {
                        text = nProp.GetString() ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(text)) text = "Button";
                    text = text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    
                    var styleProps = new List<string>();
                    if (node.TryGetProperty("backgroundColor", out var bgProp) && !string.IsNullOrWhiteSpace(bgProp.GetString()))
                    {
                        string cCode = AlignmentHelper.ToFlutterColorCode(bgProp.GetString());
                        if (!string.IsNullOrWhiteSpace(cCode)) styleProps.Add($"backgroundColor: {cCode}");
                    }
                    else
                    {
                        styleProps.Add("backgroundColor: const Color(0xFF2196F3)");
                    }

                    if (node.TryGetProperty("textColor", out var tcProp) && !string.IsNullOrWhiteSpace(tcProp.GetString()))
                    {
                        string cCode = AlignmentHelper.ToFlutterColorCode(tcProp.GetString());
                        if (!string.IsNullOrWhiteSpace(cCode)) styleProps.Add($"foregroundColor: {cCode}");
                    }
                    else
                    {
                        styleProps.Add("foregroundColor: Colors.white");
                    }

                    // Button Inner Padding
                    if (node.TryGetProperty("padding", out var btnPProp) && !string.IsNullOrWhiteSpace(btnPProp.GetString()))
                    {
                        string pVal = btnPProp.GetString()!.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                            {
                                styleProps.Add($"padding: {edgeInsetsCode}");
                            }
                        }
                    }

                    double cr = -1;
                    if (node.TryGetProperty("cornerRadius", out var crP) && !string.IsNullOrWhiteSpace(crP.GetString()) && double.TryParse(crP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dCr))
                    {
                        cr = dCr;
                    }

                    string bc = node.TryGetProperty("borderColor", out var bcP) ? bcP.GetString() ?? "" : "";
                    double bw = node.TryGetProperty("borderWidth", out var bwP) && double.TryParse(bwP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dBw) ? dBw : 0;
                    string bcCode = AlignmentHelper.ToFlutterColorCode(bc);

                    if (cr >= 0 || bw > 0)
                    {
                        var shapeParams = new List<string>();
                        double effectiveCr = cr >= 0 ? cr : 20.0;
                        string crStr = effectiveCr % 1 == 0 ? ((int)effectiveCr).ToString() : effectiveCr.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        shapeParams.Add($"borderRadius: BorderRadius.circular({crStr})");

                        if (bw > 0)
                        {
                            string bwStr = bw % 1 == 0 ? ((int)bw).ToString() : bw.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            string colParam = !string.IsNullOrWhiteSpace(bcCode) ? $"color: {bcCode}, " : "";
                            shapeParams.Add($"side: BorderSide({colParam}width: {bwStr})");
                        }
                        styleProps.Add($"shape: RoundedRectangleBorder({string.Join(", ", shapeParams)})");
                    }

                    if (node.TryGetProperty("hasShadow", out var hsProp) && hsProp.ValueKind == JsonValueKind.False)
                    {
                        styleProps.Add("elevation: 0");
                    }

                    var btnTextStyles = new List<string>();
                    if (node.TryGetProperty("textSize", out var btnTsProp) && double.TryParse(btnTsProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double btnTs) && btnTs > 0)
                    {
                        string tsStr = btnTs % 1 == 0 ? ((int)btnTs).ToString() : btnTs.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        btnTextStyles.Add($"fontSize: {tsStr}");
                    }
                    if (node.TryGetProperty("fontWeight", out var fwProp) && fwProp.GetString() == "Bold")
                    {
                        btnTextStyles.Add("fontWeight: FontWeight.bold");
                    }

                    string btnTextStyle = btnTextStyles.Count > 0 ? $", style: TextStyle({string.Join(", ", btnTextStyles)})" : "";
                    string styleParam = styleProps.Count > 0 ? $"style: ElevatedButton.styleFrom({string.Join(", ", styleProps)}), " : "";
                    string btnWidget = $"ElevatedButton({styleParam}onPressed: () async {{{eventBlock}}}, child: Text(\"{text}\"{btnTextStyle}))";

                    return btnWidget;
                }

                case "TextField":
                {
                    string tfId = node.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                    string hint = node.TryGetProperty("hintText", out var hintProp) ? hintProp.GetString() ?? "" : "";
                    if (string.IsNullOrEmpty(hint) && node.TryGetProperty("text", out var nameProp))
                    {
                        hint = nameProp.GetString() ?? "";
                    }
                    hint = hint.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    
                    var decorProps = new List<string>();
                    if (!string.IsNullOrEmpty(hint)) decorProps.Add($"hintText: \"{hint}\"");

                    string bgCol = node.TryGetProperty("backgroundColor", out var bgP) ? bgP.GetString() ?? "" : "";
                    string bgCode = AlignmentHelper.ToFlutterColorCode(bgCol);
                    if (!string.IsNullOrWhiteSpace(bgCode))
                    {
                        decorProps.Add("filled: true");
                        decorProps.Add($"fillColor: {bgCode}");
                    }

                    double cr = node.TryGetProperty("cornerRadius", out var crP) && double.TryParse(crP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dCr) ? dCr : 8.0;
                    string bc = node.TryGetProperty("borderColor", out var bcP) ? bcP.GetString() ?? "" : "";
                    double bw = node.TryGetProperty("borderWidth", out var bwP) && double.TryParse(bwP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dBw) ? dBw : 1.0;
                    string bcCode = AlignmentHelper.ToFlutterColorCode(bc);

                    var borderParams = new List<string>();
                    if (cr >= 0)
                    {
                        string crStr = cr % 1 == 0 ? ((int)cr).ToString() : cr.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        borderParams.Add($"borderRadius: BorderRadius.circular({crStr})");
                    }
                    if (bw > 0)
                    {
                        string bwStr = bw % 1 == 0 ? ((int)bw).ToString() : bw.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        string colParam = !string.IsNullOrWhiteSpace(bcCode) ? $"color: {bcCode}, " : "";
                        borderParams.Add($"borderSide: BorderSide({colParam}width: {bwStr})");
                    }
                    decorProps.Add($"border: OutlineInputBorder({string.Join(", ", borderParams)})");

                    // Padding for TextField (contentPadding)
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                    {
                        string pVal = pProp.GetString()!.Trim();
                        if (!pVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !pVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                        {
                            string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(pVal);
                            if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                            {
                                decorProps.Add($"contentPadding: {edgeInsetsCode}");
                            }
                        }
                    }
                    else
                    {
                        decorProps.Add("contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10)");
                    }

                    var tfProps = new List<string>();
                    if (!string.IsNullOrEmpty(tfId))
                    {
                        string ctrlVar = $"_ctrl{ToPascalCase(tfId)}";
                        tfProps.Add($"controller: {ctrlVar}");
                    }

                    if (node.TryGetProperty("obscureText", out var obsProp) && obsProp.ValueKind == JsonValueKind.True)
                    {
                        tfProps.Add("obscureText: true");
                    }

                    // Text style (textColor, textSize)
                    var tfStyleProps = new List<string>();
                    if (node.TryGetProperty("textColor", out var tcP) && !string.IsNullOrWhiteSpace(tcP.GetString()))
                    {
                        string tcCode = AlignmentHelper.ToFlutterColorCode(tcP.GetString());
                        if (!string.IsNullOrWhiteSpace(tcCode)) tfStyleProps.Add($"color: {tcCode}");
                    }
                    if (node.TryGetProperty("textSize", out var tsP) && double.TryParse(tsP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double tsVal) && tsVal > 0)
                    {
                        string tsStr = tsVal % 1 == 0 ? ((int)tsVal).ToString() : tsVal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        tfStyleProps.Add($"fontSize: {tsStr}");
                    }
                    if (tfStyleProps.Count > 0)
                    {
                        tfProps.Add($"style: TextStyle({string.Join(", ", tfStyleProps)})");
                    }

                    if (decorProps.Count > 0)
                    {
                        tfProps.Add($"decoration: InputDecoration({string.Join(", ", decorProps)})");
                    }

                    return $"TextField({string.Join(", ", tfProps)})";
                }

                case "Image":
                {
                    string asset = "";
                    if (node.TryGetProperty("assetPath", out var apProp) && !string.IsNullOrWhiteSpace(apProp.GetString()))
                        asset = apProp.GetString()!;
                    else if (node.TryGetProperty("source", out var srcProp) && !string.IsNullOrWhiteSpace(srcProp.GetString()))
                        asset = srcProp.GetString()!;
                    else if (node.TryGetProperty("asset", out var aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                        asset = aProp.GetString()!;
                    else if (node.TryGetProperty("path", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                        asset = pProp.GetString()!;

                    string fitStr = node.TryGetProperty("boxFit", out var bfProp) && !string.IsNullOrWhiteSpace(bfProp.GetString())
                        ? bfProp.GetString()!
                        : "Contain";
                    string flutterFit = "BoxFit.contain";
                    switch (fitStr.Trim().ToLowerInvariant())
                    {
                        case "cover": flutterFit = "BoxFit.cover"; break;
                        case "fill": flutterFit = "BoxFit.fill"; break;
                        case "fitwidth": flutterFit = "BoxFit.fitWidth"; break;
                        case "fitheight": flutterFit = "BoxFit.fitHeight"; break;
                        case "none": flutterFit = "BoxFit.none"; break;
                        case "scaledown": flutterFit = "BoxFit.scaleDown"; break;
                        default: flutterFit = "BoxFit.contain"; break;
                    }

                    if (string.IsNullOrWhiteSpace(asset))
                    {
                        return "Container(width: 120, height: 120, decoration: BoxDecoration(color: const Color(0xFFEEEEEE), borderRadius: BorderRadius.circular(4)), child: const Column(mainAxisAlignment: MainAxisAlignment.center, children: [Icon(Icons.image, size: 40, color: Color(0xFF9E9E9E)), SizedBox(height: 4), Text(\"Image\", style: TextStyle(color: Color(0xFF757575), fontSize: 12))]))";
                    }

                    string fileName = Path.GetFileName(asset.Trim().Replace('\\', '/'));
                    string flutterAssetPath = $"assets/{fileName}";
                    string imgWidget = $"Image.asset(\"{flutterAssetPath}\", fit: {flutterFit})";

                    if (node.TryGetProperty("cornerRadius", out var crProp) && double.TryParse(crProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cr) && cr > 0)
                    {
                        string crStr = cr % 1 == 0 ? ((int)cr).ToString() : cr.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        imgWidget = $"ClipRRect(borderRadius: BorderRadius.circular({crStr}), child: {imgWidget})";
                    }

                    return imgWidget;
                }

                case "Checkbox":
                {
                    string cbVar = node.TryGetProperty("id", out var cbIdProp) && !string.IsNullOrWhiteSpace(cbIdProp.GetString())
                        ? ToCamelCase(cbIdProp.GetString()!)
                        : "checkboxValue";
                    var cbProps = new List<string> { $"value: {cbVar}" };
                    
                    string acCode = "";
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                    }
                    else
                    {
                        acCode = AlignmentHelper.ToFlutterColorCode("#2196F3");
                    }
                    if (!string.IsNullOrWhiteSpace(acCode)) cbProps.Add($"activeColor: {acCode}");

                    cbProps.Add($"onChanged: (bool? value) {{\n                setState(() {{ {cbVar} = value ?? false; }});{(string.IsNullOrWhiteSpace(eventBlock) ? "" : eventBlock)}\n              }}");
                    string cbWidget = $"Checkbox({string.Join(", ", cbProps)})";

                    string label = "";
                    if (node.TryGetProperty("text", out var tProp) && !string.IsNullOrWhiteSpace(tProp.GetString()))
                        label = tProp.GetString()!;
                    else if (node.TryGetProperty("name", out var nProp) && !string.IsNullOrWhiteSpace(nProp.GetString()))
                        label = nProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        label = label.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                        var lblStyles = new List<string>();
                        if (node.TryGetProperty("textColor", out var tcProp) && !string.IsNullOrWhiteSpace(tcProp.GetString()))
                        {
                            string cCode = AlignmentHelper.ToFlutterColorCode(tcProp.GetString());
                            if (!string.IsNullOrWhiteSpace(cCode)) lblStyles.Add($"color: {cCode}");
                        }
                        if (node.TryGetProperty("textSize", out var tsProp) && double.TryParse(tsProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double ts) && ts > 0)
                        {
                            string tsStr = ts % 1 == 0 ? ((int)ts).ToString() : ts.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            lblStyles.Add($"fontSize: {tsStr}");
                        }
                        string textStyleArg = lblStyles.Count > 0 ? $", style: TextStyle({string.Join(", ", lblStyles)})" : "";
                        return $"Row(mainAxisSize: MainAxisSize.min, children: [{cbWidget}, const SizedBox(width: 8), Text(\"{label}\"{textStyleArg})])";
                    }

                    return cbWidget;
                }

                case "Switch":
                {
                    string swVar = node.TryGetProperty("id", out var swIdProp) && !string.IsNullOrWhiteSpace(swIdProp.GetString())
                        ? ToCamelCase(swIdProp.GetString()!)
                        : "switchValue";
                    var swProps = new List<string> { $"value: {swVar}" };
                    
                    string acCode = "";
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                    }
                    else
                    {
                        acCode = AlignmentHelper.ToFlutterColorCode("#2196F3");
                    }
                    if (!string.IsNullOrWhiteSpace(acCode)) swProps.Add($"activeColor: {acCode}");

                    swProps.Add($"onChanged: (bool value) {{\n                setState(() {{ {swVar} = value; }});{(string.IsNullOrWhiteSpace(eventBlock) ? "" : eventBlock)}\n              }}");
                    return $"Switch({string.Join(", ", swProps)})";
                }

                case "Slider":
                {
                    string slVar = node.TryGetProperty("id", out var slIdProp) && !string.IsNullOrWhiteSpace(slIdProp.GetString())
                        ? $"{ToCamelCase(slIdProp.GetString()!)}Value"
                        : "sliderValue";

                    double minVal = node.TryGetProperty("minValue", out var minP) && minP.TryGetDouble(out var dMin) ? dMin : 0.0;
                    double maxVal = node.TryGetProperty("maxValue", out var maxP) && maxP.TryGetDouble(out var dMax) ? dMax : 100.0;
                    
                    var slProps = new List<string>
                    {
                        $"value: {slVar}",
                        $"min: {minVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                        $"max: {maxVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                    };
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        string acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                        if (!string.IsNullOrWhiteSpace(acCode)) slProps.Add($"activeColor: {acCode}");
                    }
                    slProps.Add($"onChanged: (double value) {{\n                setState(() {{ {slVar} = value; }});{(string.IsNullOrWhiteSpace(eventBlock) ? "" : eventBlock)}\n              }}");
                    return $"Slider({string.Join(", ", slProps)})";
                }

                default:
                    return "SizedBox()";
            }
        }
    }
}
