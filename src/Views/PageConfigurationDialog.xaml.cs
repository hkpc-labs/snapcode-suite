using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Snapcode.Models;

namespace Snapcode.Views
{
    public sealed partial class PageConfigurationDialog : ContentDialog
    {
        private PageMetadata _metadata;

        public PageConfigurationDialog(string pageName, PageMetadata? metadata)
        {
            this.InitializeComponent();

            _metadata = metadata ?? new PageMetadata { Name = pageName };
            _metadata.Name = pageName;

            LoadMetadataIntoUi();
        }

        public PageMetadata GetUpdatedMetadata()
        {
            if (TxtPageTitle != null) _metadata.Title = TxtPageTitle.Text.Trim();
            if (TxtRoutePath != null) _metadata.RoutePath = TxtRoutePath.Text.Trim();
            if (ComboPageType != null) _metadata.Type = (ComboPageType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Page";

            // AppBar
            if (ToggleShowAppBar != null) _metadata.ShowAppBar = ToggleShowAppBar.IsOn;
            if (TxtAppBarTitle != null) _metadata.AppBarTitle = TxtAppBarTitle.Text.Trim();
            if (ToggleCenterTitle != null) _metadata.AppBarCenterTitle = ToggleCenterTitle.IsOn;
            if (TxtAppBarBgColor != null) _metadata.AppBarBackgroundColor = CleanHex(TxtAppBarBgColor.Text, "#2196F3");
            if (TxtAppBarFgColor != null) _metadata.AppBarForegroundColor = CleanHex(TxtAppBarFgColor.Text, "#FFFFFF");
            if (SliderElevation != null) _metadata.AppBarElevation = SliderElevation.Value;
            if (ComboLeadingIcon != null) _metadata.AppBarLeadingIcon = (ComboLeadingIcon.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "back";
            if (ToggleAppBarActions != null) _metadata.AppBarShowActions = ToggleAppBarActions.IsOn;

            // Orientation
            if (ComboOrientation != null) _metadata.Orientation = (ComboOrientation.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Portrait";
            if (ToggleSafeArea != null) _metadata.SafeArea = ToggleSafeArea.IsOn;
            if (ToggleResizeToAvoidBottomInset != null) _metadata.ResizeToAvoidBottomInset = ToggleResizeToAvoidBottomInset.IsOn;
            if (ToggleHideStatusBar != null) _metadata.IsStatusBarHidden = ToggleHideStatusBar.IsOn;

            // Scaffold & Components
            if (TxtScaffoldBgColor != null) _metadata.ScaffoldBackgroundColor = CleanHex(TxtScaffoldBgColor.Text, "#FFFFFF");
            if (ToggleHasFAB != null) _metadata.HasFAB = ToggleHasFAB.IsOn;
            if (ComboFabIcon != null) _metadata.FabIcon = (ComboFabIcon.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "add";
            if (TxtFabColor != null) _metadata.FabBackgroundColor = CleanHex(TxtFabColor.Text, "#2196F3");
            if (TxtFabTooltip != null) _metadata.FabTooltip = TxtFabTooltip.Text.Trim();
            if (ToggleHasDrawer != null) _metadata.HasDrawer = ToggleHasDrawer.IsOn;
            if (ToggleHasBottomBar != null) _metadata.HasBottomNavigationBar = ToggleHasBottomBar.IsOn;

            return _metadata;
        }

        private void LoadMetadataIntoUi()
        {
            if (TxtPageName != null) TxtPageName.Text = _metadata.Name ?? "";
            if (TxtPageTitle != null) TxtPageTitle.Text = _metadata.Title ?? "";
            if (TxtRoutePath != null) TxtRoutePath.Text = _metadata.RoutePath ?? "";
            if (ComboPageType != null) SelectComboByTag(ComboPageType, _metadata.Type);

            // AppBar
            if (ToggleShowAppBar != null) ToggleShowAppBar.IsOn = _metadata.ShowAppBar;
            if (TxtAppBarTitle != null) TxtAppBarTitle.Text = _metadata.AppBarTitle ?? "";
            if (ToggleCenterTitle != null) ToggleCenterTitle.IsOn = _metadata.AppBarCenterTitle;
            if (TxtAppBarBgColor != null) TxtAppBarBgColor.Text = _metadata.AppBarBackgroundColor ?? "#2196F3";
            if (TxtAppBarFgColor != null) TxtAppBarFgColor.Text = _metadata.AppBarForegroundColor ?? "#FFFFFF";
            if (SliderElevation != null) SliderElevation.Value = _metadata.AppBarElevation;
            if (ComboLeadingIcon != null) SelectComboByTag(ComboLeadingIcon, _metadata.AppBarLeadingIcon);
            if (ToggleAppBarActions != null) ToggleAppBarActions.IsOn = _metadata.AppBarShowActions;
            if (AppBarDetailsPanel != null) AppBarDetailsPanel.Visibility = _metadata.ShowAppBar ? Visibility.Visible : Visibility.Collapsed;

            // Orientation
            if (ComboOrientation != null) SelectComboByTag(ComboOrientation, _metadata.Orientation);
            if (ToggleSafeArea != null) ToggleSafeArea.IsOn = _metadata.SafeArea;
            if (ToggleResizeToAvoidBottomInset != null) ToggleResizeToAvoidBottomInset.IsOn = _metadata.ResizeToAvoidBottomInset;
            if (ToggleHideStatusBar != null) ToggleHideStatusBar.IsOn = _metadata.IsStatusBarHidden;

            // Scaffold & Components
            if (TxtScaffoldBgColor != null) TxtScaffoldBgColor.Text = _metadata.ScaffoldBackgroundColor ?? "#FFFFFF";
            if (ToggleHasFAB != null) ToggleHasFAB.IsOn = _metadata.HasFAB;
            if (FabDetailsPanel != null) FabDetailsPanel.Visibility = _metadata.HasFAB ? Visibility.Visible : Visibility.Collapsed;
            if (ComboFabIcon != null) SelectComboByTag(ComboFabIcon, _metadata.FabIcon);
            if (TxtFabColor != null) TxtFabColor.Text = _metadata.FabBackgroundColor ?? "#2196F3";
            if (TxtFabTooltip != null) TxtFabTooltip.Text = _metadata.FabTooltip ?? "Action";
            if (ToggleHasDrawer != null) ToggleHasDrawer.IsOn = _metadata.HasDrawer;
            if (ToggleHasBottomBar != null) ToggleHasBottomBar.IsOn = _metadata.HasBottomNavigationBar;
        }

        private void SelectComboByTag(ComboBox combo, string? tag)
        {
            if (string.IsNullOrEmpty(tag) || combo == null) return;
            foreach (var item in combo.Items.OfType<ComboBoxItem>())
            {
                if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
        }

        private string CleanHex(string? input, string fallback)
        {
            if (string.IsNullOrWhiteSpace(input)) return fallback;
            string val = input.Trim();
            if (!val.StartsWith("#")) val = "#" + val;
            return val;
        }

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clicked)
            {
                if (TabGeneral != null) TabGeneral.IsChecked = clicked == TabGeneral;
                if (TabAppBar != null) TabAppBar.IsChecked = clicked == TabAppBar;
                if (TabOrientation != null) TabOrientation.IsChecked = clicked == TabOrientation;
                if (TabScaffold != null) TabScaffold.IsChecked = clicked == TabScaffold;

                if (PanelGeneral != null) PanelGeneral.Visibility = clicked == TabGeneral ? Visibility.Visible : Visibility.Collapsed;
                if (PanelAppBar != null) PanelAppBar.Visibility = clicked == TabAppBar ? Visibility.Visible : Visibility.Collapsed;
                if (PanelOrientation != null) PanelOrientation.Visibility = clicked == TabOrientation ? Visibility.Visible : Visibility.Collapsed;
                if (PanelScaffold != null) PanelScaffold.Visibility = clicked == TabScaffold ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleShowAppBar_Toggled(object sender, RoutedEventArgs e)
        {
            if (AppBarDetailsPanel != null && ToggleShowAppBar != null)
            {
                AppBarDetailsPanel.Visibility = ToggleShowAppBar.IsOn ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleHasFAB_Toggled(object sender, RoutedEventArgs e)
        {
            if (FabDetailsPanel != null && ToggleHasFAB != null)
            {
                FabDetailsPanel.Visibility = ToggleHasFAB.IsOn ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void AppBarColorChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string hex && TxtAppBarBgColor != null)
            {
                TxtAppBarBgColor.Text = hex;
            }
        }

        private void ScaffoldColorChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string hex && TxtScaffoldBgColor != null)
            {
                TxtScaffoldBgColor.Text = hex;
            }
        }
    }
}
