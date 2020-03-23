using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace tokenizingtextbox
{
    public class TokenizingTextBoxItem : ButtonBase
    {
        /// <summary>
        /// Identifies the <see cref="IsSelected"/> property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(TokenizingTextBoxItem),
            new PropertyMetadata(false, new PropertyChangedCallback(TokenizingTextBoxItem_IsSelectedChanged)));

        static TokenizingTextBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizingTextBoxItem), new FrameworkPropertyMetadata(typeof(TokenizingTextBoxItem)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizingTextBoxItem"/> class.
        /// </summary>
        public TokenizingTextBoxItem()
        {
            //var pointerEventHandler = new PointerEventHandler((s, e) => UpdateVisualState());
            var dependencyPropertyChangedEventHandler = new DependencyPropertyChangedEventHandler((d, e) => UpdateVisualState());

            IsEnabledChanged += dependencyPropertyChangedEventHandler;
            KeyDown += TokenizingTextBoxItem_KeyDown;
        }

        /// <summary>
        /// Event raised when the 'Clear' Button is clicked.
        /// </summary>
        public event RoutedEventHandler ClearClicked;

        /// <summary>
        /// Gets or sets a value indicating whether this item is currently in a selected state.
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        private static void TokenizingTextBoxItem_IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TokenizingTextBoxItem item)
            {
                if (item.IsSelected)
                {
                    VisualStateManager.GoToState(item, "Selected", true);
                }
                else
                {
                    VisualStateManager.GoToState(item, "Unselected", true);
                }
            }
        }

        private void TokenizingTextBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back:
                case Key.Delete:
                    ClearClicked?.Invoke(this, e);
                    break;
            }
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
            /*
            else if (IsPressed)
            {
                VisualStateManager.GoToState(this, "Pressed", useTransitions);
            }
            else if (IsPointerOver)
            {
                VisualStateManager.GoToState(this, "PointerOver", useTransitions);
            }
            */
            else
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
        }
    }
}
