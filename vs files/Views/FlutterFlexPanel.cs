using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Snapcode.Views
{
    public enum FlutterMainAxisAlignment
    {
        Start,
        Center,
        End,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    public enum FlutterCrossAxisAlignment
    {
        Start,
        Center,
        End,
        Stretch
    }

    public enum FlutterMainAxisSize
    {
        Max,
        Min
    }

    /// <summary>
    /// Custom WinUI 3 Panel replicating Flutter's Flex layout system for Row and Column.
    /// Handles MainAxisAlignment, CrossAxisAlignment, MainAxisSize, and Expanded(flex) proportional sizing.
    /// </summary>
    public class FlutterFlexPanel : Panel
    {
        #region Attached Dependency Property: Flex

        public static readonly DependencyProperty FlexProperty =
            DependencyProperty.RegisterAttached(
                "Flex",
                typeof(int),
                typeof(FlutterFlexPanel),
                new PropertyMetadata(0, OnLayoutPropertyChanged));

        public static int GetFlex(DependencyObject obj) => (int)obj.GetValue(FlexProperty);
        public static void SetFlex(DependencyObject obj, int value) => obj.SetValue(FlexProperty, value);

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(FlutterFlexPanel),
                new PropertyMetadata(Orientation.Vertical, OnLayoutPropertyChanged));

        public static readonly DependencyProperty MainAxisAlignmentProperty =
            DependencyProperty.Register(
                nameof(MainAxisAlignment),
                typeof(FlutterMainAxisAlignment),
                typeof(FlutterFlexPanel),
                new PropertyMetadata(FlutterMainAxisAlignment.Start, OnLayoutPropertyChanged));

        public static readonly DependencyProperty CrossAxisAlignmentProperty =
            DependencyProperty.Register(
                nameof(CrossAxisAlignment),
                typeof(FlutterCrossAxisAlignment),
                typeof(FlutterFlexPanel),
                new PropertyMetadata(FlutterCrossAxisAlignment.Center, OnLayoutPropertyChanged));

        public static readonly DependencyProperty MainAxisSizeProperty =
            DependencyProperty.Register(
                nameof(MainAxisSize),
                typeof(FlutterMainAxisSize),
                typeof(FlutterFlexPanel),
                new PropertyMetadata(FlutterMainAxisSize.Max, OnLayoutPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public FlutterMainAxisAlignment MainAxisAlignment
        {
            get => (FlutterMainAxisAlignment)GetValue(MainAxisAlignmentProperty);
            set => SetValue(MainAxisAlignmentProperty, value);
        }

        public FlutterCrossAxisAlignment CrossAxisAlignment
        {
            get => (FlutterCrossAxisAlignment)GetValue(CrossAxisAlignmentProperty);
            set => SetValue(CrossAxisAlignmentProperty, value);
        }

        public FlutterMainAxisSize MainAxisSize
        {
            get => (FlutterMainAxisSize)GetValue(MainAxisSizeProperty);
            set => SetValue(MainAxisSizeProperty, value);
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlutterFlexPanel panel)
            {
                panel.InvalidateMeasure();
                panel.InvalidateArrange();
            }
            else if (d is FrameworkElement fe && fe.Parent is FlutterFlexPanel parentPanel)
            {
                parentPanel.InvalidateMeasure();
                parentPanel.InvalidateArrange();
            }
        }

        #endregion

        #region Helper Parsers

        public static FlutterMainAxisAlignment ParseMainAxisAlignment(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return FlutterMainAxisAlignment.Start;
            return val.Trim().ToLowerInvariant() switch
            {
                "center" => FlutterMainAxisAlignment.Center,
                "end" => FlutterMainAxisAlignment.End,
                "spacebetween" or "space_between" => FlutterMainAxisAlignment.SpaceBetween,
                "spacearound" or "space_around" => FlutterMainAxisAlignment.SpaceAround,
                "spaceevenly" or "space_evenly" => FlutterMainAxisAlignment.SpaceEvenly,
                _ => FlutterMainAxisAlignment.Start
            };
        }

        public static FlutterCrossAxisAlignment ParseCrossAxisAlignment(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return FlutterCrossAxisAlignment.Center;
            return val.Trim().ToLowerInvariant() switch
            {
                "start" => FlutterCrossAxisAlignment.Start,
                "end" => FlutterCrossAxisAlignment.End,
                "stretch" => FlutterCrossAxisAlignment.Stretch,
                _ => FlutterCrossAxisAlignment.Center // Flutter default is center!
            };
        }

        #endregion

        #region MeasureOverride

        protected override Size MeasureOverride(Size availableSize)
        {
            bool isVertical = Orientation == Orientation.Vertical;
            double mainAvailable = isVertical ? availableSize.Height : availableSize.Width;
            double crossAvailable = isVertical ? availableSize.Width : availableSize.Height;
            bool isMainBounded = !double.IsInfinity(mainAvailable) && !double.IsNaN(mainAvailable);

            int totalFlex = 0;
            double totalNonFlexMainSize = 0;
            double maxCrossSize = 0;

            // Pass 1: Measure non-flex children
            foreach (UIElement child in Children)
            {
                int flex = GetFlex(child);
                if (flex > 0 && isMainBounded)
                {
                    totalFlex += flex;
                }
                else
                {
                    Size childConstraint = isVertical
                        ? new Size(crossAvailable, double.PositiveInfinity)
                        : new Size(double.PositiveInfinity, crossAvailable);

                    child.Measure(childConstraint);

                    double childMain = isVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
                    double childCross = isVertical ? child.DesiredSize.Width : child.DesiredSize.Height;

                    totalNonFlexMainSize += childMain;
                    maxCrossSize = Math.Max(maxCrossSize, childCross);
                }
            }

            // Pass 2: Measure flex children (Expanded)
            double remainingMainSize = Math.Max(0, mainAvailable - totalNonFlexMainSize);
            if (totalFlex > 0 && isMainBounded)
            {
                double spacePerFlex = remainingMainSize / totalFlex;
                foreach (UIElement child in Children)
                {
                    int flex = GetFlex(child);
                    if (flex > 0)
                    {
                        double childFlexMain = spacePerFlex * flex;
                        Size childConstraint = isVertical
                            ? new Size(crossAvailable, childFlexMain)
                            : new Size(childFlexMain, crossAvailable);

                        child.Measure(childConstraint);

                        double childCross = isVertical ? child.DesiredSize.Width : child.DesiredSize.Height;
                        maxCrossSize = Math.Max(maxCrossSize, childCross);
                    }
                }
            }

            // Determine final desired main size
            double finalMainSize;
            if (MainAxisSize == FlutterMainAxisSize.Max && isMainBounded)
            {
                finalMainSize = mainAvailable;
            }
            else
            {
                finalMainSize = totalNonFlexMainSize;
                if (totalFlex > 0 && isMainBounded)
                {
                    finalMainSize += remainingMainSize;
                }
            }

            // Determine final desired cross size
            double finalCrossSize;
            if (CrossAxisAlignment == FlutterCrossAxisAlignment.Stretch && !double.IsInfinity(crossAvailable))
            {
                finalCrossSize = crossAvailable;
            }
            else
            {
                finalCrossSize = double.IsInfinity(crossAvailable) ? maxCrossSize : Math.Min(maxCrossSize, crossAvailable);
            }

            return isVertical
                ? new Size(finalCrossSize, finalMainSize)
                : new Size(finalMainSize, finalCrossSize);
        }

        #endregion

        #region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            int childCount = Children.Count;
            if (childCount == 0) return finalSize;

            bool isVertical = Orientation == Orientation.Vertical;
            double finalMain = isVertical ? finalSize.Height : finalSize.Width;
            double finalCross = isVertical ? finalSize.Width : finalSize.Height;

            int totalFlex = 0;
            double totalNonFlexMain = 0;

            foreach (UIElement child in Children)
            {
                int flex = GetFlex(child);
                if (flex > 0) totalFlex += flex;
                else totalNonFlexMain += isVertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            }

            double remainingMain = Math.Max(0, finalMain - totalNonFlexMain);
            double spacePerFlex = totalFlex > 0 ? (remainingMain / totalFlex) : 0;

            // Compute allocated main size for each child
            double[] childMainSizes = new double[childCount];
            double totalChildrenMain = 0;

            for (int i = 0; i < childCount; i++)
            {
                UIElement child = Children[i];
                int flex = GetFlex(child);
                double allocated = (flex > 0)
                    ? (spacePerFlex * flex)
                    : (isVertical ? child.DesiredSize.Height : child.DesiredSize.Width);

                childMainSizes[i] = allocated;
                totalChildrenMain += allocated;
            }

            double freeSpace = Math.Max(0, finalMain - totalChildrenMain);
            double currentMain = 0;
            double spacing = 0;

            // Apply MainAxisAlignment when there are no flex children
            if (totalFlex == 0)
            {
                switch (MainAxisAlignment)
                {
                    case FlutterMainAxisAlignment.Start:
                        currentMain = 0;
                        spacing = 0;
                        break;
                    case FlutterMainAxisAlignment.End:
                        currentMain = freeSpace;
                        spacing = 0;
                        break;
                    case FlutterMainAxisAlignment.Center:
                        currentMain = freeSpace / 2.0;
                        spacing = 0;
                        break;
                    case FlutterMainAxisAlignment.SpaceBetween:
                        currentMain = 0;
                        spacing = childCount > 1 ? (freeSpace / (childCount - 1)) : 0;
                        break;
                    case FlutterMainAxisAlignment.SpaceAround:
                        spacing = childCount > 0 ? (freeSpace / childCount) : 0;
                        currentMain = spacing / 2.0;
                        break;
                    case FlutterMainAxisAlignment.SpaceEvenly:
                        spacing = childCount > 0 ? (freeSpace / (childCount + 1)) : 0;
                        currentMain = spacing;
                        break;
                }
            }

            // Arrange each child along main and cross axes
            for (int i = 0; i < childCount; i++)
            {
                UIElement child = Children[i];
                double childMainSize = childMainSizes[i];
                double childCrossDesired = isVertical ? child.DesiredSize.Width : child.DesiredSize.Height;
                double childCrossSize;
                double childCrossPos;

                switch (CrossAxisAlignment)
                {
                    case FlutterCrossAxisAlignment.Stretch:
                        childCrossSize = finalCross;
                        childCrossPos = 0;
                        break;
                    case FlutterCrossAxisAlignment.Start:
                        childCrossSize = Math.Min(childCrossDesired, finalCross);
                        childCrossPos = 0;
                        break;
                    case FlutterCrossAxisAlignment.End:
                        childCrossSize = Math.Min(childCrossDesired, finalCross);
                        childCrossPos = finalCross - childCrossSize;
                        break;
                    case FlutterCrossAxisAlignment.Center:
                    default:
                        childCrossSize = Math.Min(childCrossDesired, finalCross);
                        childCrossPos = (finalCross - childCrossSize) / 2.0;
                        break;
                }

                Rect arrangeRect = isVertical
                    ? new Rect(childCrossPos, currentMain, childCrossSize, childMainSize)
                    : new Rect(currentMain, childCrossPos, childMainSize, childCrossSize);

                child.Arrange(arrangeRect);
                currentMain += childMainSize + spacing;
            }

            return finalSize;
        }

        #endregion
    }
}
