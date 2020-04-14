using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tokenizingtextbox
{
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_WrapPanel, Type = typeof(Panel))]
    public class TokenizingTextBox : ListBox
    {
        #region Dependency Properties

        #region Property Keys

        private static readonly DependencyPropertyKey CanUserAddPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanUserAdd), typeof(bool), typeof(TokenizingTextBox), null);
        private static readonly DependencyPropertyKey CanUserRemovePropertyKey = DependencyProperty.RegisterReadOnly(nameof(CanUserRemove), typeof(bool), typeof(TokenizingTextBox), null);

        #endregion Property Keys

        #region Properties

        public static readonly DependencyProperty AcceptsReturnProperty = KeyboardNavigation.AcceptsReturnProperty.AddOwner(typeof(TokenizingTextBox));
        public static readonly DependencyProperty CanUserAddProperty = CanUserAddPropertyKey.DependencyProperty;
        public static readonly DependencyProperty CanUserRemoveProperty = CanUserRemovePropertyKey.DependencyProperty;
        public static readonly DependencyProperty TokenDelimiterProperty = DependencyProperty.Register(
            nameof(TokenDelimiter),
            typeof(string),
            typeof(TokenizingTextBox),
            new PropertyMetadata(" "));

        #endregion Properties

        #region Accessors

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public bool CanUserAdd => (bool)GetValue(CanUserAddProperty);

        public bool CanUserRemove => (bool)GetValue(CanUserRemoveProperty);

        public string TokenDelimiter
        {
            get => (string)GetValue(TokenDelimiterProperty);
            set => SetValue(TokenDelimiterProperty, value);
        }

        #endregion Accessors

        #endregion Dependency Properties

        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";
        private TextBox _textBox;
        private WrapPanel _wrapPanel;

        static TokenizingTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizingTextBox), new FrameworkPropertyMetadata(typeof(TokenizingTextBox)));

            SelectionModeProperty.OverrideMetadata(typeof(TokenizingTextBox), new FrameworkPropertyMetadata(SelectionMode.Extended));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_textBox != null)
            {
                _textBox.Loaded -= OnASBLoaded;

                _textBox.TextChanged -= TextBox_TextChanged;
            }

            _textBox = (TextBox)GetTemplateChild(PART_TextBox);
            _wrapPanel = (WrapPanel)GetTemplateChild(PART_WrapPanel);

            if (_textBox != null)
            {
                _textBox.Loaded += OnASBLoaded;

                _textBox.TextChanged += TextBox_TextChanged;
            }
        }

        protected override DependencyObject GetContainerForItemOverride() => new TokenizingTextBoxItem();

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            IEditableCollectionViewAddNewItem view = Items;
            SetValue(CanUserAddPropertyKey, view.CanAddNewItem);
            SetValue(CanUserRemovePropertyKey, view.CanRemove);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                bool goBack;
                switch (e.Key)
                {
                    case Key.Delete:
                        goBack = false;
                        break;

                    case Key.Back:
                        goBack = true;
                        break;

                    default:
                        return;
                }

                e.Handled = true;
                if (!CanUserRemove)
                {
                    return;
                }
                int smallestIndex = int.MaxValue;
                IEditableCollectionView items = Items;
                using (_selectedItems.DeferRemove())
                {
                    foreach (var item in _selectedItems)
                    {
                        if (item.Index < smallestIndex)
                        {
                            smallestIndex = item.Index;
                        }
                        items.RemoveAt(item.Index);
                    }
                    SelectedItemsImpl.Clear();
                }
                if (Items.Count > 0)
                {
                    var selectIndex = smallestIndex;
                    if (goBack)
                        selectIndex = Math.Max(0, selectIndex - 1);
                    selectIndex = Math.Min(selectIndex, Items.Count - 1);

                    var container = ItemInfoFromIndex(selectIndex).Container;
                    SetIsSelected(container, true);
                    ((UIElement)container).Focus();
                }
                else
                {
                    _textBox.Focus();
                }
            }
            base.OnKeyDown(e);
        }

        private void AddToken(string token)
        {
            token = token.Trim();

            if (token.Length > 0)
            {
                IEditableCollectionViewAddNewItem items = Items;
                items.AddNewItem(token);
                items.CommitNew();
            }
        }

        private void OnASBLoaded(object sender, RoutedEventArgs e)
        {
            if (_textBox != null)
            {
                _textBox.PreviewKeyDown -= this.TextBox_PreviewKeyDown;
            }

            _textBox.PreviewKeyDown += this.TextBox_PreviewKeyDown;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int currentCursorPosition = _textBox.SelectionStart;
            if (currentCursorPosition == 0 && _textBox.SelectionLength == 0 && e.Key == Key.Back && Items.Count > 0)
            {
                e.Handled = true;
                var container = ItemContainerGenerator.ContainerFromIndex(Items.Count - 1);
                if (container is IInputElement element)
                {
                    Keyboard.Focus(element);
                }
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (AcceptsReturn)
                {
                    AddToken(_textBox.Text);
                    _textBox.Text = string.Empty;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = _textBox.Text;
            if (!string.IsNullOrEmpty(TokenDelimiter) && t.Contains(TokenDelimiter))
            {
                bool lastDelimited = t[t.Length - 1] == TokenDelimiter[0];

                string[] tokens = t.Split(new[] { TokenDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                int numberToProcess = lastDelimited ? tokens.Length : tokens.Length - 1;
                for (int position = 0; position < numberToProcess; position++)
                {
                    AddToken(tokens[position]);
                }

                if (lastDelimited)
                {
                    _textBox.Text = string.Empty;
                    //_wrapPanel.InvalidateMeasure();
                }
                else
                {
                    _textBox.Text = tokens[tokens.Length - 1];
                    _textBox.CaretIndex = _textBox.Text.Length;
                }
            }
        }
    }
}
