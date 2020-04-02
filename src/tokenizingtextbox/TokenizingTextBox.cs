using System;
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
        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
            nameof(AcceptsReturn),
            typeof(bool),
            typeof(TokenizingTextBox),
            new PropertyMetadata(true));

        public static readonly DependencyProperty TokenDelimiterProperty = DependencyProperty.Register(
            nameof(TokenDelimiter),
            typeof(string),
            typeof(TokenizingTextBox),
            new PropertyMetadata(" "));

        private const string PART_TextBox = "PART_TextBox";
        private const string PART_WrapPanel = "PART_WrapPanel";

        private TextBox _textBox;
        private WrapPanel _wrapPanel;

        static TokenizingTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizingTextBox), new FrameworkPropertyMetadata(typeof(TokenizingTextBox)));
        }

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public string TokenDelimiter
        {
            get => (string)GetValue(TokenDelimiterProperty);
            set => SetValue(TokenDelimiterProperty, value);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Delete || e.Key == Key.Back)
                {
                    e.Handled = true;
                    IEditableCollectionView items = Items;
                    using (_selectedItems.DeferRemove())
                    {
                        foreach (var item in _selectedItems)
                        {
                            items.RemoveAt(item.Index);
                        }
                    }
                    items.CommitEdit();
                    _wrapPanel.InvalidateMeasure();
                }
            }
            base.OnKeyDown(e);
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
                    _wrapPanel.InvalidateMeasure();
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
