using System;
using System.Text.Json;
using System.Collections.Generic;

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

            return widgetCode;
        }

        private static string CompileLayoutWidget(JsonElement node)
        {
            string type = node.TryGetProperty("type", out var typeProp) ? typeProp.GetString() ?? string.Empty : string.Empty;

            switch (type)
            {
                case "Row":
                case "Column":
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
                    return $"{type}(children: [{string.Join(", ", children)}])";
                }

                case "Container":
                {
                    if (node.TryGetProperty("children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array && childrenProp.GetArrayLength() > 0)
                    {
                        return $"Container(child: {CompileWidget(childrenProp[0])})";
                    }
                    return "Container()";
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
                    return $"Text(\"{textVal}\")";
                }

                case "Button":
                {
                    string text = node.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? string.Empty : string.Empty;
                    text = text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    return $"ElevatedButton(onPressed: () async {{{eventBlock}}}, child: Text(\"{text}\"))";
                }

                case "TextField":
                {
                    string tfId = node.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                    string hint = node.TryGetProperty("hintText", out var hintProp) ? hintProp.GetString() ?? "" : "";
                    hint = hint.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$");
                    
                    if (!string.IsNullOrEmpty(tfId))
                    {
                        return $"TextField(controller: _controllers['{tfId}'], decoration: InputDecoration(hintText: \"{hint}\"))";
                    }
                    return "TextField()";
                }

                case "Image":
                {
                    string source = node.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? string.Empty : string.Empty;
                    return $"Image.asset(\"{source}\")";
                }

                case "Checkbox":
                {
                    string cbVar = node.TryGetProperty("id", out var cbId) ? cbId.GetString() ?? "checkboxValue" : "checkboxValue";
                    return $"Checkbox(value: {cbVar}, onChanged: (bool? value) async {{{eventBlock}}})";
                }

                case "Switch":
                {
                    string swVar = node.TryGetProperty("id", out var swId) ? swId.GetString() ?? "switchValue" : "switchValue";
                    return $"Switch(value: {swVar}, onChanged: (bool value) async {{{eventBlock}}})";
                }

                case "Slider":
                {
                    double minVal = node.TryGetProperty("minValue", out var minP) && minP.TryGetDouble(out var dMin) ? dMin : 0.0;
                    double maxVal = node.TryGetProperty("maxValue", out var maxP) && maxP.TryGetDouble(out var dMax) ? dMax : 100.0;
                    double curVal = node.TryGetProperty("currentValue", out var curP) && curP.TryGetDouble(out var dCur) ? dCur : minVal;
                    if (curVal < minVal) curVal = minVal;
                    if (curVal > maxVal) curVal = maxVal;
                    return $"Slider(value: {curVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}, min: {minVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}, max: {maxVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}, onChanged: (double value) async {{{eventBlock}}})";
                }

                default:
                    return "SizedBox()";
            }
        }
    }
}
