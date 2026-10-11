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
            if (childItem.Width == "Match Parent")
                h = HorizontalAlignment.Stretch;

            if (childItem.Height == "Match Parent")
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
    }
}
