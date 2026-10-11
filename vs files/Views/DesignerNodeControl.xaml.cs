using System;
using System.IO;
using System.Collections.Specialized;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Snapcode.Views
{
    public sealed partial class DesignerNodeControl : UserControl
    {
        public WidgetItem NodeItem { get; private set; }
        private NativeUIDesigner _parentDesigner;
        public DesignerNodeControl? ParentNodeControl { get; private set; }
        private Panel? _layoutContainer; // If this node is a Row/Column/Container/etc.
        private FrameworkElement? _mockElement;

        public DesignerNodeControl(WidgetItem item, NativeUIDesigner designer, DesignerNodeControl? parentNode = null)
        {
            this.InitializeComponent();
            NodeItem = item;
            _parentDesigner = designer;
            ParentNodeControl = parentNode;
            _parentDesigner.NodeRegistry[item] = this;

            RenderMockView();

            if (NodeItem.Children != null)
            {
                NodeItem.Children.CollectionChanged += Children_CollectionChanged;
            }
            NodeItem.PropertyChanged += NodeItem_PropertyChanged;
        }

        private void NodeItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RenderMockView();
            ParentNodeControl?.RefreshLayout();
            if (_parentDesigner != null && _parentDesigner.SelectedNodeControl == this)
            {
                SetSelected(true);
            }
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RenderChildren();
            RefreshLayout();
        }

        public void RefreshLayout()
        {
            if (_layoutContainer is FlutterFlexPanel flex)
            {
                var (gravMain, gravCross) = AlignmentHelper.MapGravityToFlex(NodeItem.Gravity, flex.Orientation);
                flex.MainAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.MainAxisAlignment)
                    ? FlutterFlexPanel.ParseMainAxisAlignment(NodeItem.MainAxisAlignment)
                    : gravMain;
                flex.CrossAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.CrossAxisAlignment)
                    ? FlutterFlexPanel.ParseCrossAxisAlignment(NodeItem.CrossAxisAlignment)
                    : gravCross;
                if (flex.Orientation == Orientation.Vertical)
                {
                    flex.MainAxisSize = string.Equals(NodeItem.Height, "Auto", StringComparison.OrdinalIgnoreCase)
                        ? FlutterMainAxisSize.Min
                        : FlutterMainAxisSize.Max;
                }
                else
                {
                    flex.MainAxisSize = string.Equals(NodeItem.Width, "Auto", StringComparison.OrdinalIgnoreCase)
                        ? FlutterMainAxisSize.Min
                        : FlutterMainAxisSize.Max;
                }
                flex.InvalidateMeasure();
                flex.InvalidateArrange();
            }
            else if (_layoutContainer != null)
            {
                _layoutContainer.InvalidateMeasure();
                _layoutContainer.InvalidateArrange();
            }
            this.InvalidateMeasure();
            this.InvalidateArrange();
            ParentNodeControl?.RefreshLayout();
        }

        #region Value Parsers

        private SolidColorBrush ParseColor(string? hex, Windows.UI.Color fallback = default)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                return new SolidColorBrush(fallback == default ? Colors.Transparent : fallback);

            if (hex.Equals("White", StringComparison.OrdinalIgnoreCase)) hex = "#FFFFFF";
            else if (hex.Equals("Black", StringComparison.OrdinalIgnoreCase)) hex = "#000000";
            else if (hex.Equals("Material Red", StringComparison.OrdinalIgnoreCase)) hex = "#F44336";
            else if (hex.Equals("Material Blue", StringComparison.OrdinalIgnoreCase)) hex = "#2196F3";

            try
            {
                hex = hex.Replace("#", "").Trim();
                byte a = 255;
                if (hex.Length == 8)
                {
                    a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    hex = hex.Substring(2);
                }
                if (hex.Length == 6)
                {
                    byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    return new SolidColorBrush(ColorHelper.FromArgb(a, r, g, b));
                }
            }
            catch { }

            return new SolidColorBrush(fallback == default ? Colors.Transparent : fallback);
        }

        private double ParseDouble(string? val, double def)
        {
            if (string.IsNullOrWhiteSpace(val)) return def;
            if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
                return result;
            return def;
        }

        private Thickness ParseThickness(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return new Thickness(0);
            var parts = val.Split(',');
            if (parts.Length == 1 && double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double p))
                return new Thickness(p);
            if (parts.Length == 4 &&
                double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double l) &&
                double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double t) &&
                double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double r) &&
                double.TryParse(parts[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double b))
                return new Thickness(l, t, r, b);
            return new Thickness(0);
        }

        #endregion

        public void RenderMockView()
        {
            ContentHost.Children.Clear();
            _layoutContainer = null;

            FrameworkElement? mockElement = null;
            bool hasCustomBg = !string.IsNullOrWhiteSpace(NodeItem.BackgroundColor) && !NodeItem.BackgroundColor.Equals("Transparent", StringComparison.OrdinalIgnoreCase);
            SolidColorBrush bgBrush = ParseColor(NodeItem.BackgroundColor);

            bool hasCustomText = !string.IsNullOrWhiteSpace(NodeItem.TextColor) && !NodeItem.TextColor.Equals("Transparent", StringComparison.OrdinalIgnoreCase);
            SolidColorBrush textBrush = ParseColor(NodeItem.TextColor);
            SolidColorBrush fallbackTextBrush = hasCustomText ? textBrush : new SolidColorBrush(ColorHelper.FromArgb(222, 0, 0, 0)); // Flutter black87

            switch (NodeItem.Type)
            {
                case "Row":
                {
                    var (gravMain, gravCross) = AlignmentHelper.MapGravityToFlex(NodeItem.Gravity, Orientation.Horizontal);
                    bool isAutoWidth = string.Equals(NodeItem.Width, "Auto", StringComparison.OrdinalIgnoreCase);
                    var flex = new FlutterFlexPanel
                    {
                        Orientation = Orientation.Horizontal,
                        MainAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.MainAxisAlignment)
                            ? FlutterFlexPanel.ParseMainAxisAlignment(NodeItem.MainAxisAlignment)
                            : gravMain,
                        CrossAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.CrossAxisAlignment)
                            ? FlutterFlexPanel.ParseCrossAxisAlignment(NodeItem.CrossAxisAlignment)
                            : gravCross,
                        MainAxisSize = isAutoWidth ? FlutterMainAxisSize.Min : FlutterMainAxisSize.Max,
                        MinHeight = 44,
                        MinWidth = 44
                    };
                    var border = new Border
                    {
                        Child = flex,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    _layoutContainer = flex;
                    mockElement = border;
                    break;
                }

                case "Column":
                {
                    var (gravMain, gravCross) = AlignmentHelper.MapGravityToFlex(NodeItem.Gravity, Orientation.Vertical);
                    bool isAutoHeight = string.Equals(NodeItem.Height, "Auto", StringComparison.OrdinalIgnoreCase);
                    var flex = new FlutterFlexPanel
                    {
                        Orientation = Orientation.Vertical,
                        MainAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.MainAxisAlignment)
                            ? FlutterFlexPanel.ParseMainAxisAlignment(NodeItem.MainAxisAlignment)
                            : gravMain,
                        CrossAxisAlignment = !string.IsNullOrWhiteSpace(NodeItem.CrossAxisAlignment)
                            ? FlutterFlexPanel.ParseCrossAxisAlignment(NodeItem.CrossAxisAlignment)
                            : gravCross,
                        MainAxisSize = isAutoHeight ? FlutterMainAxisSize.Min : FlutterMainAxisSize.Max,
                        MinHeight = 44,
                        MinWidth = 44
                    };
                    var border = new Border
                    {
                        Child = flex,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    _layoutContainer = flex;
                    mockElement = border;
                    break;
                }

                case "Expanded":
                {
                    int flexFactor = NodeItem.Flex > 0 ? NodeItem.Flex : 1;
                    FlutterFlexPanel.SetFlex(this, flexFactor);

                    var expBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(120, 33, 150, 243)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(ColorHelper.FromArgb(14, 33, 150, 243)),
                        MinHeight = 36,
                        MinWidth = 36,
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    var expGrid = new Grid();
                    expBorder.Child = expGrid;
                    _layoutContainer = expGrid;
                    mockElement = expBorder;
                    break;
                }

                case "Container":
                {
                    var contBorder = new Border
                    {
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding),
                        MinHeight = 40,
                        MinWidth = 40
                    };
                    var contGrid = new Grid();
                    contBorder.Child = contGrid;
                    _layoutContainer = contGrid;
                    mockElement = contBorder;
                    break;
                }

                case "Button":
                {
                    string btnContent = string.IsNullOrWhiteSpace(NodeItem.Name) ? "Button" : NodeItem.Name;
                    var btnText = new TextBlock
                    {
                        Text = btnContent,
                        Foreground = hasCustomText ? textBrush : new SolidColorBrush(Colors.White),
                        FontSize = ParseDouble(NodeItem.TextSize, 14),
                        FontWeight = NodeItem.FontWeight == "Bold" ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsHitTestVisible = false
                    };
                    var btnBorder = new Border
                    {
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(ColorHelper.FromArgb(255, 33, 150, 243)), // #2196F3
                        CornerRadius = new CornerRadius(ParseDouble(NodeItem.CornerRadius, 20)),
                        MinHeight = 40,
                        MinWidth = 64,
                        Padding = string.IsNullOrWhiteSpace(NodeItem.Padding) ? new Thickness(16, 8, 16, 8) : ParseThickness(NodeItem.Padding),
                        Child = btnText
                    };
                    mockElement = btnBorder;
                    break;
                }

                case "Text":
                {
                    string txtContent = string.IsNullOrEmpty(NodeItem.Name) ? "Text" : NodeItem.Name;
                    var txtBlock = new TextBlock
                    {
                        Text = txtContent,
                        Foreground = fallbackTextBrush,
                        FontSize = ParseDouble(NodeItem.TextSize, 14),
                        TextWrapping = TextWrapping.Wrap,
                        IsHitTestVisible = false
                    };
                    if (NodeItem.FontWeight == "Bold") txtBlock.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
                    if (!string.IsNullOrWhiteSpace(NodeItem.FontFamily)) txtBlock.FontFamily = new FontFamily(NodeItem.FontFamily);

                    if (NodeItem.TextAlign == "Center") txtBlock.TextAlignment = TextAlignment.Center;
                    else if (NodeItem.TextAlign == "Right") txtBlock.TextAlignment = TextAlignment.Right;
                    else if (NodeItem.TextAlign == "Justify") txtBlock.TextAlignment = TextAlignment.Justify;
                    else txtBlock.TextAlignment = TextAlignment.Left;

                    mockElement = new Border
                    {
                        Child = txtBlock,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    break;
                }

                case "TextField":
                {
                    var tfGrid = new Grid { VerticalAlignment = VerticalAlignment.Center };
                    string placeholder = !string.IsNullOrEmpty(NodeItem.HintText) ? NodeItem.HintText : (string.IsNullOrEmpty(NodeItem.Name) ? "Enter text" : NodeItem.Name);
                    string displayText = NodeItem.ObscureText ? "••••••••" : placeholder;

                    var tfText = new TextBlock
                    {
                        Text = displayText,
                        Foreground = NodeItem.ObscureText
                            ? (hasCustomText ? textBrush : new SolidColorBrush(Colors.Black))
                            : (hasCustomText ? textBrush : new SolidColorBrush(ColorHelper.FromArgb(180, 117, 117, 117))),
                        FontSize = ParseDouble(NodeItem.TextSize, 14),
                        VerticalAlignment = VerticalAlignment.Center,
                        IsHitTestVisible = false
                    };
                    tfGrid.Children.Add(tfText);

                    var tfBorder = new Border
                    {
                        Child = tfGrid,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(ColorHelper.FromArgb(255, 245, 245, 245)),
                        BorderBrush = !string.IsNullOrWhiteSpace(NodeItem.BorderColor) ? ParseColor(NodeItem.BorderColor) : new SolidColorBrush(ColorHelper.FromArgb(255, 200, 200, 200)),
                        BorderThickness = new Thickness(ParseDouble(NodeItem.BorderWidth, 1.5)),
                        CornerRadius = new CornerRadius(ParseDouble(NodeItem.CornerRadius, 8)),
                        MinHeight = 44,
                        MinWidth = 120,
                        Padding = string.IsNullOrWhiteSpace(NodeItem.Padding) ? new Thickness(14, 10, 14, 10) : ParseThickness(NodeItem.Padding)
                    };
                    mockElement = tfBorder;
                    break;
                }

                case "Image":
                {
                    mockElement = CreateImageMockElement(hasCustomBg, bgBrush);
                    break;
                }

                case "Checkbox":
                {
                    var cbPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
                    var activeColorBrush = ParseColor(NodeItem.ActiveColor, ColorHelper.FromArgb(255, 33, 150, 243));

                    var boxBorder = new Border
                    {
                        Width = 18,
                        Height = 18,
                        CornerRadius = new CornerRadius(3),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    if (NodeItem.Checked)
                    {
                        boxBorder.Background = activeColorBrush;
                        boxBorder.BorderThickness = new Thickness(0);
                        boxBorder.Child = new FontIcon
                        {
                            Glyph = "\uE73E",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Colors.White),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else
                    {
                        boxBorder.Background = new SolidColorBrush(Colors.Transparent);
                        boxBorder.BorderThickness = new Thickness(2);
                        boxBorder.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 117, 117, 117));
                    }

                    string cbLabel = string.IsNullOrWhiteSpace(NodeItem.Name) ? "Checkbox" : NodeItem.Name;
                    var cbText = new TextBlock
                    {
                        Text = cbLabel,
                        Foreground = fallbackTextBrush,
                        FontSize = ParseDouble(NodeItem.TextSize, 14),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    cbPanel.Children.Add(boxBorder);
                    cbPanel.Children.Add(cbText);
                    mockElement = new Border { Child = cbPanel, Padding = ParseThickness(NodeItem.Padding) };
                    break;
                }

                case "Switch":
                {
                    var swActiveBrush = ParseColor(NodeItem.ActiveColor, ColorHelper.FromArgb(255, 33, 150, 243));
                    var trackGrid = new Grid { Width = 52, Height = 32, VerticalAlignment = VerticalAlignment.Center };

                    var trackBorder = new Border
                    {
                        CornerRadius = new CornerRadius(16),
                        Background = NodeItem.Checked ? swActiveBrush : new SolidColorBrush(ColorHelper.FromArgb(255, 224, 224, 224)),
                        BorderThickness = NodeItem.Checked ? new Thickness(0) : new Thickness(1.5),
                        BorderBrush = NodeItem.Checked ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(ColorHelper.FromArgb(255, 189, 189, 189))
                    };

                    var thumb = new Border
                    {
                        Width = 24,
                        Height = 24,
                        CornerRadius = new CornerRadius(12),
                        Background = NodeItem.Checked ? new SolidColorBrush(Colors.White) : new SolidColorBrush(ColorHelper.FromArgb(255, 117, 117, 117)),
                        HorizontalAlignment = NodeItem.Checked ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4)
                    };

                    trackGrid.Children.Add(trackBorder);
                    trackGrid.Children.Add(thumb);
                    mockElement = new Border { Child = trackGrid, Padding = ParseThickness(NodeItem.Padding) };
                    break;
                }

                case "Slider":
                {
                    double min = NodeItem.MinValue;
                    double max = NodeItem.MaxValue;
                    if (min >= max) max = min + 1.0;
                    double val = NodeItem.CurrentValue;
                    if (val < min) val = min;
                    if (val > max) val = max;

                    var slActiveBrush = ParseColor(NodeItem.ActiveColor, ColorHelper.FromArgb(255, 33, 150, 243));
                    var sliderStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
                    var trackGrid = new Grid { Width = 140, Height = 24, VerticalAlignment = VerticalAlignment.Center };

                    var inactiveTrack = new Border
                    {
                        Height = 4,
                        CornerRadius = new CornerRadius(2),
                        Background = new SolidColorBrush(ColorHelper.FromArgb(60, 33, 150, 243)),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    double ratio = (max > min) ? Math.Max(0, Math.Min(1, (val - min) / (max - min))) : 0.5;
                    var activeTrack = new Border
                    {
                        Height = 4,
                        Width = 140 * ratio,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        CornerRadius = new CornerRadius(2),
                        Background = slActiveBrush,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var slThumb = new Border
                    {
                        Width = 18,
                        Height = 18,
                        CornerRadius = new CornerRadius(9),
                        Background = slActiveBrush,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(Math.Max(0, (140 * ratio) - 9), 0, 0, 0)
                    };

                    trackGrid.Children.Add(inactiveTrack);
                    trackGrid.Children.Add(activeTrack);
                    trackGrid.Children.Add(slThumb);

                    var valBadge = new Border
                    {
                        Background = new SolidColorBrush(ColorHelper.FromArgb(24, 0, 0, 0)),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(4, 2, 4, 2),
                        Child = new TextBlock { Text = $"{val:0.#}", FontSize = 11, Foreground = fallbackTextBrush }
                    };

                    sliderStack.Children.Add(trackGrid);
                    sliderStack.Children.Add(valBadge);
                    mockElement = new Border { Child = sliderStack, Padding = ParseThickness(NodeItem.Padding) };
                    break;
                }

                case "ListView":
                {
                    bool isHoriz = NodeItem.ScrollDirection == "Horizontal";
                    var listScroll = new ScrollViewer
                    {
                        HorizontalScrollMode = isHoriz ? ScrollMode.Enabled : ScrollMode.Disabled,
                        VerticalScrollMode = !isHoriz ? ScrollMode.Enabled : ScrollMode.Disabled,
                        HorizontalScrollBarVisibility = isHoriz ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
                        VerticalScrollBarVisibility = !isHoriz ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
                        MinHeight = isHoriz ? 80 : 160,
                        MinWidth = 120,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent)
                    };

                    var listPanel = new StackPanel { Orientation = isHoriz ? Orientation.Horizontal : Orientation.Vertical, Spacing = 4 };
                    for (int i = 1; i <= 3; i++)
                    {
                        var tileBorder = new Border
                        {
                            Background = new SolidColorBrush(ColorHelper.FromArgb(12, 0, 0, 0)),
                            CornerRadius = new CornerRadius(8),
                            Padding = new Thickness(12, 8, 12, 8),
                            Margin = new Thickness(2),
                            MinWidth = isHoriz ? 120 : double.NaN
                        };

                        var tileGrid = new Grid();
                        tileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
                        tileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        tileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });

                        var avatar = new Border
                        {
                            Width = 32,
                            Height = 32,
                            CornerRadius = new CornerRadius(16),
                            Background = new SolidColorBrush(ColorHelper.FromArgb(255, 227, 242, 253)),
                            Child = new FontIcon { Glyph = "\uE77B", FontSize = 14, Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 33, 150, 243)) }
                        };
                        Grid.SetColumn(avatar, 0);

                        var textCol = new StackPanel { Margin = new Thickness(8, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
                        textCol.Children.Add(new TextBlock { Text = $"Item {i}", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, FontSize = 13 });
                        textCol.Children.Add(new TextBlock { Text = $"Subtitle for item {i}", Foreground = new SolidColorBrush(ColorHelper.FromArgb(160, 0, 0, 0)), FontSize = 11 });
                        Grid.SetColumn(textCol, 1);

                        var chevron = new FontIcon { Glyph = "\uE76C", FontSize = 12, Foreground = new SolidColorBrush(ColorHelper.FromArgb(120, 0, 0, 0)), VerticalAlignment = VerticalAlignment.Center };
                        Grid.SetColumn(chevron, 2);

                        tileGrid.Children.Add(avatar);
                        tileGrid.Children.Add(textCol);
                        tileGrid.Children.Add(chevron);

                        tileBorder.Child = tileGrid;
                        listPanel.Children.Add(tileBorder);
                    }
                    listScroll.Content = listPanel;
                    mockElement = listScroll;
                    break;
                }

                case "ScrollView":
                {
                    var vertScroll = new ScrollViewer
                    {
                        HorizontalScrollMode = ScrollMode.Disabled,
                        VerticalScrollMode = ScrollMode.Enabled,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        MinHeight = 80,
                        MinWidth = 60,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    var vertContent = new Grid();
                    vertScroll.Content = vertContent;
                    _layoutContainer = vertContent;
                    mockElement = vertScroll;
                    break;
                }

                case "HorizontalScrollView":
                {
                    var horizScroll = new ScrollViewer
                    {
                        HorizontalScrollMode = ScrollMode.Enabled,
                        VerticalScrollMode = ScrollMode.Disabled,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        MinHeight = 60,
                        MinWidth = 80,
                        Background = hasCustomBg ? bgBrush : new SolidColorBrush(Colors.Transparent),
                        Padding = ParseThickness(NodeItem.Padding)
                    };
                    var horizContent = new Grid();
                    horizScroll.Content = horizContent;
                    _layoutContainer = horizContent;
                    mockElement = horizScroll;
                    break;
                }

                default:
                    mockElement = new TextBlock { Text = $"Widget: {NodeItem.Type}", Foreground = fallbackTextBrush, IsHitTestVisible = false };
                    break;
            }

            if (mockElement != null)
            {
                // Box Model Sizing & Alignment
                mockElement.Margin = ParseThickness(NodeItem.Margin);

                // Width Handling (dp, px, pt, %, Match Parent, Auto)
                var widthDim = AlignmentHelper.ParseDimension(NodeItem.Width, 360.0);
                if (widthDim.IsMatchParent)
                {
                    mockElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    mockElement.Width = double.NaN;
                    this.HorizontalAlignment = HorizontalAlignment.Stretch;
                    this.Width = double.NaN;
                }
                else if (widthDim.HasExplicitSize)
                {
                    mockElement.Width = widthDim.Value;
                    mockElement.HorizontalAlignment = HorizontalAlignment.Center;
                    this.Width = widthDim.Value;
                }
                else
                {
                    mockElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    mockElement.Width = double.NaN;
                    this.Width = double.NaN;
                }

                // Height Handling (dp, px, pt, %, Match Parent, Auto)
                var heightDim = AlignmentHelper.ParseDimension(NodeItem.Height, 640.0);
                if (heightDim.IsMatchParent)
                {
                    mockElement.VerticalAlignment = VerticalAlignment.Stretch;
                    mockElement.Height = double.NaN;
                    this.VerticalAlignment = VerticalAlignment.Stretch;
                    this.Height = double.NaN;
                }
                else if (heightDim.HasExplicitSize)
                {
                    mockElement.Height = heightDim.Value;
                    mockElement.VerticalAlignment = VerticalAlignment.Center;
                    this.Height = heightDim.Value;
                }
                else
                {
                    mockElement.VerticalAlignment = VerticalAlignment.Stretch;
                    mockElement.Height = double.NaN;
                    this.Height = double.NaN;
                }

                // Apply CornerRadius
                double cr = ParseDouble(NodeItem.CornerRadius, 0);
                if (cr > 0)
                {
                    if (mockElement is Border bdr) bdr.CornerRadius = new CornerRadius(cr);
                    else if (mockElement is Control ctrl) ctrl.CornerRadius = new CornerRadius(cr);
                    else if (mockElement is Grid gr) gr.CornerRadius = new CornerRadius(cr);
                    else if (mockElement is StackPanel sp) sp.CornerRadius = new CornerRadius(cr);
                }

                // Apply BorderWidth and BorderColor
                double bw = ParseDouble(NodeItem.BorderWidth, 0);
                if (bw > 0)
                {
                    var borderColor = ParseColor(NodeItem.BorderColor, Colors.Black);
                    if (mockElement is Border bdr3)
                    {
                        bdr3.BorderThickness = new Thickness(bw);
                        bdr3.BorderBrush = borderColor;
                    }
                    else if (mockElement is Control ctrl3)
                    {
                        ctrl3.BorderThickness = new Thickness(bw);
                        ctrl3.BorderBrush = borderColor;
                    }
                    else if (mockElement is Grid gr3)
                    {
                        gr3.BorderThickness = new Thickness(bw);
                        gr3.BorderBrush = borderColor;
                    }
                }

                // Apply Elevation Shadow
                if (NodeItem.HasShadow)
                {
                    mockElement.Shadow = new ThemeShadow();
                    mockElement.Translation = new System.Numerics.Vector3(0, 0, 12);
                }

                ContentHost.Children.Add(mockElement);
                _mockElement = mockElement;

                // Coordinate with parent container for WinUI 3 layout parity
                if (ParentNodeControl?._layoutContainer is FlutterFlexPanel flexParent)
                {
                    bool isFlexChild = NodeItem.Type == "Expanded";
                    if (flexParent.Orientation == Orientation.Vertical)
                    {
                        if (isFlexChild)
                        {
                            this.Height = double.NaN;
                            this.VerticalAlignment = VerticalAlignment.Stretch;
                        }
                        else if (!heightDim.HasExplicitSize && !heightDim.IsMatchParent)
                        {
                            this.VerticalAlignment = VerticalAlignment.Center;
                        }

                        if (!widthDim.HasExplicitSize && !widthDim.IsMatchParent)
                        {
                            this.HorizontalAlignment = flexParent.CrossAxisAlignment == FlutterCrossAxisAlignment.Stretch
                                ? HorizontalAlignment.Stretch
                                : HorizontalAlignment.Center;
                        }
                    }
                    else
                    {
                        if (isFlexChild)
                        {
                            this.Width = double.NaN;
                            this.HorizontalAlignment = HorizontalAlignment.Stretch;
                        }
                        else if (!widthDim.HasExplicitSize && !widthDim.IsMatchParent)
                        {
                            this.HorizontalAlignment = HorizontalAlignment.Center;
                        }

                        if (!heightDim.HasExplicitSize && !heightDim.IsMatchParent)
                        {
                            this.VerticalAlignment = flexParent.CrossAxisAlignment == FlutterCrossAxisAlignment.Stretch
                                ? VerticalAlignment.Stretch
                                : VerticalAlignment.Center;
                        }
                    }
                }
                else if (ParentNodeControl?.NodeItem?.Type == "Container")
                {
                    var (effH, effV) = AlignmentHelper.ResolveEffectiveChildAlignment(NodeItem, ParentNodeControl.NodeItem);
                    if (!widthDim.HasExplicitSize && !widthDim.IsMatchParent)
                        this.HorizontalAlignment = effH;
                    if (!heightDim.HasExplicitSize && !heightDim.IsMatchParent)
                        this.VerticalAlignment = effV;
                }
                else
                {
                    if (!widthDim.HasExplicitSize && !widthDim.IsMatchParent)
                        this.HorizontalAlignment = mockElement.HorizontalAlignment;
                    if (!heightDim.HasExplicitSize && !heightDim.IsMatchParent)
                        this.VerticalAlignment = mockElement.VerticalAlignment;
                }

                SynchronizeSelectionBorder();
            }

            if (_layoutContainer != null)
            {
                LayoutLabelBorder.Visibility = Visibility.Visible;
                LayoutLabelText.Text = NodeItem.Id ?? NodeItem.Name;
            }
            else
            {
                LayoutLabelBorder.Visibility = Visibility.Collapsed;
            }

            RenderChildren();
        }

        private FrameworkElement CreateImageMockElement(bool hasCustomBg, SolidColorBrush bgBrush)
        {
            var border = new Border
            {
                MinHeight = 80,
                MinWidth = 80,
                Background = hasCustomBg ? bgBrush : new SolidColorBrush(ColorHelper.FromArgb(20, 0, 0, 0)),
                CornerRadius = new CornerRadius(ParseDouble(NodeItem.CornerRadius, 4))
            };

            string? resolvedPath = ResolveImageAssetPath(NodeItem.AssetPath, _parentDesigner?.ProjectRoot);
            if (!string.IsNullOrEmpty(resolvedPath) && File.Exists(resolvedPath))
            {
                try
                {
                    var uri = new Uri(Path.GetFullPath(resolvedPath));
                    var bImg = new BitmapImage(uri);
                    var img = new Image { Source = bImg, IsHitTestVisible = false };

                    string fit = (NodeItem.BoxFit ?? "Contain").ToLowerInvariant();
                    if (fit == "cover") img.Stretch = Stretch.UniformToFill;
                    else if (fit == "fill") img.Stretch = Stretch.Fill;
                    else img.Stretch = Stretch.Uniform;

                    border.Child = img;
                    return border;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DesignerNodeControl] Image load error: {ex.Message}");
                }
            }

            // Material placeholder card
            var placeholderStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 4,
                Margin = new Thickness(8)
            };
            placeholderStack.Children.Add(new FontIcon
            {
                Glyph = "\uEB9F",
                FontSize = 24,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(160, 33, 150, 243))
            });
            placeholderStack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(NodeItem.AssetPath) ? "Image.asset" : Path.GetFileName(NodeItem.AssetPath),
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(ColorHelper.FromArgb(200, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            border.Child = placeholderStack;
            return border;
        }

        private static string? ResolveImageAssetPath(string? assetPath, string? projectRoot)
        {
            if (string.IsNullOrWhiteSpace(assetPath)) return null;
            if (Path.IsPathRooted(assetPath) && File.Exists(assetPath)) return assetPath;
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot)) return null;

            string norm = assetPath.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string cA = Path.Combine(projectRoot, "SnapcodeAssets", norm);
            if (File.Exists(cA)) return cA;

            string cB = Path.Combine(projectRoot, norm);
            if (File.Exists(cB)) return cB;

            string cC = Path.Combine(projectRoot, "SnapcodeAssets", Path.GetFileName(norm));
            if (File.Exists(cC)) return cC;

            return null;
        }

        private void RenderChildren()
        {
            if (_layoutContainer == null) return;

            foreach (var child in _layoutContainer.Children)
            {
                if (child is DesignerNodeControl nodeCtrl)
                {
                    nodeCtrl.Unbind();
                }
            }
            _layoutContainer.Children.Clear();

            foreach (var child in NodeItem.Children)
            {
                var childNodeCtrl = new DesignerNodeControl(child, _parentDesigner, this);
                if (_layoutContainer is FlutterFlexPanel)
                {
                    int flexVal = child.Type == "Expanded"
                        ? (child.Flex > 0 ? child.Flex : 1)
                        : 0;
                    FlutterFlexPanel.SetFlex(childNodeCtrl, flexVal);
                }
                else if (NodeItem.Type == "Container")
                {
                    var (effH, effV) = AlignmentHelper.ResolveEffectiveChildAlignment(child, NodeItem);
                    childNodeCtrl.HorizontalAlignment = effH;
                    childNodeCtrl.VerticalAlignment = effV;
                }
                _layoutContainer.Children.Add(childNodeCtrl);
            }
        }

        public void Unbind()
        {
            if (NodeItem != null)
            {
                if (NodeItem.Children != null)
                {
                    NodeItem.Children.CollectionChanged -= Children_CollectionChanged;
                }
                NodeItem.PropertyChanged -= NodeItem_PropertyChanged;
            }
        }

        public void SynchronizeSelectionBorder()
        {
            if (SelectionBorder == null) return;
            if (_mockElement == null)
            {
                SelectionBorder.Margin = new Thickness(0);
                SelectionBorder.CornerRadius = new CornerRadius(0);
                SelectionBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                SelectionBorder.VerticalAlignment = VerticalAlignment.Stretch;
                SelectionBorder.Width = double.NaN;
                SelectionBorder.Height = double.NaN;
                return;
            }

            SelectionBorder.Margin = _mockElement.Margin;
            SelectionBorder.HorizontalAlignment = _mockElement.HorizontalAlignment;
            SelectionBorder.VerticalAlignment = _mockElement.VerticalAlignment;
            SelectionBorder.Width = _mockElement.Width;
            SelectionBorder.Height = _mockElement.Height;
            SelectionBorder.MinWidth = _mockElement.MinWidth;
            SelectionBorder.MinHeight = _mockElement.MinHeight;
            SelectionBorder.MaxWidth = _mockElement.MaxWidth;
            SelectionBorder.MaxHeight = _mockElement.MaxHeight;

            // Corner Radius Synchronization
            CornerRadius cr = new CornerRadius(0);
            if (_mockElement is Border bdr)
            {
                cr = bdr.CornerRadius;
            }
            else if (_mockElement is Control ctrl)
            {
                cr = ctrl.CornerRadius;
            }
            else if (_mockElement is Grid gr)
            {
                cr = gr.CornerRadius;
            }
            else if (_mockElement is StackPanel sp)
            {
                cr = sp.CornerRadius;
            }
            else
            {
                double parsedCr = ParseDouble(NodeItem?.CornerRadius, 0);
                if (parsedCr > 0) cr = new CornerRadius(parsedCr);
            }
            SelectionBorder.CornerRadius = cr;
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected)
            {
                SynchronizeSelectionBorder();
                SelectionBorder.BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 33, 150, 243)); // Material Blue accent
                SelectionBorder.Background = new SolidColorBrush(ColorHelper.FromArgb(30, 33, 150, 243));
                SelectionBorder.BorderThickness = new Thickness(2.5);
            }
            else
            {
                SelectionBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                SelectionBorder.Background = new SolidColorBrush(Colors.Transparent);
                SelectionBorder.BorderThickness = new Thickness(2);
            }
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _parentDesigner.SelectNode(this);
            e.Handled = true;
        }

        private void UserControl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.Data.Properties["WidgetType"] = NodeItem.Type;
            args.Data.Properties["WidgetName"] = NodeItem.Name;
            args.Data.Properties["WidgetIcon"] = NodeItem.Icon;
            args.Data.Properties["ExistingWidgetId"] = NodeItem.Id;
            args.AllowedOperations = DataPackageOperation.Move;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            if (_layoutContainer != null && e.DataView.Properties.ContainsKey("WidgetType"))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = true;
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (_layoutContainer != null &&
                e.DataView.Properties.TryGetValue("WidgetType", out var typeObj) && typeObj != null)
            {
                if (e.DataView.Properties.TryGetValue("ExistingWidgetId", out var existingIdObj) && existingIdObj != null)
                {
                    string existingId = existingIdObj.ToString() ?? "";
                    var existingWidget = _parentDesigner.FindWidgetById(_parentDesigner.CanvasItems, existingId);
                    if (existingWidget != null && existingWidget != NodeItem)
                    {
                        if (!string.IsNullOrEmpty(NodeItem.Id) && _parentDesigner.FindWidgetById(existingWidget.Children, NodeItem.Id) == null)
                        {
                            _parentDesigner.RemoveWidgetFromTree(_parentDesigner.CanvasItems, existingWidget);
                            NodeItem.Children.Add(existingWidget);
                            _parentDesigner.RenderRoot();
                            _parentDesigner.SaveStateForUndo();
                            _parentDesigner.OnWidgetTreeChanged(existingWidget);
                        }
                    }
                }
                else if (e.DataView.Properties.TryGetValue("WidgetName", out var nameObj) &&
                         e.DataView.Properties.TryGetValue("WidgetIcon", out var iconObj))
                {
                    string t = typeObj.ToString() ?? "Unknown";
                    var newItem = new WidgetItem
                    {
                        Type = t,
                        Name = nameObj?.ToString() ?? "Unknown",
                        Icon = iconObj?.ToString() ?? "\uE710",
                        Id = $"{t.ToLower()}_{Guid.NewGuid().ToString().Substring(0, 4)}"
                    };

                    if (t == "Row" || t == "Column")
                    {
                        newItem.Width = "Match Parent";
                        newItem.Height = t == "Column" ? "Match Parent" : "Auto";
                        newItem.MainAxisAlignment = "Start";
                        newItem.CrossAxisAlignment = "Center";
                    }
                    else if (t == "Expanded")
                    {
                        newItem.Flex = 1;
                    }

                    NodeItem.Children.Add(newItem);
                    _parentDesigner.RenderRoot();
                    _parentDesigner.SaveStateForUndo();
                    _parentDesigner.OnWidgetTreeChanged(newItem);
                }
                e.Handled = true;
            }
        }
    }
}
