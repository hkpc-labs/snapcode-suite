using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Snapcode.Views
{
    /// <summary>
    /// Utility for resolving alignment, gravity mesh, and Flutter alignment parity.
    /// Bridges Sketchware/Android gravity flags, Flutter Alignment points, and WinUI 3 layout alignments.
    /// </summary>
    public static class AlignmentHelper
    {
        /// <summary>
        /// Parses an Android/Sketchware gravity string or Flutter alignment string into WinUI 3 alignments.
        /// Supports standard flags: center, center_horizontal, center_vertical, top, bottom, left, right,
        /// piped combinations (e.g. center_horizontal|bottom), and named points (e.g. TopCenter, BottomRight).
        /// </summary>
        public static (HorizontalAlignment Horizontal, VerticalAlignment Vertical) ParseGravity(string? gravity)
        {
            if (string.IsNullOrWhiteSpace(gravity))
                return (HorizontalAlignment.Left, VerticalAlignment.Top);

            string g = gravity.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "");

            // 9-Point Named Shortcuts
            switch (g)
            {
                case "topleft": return (HorizontalAlignment.Left, VerticalAlignment.Top);
                case "topcenter": return (HorizontalAlignment.Center, VerticalAlignment.Top);
                case "topright": return (HorizontalAlignment.Right, VerticalAlignment.Top);
                case "centerleft": return (HorizontalAlignment.Left, VerticalAlignment.Center);
                case "center": return (HorizontalAlignment.Center, VerticalAlignment.Center);
                case "centerright": return (HorizontalAlignment.Right, VerticalAlignment.Center);
                case "bottomleft": return (HorizontalAlignment.Left, VerticalAlignment.Bottom);
                case "bottomcenter": return (HorizontalAlignment.Center, VerticalAlignment.Bottom);
                case "bottomright": return (HorizontalAlignment.Right, VerticalAlignment.Bottom);
                case "stretch":
                case "fill": return (HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
            }

            // Piped or combined tokens (e.g. "centerhorizontal|bottom")
            var parts = g.Split(new[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            HorizontalAlignment horiz = HorizontalAlignment.Left;
            VerticalAlignment vert = VerticalAlignment.Top;
            bool horizSet = false;
            bool vertSet = false;

            foreach (var part in parts)
            {
                string p = part.Trim();

                if (p == "center")
                {
                    if (!horizSet) horiz = HorizontalAlignment.Center;
                    if (!vertSet) vert = VerticalAlignment.Center;
                }
                else if (p == "centerhorizontal" || p == "centerh")
                {
                    horiz = HorizontalAlignment.Center;
                    horizSet = true;
                }
                else if (p == "centervertical" || p == "centerv")
                {
                    vert = VerticalAlignment.Center;
                    vertSet = true;
                }
                else if (p == "left" || p == "start")
                {
                    horiz = HorizontalAlignment.Left;
                    horizSet = true;
                }
                else if (p == "right" || p == "end")
                {
                    horiz = HorizontalAlignment.Right;
                    horizSet = true;
                }
                else if (p == "top")
                {
                    vert = VerticalAlignment.Top;
                    vertSet = true;
                }
                else if (p == "bottom")
                {
                    vert = VerticalAlignment.Bottom;
                    vertSet = true;
                }
                else if (p == "fill" || p == "stretch")
                {
                    if (!horizSet) horiz = HorizontalAlignment.Stretch;
                    if (!vertSet) vert = VerticalAlignment.Stretch;
                }
            }

            return (horiz, vert);
        }

        /// <summary>
        /// Resolves effective child alignment inside a parent Container, meshing child alignment,
        /// container gravity, and sizing constraints.
        /// </summary>
        public static (HorizontalAlignment Horizontal, VerticalAlignment Vertical) ResolveEffectiveChildAlignment(
            WidgetItem childItem,
            WidgetItem? parentContainerItem)
        {
            // 1. Base alignment from parent container gravity or alignment (defaults to TopLeft)
            var (h, v) = (HorizontalAlignment.Left, VerticalAlignment.Top);

            if (parentContainerItem != null)
            {
                if (!string.IsNullOrWhiteSpace(parentContainerItem.Gravity))
                {
                    (h, v) = ParseGravity(parentContainerItem.Gravity);
                }
                else if (!string.IsNullOrWhiteSpace(parentContainerItem.Alignment))
                {
                    (h, v) = ParseGravity(parentContainerItem.Alignment);
                }
            }

            // 2. Child alignment override (if child explicitly sets alignment)
            if (!string.IsNullOrWhiteSpace(childItem.Alignment) &&
                !childItem.Alignment.Equals("Default", StringComparison.OrdinalIgnoreCase) &&
                !childItem.Alignment.Equals("Inherit", StringComparison.OrdinalIgnoreCase))
            {
                (h, v) = ParseGravity(childItem.Alignment);
            }

            // 3. Sizing constraints precedence (Match Parent always forces Stretch)
            var wDim = ParseDimension(childItem.Width);
            if (wDim.IsMatchParent)
                h = HorizontalAlignment.Stretch;

            var hDim = ParseDimension(childItem.Height);
            if (hDim.IsMatchParent)
                v = VerticalAlignment.Stretch;

            return (h, v);
        }

        /// <summary>
        /// Maps a gravity string to Flutter flex alignments (MainAxisAlignment and CrossAxisAlignment)
        /// for a Row or Column.
        /// </summary>
        public static (FlutterMainAxisAlignment MainAxis, FlutterCrossAxisAlignment CrossAxis) MapGravityToFlex(
            string? gravity,
            Orientation orientation)
        {
            if (string.IsNullOrWhiteSpace(gravity))
            {
                return (FlutterMainAxisAlignment.Start, FlutterCrossAxisAlignment.Center);
            }

            var (h, v) = ParseGravity(gravity);

            if (orientation == Orientation.Horizontal) // Row
            {
                FlutterMainAxisAlignment mainAxis = h switch
                {
                    HorizontalAlignment.Center => FlutterMainAxisAlignment.Center,
                    HorizontalAlignment.Right => FlutterMainAxisAlignment.End,
                    _ => FlutterMainAxisAlignment.Start
                };

                FlutterCrossAxisAlignment crossAxis = v switch
                {
                    VerticalAlignment.Top => FlutterCrossAxisAlignment.Start,
                    VerticalAlignment.Bottom => FlutterCrossAxisAlignment.End,
                    VerticalAlignment.Stretch => FlutterCrossAxisAlignment.Stretch,
                    _ => FlutterCrossAxisAlignment.Center
                };

                return (mainAxis, crossAxis);
            }
            else // Column
            {
                FlutterMainAxisAlignment mainAxis = v switch
                {
                    VerticalAlignment.Center => FlutterMainAxisAlignment.Center,
                    VerticalAlignment.Bottom => FlutterMainAxisAlignment.End,
                    _ => FlutterMainAxisAlignment.Start
                };

                FlutterCrossAxisAlignment crossAxis = h switch
                {
                    HorizontalAlignment.Left => FlutterCrossAxisAlignment.Start,
                    HorizontalAlignment.Right => FlutterCrossAxisAlignment.End,
                    HorizontalAlignment.Stretch => FlutterCrossAxisAlignment.Stretch,
                    _ => FlutterCrossAxisAlignment.Center
                };

                return (mainAxis, crossAxis);
            }
        }

        /// <summary>
        /// Converts an alignment or gravity string into Flutter Alignment code representation.
        /// e.g. "Center" -> "Alignment.center", "BottomRight" -> "Alignment.bottomRight".
        /// </summary>
        public static string ToFlutterAlignmentCode(string? alignmentOrGravity)
        {
            if (string.IsNullOrWhiteSpace(alignmentOrGravity))
                return "Alignment.topLeft";

            var (h, v) = ParseGravity(alignmentOrGravity);

            if (h == HorizontalAlignment.Left && v == VerticalAlignment.Top) return "Alignment.topLeft";
            if (h == HorizontalAlignment.Center && v == VerticalAlignment.Top) return "Alignment.topCenter";
            if (h == HorizontalAlignment.Right && v == VerticalAlignment.Top) return "Alignment.topRight";
            if (h == HorizontalAlignment.Left && v == VerticalAlignment.Center) return "Alignment.centerLeft";
            if (h == HorizontalAlignment.Center && v == VerticalAlignment.Center) return "Alignment.center";
            if (h == HorizontalAlignment.Right && v == VerticalAlignment.Center) return "Alignment.centerRight";
            if (h == HorizontalAlignment.Left && v == VerticalAlignment.Bottom) return "Alignment.bottomLeft";
            if (h == HorizontalAlignment.Center && v == VerticalAlignment.Bottom) return "Alignment.bottomCenter";
            if (h == HorizontalAlignment.Right && v == VerticalAlignment.Bottom) return "Alignment.bottomRight";

            return "Alignment.topLeft";
        }

        #region Dimension and Metric Units (dp, px, pt, %, Match Parent, Auto)

        public static readonly string[] AvailableAlignments = new[]
        {
            "Default",
            "TopLeft",
            "TopCenter",
            "TopRight",
            "CenterLeft",
            "Center",
            "CenterRight",
            "BottomLeft",
            "BottomCenter",
            "BottomRight"
        };

        public static readonly string[] AvailableGravities = new[]
        {
            "Default",
            "TopLeft",
            "TopCenter",
            "TopRight",
            "CenterLeft",
            "Center",
            "CenterRight",
            "BottomLeft",
            "BottomCenter",
            "BottomRight",
            "Stretch",
            "CenterHorizontal",
            "CenterVertical"
        };

        public static readonly string[] AvailableDimensionPresets = new[]
        {
            "Auto",
            "Match Parent",
            "40dp",
            "50dp",
            "80dp",
            "100dp",
            "120dp",
            "150dp",
            "200dp",
            "250dp",
            "300dp",
            "25%",
            "50%",
            "75%",
            "100%"
        };

        /// <summary>
        /// Represents the resolved dimension from user input strings like "100", "150dp", "200px", "50%", "Match Parent", "Auto".
        /// </summary>
        public class DimensionResult
        {
            public double Value { get; set; } = double.NaN;
            public string Unit { get; set; } = "auto";
            public bool IsMatchParent { get; set; }
            public bool IsAuto { get; set; } = true;
            public bool IsPercentage { get; set; }
            public double PercentageValue { get; set; }
            public bool HasExplicitSize => !IsAuto && !IsMatchParent && (!double.IsNaN(Value) && Value > 0 || IsPercentage);
        }

        /// <summary>
        /// Parses a dimension string supporting dp, px, pt, %, Match Parent, and Auto.
        /// </summary>
        public static DimensionResult ParseDimension(string? dim, double viewportReference = 360.0)
        {
            var res = new DimensionResult();
            if (string.IsNullOrWhiteSpace(dim))
            {
                res.IsAuto = true;
                return res;
            }

            string d = dim.Trim();

            // Match Parent variants
            if (d.Equals("Match Parent", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("match_parent", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("MatchParent", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("fill", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("stretch", StringComparison.OrdinalIgnoreCase))
            {
                res.IsMatchParent = true;
                res.IsAuto = false;
                res.Unit = "match_parent";
                res.Value = double.PositiveInfinity;
                return res;
            }

            // Auto / wrap_content variants
            if (d.Equals("Auto", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("auto", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("wrap_content", StringComparison.OrdinalIgnoreCase) ||
                d.Equals("WrapContent", StringComparison.OrdinalIgnoreCase))
            {
                res.IsAuto = true;
                res.Unit = "auto";
                return res;
            }

            // Percentage (e.g. "50%")
            if (d.EndsWith("%"))
            {
                string numPart = d.Substring(0, d.Length - 1).Trim();
                if (double.TryParse(numPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pVal))
                {
                    res.IsPercentage = true;
                    res.IsAuto = false;
                    res.PercentageValue = pVal;
                    res.Unit = "%";
                    res.Value = Math.Round(viewportReference * (pVal / 100.0));
                    return res;
                }
            }

            // Strip unit suffix: dp, px, pt
            string unit = "dp";
            string numStr = d;
            if (d.EndsWith("dp", StringComparison.OrdinalIgnoreCase))
            {
                unit = "dp";
                numStr = d.Substring(0, d.Length - 2).Trim();
            }
            else if (d.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                unit = "px";
                numStr = d.Substring(0, d.Length - 2).Trim();
            }
            else if (d.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
            {
                unit = "pt";
                numStr = d.Substring(0, d.Length - 2).Trim();
            }

            if (double.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double num))
            {
                res.IsAuto = false;
                res.Unit = unit;
                res.Value = num;
                return res;
            }

            res.IsAuto = true;
            return res;
        }

        /// <summary>
        /// Emits exact Flutter Dart code representation for a dimension property.
        /// </summary>
        public static string ToFlutterDimensionCode(string? dim, string axis = "width")
        {
            var res = ParseDimension(dim);
            if (res.IsMatchParent) return "double.infinity";
            if (res.IsPercentage)
            {
                double factor = res.PercentageValue / 100.0;
                return $"MediaQuery.of(context).size.{axis} * {factor.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            if (res.HasExplicitSize && !double.IsNaN(res.Value) && res.Value > 0)
            {
                return res.Value % 1 == 0 ? ((int)res.Value).ToString() : res.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            return "";
        }

        /// <summary>
        /// Emits exact Flutter Dart EdgeInsets representation for padding or margin.
        /// </summary>
        public static string ToFlutterEdgeInsetsCode(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return "";
            var parts = val.Split(',');
            if (parts.Length == 1 && double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
            {
                if (p <= 0) return "";
                string pStr = p % 1 == 0 ? ((int)p).ToString() : p.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return $"const EdgeInsets.all({pStr})";
            }
            if (parts.Length == 2 &&
                double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double h) &&
                double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
            {
                string hStr = h % 1 == 0 ? ((int)h).ToString() : h.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string vStr = v % 1 == 0 ? ((int)v).ToString() : v.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return $"const EdgeInsets.symmetric(horizontal: {hStr}, vertical: {vStr})";
            }
            if (parts.Length == 4 &&
                double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double l) &&
                double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double t) &&
                double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double r) &&
                double.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double b))
            {
                string lStr = l % 1 == 0 ? ((int)l).ToString() : l.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string tStr = t % 1 == 0 ? ((int)t).ToString() : t.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string rStr = r % 1 == 0 ? ((int)r).ToString() : r.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string bStr = b % 1 == 0 ? ((int)b).ToString() : b.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return $"const EdgeInsets.fromLTRB({lStr}, {tStr}, {rStr}, {bStr})";
            }
            return "";
        }

        #endregion
    }
}
