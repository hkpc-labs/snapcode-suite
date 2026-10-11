using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;

namespace Snapcode.Views
{
    public class WidgetItem : System.ComponentModel.INotifyPropertyChanged
    {
        private string _name = "";
        public string Name 
        { 
            get => _name; 
            set { _name = value; OnPropertyChanged(nameof(Name)); } 
        }

        private string _id = "";
        public string Id 
        { 
            get => _id; 
            set { _id = value; OnPropertyChanged(nameof(Id)); } 
        }

        private string _width = "Auto";
        public string Width { get => _width; set { _width = value; OnPropertyChanged(nameof(Width)); } }

        private string _height = "Auto";
        public string Height { get => _height; set { _height = value; OnPropertyChanged(nameof(Height)); } }

        private string _padding = "8";
        public string Padding { get => _padding; set { _padding = value; OnPropertyChanged(nameof(Padding)); } }

        private string _margin = "0";
        public string Margin { get => _margin; set { _margin = value; OnPropertyChanged(nameof(Margin)); } }

        private string _backgroundColor = "Transparent";
        public string BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); } }

        private string _textColor = "#000000";
        public string TextColor { get => _textColor; set { _textColor = value; OnPropertyChanged(nameof(TextColor)); } }

        private string _textSize = "14";
        public string TextSize { get => _textSize; set { _textSize = value; OnPropertyChanged(nameof(TextSize)); } }

        private string _cornerRadius = "0";
        public string CornerRadius { get => _cornerRadius; set { _cornerRadius = value; OnPropertyChanged(nameof(CornerRadius)); } }

        private string _borderWidth = "0";
        public string BorderWidth { get => _borderWidth; set { _borderWidth = value; OnPropertyChanged(nameof(BorderWidth)); } }

        private string _borderColor = "Transparent";
        public string BorderColor { get => _borderColor; set { _borderColor = value; OnPropertyChanged(nameof(BorderColor)); } }

        private string _alignment = "";
        public string Alignment { get => _alignment; set { _alignment = value; OnPropertyChanged(nameof(Alignment)); } }

        private string _gravity = "";
        public string Gravity { get => _gravity; set { _gravity = value; OnPropertyChanged(nameof(Gravity)); } }

        private string _assetPath = "";
        public string AssetPath { get => _assetPath; set { _assetPath = value; OnPropertyChanged(nameof(AssetPath)); } }

        // Widget-Specific Properties
        private string _fontWeight = "Normal";
        public string FontWeight { get => _fontWeight; set { _fontWeight = value; OnPropertyChanged(nameof(FontWeight)); } }

        private string _textAlign = "Left";
        public string TextAlign { get => _textAlign; set { _textAlign = value; OnPropertyChanged(nameof(TextAlign)); } }

        private string _mainAxisAlignment = "";
        public string MainAxisAlignment { get => _mainAxisAlignment; set { _mainAxisAlignment = value; OnPropertyChanged(nameof(MainAxisAlignment)); } }

        private string _crossAxisAlignment = "";
        public string CrossAxisAlignment { get => _crossAxisAlignment; set { _crossAxisAlignment = value; OnPropertyChanged(nameof(CrossAxisAlignment)); } }

        private string _boxFit = "Contain";
        public string BoxFit { get => _boxFit; set { _boxFit = value; OnPropertyChanged(nameof(BoxFit)); } }

        private bool _obscureText = false;
        public bool ObscureText { get => _obscureText; set { _obscureText = value; OnPropertyChanged(nameof(ObscureText)); } }

        private string _scrollDirection = "Vertical";
        public string ScrollDirection { get => _scrollDirection; set { _scrollDirection = value; OnPropertyChanged(nameof(ScrollDirection)); } }

        private string _assetUsage = "Image";
        public string AssetUsage { get => _assetUsage; set { _assetUsage = value; OnPropertyChanged(nameof(AssetUsage)); } }

        private bool _hasShadow = false;
        public bool HasShadow { get => _hasShadow; set { _hasShadow = value; OnPropertyChanged(nameof(HasShadow)); } }

        private bool _isChecked = false;
        public bool Checked { get => _isChecked; set { _isChecked = value; OnPropertyChanged(nameof(Checked)); } }

        private string _activeColor = "#2196F3";
        public string ActiveColor { get => _activeColor; set { _activeColor = value; OnPropertyChanged(nameof(ActiveColor)); } }

        private double _minValue = 0;
        public double MinValue { get => _minValue; set { _minValue = value; OnPropertyChanged(nameof(MinValue)); } }

        private double _maxValue = 100;
        public double MaxValue { get => _maxValue; set { _maxValue = value; OnPropertyChanged(nameof(MaxValue)); } }

        private double _currentValue = 50;
        public double CurrentValue { get => _currentValue; set { _currentValue = value; OnPropertyChanged(nameof(CurrentValue)); } }

        private string _hintText = "";
        public string HintText { get => _hintText; set { _hintText = value; OnPropertyChanged(nameof(HintText)); } }

        private string _inputType = "Text";
        public string InputType { get => _inputType; set { _inputType = value; OnPropertyChanged(nameof(InputType)); } }

        private string _fontFamily = "";
        public string FontFamily { get => _fontFamily; set { _fontFamily = value; OnPropertyChanged(nameof(FontFamily)); } }

        private int _flex = 1;
        public int Flex { get => _flex; set { _flex = value; OnPropertyChanged(nameof(Flex)); } }

        public string Icon { get; set; } = "";
        public string Type { get; set; } = "";
        public ObservableCollection<WidgetItem> Children { get; set; } = new();

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class NativeUIDesigner : UserControl
    {
        public ObservableCollection<WidgetItem> PaletteItems { get; set; } = new();
        public ObservableCollection<WidgetItem> CanvasItems { get; set; } = new();

        public string ProjectRoot { get; set; }

        public NativeUIDesigner()
        {
            this.InitializeComponent();
            
            PaletteItems.Add(new WidgetItem { Name = "Row", Type = "Row", Icon = "\uE8A5" });
            PaletteItems.Add(new WidgetItem { Name = "Column", Type = "Column", Icon = "\uE8FD" });
            PaletteItems.Add(new WidgetItem { Name = "Container", Type = "Container", Icon = "\uE7B3" });
            PaletteItems.Add(new WidgetItem { Name = "Text", Type = "Text", Icon = "\uE8D2" });
            PaletteItems.Add(new WidgetItem { Name = "Button", Type = "Button", Icon = "\uE710" });
            PaletteItems.Add(new WidgetItem { Name = "TextField", Type = "TextField", Icon = "\uE792" });
            PaletteItems.Add(new WidgetItem { Name = "Image", Type = "Image", Icon = "\uEB9F" });
            PaletteItems.Add(new WidgetItem { Name = "Checkbox", Type = "Checkbox", Icon = "\uE73A" });
            PaletteItems.Add(new WidgetItem { Name = "Switch", Type = "Switch", Icon = "\uE718" });
            PaletteItems.Add(new WidgetItem { Name = "Slider", Type = "Slider", Icon = "\uE70F" });
            PaletteItems.Add(new WidgetItem { Name = "ListView", Type = "ListView", Icon = "\uE8FD" });
            PaletteItems.Add(new WidgetItem { Name = "ScrollView", Type = "ScrollView", Icon = "\uE8CB" });
            PaletteItems.Add(new WidgetItem { Name = "H-ScrollView", Type = "HorizontalScrollView", Icon = "\uE8CC" });
            PaletteItems.Add(new WidgetItem { Name = "Expanded", Type = "Expanded", Icon = "\uE74C" });

            PaletteList.ItemsSource = PaletteItems;
        }

        private void PaletteList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count > 0 && e.Items[0] is WidgetItem item)
            {
                e.Data.Properties["WidgetType"] = item.Type;
                e.Data.Properties["WidgetName"] = item.Name;
                e.Data.Properties["WidgetIcon"] = item.Icon;
                e.Data.RequestedOperation = DataPackageOperation.Copy;
            }
        }



        private DesignerNodeControl _selectedNodeControl;
        public DesignerNodeControl SelectedNodeControl => _selectedNodeControl;
        private bool _isUpdatingSelectionUI = false;
        private int _widgetIdCounter = 1;
        public ObservableCollection<WidgetItem> AllWidgetsFlat { get; set; } = new();

        private Windows.UI.Color ParseHexColor(string hex, Windows.UI.Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            hex = hex.Replace("#", "");
            if (hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase)) return Microsoft.UI.Colors.Transparent;
            if (hex.Equals("White", StringComparison.OrdinalIgnoreCase)) return Microsoft.UI.Colors.White;
            if (hex.Equals("Black", StringComparison.OrdinalIgnoreCase)) return Microsoft.UI.Colors.Black;
            
            try
            {
                if (hex.Length == 6)
                {
                    return Windows.UI.Color.FromArgb(255, 
                        byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
                }
                if (hex.Length == 8)
                {
                    return Windows.UI.Color.FromArgb(
                        byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                }
            }
            catch { }
            return fallback;
        }

        public event EventHandler<(string WidgetId, string EventType)> OpenEventWorkspaceRequested;

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (PropertiesPanel == null || EventsPanel == null || RadioProperties == null || RadioEvents == null) return;
            if (RadioProperties.IsChecked == true)
            {
                PropertiesPanel.Visibility = _selectedWidget != null ? Visibility.Visible : Visibility.Collapsed;
                EventsPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                PropertiesPanel.Visibility = Visibility.Collapsed;
                EventsPanel.Visibility = _selectedWidget != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListEvents.SelectedItem is string eventType && _selectedWidget != null)
            {
                // Request opening of this event's workspace
                OpenEventWorkspaceRequested?.Invoke(this, (_selectedWidget.Id, eventType));
                ListEvents.SelectedItem = null; // Clear selection to allow re-clicking
            }
        }

        private void SelectedWidget_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UIStateChanged?.Invoke(this, EventArgs.Empty);

            if (_selectedWidget != null && NodeRegistry.TryGetValue(_selectedWidget, out var nodeControl))
            {
                nodeControl.RenderMockView();
                nodeControl.ParentNodeControl?.RefreshLayout();
                _selectedNodeControl?.SetSelected(true);
            }
            else
            {
                RenderRoot();
            }
        }

        public void SelectNode(DesignerNodeControl node)
        {
            try
            {
                _isUpdatingSelectionUI = true;

                if (_selectedNodeControl != null)
                {
                    _selectedNodeControl.SetSelected(false);
                }

                _selectedNodeControl = node;
                if (_selectedNodeControl != null)
                {
                    _selectedNodeControl.SetSelected(true);
                }

            if (WidgetNavigator != null)
            {
                WidgetNavigator.SelectionChanged -= WidgetNavigator_SelectionChanged;
                WidgetNavigator.SelectedItem = node?.NodeItem;
                WidgetNavigator.SelectionChanged += WidgetNavigator_SelectionChanged;
            }

            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged -= SelectedWidget_PropertyChanged;
            }
            _selectedWidget = node?.NodeItem;
            if (_selectedWidget != null) {
                _selectedWidget.PropertyChanged += SelectedWidget_PropertyChanged;
            }

            if (_selectedWidget != null)
            {
                TxtNoSelection.Visibility = Visibility.Collapsed;
                if (RadioProperties.IsChecked == true) PropertiesPanel.Visibility = Visibility.Visible;
                if (RadioEvents.IsChecked == true) EventsPanel.Visibility = Visibility.Visible;
                
                // Populate Events List
                List<string> availableEvents = new List<string> { "onClick", "onLongPress" };
                if (_selectedWidget.Type == "TextField") availableEvents.Add("onTextChanged");
                else if (_selectedWidget.Type == "Switch") availableEvents.Add("onSwitchChanged");
                else if (_selectedWidget.Type == "Checkbox") availableEvents.Add("onCheckboxChanged");
                else if (_selectedWidget.Type == "Slider") availableEvents.Add("onSliderChanged");
                ListEvents.ItemsSource = availableEvents;
                
                TxtPropType.Text = $"Type: {_selectedWidget.Type}";
                InputPropId.Text = _selectedWidget.Id ?? "";
                InputPropName.Text = _selectedWidget.Name ?? "";
                InputPropAssetPath.Text = _selectedWidget.AssetPath ?? "";

                string wVal = _selectedWidget.Width ?? "Auto";
                ComboPropWidth.Text = wVal;
                ComboPropWidth.SelectedItem = wVal;

                string hVal = _selectedWidget.Height ?? "Auto";
                ComboPropHeight.Text = hVal;
                ComboPropHeight.SelectedItem = hVal;

                InputPropPadding.Text = _selectedWidget.Padding ?? "8";
                InputPropMargin.Text = _selectedWidget.Margin ?? "0";
                
                var bgColor = ParseHexColor(_selectedWidget.BackgroundColor, Microsoft.UI.Colors.Transparent);
                PickerPropBgColor.Color = bgColor;
                PreviewPropBgColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(bgColor);

                var txtColor = ParseHexColor(_selectedWidget.TextColor, Microsoft.UI.Colors.Black);
                PickerPropTextColor.Color = txtColor;
                PreviewPropTextColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(txtColor);
                InputPropTextSize.Text = _selectedWidget.TextSize ?? "14";
                
                string alignVal = string.IsNullOrWhiteSpace(_selectedWidget.Alignment) ? "Default" : _selectedWidget.Alignment;
                ComboPropAlignment.Text = alignVal;
                ComboPropAlignment.SelectedItem = alignVal;

                string gravVal = string.IsNullOrWhiteSpace(_selectedWidget.Gravity) ? "Default" : _selectedWidget.Gravity;
                ComboPropGravity.Text = gravVal;
                ComboPropGravity.SelectedItem = gravVal;
                InputPropBorderWidth.Text = _selectedWidget.BorderWidth ?? "0";
                InputPropCornerRadius.Text = _selectedWidget.CornerRadius ?? "0";
                
                var borderColor = ParseHexColor(_selectedWidget.BorderColor, Microsoft.UI.Colors.Transparent);
                PickerPropBorderColor.Color = borderColor;
                PreviewPropBorderColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(borderColor);
                
                ToggleShadow.IsOn = _selectedWidget.HasShadow;

                // Widget Specific UI
                PanelTextSpecific.Visibility = (_selectedWidget.Type == "Text" || _selectedWidget.Type == "Button") ? Visibility.Visible : Visibility.Collapsed;
                PanelFlexSpecific.Visibility = (_selectedWidget.Type == "Row" || _selectedWidget.Type == "Column") ? Visibility.Visible : Visibility.Collapsed;
                PanelImageSpecific.Visibility = (_selectedWidget.Type == "Image") ? Visibility.Visible : Visibility.Collapsed;
                PanelInputSpecific.Visibility = (_selectedWidget.Type == "TextField") ? Visibility.Visible : Visibility.Collapsed;
                PanelListSpecific.Visibility = (_selectedWidget.Type == "ListView" || _selectedWidget.Type == "ScrollView" || _selectedWidget.Type == "HorizontalScrollView") ? Visibility.Visible : Visibility.Collapsed;
                PanelToggleSpecific.Visibility = (_selectedWidget.Type == "Switch" || _selectedWidget.Type == "Checkbox") ? Visibility.Visible : Visibility.Collapsed;
                PanelSliderSpecific.Visibility = (_selectedWidget.Type == "Slider") ? Visibility.Visible : Visibility.Collapsed;
                PanelExpandedSpecific.Visibility = (_selectedWidget.Type == "Expanded") ? Visibility.Visible : Visibility.Collapsed;

                if (PanelTextSpecific.Visibility == Visibility.Visible)
                {
                    ComboFontWeight.SelectedItem = _selectedWidget.FontWeight ?? "Normal";
                    ComboTextAlign.SelectedItem = _selectedWidget.TextAlign ?? "Left";
                    InputPropFontFamily.Text = _selectedWidget.FontFamily ?? "";
                }
                if (PanelFlexSpecific.Visibility == Visibility.Visible)
                {
                    ComboMainAxis.SelectedItem = string.IsNullOrWhiteSpace(_selectedWidget.MainAxisAlignment) ? "Start" : _selectedWidget.MainAxisAlignment;
                    ComboCrossAxis.SelectedItem = string.IsNullOrWhiteSpace(_selectedWidget.CrossAxisAlignment) ? "Center" : _selectedWidget.CrossAxisAlignment;
                }
                if (PanelImageSpecific.Visibility == Visibility.Visible)
                {
                    ComboBoxFit.SelectedItem = _selectedWidget.BoxFit ?? "Contain";
                    ComboAssetUsage.SelectedItem = _selectedWidget.AssetUsage ?? "Image";
                }
                if (PanelInputSpecific.Visibility == Visibility.Visible)
                {
                    ToggleObscureText.IsOn = _selectedWidget.ObscureText;
                    InputPropHintText.Text = _selectedWidget.HintText ?? "";
                    ComboInputType.SelectedItem = _selectedWidget.InputType ?? "Text";
                }
                if (PanelListSpecific.Visibility == Visibility.Visible)
                {
                    ComboScrollDirection.SelectedItem = _selectedWidget.ScrollDirection ?? "Vertical";
                }
                if (PanelToggleSpecific.Visibility == Visibility.Visible)
                {
                    ToggleIsChecked.IsOn = _selectedWidget.Checked;
                    var activeColor = ParseHexColor(_selectedWidget.ActiveColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243)); // #2196F3
                    PickerPropActiveColor.Color = activeColor;
                    PreviewPropActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(activeColor);
                }
                if (PanelSliderSpecific.Visibility == Visibility.Visible)
                {
                    InputPropMinValue.Text = _selectedWidget.MinValue.ToString();
                    InputPropMaxValue.Text = _selectedWidget.MaxValue.ToString();
                    InputPropCurrentValue.Text = _selectedWidget.CurrentValue.ToString();
                    var activeColor = ParseHexColor(_selectedWidget.ActiveColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243));
                    PickerPropSliderActiveColor.Color = activeColor;
                    PreviewPropSliderActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(activeColor);
                }
                if (PanelExpandedSpecific.Visibility == Visibility.Visible)
                {
                    InputPropFlex.Text = _selectedWidget.Flex.ToString();
                }

                _selectedNodeControl.SetSelected(true);
            }
            else
            {
                PropertiesPanel.Visibility = Visibility.Collapsed;
                if (EventsPanel != null) EventsPanel.Visibility = Visibility.Collapsed;
                TxtNoSelection.Visibility = Visibility.Visible;
            }
            }
            finally
            {
                _isUpdatingSelectionUI = false;
            }
        }

        private WidgetItem _selectedWidget;

        private void InputPropId_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.Id = InputPropId.Text; }
        private void InputPropName_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.Name = InputPropName.Text; }
        private void InputPropPadding_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.Padding = InputPropPadding.Text; }
        private void InputPropMargin_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.Margin = InputPropMargin.Text; }
        private void InputPropTextSize_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.TextSize = InputPropTextSize.Text; }
        
        private void InputPropAssetPath_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.AssetPath = InputPropAssetPath.Text; }
        
        private void ComboPropAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (ComboPropAlignment.SelectedItem != null)
                ApplyAlignmentSelection(ComboPropAlignment.SelectedItem.ToString());
        }

        private void ComboPropAlignment_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            ApplyAlignmentSelection(args.Text);
            args.Handled = true;
        }

        private void ComboPropAlignment_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyAlignmentSelection(ComboPropAlignment.Text);
        }

        private void ApplyAlignmentSelection(string? val)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            string trimmed = val?.Trim() ?? "";
            _selectedWidget.Alignment = trimmed.Equals("Default", StringComparison.OrdinalIgnoreCase) ? "" : trimmed;
        }

        private void ComboPropGravity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (ComboPropGravity.SelectedItem != null)
                ApplyGravitySelection(ComboPropGravity.SelectedItem.ToString());
        }

        private void ComboPropGravity_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            ApplyGravitySelection(args.Text);
            args.Handled = true;
        }

        private void ComboPropGravity_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyGravitySelection(ComboPropGravity.Text);
        }

        private void ApplyGravitySelection(string? val)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            string trimmed = val?.Trim() ?? "";
            _selectedWidget.Gravity = trimmed.Equals("Default", StringComparison.OrdinalIgnoreCase) ? "" : trimmed;
        }
        private void InputPropBorderWidth_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.BorderWidth = InputPropBorderWidth.Text; }
        private void InputPropCornerRadius_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.CornerRadius = InputPropCornerRadius.Text; }
        
        private void PickerPropBgColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var hex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
                _selectedWidget.BackgroundColor = hex;
                PreviewPropBgColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
            }
        }

        private void PickerPropTextColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var hex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
                _selectedWidget.TextColor = hex;
                PreviewPropTextColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
            }
        }

        private void PickerPropBorderColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var hex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
                _selectedWidget.BorderColor = hex;
                PreviewPropBorderColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
            }
        }

        private void PickerPropActiveColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var hex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
                _selectedWidget.ActiveColor = hex;
                PreviewPropActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
            }
        }

        private void PickerPropSliderActiveColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var hex = $"#{args.NewColor.A:X2}{args.NewColor.R:X2}{args.NewColor.G:X2}{args.NewColor.B:X2}";
                _selectedWidget.ActiveColor = hex;
                PreviewPropSliderActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(args.NewColor);
            }
        }

        private void ComboPropWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (ComboPropWidth.SelectedItem != null)
                _selectedWidget.Width = ComboPropWidth.SelectedItem.ToString() ?? "Auto";
        }

        private void ComboPropWidth_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (!string.IsNullOrWhiteSpace(args.Text))
            {
                _selectedWidget.Width = args.Text.Trim();
                args.Handled = true;
            }
        }

        private void ComboPropWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (!string.IsNullOrWhiteSpace(ComboPropWidth.Text))
                _selectedWidget.Width = ComboPropWidth.Text.Trim();
        }

        private void ComboPropHeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (ComboPropHeight.SelectedItem != null)
                _selectedWidget.Height = ComboPropHeight.SelectedItem.ToString() ?? "Auto";
        }

        private void ComboPropHeight_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (!string.IsNullOrWhiteSpace(args.Text))
            {
                _selectedWidget.Height = args.Text.Trim();
                args.Handled = true;
            }
        }

        private void ComboPropHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingSelectionUI || _selectedWidget == null) return;
            if (!string.IsNullOrWhiteSpace(ComboPropHeight.Text))
                _selectedWidget.Height = ComboPropHeight.Text.Trim();
        }

        private void ComboFontWeight_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboFontWeight.SelectedItem != null) _selectedWidget.FontWeight = ComboFontWeight.SelectedItem.ToString(); }
        private void ComboTextAlign_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboTextAlign.SelectedItem != null) _selectedWidget.TextAlign = ComboTextAlign.SelectedItem.ToString(); }
        private void ComboMainAxis_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboMainAxis.SelectedItem != null) _selectedWidget.MainAxisAlignment = ComboMainAxis.SelectedItem.ToString(); }
        private void ComboCrossAxis_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboCrossAxis.SelectedItem != null) _selectedWidget.CrossAxisAlignment = ComboCrossAxis.SelectedItem.ToString(); }
        private void ComboBoxFit_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboBoxFit.SelectedItem != null) _selectedWidget.BoxFit = ComboBoxFit.SelectedItem.ToString(); }
        private void ToggleObscureText_Toggled(object sender, RoutedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.ObscureText = ToggleObscureText.IsOn; }
        private void ComboScrollDirection_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboScrollDirection.SelectedItem != null) _selectedWidget.ScrollDirection = ComboScrollDirection.SelectedItem.ToString(); }
        private void ComboAssetUsage_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboAssetUsage.SelectedItem != null) _selectedWidget.AssetUsage = ComboAssetUsage.SelectedItem.ToString(); }
        private void ToggleShadow_Toggled(object sender, RoutedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.HasShadow = ToggleShadow.IsOn; }

        private void InputPropHintText_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.HintText = InputPropHintText.Text; }
        private void ComboInputType_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && ComboInputType.SelectedItem != null) _selectedWidget.InputType = ComboInputType.SelectedItem.ToString(); }
        private void ToggleIsChecked_Toggled(object sender, RoutedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null) _selectedWidget.Checked = ToggleIsChecked.IsOn; }
        private void InputPropMinValue_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && double.TryParse(InputPropMinValue.Text, out double val)) _selectedWidget.MinValue = val; }
        private void InputPropMaxValue_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && double.TryParse(InputPropMaxValue.Text, out double val)) _selectedWidget.MaxValue = val; }
        private void InputPropCurrentValue_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && double.TryParse(InputPropCurrentValue.Text, out double val)) _selectedWidget.CurrentValue = val; }
        private void InputPropFlex_TextChanged(object sender, TextChangedEventArgs e) { if (_isUpdatingSelectionUI) return; if (_selectedWidget != null && int.TryParse(InputPropFlex.Text, out int val)) _selectedWidget.Flex = val; }

        private async void BtnBrowseFontAsset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectRoot)) return;
            var dialog = new AssetManagerDialog(ProjectRoot, true, "font") { XamlRoot = this.XamlRoot };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.SelectedAsset != null)
            {
                InputPropFontFamily.Text = dialog.SelectedAsset.FileName;
                if (_selectedWidget != null) _selectedWidget.FontFamily = dialog.SelectedAsset.FileName;
            }
        }

        private async void BtnBrowseAsset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ProjectRoot)) return;
            var dialog = new AssetManagerDialog(ProjectRoot, true, "image")
            {
                XamlRoot = this.XamlRoot
            };
            
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.SelectedAsset != null)
            {
                InputPropAssetPath.Text = dialog.SelectedAsset.FileName;
                if (_selectedWidget != null) _selectedWidget.AssetPath = dialog.SelectedAsset.FileName;
            }
        }

        private void RootCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey("WidgetType"))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        private void RootCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.TryGetValue("WidgetType", out var typeObj) && typeObj != null)
            {
                if (e.DataView.Properties.TryGetValue("ExistingWidgetId", out var existingIdObj) && existingIdObj != null)
                {
                    string existingId = existingIdObj.ToString() ?? "";
                    var existingWidget = FindWidgetById(CanvasItems, existingId);
                    if (existingWidget != null)
                    {
                        RemoveWidgetFromTree(CanvasItems, existingWidget);
                        CanvasItems.Add(existingWidget);
                        RenderRoot();
                        SaveStateForUndo();
                        OnWidgetTreeChanged(existingWidget);
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
                        Id = $"{t.ToLower()}_{_widgetIdCounter++}"
                    };

                    if (t == "Row" || t == "Column")
                    {
                        newItem.Width = "Match Parent";
                        newItem.Height = t == "Column" ? "Match Parent" : "Auto";
                        newItem.MainAxisAlignment = "Start";
                        newItem.CrossAxisAlignment = "Center";
                    }

                    CanvasItems.Add(newItem);
                    RenderRoot();
                    SaveStateForUndo();
                    OnWidgetTreeChanged(newItem);
                }
            }
        }

        public event EventHandler<string> WidgetTreeFlatJsonChanged;
        public event EventHandler UIStateChanged;

        private void UpdateWidgetNavigator()
        {
            AllWidgetsFlat.Clear();
            PopulateFlatList(CanvasItems);
            
            if (WidgetNavigator != null)
            {
                WidgetNavigator.SelectionChanged -= WidgetNavigator_SelectionChanged;
                WidgetNavigator.ItemsSource = null;
                WidgetNavigator.ItemsSource = AllWidgetsFlat;
                if (_selectedWidget != null && AllWidgetsFlat.Contains(_selectedWidget))
                {
                    WidgetNavigator.SelectedItem = _selectedWidget;
                }
                WidgetNavigator.SelectionChanged += WidgetNavigator_SelectionChanged;
            }

            // Notify listeners (like EditorPage/Blockly) of the new widget list
            var simpleList = AllWidgetsFlat.Select(w => new { id = w.Id, type = w.Type }).ToList();
            string json = System.Text.Json.JsonSerializer.Serialize(simpleList);
            WidgetTreeFlatJsonChanged?.Invoke(this, json);
        }

        private void PopulateFlatList(IEnumerable<WidgetItem> items)
        {
            foreach(var item in items)
            {
                AllWidgetsFlat.Add(item);
                PopulateFlatList(item.Children);
            }
        }

        public Dictionary<WidgetItem, DesignerNodeControl> NodeRegistry { get; } = new();

        private void WidgetNavigator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WidgetNavigator.SelectedItem is WidgetItem selectedItem)
            {
                var node = FindNodeControl(selectedItem);
                if (node != null)
                {
                    SelectNode(node);
                }
            }
        }

        private DesignerNodeControl FindNodeControl(WidgetItem target)
        {
            return NodeRegistry.TryGetValue(target, out var node) ? node : null;
        }

        public void RenderRoot()
        {
            var prevSelected = _selectedWidget;
            RootCanvas.Children.Clear();
            NodeRegistry.Clear();
            foreach (var item in CanvasItems)
            {
                RootCanvas.Children.Add(new DesignerNodeControl(item, this));
            }
            UpdateWidgetNavigator();
            if (prevSelected != null && NodeRegistry.TryGetValue(prevSelected, out var newNodeCtrl))
            {
                SelectNode(newNodeCtrl);
            }
        }

        public void OnWidgetTreeChanged(WidgetItem newlyAddedItem = null)
        {
            UpdateWidgetNavigator();
            if (newlyAddedItem != null)
            {
                DispatcherQueue.TryEnqueue(() => 
                {
                    var node = FindNodeControl(newlyAddedItem);
                    if (node != null) SelectNode(node);
                    WidgetNavigator.SelectedItem = newlyAddedItem;
                });
            }
        }

        private void BtnDeleteWidget_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWidget != null)
            {
                RemoveWidgetFromTree(CanvasItems, _selectedWidget);
                SelectNode(null);
                RenderRoot();
                SaveStateForUndo();
            }
        }

        public bool RemoveWidgetFromTree(ObservableCollection<WidgetItem> list, WidgetItem target)
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

        public WidgetItem FindWidgetById(ObservableCollection<WidgetItem> list, string id)
        {
            foreach (var item in list)
            {
                if (item.Id == id) return item;
                var found = FindWidgetById(item.Children, id);
                if (found != null) return found;
            }
            return null;
        }

        public string GetUIJson()
        {
            // Simple serialization of the canvas tree
            var list = new List<object>();
            foreach (var item in CanvasItems)
            {
                list.Add(SerializeItem(item));
            }
            return JsonSerializer.Serialize(list);
        }

        private object SerializeItem(WidgetItem item)
        {
            var childrenList = new List<object>();
            foreach (var child in item.Children)
            {
                childrenList.Add(SerializeItem(child));
            }

            return new
            {
                id = string.IsNullOrWhiteSpace(item.Id) ? System.Guid.NewGuid().ToString() : item.Id,
                type = item.Type,
                text = item.Name, // Treat name as text/value for simple widgets
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

        public void SetPageName(string name)
        {
            TxtAppTitle.Text = $"{name}.dart";
        }

        private Stack<string> _undoStack = new Stack<string>();
        private Stack<string> _redoStack = new Stack<string>();
        private bool _isRestoringState = false;
        private const int MaxUndoDepth = 50;

        public void SaveStateForUndo()
        {
            UIStateChanged?.Invoke(this, EventArgs.Empty);
            if (_isRestoringState) return;
            string currentState = GetUIJson();
            if (_undoStack.Count == 0 || _undoStack.Peek() != currentState)
            {
                _undoStack.Push(currentState);
                if (_undoStack.Count > MaxUndoDepth)
                {
                    var items = _undoStack.ToArray();
                    _undoStack.Clear();
                    for (int i = MaxUndoDepth - 1; i >= 0; i--)
                    {
                        _undoStack.Push(items[i]);
                    }
                }
                _redoStack.Clear();
            }
        }

        public void Undo()
        {
            if (_undoStack.Count > 1)
            {
                _redoStack.Push(_undoStack.Pop());
                string previousState = _undoStack.Peek();
                _isRestoringState = true;
                LoadUIJson(previousState);
                _isRestoringState = false;
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                string nextState = _redoStack.Pop();
                _undoStack.Push(nextState);
                _isRestoringState = true;
                LoadUIJson(nextState);
                _isRestoringState = false;
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void Undo_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            Undo();
            args.Handled = true;
        }

        private void Redo_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            Redo();
            args.Handled = true;
        }

        public void LoadUIJson(string json)
        {
            SelectNode(null);
            CanvasItems.Clear();
            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
            {
                RenderRoot();
                OnWidgetTreeChanged(null);
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        CanvasItems.Add(DeserializeItem(elem));
                    }
                }
                RenderRoot();
                OnWidgetTreeChanged(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load native UI JSON: {ex.Message}");
            }
        }

        private WidgetItem DeserializeItem(JsonElement elem)
        {
            string type = elem.TryGetProperty("type", out var tProp) ? tProp.GetString() ?? "Unknown" : "Unknown";
            string id = elem.TryGetProperty("id", out var iProp) ? iProp.GetString() ?? "" : "";
            string text = elem.TryGetProperty("text", out var txtProp) ? txtProp.GetString() ?? type : type;
            
            var item = new WidgetItem
            {
                Type = type,
                Name = text,
                Id = id,
                Icon = "\uE7B3", // Default icon
                Width = elem.TryGetProperty("width", out var wProp) ? wProp.GetString() ?? "Auto" : "Auto",
                Height = elem.TryGetProperty("height", out var hProp) ? hProp.GetString() ?? "Auto" : "Auto",
                Padding = elem.TryGetProperty("padding", out var pProp) ? pProp.GetString() ?? "8" : "8",
                Margin = elem.TryGetProperty("margin", out var mProp) ? mProp.GetString() ?? "0" : "0",
                BackgroundColor = elem.TryGetProperty("backgroundColor", out var bgProp) ? bgProp.GetString() ?? "Transparent" : "Transparent",
                TextColor = elem.TryGetProperty("textColor", out var tcProp) ? tcProp.GetString() ?? "#000000" : "#000000",
                TextSize = elem.TryGetProperty("textSize", out var tsProp) ? tsProp.GetString() ?? "14" : "14",
                CornerRadius = elem.TryGetProperty("cornerRadius", out var crProp) ? crProp.GetString() ?? "0" : "0",
                BorderWidth = elem.TryGetProperty("borderWidth", out var bwProp) ? bwProp.GetString() ?? "0" : "0",
                BorderColor = elem.TryGetProperty("borderColor", out var bcProp) ? bcProp.GetString() ?? "Transparent" : "Transparent",
                Alignment = elem.TryGetProperty("alignment", out var aProp) ? aProp.GetString() ?? "" : "",
                Gravity = elem.TryGetProperty("gravity", out var gProp) ? gProp.GetString() ?? "" : "",
                AssetPath = elem.TryGetProperty("assetPath", out var apProp) ? apProp.GetString() ?? "" : "",
                FontWeight = elem.TryGetProperty("fontWeight", out var fwProp) ? fwProp.GetString() ?? "Normal" : "Normal",
                TextAlign = elem.TryGetProperty("textAlign", out var taProp) ? taProp.GetString() ?? "Left" : "Left",
                MainAxisAlignment = elem.TryGetProperty("mainAxisAlignment", out var maaProp) ? maaProp.GetString() ?? "" : "",
                CrossAxisAlignment = elem.TryGetProperty("crossAxisAlignment", out var caaProp) ? caaProp.GetString() ?? "" : "",
                BoxFit = elem.TryGetProperty("boxFit", out var bfProp) ? bfProp.GetString() ?? "Contain" : "Contain",
                ObscureText = elem.TryGetProperty("obscureText", out var otProp) && otProp.ValueKind == JsonValueKind.True,
                ScrollDirection = elem.TryGetProperty("scrollDirection", out var sdProp) ? sdProp.GetString() ?? "Vertical" : "Vertical",
                AssetUsage = elem.TryGetProperty("assetUsage", out var auProp) ? auProp.GetString() ?? "Image" : "Image",
                HasShadow = elem.TryGetProperty("hasShadow", out var hsProp) && hsProp.ValueKind == JsonValueKind.True,
                Checked = elem.TryGetProperty("isChecked", out var icProp) && icProp.ValueKind == JsonValueKind.True,
                ActiveColor = elem.TryGetProperty("activeColor", out var acProp) ? acProp.GetString() ?? "#2196F3" : "#2196F3",
                MinValue = elem.TryGetProperty("minValue", out var minProp) && minProp.TryGetDouble(out var min) ? min : 0,
                MaxValue = elem.TryGetProperty("maxValue", out var maxProp) && maxProp.TryGetDouble(out var max) ? max : 100,
                CurrentValue = elem.TryGetProperty("currentValue", out var valProp) && valProp.TryGetDouble(out var val) ? val : 50,
                HintText = elem.TryGetProperty("hintText", out var htProp) ? htProp.GetString() ?? "" : "",
                InputType = elem.TryGetProperty("inputType", out var itProp) ? itProp.GetString() ?? "Text" : "Text",
                FontFamily = elem.TryGetProperty("fontFamily", out var ffProp) ? ffProp.GetString() ?? "" : "",
                Flex = elem.TryGetProperty("flex", out var flxProp) && flxProp.TryGetInt32(out var flx) ? flx : 1
            };

            if (elem.TryGetProperty("children", out var cProp) && cProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in cProp.EnumerateArray())
                {
                    item.Children.Add(DeserializeItem(child));
                }
            }

            return item;
        }
    }
}
