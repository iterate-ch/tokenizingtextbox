﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace tokenizingtextbox
{
    public partial class WrapPanel : Panel
    {
        /// <summary>
        /// Identifies the <see cref="HorizontalSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(
                nameof(HorizontalSpacing),
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(0d, LayoutPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(WrapPanel),
                new PropertyMetadata(Orientation.Horizontal, LayoutPropertyChanged));

        /// <summary>
        /// Identifies the Padding dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="Padding"/> dependency property.</returns>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(WrapPanel),
                new PropertyMetadata(default(Thickness), LayoutPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="StretchChild"/> dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="StretchChild"/> dependency property.</returns>
        public static readonly DependencyProperty StretchChildProperty =
            DependencyProperty.Register(
                nameof(StretchChild),
                typeof(StretchChild),
                typeof(WrapPanel),
                new PropertyMetadata(StretchChild.None, LayoutPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="VerticalSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(
                nameof(VerticalSpacing),
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(0d, LayoutPropertyChanged));

        /// <summary>
        /// Gets or sets a uniform Horizontal distance (in pixels) between items when <see cref="Orientation"/> is set to Horizontal,
        /// or between columns of items when <see cref="Orientation"/> is set to Vertical.
        /// </summary>
        public double HorizontalSpacing
        {
            get { return (double)GetValue(HorizontalSpacingProperty); }
            set { SetValue(HorizontalSpacingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the orientation of the WrapPanel.
        /// Horizontal means that child controls will be added horizontally until the width of the panel is reached, then a new row is added to add new child controls.
        /// Vertical means that children will be added vertically until the height of the panel is reached, then a new column is added.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the distance between the border and its child object.
        /// </summary>
        /// <returns>
        /// The dimensions of the space between the border and its child as a Thickness value.
        /// Thickness is a structure that stores dimension values using pixel measures.
        /// </returns>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating how to arrange child items
        /// </summary>
        public StretchChild StretchChild
        {
            get { return (StretchChild)GetValue(StretchChildProperty); }
            set { SetValue(StretchChildProperty, value); }
        }

        /// <summary>
        /// Gets or sets a uniform Vertical distance (in pixels) between items when <see cref="Orientation"/> is set to Vertical,
        /// or between rows of items when <see cref="Orientation"/> is set to Horizontal.
        /// </summary>
        public double VerticalSpacing
        {
            get { return (double)GetValue(VerticalSpacingProperty); }
            set { SetValue(VerticalSpacingProperty, value); }
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count > 0)
            {
                var parentMeasure = new UvMeasure(Orientation, finalSize.Width, finalSize.Height);
                var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
                var paddingStart = new UvMeasure(Orientation, Padding.Left, Padding.Top);
                var paddingEnd = new UvMeasure(Orientation, Padding.Right, Padding.Bottom);
                var position = new UvMeasure(Orientation, Padding.Left, Padding.Top);

                double currentV = 0;
                void arrange(UIElement child, bool isLast = false)
                {
                    if (child is FrameworkElement fe && fe.TemplateChild is Panel nestedPanel)
                    {
                        if (nestedPanel.Children.Count > 0)
                        {
                            var nestedIndex = nestedPanel.Children.Count;
                            for (var i = 0; i < nestedIndex; i++)
                            {
                                arrange(nestedPanel.Children[i], isLast && (nestedIndex - i) == 1);
                            }
                        }
                        return;
                    }

                    var desiredMeasure = new UvMeasure(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                    if (desiredMeasure.U == 0)
                    {
                        return; // if an item is collapsed, avoid adding the spacing
                    }

                    if ((desiredMeasure.U + position.U + paddingEnd.U) > parentMeasure.U)
                    {
                        // next row!
                        position.U = paddingStart.U;
                        position.V += currentV + spacingMeasure.V;
                        currentV = 0;
                    }

                    // Stretch the last item to fill the available space
                    if (isLast && StretchChild == StretchChild.Last)
                    {
                        desiredMeasure.U = parentMeasure.U - position.U;
                    }

                    // place the item
                    if (Orientation == Orientation.Horizontal)
                    {
                        child.Arrange(new Rect(position.U, position.V, desiredMeasure.U, desiredMeasure.V));
                    }
                    else
                    {
                        child.Arrange(new Rect(position.V, position.U, desiredMeasure.V, desiredMeasure.U));
                    }

                    // adjust the location for the next items
                    position.U += desiredMeasure.U + spacingMeasure.U;
                    currentV = Math.Max(desiredMeasure.V, currentV);
                }

                var lastIndex = Children.Count;
                for (var i = 0; i < lastIndex; i++)
                {
                    arrange(Children[i], (lastIndex - i) == 1);
                }
            }

            return finalSize;
        }
        
        /// <inheritdoc />
        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize.Width = availableSize.Width - Padding.Left - Padding.Right;
            availableSize.Height = availableSize.Height - Padding.Top - Padding.Bottom;
            var totalMeasure = UvMeasure.Zero;
            var parentMeasure = new UvMeasure(Orientation, availableSize.Width, availableSize.Height);
            var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
            var lineMeasure = UvMeasure.Zero;

            void measure(UIElementCollection elementCollection)
            {
                foreach (UIElement child in elementCollection)
                {
                    if (child is FrameworkElement fe && fe.TemplateChild is Panel nestedPanel)
                    {
                        measure(nestedPanel.Children);
                        continue;
                    }

                    child.Measure(availableSize);
                    var currentMeasure = new UvMeasure(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                    if (currentMeasure.U == 0)
                    {
                        continue; // ignore collapsed items
                    }

                    // if this is the first item, do not add spacing. Spacing is added to the "left"
                    double uChange = lineMeasure.U == 0
                        ? currentMeasure.U
                        : currentMeasure.U + spacingMeasure.U;
                    if (parentMeasure.U >= uChange + lineMeasure.U)
                    {
                        lineMeasure.U += uChange;
                        lineMeasure.V = Math.Max(lineMeasure.V, currentMeasure.V);
                    }
                    else
                    {
                        // new line should be added
                        // to get the max U to provide it correctly to ui width ex: ---| or -----|
                        totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
                        totalMeasure.V += lineMeasure.V + spacingMeasure.V;

                        // if the next new row still can handle more controls
                        if (parentMeasure.U > currentMeasure.U)
                        {
                            // set lineMeasure initial values to the currentMeasure to be calculated later on the new loop
                            lineMeasure = currentMeasure;
                        }

                        // the control will take one row alone
                        else
                        {
                            // validate the new control measures
                            totalMeasure.U = Math.Max(currentMeasure.U, totalMeasure.U);
                            totalMeasure.V += currentMeasure.V;

                            // add new empty line
                            lineMeasure = UvMeasure.Zero;
                        }
                    }
                }
            }
            measure(Children);

            // update value with the last line
            // if the the last loop is(parentMeasure.U > currentMeasure.U + lineMeasure.U) the total isn't calculated then calculate it
            // if the last loop is (parentMeasure.U > currentMeasure.U) the currentMeasure isn't added to the total so add it here
            // for the last condition it is zeros so adding it will make no difference
            // this way is faster than an if condition in every loop for checking the last item
            totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
            totalMeasure.V += lineMeasure.V;

            totalMeasure.U = Math.Ceiling(totalMeasure.U);

            return Orientation == Orientation.Horizontal ? new Size(totalMeasure.U, totalMeasure.V) : new Size(totalMeasure.V, totalMeasure.U);
        }

        private static void LayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WrapPanel wp)
            {
                wp.InvalidateMeasure();
                wp.InvalidateArrange();
            }
        }
    }
}
