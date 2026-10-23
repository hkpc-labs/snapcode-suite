using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private int _flex = 0;
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

    public class DeviceResolutionPreset
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Mobile"; // "Mobile", "Tablet", "Desktop"
        public double Width { get; set; } = 390;
        public double Height { get; set; } = 844;

        public override string ToString() => $"{Name} ({Width} x {Height})";
    }

    public sealed partial class NativeUIDesigner : UserControl
    {
        public ObservableCollection<WidgetItem> PaletteItems { get; set; } = new();
        public ObservableCollection<WidgetItem> CanvasItems { get; set; } = new();

        public string ProjectRoot { get; set; }

        private List<DeviceResolutionPreset> _resolutionPresets = new();
        private DeviceResolutionPreset? _selectedPreset;
        private bool _isUpdatingPresetSelection = false;
        private bool _isDualPreview = false;
        private double _canvasScale = 1.0;

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
            PaletteItems.Add(new WidgetItem { Name = "Expanded", Type = "Expanded", Flex = 1, Icon = "\uE74C" });

            PaletteList.ItemsSource = PaletteItems;

            InitializeResolutionPresets();

            // Bind actions to centralized ShortcutManager
            var sm = Snapcode.Services.ShortcutManager.Instance;
            sm.BindAction("designer.undo", () => DispatcherQueue.TryEnqueue(Undo));
            sm.BindAction("designer.redo", () => DispatcherQueue.TryEnqueue(Redo));
            sm.BindAction("designer.rotate", () => DispatcherQueue.TryEnqueue(RotateCanvas));
            sm.BindAction("designer.dual_preview", () => DispatcherQueue.TryEnqueue(ToggleDualPreview));
            sm.BindAction("designer.mobile", () => DispatcherQueue.TryEnqueue(() => BtnDeviceMobile_Click(this, new RoutedEventArgs())));
            sm.BindAction("designer.tablet", () => DispatcherQueue.TryEnqueue(() => BtnDeviceTablet_Click(this, new RoutedEventArgs())));
            sm.BindAction("designer.desktop", () => DispatcherQueue.TryEnqueue(() => BtnDeviceDesktop_Click(this, new RoutedEventArgs())));
            sm.BindAction("designer.delete", () => DispatcherQueue.TryEnqueue(DeleteSelectedWidget));
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
            return AlignmentHelper.ParseColor(hex, fallback);
        }

        public class DesignerEventItem
        {
            public string EventType { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string Description { get; set; } = "";
            public string Icon { get; set; } = "\uE768";
        }

        public event EventHandler<(string WidgetId, string EventType)> OpenEventWorkspaceRequested;
        public event EventHandler<string>? OpenDrawerDesignerRequested;
        public Snapcode.Models.PageMetadata? CurrentMetadata => _currentMetadata;

        private void BtnDrawerLeading_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMetadata?.HasDrawer == true && !string.IsNullOrEmpty(_currentPageName))
            {
                OpenDrawerDesignerRequested?.Invoke(this, $"{_currentPageName}_drawer");
            }
        }

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
                if (_selectedWidget != null)
                {
                    PopulateEventsForSelectedWidget();
                }
            }
        }

        private void PopulateEventsForSelectedWidget()
        {
            if (_selectedWidget == null)
            {
                if (EventsPanel != null) EventsPanel.Visibility = Visibility.Collapsed;
                if (TxtEventsTargetWidget != null) TxtEventsTargetWidget.Text = "Available Events";
                if (ListEvents != null) ListEvents.ItemsSource = null;
                return;
            }

            if (TxtEventsTargetWidget != null)
            {
                string wName = !string.IsNullOrWhiteSpace(_selectedWidget.Name) ? _selectedWidget.Name : _selectedWidget.Id;
                TxtEventsTargetWidget.Text = $"Events: {wName} ({_selectedWidget.Type})";
            }

            var events = new List<DesignerEventItem>();
            string t = (_selectedWidget.Type ?? "").ToLowerInvariant().Trim();

            if (t == "button")
            {
                events.Add(new DesignerEventItem { EventType = "onClick", DisplayName = "onTap / onClick", Description = "Triggered when button is tapped", Icon = "\uE8A7" });
                events.Add(new DesignerEventItem { EventType = "onLongPress", DisplayName = "onLongPress", Description = "Triggered when button is pressed and held", Icon = "\uE81E" });
            }
            else if (t == "textfield" || t == "edittext")
            {
                events.Add(new DesignerEventItem { EventType = "onChanged", DisplayName = "onChanged (Text Changed)", Description = "Triggered whenever input text changes", Icon = "\uE8AC" });
                events.Add(new DesignerEventItem { EventType = "onSubmitted", DisplayName = "onSubmitted (Enter / Done)", Description = "Triggered when user submits or presses Enter", Icon = "\uE73E" });
            }
            else if (t == "slider")
            {
                events.Add(new DesignerEventItem { EventType = "onChanged", DisplayName = "onChanged (Value Changed)", Description = "Triggered when slider value changes", Icon = "\uE9E9" });
            }
            else if (t == "checkbox")
            {
                events.Add(new DesignerEventItem { EventType = "onChanged", DisplayName = "onChanged (Checked / Unchecked)", Description = "Triggered when checkbox is toggled", Icon = "\uE73A" });
            }
            else if (t == "switch")
            {
                events.Add(new DesignerEventItem { EventType = "onChanged", DisplayName = "onChanged (Toggled On / Off)", Description = "Triggered when switch is toggled", Icon = "\uF08C" });
            }
            else if (t == "image" || t == "text" || t == "textview" || t == "container" || t == "card" || t == "column" || t == "row")
            {
                events.Add(new DesignerEventItem { EventType = "onClick", DisplayName = "onTap / onClick", Description = $"Triggered when {t} is tapped", Icon = "\uE8A7" });
                events.Add(new DesignerEventItem { EventType = "onLongPress", DisplayName = "onLongPress", Description = $"Triggered when {t} is long-pressed", Icon = "\uE81E" });
            }
            else
            {
                // Fallback for custom or other widgets
                events.Add(new DesignerEventItem { EventType = "onClick", DisplayName = "onTap / onClick", Description = "Triggered on tap", Icon = "\uE8A7" });
                events.Add(new DesignerEventItem { EventType = "onLongPress", DisplayName = "onLongPress", Description = "Triggered on long press", Icon = "\uE81E" });
            }

            if (ListEvents != null)
            {
                ListEvents.ItemsSource = events;
            }
        }

        private void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListEvents.SelectedItem is DesignerEventItem item && _selectedWidget != null)
            {
                // Request opening of this event's workspace
                OpenEventWorkspaceRequested?.Invoke(this, (_selectedWidget.Id, item.EventType));
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

            if (_isDualPreview)
            {
                RenderMirror();
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
                
                PopulateEventsForSelectedWidget();
                
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
                
                bool hasBg = !string.IsNullOrWhiteSpace(_selectedWidget.BackgroundColor) && !_selectedWidget.BackgroundColor.Equals("Transparent", StringComparison.OrdinalIgnoreCase);
                var bgColor = ParseHexColor(_selectedWidget.BackgroundColor, Microsoft.UI.Colors.Transparent);
                PreviewPropBgColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(bgColor);
                PickerPropBgColor.Color = hasBg ? bgColor : Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243);

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
                
                bool hasBorder = !string.IsNullOrWhiteSpace(_selectedWidget.BorderColor) && !_selectedWidget.BorderColor.Equals("Transparent", StringComparison.OrdinalIgnoreCase);
                var borderColor = ParseHexColor(_selectedWidget.BorderColor, Microsoft.UI.Colors.Transparent);
                PreviewPropBorderColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(borderColor);
                PickerPropBorderColor.Color = hasBorder ? borderColor : Microsoft.UI.Colors.Black;
                
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
                if (ListEvents != null) ListEvents.ItemsSource = null;
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
                var newColor = args.NewColor;
                if (newColor.A == 0 && (args.OldColor.A == 0 || (newColor.R != 255 || newColor.G != 255 || newColor.B != 255)))
                {
                    newColor = Windows.UI.Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                }
                var hex = newColor.A == 255
                    ? $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}"
                    : $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _selectedWidget.BackgroundColor = hex;
                PreviewPropBgColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);
            }
        }

        private void PickerPropTextColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var newColor = args.NewColor;
                if (newColor.A == 0 && (args.OldColor.A == 0 || (newColor.R != 255 || newColor.G != 255 || newColor.B != 255)))
                {
                    newColor = Windows.UI.Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                }
                var hex = newColor.A == 255
                    ? $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}"
                    : $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _selectedWidget.TextColor = hex;
                PreviewPropTextColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);
            }
        }

        private void PickerPropBorderColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var newColor = args.NewColor;
                if (newColor.A == 0 && (args.OldColor.A == 0 || (newColor.R != 255 || newColor.G != 255 || newColor.B != 255)))
                {
                    newColor = Windows.UI.Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                }
                var hex = newColor.A == 255
                    ? $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}"
                    : $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _selectedWidget.BorderColor = hex;
                PreviewPropBorderColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);

                // If BorderWidth was not set or 0, automatically default to 1 so the border is immediately visible!
                if (string.IsNullOrWhiteSpace(_selectedWidget.BorderWidth) || _selectedWidget.BorderWidth == "0")
                {
                    _selectedWidget.BorderWidth = "1";
                    InputPropBorderWidth.Text = "1";
                }
            }
        }

        private void PickerPropActiveColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var newColor = args.NewColor;
                if (newColor.A == 0 && (args.OldColor.A == 0 || (newColor.R != 255 || newColor.G != 255 || newColor.B != 255)))
                {
                    newColor = Windows.UI.Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                }
                var hex = newColor.A == 255
                    ? $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}"
                    : $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _selectedWidget.ActiveColor = hex;
                PreviewPropActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);
            }
        }

        private void PickerPropSliderActiveColor_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            if (_isUpdatingSelectionUI) return;
            if (_selectedWidget != null)
            {
                var newColor = args.NewColor;
                if (newColor.A == 0 && (args.OldColor.A == 0 || (newColor.R != 255 || newColor.G != 255 || newColor.B != 255)))
                {
                    newColor = Windows.UI.Color.FromArgb(255, newColor.R, newColor.G, newColor.B);
                }
                var hex = newColor.A == 255
                    ? $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}"
                    : $"#{newColor.A:X2}{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _selectedWidget.ActiveColor = hex;
                PreviewPropSliderActiveColor.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(newColor);
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
                    else if (t == "Expanded")
                    {
                        newItem.Flex = 1;
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
        public event EventHandler SaveRequested;

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

            if (_isDualPreview)
            {
                RenderMirror();
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

        private string _currentPageName = "main";
        private bool _isLandscape = false;
        private Snapcode.Models.PageMetadata? _currentMetadata;

        private void InitializeResolutionPresets()
        {
            _resolutionPresets = new List<DeviceResolutionPreset>
            {
                new DeviceResolutionPreset { Name = "Compact Phone", Category = "Mobile", Width = 360, Height = 640 },
                new DeviceResolutionPreset { Name = "Standard Phone", Category = "Mobile", Width = 390, Height = 844 },
                new DeviceResolutionPreset { Name = "Large Phone", Category = "Mobile", Width = 412, Height = 915 },
                new DeviceResolutionPreset { Name = "Small Tablet (8\")", Category = "Tablet", Width = 600, Height = 960 },
                new DeviceResolutionPreset { Name = "Standard Tablet (10\")", Category = "Tablet", Width = 800, Height = 1280 },
                new DeviceResolutionPreset { Name = "Large Tablet (12.9\")", Category = "Tablet", Width = 1024, Height = 1366 },
                new DeviceResolutionPreset { Name = "Laptop (720p)", Category = "Desktop", Width = 1280, Height = 720 },
                new DeviceResolutionPreset { Name = "Standard Laptop", Category = "Desktop", Width = 1366, Height = 768 },
                new DeviceResolutionPreset { Name = "Full HD Desktop", Category = "Desktop", Width = 1920, Height = 1080 }
            };

            _selectedPreset = _resolutionPresets[1]; // Standard Phone (390 x 844)
            if (ComboDeviceResolution != null)
            {
                ComboDeviceResolution.ItemsSource = _resolutionPresets;
                ComboDeviceResolution.SelectedItem = _selectedPreset;
            }
            UpdateCanvasOrientation(animate: false);
        }

        private void BtnDeviceMobile_Click(object sender, RoutedEventArgs e)
        {
            _isLandscape = false;
            SelectPresetByCategory("Mobile", 390, 844);
        }

        private void BtnDeviceTablet_Click(object sender, RoutedEventArgs e)
        {
            _isLandscape = false;
            SelectPresetByCategory("Tablet", 800, 1280);
        }

        private void BtnDeviceDesktop_Click(object sender, RoutedEventArgs e)
        {
            _isLandscape = true;
            SelectPresetByCategory("Desktop", 1366, 768);
        }

        private void SelectPresetByCategory(string category, double preferredW, double preferredH)
        {
            var preset = _resolutionPresets.FirstOrDefault(p => p.Category == category && p.Width == preferredW && p.Height == preferredH)
                         ?? _resolutionPresets.FirstOrDefault(p => p.Category == category);
            if (preset != null && ComboDeviceResolution != null)
            {
                _isUpdatingPresetSelection = true;
                ComboDeviceResolution.SelectedItem = preset;
                _isUpdatingPresetSelection = false;
                ApplyDeviceResolution(preset, animate: true);
            }
        }

        private void ComboDeviceResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingPresetSelection) return;
            if (ComboDeviceResolution?.SelectedItem is DeviceResolutionPreset preset)
            {
                if (preset.Category == "Desktop" && !_isLandscape)
                {
                    _isLandscape = true;
                }
                else if (preset.Category != "Desktop" && _isLandscape && _selectedPreset?.Category == "Desktop")
                {
                    _isLandscape = false;
                }
                ApplyDeviceResolution(preset, animate: true);
            }
        }

        private void UpdateCategoryButtonStyles(string activeCategory)
        {
            if (BtnDeviceMobile == null || BtnDeviceTablet == null || BtnDeviceDesktop == null) return;

            var accentBg = Application.Current.Resources.TryGetValue("AccentFillColorDefaultBrush", out var aBg) && aBg is Microsoft.UI.Xaml.Media.Brush ab ? ab : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 215));
            var accentFg = Application.Current.Resources.TryGetValue("TextOnAccentFillColorPrimaryBrush", out var aFg) && aFg is Microsoft.UI.Xaml.Media.Brush af ? af : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
            var transBg = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            var normFg = Application.Current.Resources.TryGetValue("TextFillColorPrimaryBrush", out var nFg) && nFg is Microsoft.UI.Xaml.Media.Brush nf ? nf : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);

            BtnDeviceMobile.Background = activeCategory == "Mobile" ? accentBg : transBg;
            BtnDeviceMobile.Foreground = activeCategory == "Mobile" ? accentFg : normFg;

            BtnDeviceTablet.Background = activeCategory == "Tablet" ? accentBg : transBg;
            BtnDeviceTablet.Foreground = activeCategory == "Tablet" ? accentFg : normFg;

            BtnDeviceDesktop.Background = activeCategory == "Desktop" ? accentBg : transBg;
            BtnDeviceDesktop.Foreground = activeCategory == "Desktop" ? accentFg : normFg;
        }

        private void ApplyDeviceResolution(DeviceResolutionPreset preset, bool animate = true)
        {
            if (preset == null) return;
            _selectedPreset = preset;
            UpdateCategoryButtonStyles(preset.Category);
            UpdateCanvasOrientation(animate);
        }

        private (double width, double height) GetDimensionsForOrientation(DeviceResolutionPreset? preset, bool isLandscape)
        {
            if (preset == null) return (390, 844);
            double minDim = Math.Min(preset.Width, preset.Height);
            double maxDim = Math.Max(preset.Width, preset.Height);

            return isLandscape ? (maxDim, minDim) : (minDim, maxDim);
        }

        public void SetPageName(string name)
        {
            _currentPageName = name;
            if (_currentMetadata != null)
            {
                ApplyPageMetadata(_currentMetadata);
            }
            else
            {
                if (TxtAppTitle != null) TxtAppTitle.Text = $"{name}.dart";
                if (SecondaryTxtAppTitle != null) SecondaryTxtAppTitle.Text = $"{name}.dart";
            }
        }

        private void BtnRotateCanvas_Click(object sender, RoutedEventArgs e)
        {
            _isLandscape = !_isLandscape;
            UpdateCanvasOrientation(animate: true);
            if (_isDualPreview)
            {
                RenderMirror();
            }
        }

        public void UpdateCanvasOrientation(bool animate = true)
        {
            if (_selectedPreset == null && _resolutionPresets != null && _resolutionPresets.Count > 1)
            {
                _selectedPreset = _resolutionPresets[1];
            }

            var (pW, pH) = GetDimensionsForOrientation(_selectedPreset, _isLandscape);
            var (sW, sH) = GetDimensionsForOrientation(_selectedPreset, !_isLandscape);

            if (PhoneFrameBorder != null)
            {
                if (animate)
                {
                    AnimateFrameSize(PhoneFrameBorder, pW, pH);
                }
                else
                {
                    PhoneFrameBorder.Width = pW;
                    PhoneFrameBorder.Height = pH;
                }
            }

            if (SecondaryFrameBorder != null && _isDualPreview)
            {
                if (animate)
                {
                    AnimateFrameSize(SecondaryFrameBorder, sW, sH);
                }
                else
                {
                    SecondaryFrameBorder.Width = sW;
                    SecondaryFrameBorder.Height = sH;
                }
            }

            string primaryOrientText = _isLandscape ? "Landscape" : "Portrait";
            string secondaryOrientText = !_isLandscape ? "Landscape" : "Portrait";

            if (TxtCanvasInfo != null)
            {
                TxtCanvasInfo.Text = $"{pW} x {pH} ({primaryOrientText})";
            }
            if (BtnRotateCanvas != null)
            {
                ToolTipService.SetToolTip(BtnRotateCanvas, $"Rotate Canvas Orientation (Ctrl+R) - Current: {primaryOrientText}");
            }
            if (TxtPrimaryOrientationBadge != null)
            {
                TxtPrimaryOrientationBadge.Text = $"{primaryOrientText} ({pW}x{pH})";
            }
            if (TxtSecondaryOrientationBadge != null)
            {
                TxtSecondaryOrientationBadge.Text = $"{secondaryOrientText} ({sW}x{sH})";
            }

            CheckDisplayAccommodateSize();
        }

        private void AnimateFrameSize(FrameworkElement element, double targetWidth, double targetHeight, Action? onCompleted = null)
        {
            if (double.IsNaN(element.Width) || element.Width <= 0) element.Width = targetWidth;
            if (double.IsNaN(element.Height) || element.Height <= 0) element.Height = targetHeight;

            if (Math.Abs(element.Width - targetWidth) < 1 && Math.Abs(element.Height - targetHeight) < 1)
            {
                element.Width = targetWidth;
                element.Height = targetHeight;
                onCompleted?.Invoke();
                return;
            }

            var sb = new Microsoft.UI.Xaml.Media.Animation.Storyboard();

            var animW = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = element.Width,
                To = targetWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(280)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animW, element);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animW, "Width");
            sb.Children.Add(animW);

            var animH = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = element.Height,
                To = targetHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(280)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animH, element);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animH, "Height");
            sb.Children.Add(animH);

            sb.Completed += (s, e) =>
            {
                element.Width = targetWidth;
                element.Height = targetHeight;
                onCompleted?.Invoke();
            };

            sb.Begin();
        }

        private void BtnDualPreview_Click(object sender, RoutedEventArgs e)
        {
            _isDualPreview = BtnDualPreview != null && BtnDualPreview.IsChecked == true;
            UpdateDualPreviewLayout(animate: true);
            if (_isDualPreview)
            {
                RenderMirror();
            }
        }

        private void UpdateDualPreviewLayout(bool animate = true)
        {
            if (SecondaryFrameWrapper == null || SecondaryFrameBorder == null) return;

            if (_isDualPreview)
            {
                var (sW, sH) = GetDimensionsForOrientation(_selectedPreset, !_isLandscape);
                SecondaryFrameBorder.Width = sW;
                SecondaryFrameBorder.Height = sH;
                string secondaryOrientText = !_isLandscape ? "Landscape" : "Portrait";
                if (TxtSecondaryOrientationBadge != null)
                {
                    TxtSecondaryOrientationBadge.Text = $"{secondaryOrientText} ({sW}x{sH})";
                }

                SecondaryFrameWrapper.Visibility = Visibility.Visible;

                if (animate && SecondaryTransform != null)
                {
                    SecondaryTransform.X = 50;
                    SecondaryFrameWrapper.Opacity = 0;

                    var sb = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
                    var animX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 50,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                        EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                        EnableDependentAnimation = true
                    };
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animX, SecondaryTransform);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animX, "X");
                    sb.Children.Add(animX);

                    var animOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 0,
                        To = 1.0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(280)),
                        EnableDependentAnimation = true
                    };
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animOp, SecondaryFrameWrapper);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animOp, "Opacity");
                    sb.Children.Add(animOp);

                    sb.Begin();
                }
                else
                {
                    if (SecondaryTransform != null) SecondaryTransform.X = 0;
                    SecondaryFrameWrapper.Opacity = 1.0;
                }
            }
            else
            {
                if (animate && SecondaryTransform != null)
                {
                    var sb = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
                    var animX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 0,
                        To = 40,
                        Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                        EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn },
                        EnableDependentAnimation = true
                    };
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animX, SecondaryTransform);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animX, "X");
                    sb.Children.Add(animX);

                    var animOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                    {
                        From = 1.0,
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                        EnableDependentAnimation = true
                    };
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animOp, SecondaryFrameWrapper);
                    Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animOp, "Opacity");
                    sb.Children.Add(animOp);

                    sb.Completed += (s, e) =>
                    {
                        SecondaryFrameWrapper.Visibility = Visibility.Collapsed;
                    };
                    sb.Begin();
                }
                else
                {
                    SecondaryFrameWrapper.Opacity = 0;
                    SecondaryFrameWrapper.Visibility = Visibility.Collapsed;
                }
            }

            CheckDisplayAccommodateSize();
        }

        private void BtnSetSecondaryAsPrimary_Click(object sender, RoutedEventArgs e)
        {
            _isLandscape = !_isLandscape;
            UpdateCanvasOrientation(animate: true);
            if (_isDualPreview)
            {
                RenderMirror();
            }
        }

        private void ComboCanvasScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboCanvasScale?.SelectedItem is ComboBoxItem item &&
                item.Tag != null &&
                double.TryParse(item.Tag.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double scale))
            {
                _canvasScale = scale;
                if (CanvasScaleTransform != null)
                {
                    CanvasScaleTransform.ScaleX = scale;
                    CanvasScaleTransform.ScaleY = scale;
                }
                CheckDisplayAccommodateSize();
            }
        }

        private void CanvasScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckDisplayAccommodateSize();
        }

        public void CheckDisplayAccommodateSize()
        {
            if (CanvasScrollViewer == null || CanvasContainer == null) return;

            double availableWidth = CanvasScrollViewer.ActualWidth;
            if (availableWidth <= 0) return;

            if (_isDualPreview)
            {
                var (pW, pH) = GetDimensionsForOrientation(_selectedPreset, _isLandscape);
                var (sW, sH) = GetDimensionsForOrientation(_selectedPreset, !_isLandscape);

                // Total required width for side-by-side: (primary + secondary + spacing + margins) * scale
                double spacingAndPadding = 32 + 28 + 32;
                double requiredWidthSideBySide = (pW + sW + spacingAndPadding) * _canvasScale;

                if (availableWidth < requiredWidthSideBySide)
                {
                    // Not enough width for side-by-side -> stack vertically
                    if (CanvasContainer.Orientation != Orientation.Vertical)
                    {
                        CanvasContainer.Orientation = Orientation.Vertical;
                    }
                    if (InfoDualPreviewNotice != null)
                    {
                        InfoDualPreviewNotice.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    // Ample width -> arrange horizontally side-by-side
                    if (CanvasContainer.Orientation != Orientation.Horizontal)
                    {
                        CanvasContainer.Orientation = Orientation.Horizontal;
                    }
                    if (InfoDualPreviewNotice != null)
                    {
                        InfoDualPreviewNotice.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                if (CanvasContainer.Orientation != Orientation.Horizontal)
                {
                    CanvasContainer.Orientation = Orientation.Horizontal;
                }
                if (InfoDualPreviewNotice != null)
                {
                    InfoDualPreviewNotice.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnCloseNotice_Click(object sender, RoutedEventArgs e)
        {
            if (InfoDualPreviewNotice != null)
            {
                InfoDualPreviewNotice.Visibility = Visibility.Collapsed;
            }
        }

        public void RenderMirror()
        {
            if (SecondaryRootCanvas == null || !_isDualPreview) return;
            SecondaryRootCanvas.Children.Clear();
            foreach (var item in CanvasItems)
            {
                SecondaryRootCanvas.Children.Add(new DesignerNodeControl(item, this, parentNode: null, isReadOnlyMirror: true));
            }
        }

        /// <summary>
        /// Animates the designer canvas with a smooth directional slide and cross-fade transition
        /// during page switching, providing clear tactile visual feedback without UI hitching.
        /// </summary>
        public async Task AnimatePageTransitionAsync(int direction, Action updateContentAction)
        {
            if (PrimaryTransform == null || PrimaryFrameWrapper == null)
            {
                updateContentAction();
                return;
            }

            double exitTargetX = direction >= 0 ? -36 : 36;
            double enterStartX = direction >= 0 ? 36 : -36;

            var exitTcs = new TaskCompletionSource<bool>();
            var exitSb = new Microsoft.UI.Xaml.Media.Animation.Storyboard();

            var animExitX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = exitTargetX,
                Duration = new Duration(TimeSpan.FromMilliseconds(110)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn },
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animExitX, PrimaryTransform);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animExitX, "X");
            exitSb.Children.Add(animExitX);

            var animExitOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = PrimaryFrameWrapper.Opacity,
                To = 0.15,
                Duration = new Duration(TimeSpan.FromMilliseconds(110)),
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animExitOp, PrimaryFrameWrapper);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animExitOp, "Opacity");
            exitSb.Children.Add(animExitOp);

            bool animateSecondary = _isDualPreview && SecondaryTransform != null && SecondaryFrameWrapper != null && SecondaryFrameWrapper.Visibility == Visibility.Visible;
            if (animateSecondary)
            {
                var animSecExitX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = exitTargetX,
                    Duration = new Duration(TimeSpan.FromMilliseconds(110)),
                    EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseIn },
                    EnableDependentAnimation = true
                };
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animSecExitX, SecondaryTransform!);
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animSecExitX, "X");
                exitSb.Children.Add(animSecExitX);

                var animSecExitOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                {
                    From = SecondaryFrameWrapper!.Opacity,
                    To = 0.15,
                    Duration = new Duration(TimeSpan.FromMilliseconds(110)),
                    EnableDependentAnimation = true
                };
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animSecExitOp, SecondaryFrameWrapper);
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animSecExitOp, "Opacity");
                exitSb.Children.Add(animSecExitOp);
            }

            exitSb.Completed += (s, e) => exitTcs.TrySetResult(true);
            exitSb.Begin();
            await exitTcs.Task;

            // Apply content updates (new page metadata, name, and visual widget tree)
            try
            {
                updateContentAction();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NativeUIDesigner] Content update error during page transition: {ex.Message}");
            }

            // Reposition to opposite side for entrance slide-in
            PrimaryTransform.X = enterStartX;
            PrimaryFrameWrapper.Opacity = 0.15;
            if (animateSecondary)
            {
                SecondaryTransform!.X = enterStartX;
                SecondaryFrameWrapper!.Opacity = 0.15;
            }

            var enterTcs = new TaskCompletionSource<bool>();
            var enterSb = new Microsoft.UI.Xaml.Media.Animation.Storyboard();

            var animEnterX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = enterStartX,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animEnterX, PrimaryTransform);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animEnterX, "X");
            enterSb.Children.Add(animEnterX);

            var animEnterOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
            {
                From = 0.15,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                EnableDependentAnimation = true
            };
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animEnterOp, PrimaryFrameWrapper);
            Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animEnterOp, "Opacity");
            enterSb.Children.Add(animEnterOp);

            if (animateSecondary)
            {
                var animSecEnterX = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                {
                    From = enterStartX,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                    EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                    EnableDependentAnimation = true
                };
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animSecEnterX, SecondaryTransform!);
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animSecEnterX, "X");
                enterSb.Children.Add(animSecEnterX);

                var animSecEnterOp = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
                {
                    From = 0.15,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                    EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseOut },
                    EnableDependentAnimation = true
                };
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animSecEnterOp, SecondaryFrameWrapper!);
                Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animSecEnterOp, "Opacity");
                enterSb.Children.Add(animSecEnterOp);
            }

            enterSb.Completed += (s, e) =>
            {
                PrimaryTransform.X = 0;
                PrimaryFrameWrapper.Opacity = 1.0;
                if (animateSecondary)
                {
                    SecondaryTransform!.X = 0;
                    SecondaryFrameWrapper!.Opacity = 1.0;
                    RenderMirror();
                }
                enterTcs.TrySetResult(true);
            };
            enterSb.Begin();
            await enterTcs.Task;
        }

        public void ApplyPageMetadata(Snapcode.Models.PageMetadata? metadata)
        {
            _currentMetadata = metadata;
            if (metadata == null) return;

            // Orientation
            if (string.Equals(metadata.Orientation, "Landscape", StringComparison.OrdinalIgnoreCase))
            {
                _isLandscape = true;
            }
            else if (string.Equals(metadata.Orientation, "Portrait", StringComparison.OrdinalIgnoreCase))
            {
                _isLandscape = false;
            }
            UpdateCanvasOrientation(animate: false);

            // Status Bar
            var sbVis = metadata.IsStatusBarHidden ? Visibility.Collapsed : Visibility.Visible;
            if (MockStatusBar != null) MockStatusBar.Visibility = sbVis;
            if (SecondaryMockStatusBar != null) SecondaryMockStatusBar.Visibility = sbVis;

            bool isDrawer = metadata.Type == "Drawer" || (!string.IsNullOrEmpty(_currentPageName) && _currentPageName.EndsWith("_drawer", StringComparison.OrdinalIgnoreCase));
            if (MockDrawerHeaderBanner != null)
            {
                MockDrawerHeaderBanner.Visibility = isDrawer ? Visibility.Visible : Visibility.Collapsed;
            }

            if (isDrawer)
            {
                if (MockStatusBar != null) MockStatusBar.Visibility = Visibility.Collapsed;
                if (PhoneFrameBorder != null)
                {
                    PhoneFrameBorder.Width = 304;
                    PhoneFrameBorder.CornerRadius = new CornerRadius(0, 16, 16, 0);
                }
            }
            else
            {
                if (PhoneFrameBorder != null)
                {
                    PhoneFrameBorder.Width = _isLandscape ? 844 : 390;
                    PhoneFrameBorder.CornerRadius = new CornerRadius(16);
                }
            }

            // MockToolBar / AppBar
            var barFgBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
            if (!string.IsNullOrEmpty(metadata.AppBarForegroundColor))
            {
                var barFg = ParseHexColor(metadata.AppBarForegroundColor, Microsoft.UI.Colors.White);
                barFgBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(barFg);
            }

            var tbVis = (metadata.ShowAppBar && !isDrawer) ? Visibility.Visible : Visibility.Collapsed;
            if (MockToolBar != null)
            {
                MockToolBar.Visibility = tbVis;
                if (!string.IsNullOrEmpty(metadata.AppBarBackgroundColor))
                {
                    var barBg = ParseHexColor(metadata.AppBarBackgroundColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243));
                    MockToolBar.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(barBg);
                }
            }
            if (SecondaryMockToolBar != null)
            {
                SecondaryMockToolBar.Visibility = tbVis;
                if (!string.IsNullOrEmpty(metadata.AppBarBackgroundColor))
                {
                    var barBg = ParseHexColor(metadata.AppBarBackgroundColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243));
                    SecondaryMockToolBar.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(barBg);
                }
            }

            // AppBar Leading Icon (Drawer or Back button)
            if (BtnDrawerLeading != null)
            {
                if (metadata.HasDrawer || metadata.AppBarLeadingIcon == "menu")
                {
                    BtnDrawerLeading.Visibility = Visibility.Visible;
                    BtnDrawerLeading.Content = "\uE700"; // Hamburger drawer icon
                    BtnDrawerLeading.Foreground = barFgBrush;
                }
                else if (metadata.AppBarLeadingIcon == "back")
                {
                    BtnDrawerLeading.Visibility = Visibility.Visible;
                    BtnDrawerLeading.Content = "\uE72B"; // Back arrow icon
                    BtnDrawerLeading.Foreground = barFgBrush;
                }
                else
                {
                    BtnDrawerLeading.Visibility = Visibility.Collapsed;
                }
            }

            // Sync AppBar action icons visibility and foreground
            if (MockAppBarActions != null)
            {
                MockAppBarActions.Visibility = metadata.AppBarShowActions ? Visibility.Visible : Visibility.Collapsed;
                if (MockActionSearch != null) MockActionSearch.Foreground = barFgBrush;
                if (MockActionMore != null) MockActionMore.Foreground = barFgBrush;
            }

            string title = !string.IsNullOrWhiteSpace(metadata.AppBarTitle)
                ? metadata.AppBarTitle
                : (!string.IsNullOrWhiteSpace(metadata.Title) ? metadata.Title : $"{_currentPageName}.dart");

            if (TxtAppTitle != null)
            {
                TxtAppTitle.Text = title;
                TxtAppTitle.Foreground = barFgBrush;
                TxtAppTitle.HorizontalAlignment = metadata.AppBarCenterTitle ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                TxtAppTitle.Margin = (metadata.AppBarCenterTitle || BtnDrawerLeading?.Visibility != Visibility.Visible)
                    ? new Thickness(16, 0, 0, 0)
                    : new Thickness(52, 0, 0, 0);
            }
            if (SecondaryTxtAppTitle != null)
            {
                SecondaryTxtAppTitle.Text = title;
                SecondaryTxtAppTitle.Foreground = barFgBrush;
                SecondaryTxtAppTitle.HorizontalAlignment = metadata.AppBarCenterTitle ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            }

            // AppBar Elevation Shadow
            if (MockAppBarShadow != null)
            {
                if (metadata.ShowAppBar && metadata.AppBarElevation > 0)
                {
                    MockAppBarShadow.Visibility = Visibility.Visible;
                    MockAppBarShadow.Height = Math.Max(4, Math.Min(16, metadata.AppBarElevation * 1.5));
                    MockAppBarShadow.Opacity = Math.Min(1.0, 0.3 + (metadata.AppBarElevation * 0.07));
                }
                else
                {
                    MockAppBarShadow.Visibility = Visibility.Collapsed;
                }
            }
            if (SecondaryMockAppBarShadow != null)
            {
                if (metadata.ShowAppBar && metadata.AppBarElevation > 0)
                {
                    SecondaryMockAppBarShadow.Visibility = Visibility.Visible;
                    SecondaryMockAppBarShadow.Height = Math.Max(4, Math.Min(16, metadata.AppBarElevation * 1.5));
                    SecondaryMockAppBarShadow.Opacity = Math.Min(1.0, 0.3 + (metadata.AppBarElevation * 0.07));
                }
                else
                {
                    SecondaryMockAppBarShadow.Visibility = Visibility.Collapsed;
                }
            }

            // Floating Action Button
            var fabVis = metadata.HasFAB ? Visibility.Visible : Visibility.Collapsed;
            if (MockFAB != null)
            {
                MockFAB.Visibility = fabVis;
                if (metadata.HasFAB)
                {
                    var fabBg = ParseHexColor(metadata.FabBackgroundColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243));
                    MockFAB.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(fabBg);
                    if (MockFABIcon != null)
                    {
                        MockFABIcon.Glyph = GetFabGlyph(metadata.FabIcon);
                    }
                }
            }
            if (SecondaryMockFAB != null)
            {
                SecondaryMockFAB.Visibility = fabVis;
                if (metadata.HasFAB)
                {
                    var fabBg = ParseHexColor(metadata.FabBackgroundColor, Microsoft.UI.ColorHelper.FromArgb(255, 33, 150, 243));
                    SecondaryMockFAB.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(fabBg);
                    if (SecondaryMockFABIcon != null)
                    {
                        SecondaryMockFABIcon.Glyph = GetFabGlyph(metadata.FabIcon);
                    }
                }
            }

            // Scaffold background
            if (RootCanvas != null && !string.IsNullOrEmpty(metadata.ScaffoldBackgroundColor))
            {
                var scaffoldBg = ParseHexColor(metadata.ScaffoldBackgroundColor, Microsoft.UI.Colors.White);
                RootCanvas.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(scaffoldBg);
                if (SecondaryRootCanvas != null)
                {
                    SecondaryRootCanvas.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(scaffoldBg);
                }
            }
        }

        private string GetFabGlyph(string? iconName)
        {
            return (iconName ?? "").ToLowerInvariant() switch
            {
                "edit" => "\uE70F",
                "check" => "\uE73E",
                "camera" => "\uE722",
                "share" => "\uE72D",
                "message" => "\uE8BD",
                _ => "\uE710" // add
            };
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
                UIStateChanged?.Invoke(this, EventArgs.Empty);
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
                UIStateChanged?.Invoke(this, EventArgs.Empty);
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

        private void BtnTopUndo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void BtnTopRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        public void RotateCanvas()
        {
            _isLandscape = !_isLandscape;
            UpdateCanvasOrientation(animate: true);
            if (_isDualPreview)
            {
                RenderMirror();
            }
        }

        public void ToggleDualPreview()
        {
            if (BtnDualPreview != null)
            {
                BtnDualPreview.IsChecked = !(BtnDualPreview.IsChecked == true);
                BtnDualPreview_Click(BtnDualPreview, new RoutedEventArgs());
            }
        }

        public void DeleteSelectedWidget()
        {
            if (_selectedWidget != null)
            {
                RemoveWidgetFromTree(CanvasItems, _selectedWidget);
                SelectNode(null);
                RenderRoot();
                SaveStateForUndo();
            }
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

        private void Rotate_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            RotateCanvas();
            args.Handled = true;
        }

        private void DualPreview_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            ToggleDualPreview();
            args.Handled = true;
        }

        private void Mobile_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            BtnDeviceMobile_Click(this, new RoutedEventArgs());
            args.Handled = true;
        }

        private void Tablet_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            BtnDeviceTablet_Click(this, new RoutedEventArgs());
            args.Handled = true;
        }

        private void Desktop_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            BtnDeviceDesktop_Click(this, new RoutedEventArgs());
            args.Handled = true;
        }

        private void Delete_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(this.XamlRoot);
            if (focused is not TextBox && focused is not PasswordBox && focused is not RichEditBox)
            {
                DeleteSelectedWidget();
                args.Handled = true;
            }
        }

        private void Save_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
            args.Handled = true;
        }

        private void UserControl_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(this.XamlRoot);
            bool isTextEditing = focused is TextBox || focused is PasswordBox || focused is RichEditBox;

            var mods = Snapcode.Services.ShortcutManager.GetCurrentModifiers();

            if (isTextEditing)
            {
                // In text boxes, allow native text edit shortcuts
                if (Snapcode.Services.ShortcutManager.IsStandardTextEditingShortcut(e.Key, mods))
                {
                    return;
                }
            }

            // Route through Centralized ShortcutManager
            if (Snapcode.Services.ShortcutManager.Instance.TryExecute(e.Key, mods, "Designer"))
            {
                e.Handled = true;
            }
        }

        public void LoadUIJson(string json)
        {
            SelectNode(null);
            CanvasItems.Clear();
            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "{}")
            {
                RenderRoot();
                OnWidgetTreeChanged(null);
                if (!_isRestoringState)
                {
                    _undoStack.Clear();
                    _redoStack.Clear();
                    _undoStack.Push(GetUIJson());
                }
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

                if (!_isRestoringState)
                {
                    _undoStack.Clear();
                    _redoStack.Clear();
                    _undoStack.Push(GetUIJson());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load native UI JSON: {ex.Message}");
            }
        }

        public static WidgetItem DeserializeItem(JsonElement elem)
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
                Flex = elem.TryGetProperty("flex", out var flxProp) && flxProp.TryGetInt32(out var flx)
                    ? (type == "Expanded" ? (flx > 0 ? flx : 1) : 0)
                    : (type == "Expanded" ? 1 : 0)
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
