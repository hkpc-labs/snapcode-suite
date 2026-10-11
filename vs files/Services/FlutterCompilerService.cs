using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Snapcode.Views;

namespace SnapCode.Services
{
    public class FlutterCompilerService
    {
        public static string GenerateBuildMethod(string uiJson)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(uiJson);
                string rootWidgetCode;
                
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var children = new List<string>();
                    foreach (var child in doc.RootElement.EnumerateArray())
                    {
                        children.Add(CompileWidget(child));
                    }
                    rootWidgetCode = $"Column(children: [{string.Join(", ", children)}])";
                }
                else
                {
                    rootWidgetCode = CompileWidget(doc.RootElement);
                }
                
                return $@"
Widget build(BuildContext context) {{
    return {rootWidgetCode};
}}
".Trim();
            }
            catch (Exception ex)
            {
                return $@"// Error parsing JSON: {ex.Message}
Widget build(BuildContext context) {{
    return Container();
}}";
            }
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

            // Sizing wrapper for widgets with explicit width or height (excluding Container which handles dimensions natively)
            if (type != "Container")
            {
                bool hasExplicitW = node.TryGetProperty("width", out var wProp) &&
                    !string.IsNullOrWhiteSpace(wProp.GetString()) &&
                    !string.Equals(wProp.GetString(), "Auto", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(wProp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);

                bool hasExplicitH = node.TryGetProperty("height", out var hProp) &&
                    !string.IsNullOrWhiteSpace(hProp.GetString()) &&
                    !string.Equals(hProp.GetString(), "Auto", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(hProp.GetString(), "Match Parent", StringComparison.OrdinalIgnoreCase);

                if (hasExplicitW || hasExplicitH)
                {
                    string wCode = hasExplicitW ? AlignmentHelper.ToFlutterDimensionCode(wProp.GetString(), "width") : "";
                    string hCode = hasExplicitH ? AlignmentHelper.ToFlutterDimensionCode(hProp.GetString(), "height") : "";
                    var sizedProps = new List<string>();
                    if (!string.IsNullOrWhiteSpace(wCode)) sizedProps.Add($"width: {wCode}");
                    if (!string.IsNullOrWhiteSpace(hCode)) sizedProps.Add($"height: {hCode}");
                    sizedProps.Add($"child: {widgetCode}");
                    widgetCode = $"SizedBox({string.Join(", ", sizedProps)})";
                }
            }

            // Margin wrapper for non-Container widgets (Flutter uses Padding wrapper for margin on widgets)
            if (type != "Container" && node.TryGetProperty("margin", out var marginProp) && !string.IsNullOrWhiteSpace(marginProp.GetString()))
            {
                string mVal = marginProp.GetString()!.Trim();
                if (!mVal.Equals("0", StringComparison.OrdinalIgnoreCase) && !mVal.Equals("0,0,0,0", StringComparison.OrdinalIgnoreCase))
                {
                    string edgeInsetsCode = AlignmentHelper.ToFlutterEdgeInsetsCode(mVal);
                    if (!string.IsNullOrWhiteSpace(edgeInsetsCode))
                    {
                        widgetCode = $"Padding(padding: {edgeInsetsCode}, child: {widgetCode})";
                    }
                }
            }

            return widgetCode;
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
                    return $"{type}({string.Join(", ", props)})";
                }

                case "ListView":
                {
                    var children = new List<string>();
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in childrenProp.EnumerateArray())
                        {
                            children.Add(CompileWidget(child));
                        }
                    }
                    return $"ListView(children: [{string.Join(", ", children)}])";
                }

                case "Container":
                {
                    var props = new List<string>();

                    // Width (supports raw number, dp, px, pt, %, Match Parent)
                    if (node.TryGetProperty("width", out var wProp))
                    {
                        string wCode = AlignmentHelper.ToFlutterDimensionCode(wProp.GetString(), "width");
                        if (!string.IsNullOrWhiteSpace(wCode)) props.Add($"width: {wCode}");
                    }

                    // Height (supports raw number, dp, px, pt, %, Match Parent)
                    if (node.TryGetProperty("height", out var hProp))
                    {
                        string hCode = AlignmentHelper.ToFlutterDimensionCode(hProp.GetString(), "height");
                        if (!string.IsNullOrWhiteSpace(hCode)) props.Add($"height: {hCode}");
                    }

                    // Alignment
                    string alignStr = "";
                    if (node.TryGetProperty("alignment", out var aProp) && !string.IsNullOrWhiteSpace(aProp.GetString()))
                        alignStr = aProp.GetString()!;
                    else if (node.TryGetProperty("gravity", out var gProp) && !string.IsNullOrWhiteSpace(gProp.GetString()))
                        alignStr = gProp.GetString()!;

                    if (!string.IsNullOrWhiteSpace(alignStr) && !alignStr.Equals("Default", StringComparison.OrdinalIgnoreCase) && !alignStr.Equals("TopLeft", StringComparison.OrdinalIgnoreCase))
                    {
                        string flAlign = AlignmentHelper.ToFlutterAlignmentCode(alignStr);
                        props.Add($"alignment: {flAlign}");
                    }

                    // Padding
                    if (node.TryGetProperty("padding", out var pProp) && !string.IsNullOrWhiteSpace(pProp.GetString()))
                    {
                        string pVal = pProp.GetString()!.Trim();
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
                    if (node.TryGetProperty("margin", out var mProp) && !string.IsNullOrWhiteSpace(mProp.GetString()))
                    {
                        string mVal = mProp.GetString()!.Trim();
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
                    string bgCol = node.TryGetProperty("backgroundColor", out var bgP) ? bgP.GetString() ?? "" : "";
                    string borderCol = node.TryGetProperty("borderColor", out var bcP) ? bcP.GetString() ?? "" : "";
                    string borderW = node.TryGetProperty("borderWidth", out var bwP) ? bwP.GetString() ?? "" : "";
                    string cornerR = node.TryGetProperty("cornerRadius", out var crP) ? crP.GetString() ?? "" : "";
                    bool hasShadow = node.TryGetProperty("hasShadow", out var hsP) && hsP.ValueKind == JsonValueKind.True;

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
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        return $"SingleChildScrollView(child: {CompileWidget(childrenProp[0])})";
                    }
                    return "SingleChildScrollView()";
                }

                case "HorizontalScrollView":
                {
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        return $"SingleChildScrollView(scrollDirection: Axis.horizontal, child: {CompileWidget(childrenProp[0])})";
                    }
                    return "SingleChildScrollView(scrollDirection: Axis.horizontal)";
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
                        return $"Expanded({flexParam}child: {CompileWidget(childrenProp[0])})";
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

                    string styleParam = styleProps.Count > 0 ? $", style: TextStyle({string.Join(", ", styleProps)})" : "";
                    return $"Text(\"{textVal}\"{styleParam})";
                }

                case "Button":
                {
                    string text = node.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? string.Empty : string.Empty;
                    text = text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    
                    var styleProps = new List<string>();
                    if (node.TryGetProperty("backgroundColor", out var bgProp) && !string.IsNullOrWhiteSpace(bgProp.GetString()))
                    {
                        string cCode = AlignmentHelper.ToFlutterColorCode(bgProp.GetString());
                        if (!string.IsNullOrWhiteSpace(cCode)) styleProps.Add($"backgroundColor: {cCode}");
                    }
                    if (node.TryGetProperty("textColor", out var tcProp) && !string.IsNullOrWhiteSpace(tcProp.GetString()))
                    {
                        string cCode = AlignmentHelper.ToFlutterColorCode(tcProp.GetString());
                        if (!string.IsNullOrWhiteSpace(cCode)) styleProps.Add($"foregroundColor: {cCode}");
                    }

                    double cr = node.TryGetProperty("cornerRadius", out var crP) && double.TryParse(crP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dCr) ? dCr : -1;
                    string bc = node.TryGetProperty("borderColor", out var bcP) ? bcP.GetString() ?? "" : "";
                    double bw = node.TryGetProperty("borderWidth", out var bwP) && double.TryParse(bwP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dBw) ? dBw : 0;
                    string bcCode = AlignmentHelper.ToFlutterColorCode(bc);

                    if (cr >= 0 || bw > 0)
                    {
                        var shapeParams = new List<string>();
                        if (cr >= 0)
                        {
                            string crStr = cr % 1 == 0 ? ((int)cr).ToString() : cr.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            shapeParams.Add($"borderRadius: BorderRadius.circular({crStr})");
                        }
                        if (bw > 0)
                        {
                            string bwStr = bw % 1 == 0 ? ((int)bw).ToString() : bw.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            string colParam = !string.IsNullOrWhiteSpace(bcCode) ? $"color: {bcCode}, " : "";
                            shapeParams.Add($"side: BorderSide({colParam}width: {bwStr})");
                        }
                        styleProps.Add($"shape: RoundedRectangleBorder({string.Join(", ", shapeParams)})");
                    }

                    string styleParam = styleProps.Count > 0 ? $"style: ElevatedButton.styleFrom({string.Join(", ", styleProps)}), " : "";
                    return $"ElevatedButton({styleParam}onPressed: () async {{{eventBlock}}}, child: Text(\"{text}\"))";
                }

                case "TextField":
                {
                    string tfId = node.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                    string hint = node.TryGetProperty("hintText", out var hintProp) ? hintProp.GetString() ?? "" : "";
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

                    double cr = node.TryGetProperty("cornerRadius", out var crP) && double.TryParse(crP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dCr) ? dCr : -1;
                    string bc = node.TryGetProperty("borderColor", out var bcP) ? bcP.GetString() ?? "" : "";
                    double bw = node.TryGetProperty("borderWidth", out var bwP) && double.TryParse(bwP.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dBw) ? dBw : 0;
                    string bcCode = AlignmentHelper.ToFlutterColorCode(bc);

                    if (cr > 0 || bw > 0)
                    {
                        var borderParams = new List<string>();
                        if (cr > 0)
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
                    }

                    string decorParam = decorProps.Count > 0 ? $", decoration: InputDecoration({string.Join(", ", decorProps)})" : "";
                    if (!string.IsNullOrEmpty(tfId))
                    {
                        return $"TextField(controller: _controllers['{tfId}']{decorParam})";
                    }
                    return $"TextField({(decorProps.Count > 0 ? $"decoration: InputDecoration({string.Join(", ", decorProps)})" : "")})";
                }

                case "Image":
                {
                    string source = node.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? string.Empty : string.Empty;
                    return $"Image.asset(\"{source}\")";
                }

                case "Checkbox":
                {
                    string cbVar = node.TryGetProperty("id", out var cbId) ? cbId.GetString() ?? "checkboxValue" : "checkboxValue";
                    var cbProps = new List<string> { $"value: {cbVar}" };
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        string acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                        if (!string.IsNullOrWhiteSpace(acCode)) cbProps.Add($"activeColor: {acCode}");
                    }
                    cbProps.Add($"onChanged: (bool? value) async {{{eventBlock}}}");
                    return $"Checkbox({string.Join(", ", cbProps)})";
                }

                case "Switch":
                {
                    string swVar = node.TryGetProperty("id", out var swId) ? swId.GetString() ?? "switchValue" : "switchValue";
                    var swProps = new List<string> { $"value: {swVar}" };
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        string acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                        if (!string.IsNullOrWhiteSpace(acCode)) swProps.Add($"activeColor: {acCode}");
                    }
                    swProps.Add($"onChanged: (bool value) async {{{eventBlock}}}");
                    return $"Switch({string.Join(", ", swProps)})";
                }

                case "Slider":
                {
                    double minVal = node.TryGetProperty("minValue", out var minP) && minP.TryGetDouble(out var dMin) ? dMin : 0.0;
                    double maxVal = node.TryGetProperty("maxValue", out var maxP) && maxP.TryGetDouble(out var dMax) ? dMax : 100.0;
                    double curVal = node.TryGetProperty("currentValue", out var curP) && curP.TryGetDouble(out var dCur) ? dCur : minVal;
                    if (curVal < minVal) curVal = minVal;
                    if (curVal > maxVal) curVal = maxVal;
                    
                    var slProps = new List<string>
                    {
                        $"value: {curVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                        $"min: {minVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                        $"max: {maxVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                    };
                    if (node.TryGetProperty("activeColor", out var acP) && !string.IsNullOrWhiteSpace(acP.GetString()))
                    {
                        string acCode = AlignmentHelper.ToFlutterColorCode(acP.GetString());
                        if (!string.IsNullOrWhiteSpace(acCode)) slProps.Add($"activeColor: {acCode}");
                    }
                    slProps.Add($"onChanged: (double value) async {{{eventBlock}}}");
                    return $"Slider({string.Join(", ", slProps)})";
                }

                default:
                    return "SizedBox()";
            }
        }
    }
}
