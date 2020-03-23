using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace tokenizingtextbox
{
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_WrapPanel, Type = typeof(Panel))]
    public partial class TokenizingTextBox : Control
    {
        #region DependencyProperties

        #region First Pass

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register(
            nameof(AcceptsReturn),
            typeof(bool),
            typeof(TokenizingTextBox),
            new PropertyMetadata(true));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(TokenizingTextBox),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(TokenizingTextBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TokenDelimiterProperty = DependencyProperty.Register(
            nameof(TokenDelimiter),
            typeof(string),
            typeof(TokenizingTextBox),
            new PropertyMetadata(" "));

        private static readonly DependencyPropertyKey SelectedItemsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "SelectedItems",
                typeof(IList),
                typeof(TokenizingTextBox),
                new FrameworkPropertyMetadata((IList)null));

        #endregion First Pass

        #region Second Pass

        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

        #endregion Second Pass

        #endregion DependencyProperties

        private const string PART_TextBox = "PART_TextBox";

        private const string PART_WrapPanel = "PART_WrapPanel";

        private ItemCollection _items;

        private TextBox _textBox;

        private Panel _wrapPanel;

        static TokenizingTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizingTextBox), new FrameworkPropertyMetadata(typeof(TokenizingTextBox)));
        }

        public TokenizingTextBox()
        {
            var selectedItems = Proxy.Create(this);
            SetValue(SelectedItemsPropertyKey, selectedItems);
            ((INotifyCollectionChanged)selectedItems).CollectionChanged += OnSelectedItemsCollectionChanged;
        }

        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public ItemCollection Items
        {
            get
            {
                if (_items == null)
                {
                    CreateItemCollection();
                }

                return _items;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return Proxy.ItemsSource(Items); }
            set
            {
                if (value == null)
                {
                    ClearValue(ItemsSourceProperty);
                }
                else
                {
                    SetValue(ItemsSourceProperty, value);
                }
            }
        }

        public IList SelectedItems => (IList)GetValue(SelectedItemsProperty);

        /// <summary>
        /// Gets or sets the input text of the AutoSuggestBox template part.
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets or sets delimiter used to determine when to process text input as a new token item.
        /// </summary>
        public string TokenDelimiter
        {
            get => (string)GetValue(TokenDelimiterProperty);
            set => SetValue(TokenDelimiterProperty, value);
        }

        private bool IsInitPending
        {
            get
            {
                return Proxy.IsInitPending(this);
            }
        }

        public override void BeginInit()
        {
            base.BeginInit();

            if (_items != null)
            {
                Proxy.BeginInit(_items);
            }
        }

        /// <summary>
        ///     Initialization of this element has completed
        /// </summary>
        public override void EndInit()
        {
            if (IsInitPending)
            {
                if (_items != null)
                {
                    Proxy.EndInit(_items);
                }

                base.EndInit();
            }
        }

        /// <summary>
        /// Returns the string representation of each token item, concatenated and delimeted.
        /// </summary>
        /// <returns>Untokenized text string</returns>
        public string GetUntokenizedText(string tokenDelimiter = ", ")
        {
            var tokenStrings = new List<string>();
            foreach (var child in _wrapPanel.Children)
            {
                if (child is TokenizingTextBoxItem item)
                {
                    tokenStrings.Add(item.Content.ToString());
                }
            }

            return string.Join(tokenDelimiter, tokenStrings);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_textBox != null)
            {
                _textBox.Loaded -= OnASBLoaded;

                _textBox.TextChanged -= TextBox_TextChanged;
                _textBox.KeyDown -= TextBox_KeyDown;
            }

            _textBox = (TextBox)GetTemplateChild(PART_TextBox);
            _wrapPanel = (WrapPanel)GetTemplateChild(PART_WrapPanel);

            if (_textBox != null)
            {
                _textBox.Loaded += OnASBLoaded;

                _textBox.TextChanged += TextBox_TextChanged;
                _textBox.KeyDown += TextBox_KeyDown;
            }
        }

        /// <summary>
        /// Called when the value of ItemsSource changes.
        /// </summary>
        protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TokenizingTextBox ic = (TokenizingTextBox)d;
            IEnumerable oldValue = (IEnumerable)e.OldValue;
            IEnumerable newValue = (IEnumerable)e.NewValue;

            BindingExpressionBase beb = BindingOperations.GetBindingExpressionBase(d, ItemsSourceProperty);
            if (beb != null)
            {
                Proxy.SetItemsSource(ic.Items, newValue, (object x) => Proxy.GetSourceItem(beb, x));
            }
            else if (e.NewValue != null)
            {
                Proxy.SetItemsSource(ic.Items, newValue);
            }
            else
            {
                Proxy.ClearItemsSource(ic.Items);
            }

            ic.OnItemsSourceChanged(oldValue, newValue);
        }

        private void AddToken(object data)
        {
            //if (data is string str && TokenItemCreating != null)
            //{
            //    var ticea = new TokenItemCreatingEventArgs(str);
            //    TokenItemCreating(this, ticea);

            //    if (ticea.Cancel)
            //    {
            //        return;
            //    }

            //    if (ticea.Item != null)
            //    {
            //        data = ticea.Item; // Transformed by event implementor
            //    }
            //}
            if (Items.SourceCollection is IList list && !list.IsFixedSize)
            {
                list.Add(data);
            }
        }

        private void CopySelectedToClipboard()
        {
            /*
            if (SelectedItemsInternal.Count > 0)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;

                string tokenString = string.Empty;
                bool addSeparator = false;
                foreach (TokenizingTextBoxItem item in SelectedItemsInternal)
                {
                    if (addSeparator)
                    {
                        tokenString += TokenDelimiter + " ";
                    }
                    else
                    {
                        addSeparator = true;
                    }

                    tokenString += item.Content;
                }

                dataPackage.SetText(tokenString);
                Clipboard.SetContent(dataPackage);
            }
            */
        }

        private void CreateItemCollection()
        {
            _items = Proxy.Create(this);

            ((INotifyCollectionChanged)_items).CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemCollectionChanged);

            if (IsInitPending)
            {
                Proxy.BeginInit(_items);
            }
            else if (IsInitialized)
            {
                Proxy.BeginInit(_items);
                Proxy.EndInit(_items);
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

        private void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var childIndex = _wrapPanel.Children.Count - 1;
                    var startIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        var element = new TokenizingTextBoxItem()
                        {
                            Content = item
                        };
                        element.Click += TokenizingTextBoxItem_Click;
                        element.ClearClicked += TokenizingTextBoxItem_ClearClicked;
                        element.KeyUp += TokenizingTextBoxItem_KeyUp;

                        // TODO: Fix up adding objects to the back
                        var i = _wrapPanel.Children.Count - 1;
                        _wrapPanel.Children.Insert(i, element);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    // TODO: Handle remove of object
                    break;

                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
            }
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO
        }

        private void RemoveToken(TokenizingTextBoxItem item)
        {
            SelectedItems.Remove(item);
            if (Items.SourceCollection is IList list && !list.IsReadOnly)
            {
                list.Remove(item);
            }

            var itemIndex = Math.Max(_wrapPanel.Children.IndexOf(item) - 1, 0);
            _wrapPanel.Children.Remove(item);

            if (_wrapPanel.Children[itemIndex] is Control control)
            {
                control.Focus();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        CopySelectedToClipboard();
                    }
                    break;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int currentCursorPosition = _textBox.SelectionStart;
            if (currentCursorPosition == 0 && e.Key == Key.Back && Items.Count > 0)
            {
                // The last item is the AutoSuggestBox. Get the second to last
                UIElement itemToFocus = _wrapPanel.Children[_wrapPanel.Children.Count - 2];

                // And set focus to it
                Keyboard.Focus(itemToFocus);
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                var trim = _textBox.Text.Trim();
                AddToken(trim);
                _textBox.Text = string.Empty;
            }
        }

        private void TextBox_TextChanged(object _, TextChangedEventArgs e)
        {
            if (!(_ is TextBox sender))
            {
                return;
            }

            string t = sender.Text.Trim();

            if (!string.IsNullOrEmpty(TokenDelimiter) && t.Contains(TokenDelimiter))
            {
                bool lastDelimited = t[t.Length - 1] == TokenDelimiter[0];

#if NET48
                string[] tokens = t.Split(new[] { TokenDelimiter }, StringSplitOptions.RemoveEmptyEntries);
#else
                string[] tokens = t.Split(TokenDelimiter);
#endif
                int numberToProcess = lastDelimited ? tokens.Length : tokens.Length - 1;
                for (int position = 0; position < numberToProcess; position++)
                {
                    string token = tokens[position];
                    token = token.Trim();
                    if (token.Length > 0)
                    {
                        AddToken(token);
                    }
                }

                if (lastDelimited)
                {
                    sender.Text = string.Empty;
                }
                else
                {
                    sender.Text = tokens[tokens.Length - 1];
                }
            }
        }

        private void TokenizingTextBoxItem_ClearClicked(object _, RoutedEventArgs args)
        {
            if (!(_ is TokenizingTextBoxItem sender))
            {
                return;
            }

            bool removeMulti = false;
            foreach (var item in SelectedItems)
            {
                if (item == sender)
                {
                    removeMulti = true;
                    break;
                }
            }

            if (removeMulti)
            {
                using (var defer = ((ItemCollection)SelectedItems).DeferRefresh())
                {
                    while (SelectedItems.Count > 0)
                    {
                        var b = SelectedItems[0] as TokenizingTextBoxItem;
                        RemoveToken(b);
                    }
                }
            }
            else
            {
                RemoveToken(sender);
            }
        }

        private void TokenizingTextBoxItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is TokenizingTextBoxItem item)
            {
                if (!item.IsSelected && !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    foreach (var child in _wrapPanel.Children)
                    {
                        if (child is TokenizingTextBoxItem childItem)
                        {
                            childItem.IsSelected = false;
                        }
                    }

                    SelectedItems.Clear();
                }

                item.IsSelected = !item.IsSelected;

                if (item.IsSelected)
                {
                    SelectedItems.Add(item.Content);
                }
                else
                {
                    SelectedItems.Remove(item.Content);
                }
            }
        }

        private void TokenizingTextBoxItem_KeyUp(object sender, KeyEventArgs e)
        {
            TokenizingTextBoxItem ttbi = sender as TokenizingTextBoxItem;

            switch (e.Key)
            {
                // Required?
                case Key.Space:
                    {
                        ttbi.IsSelected = !ttbi.IsSelected;
                        break;
                    }

                case Key.C:
                    {
                        if (e.KeyboardDevice.Modifiers.HasFlag(ConsoleModifiers.Control))
                        {
                            CopySelectedToClipboard();
                        }

                        break;
                    }
            }
        }

        private static class Proxy
        {
            #region Types

            private static readonly Type BindingExpressionBaseType = typeof(BindingExpressionBase);
            private static readonly Type FrameworkElementType = typeof(FrameworkElement);
            private static readonly Type ItemCollectionType = typeof(ItemCollection);

            #endregion Types

            private static readonly MethodInfo BeginInitMethod;
            private static readonly MethodInfo ClearItemsSourceMethod;
            private static readonly MethodInfo EndInitMethod;
            private static readonly MethodInfo GetSourceItemMethod;
            private static readonly ConstructorInfo ItemCollectionConstructor;
            private static readonly PropertyInfo ItemsSourceProperty;
            private static readonly MethodInfo ReadInternalFlagMethod;
            private static readonly MethodInfo SetItemsSourceMethod;

            static Proxy()
            {
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

                ItemCollectionConstructor = ItemCollectionType.GetConstructor(flags, null, new[] { typeof(DependencyObject) }, Array.Empty<ParameterModifier>());
                BeginInitMethod = ItemCollectionType.GetMethod("BeginInit", flags);
                EndInitMethod = ItemCollectionType.GetMethod("EndInit", flags);
                ItemsSourceProperty = ItemCollectionType.GetProperty("ItemsSource", flags);
                SetItemsSourceMethod = ItemCollectionType.GetMethod("SetItemsSource", flags);
                ClearItemsSourceMethod = ItemCollectionType.GetMethod("ClearItemsSource", flags);

                ReadInternalFlagMethod = FrameworkElementType.GetMethod("ReadInternalFlag", flags);

                GetSourceItemMethod = BindingExpressionBaseType.GetMethod("GetSourceItem", flags);
            }

            public static void BeginInit(ItemCollection itemCollection) => BeginInitMethod.Invoke(itemCollection, null);

            public static void ClearItemsSource(ItemCollection itemCollection) => ClearItemsSourceMethod.Invoke(itemCollection, null);

            public static ItemCollection Create(DependencyObject modelParent) => (ItemCollection)ItemCollectionConstructor.Invoke(new object[] { modelParent });

            public static void EndInit(ItemCollection itemCollection) => EndInitMethod.Invoke(itemCollection, null);

            public static object GetSourceItem(BindingExpressionBase bindingExpression, object newValue) => GetSourceItemMethod.Invoke(bindingExpression, new object[] { newValue });

            public static bool IsInitPending(FrameworkElement element) => (bool)ReadInternalFlagMethod.Invoke(element, new object[] { 65536u });

            public static IEnumerable ItemsSource(ItemCollection collection) => (IEnumerable)ItemsSourceProperty.GetValue(collection);

            public static void SetItemsSource(ItemCollection itemCollection, IEnumerable value, Func<object, object> GetSourceItem = null) => SetItemsSourceMethod.Invoke(itemCollection, new object[] { value, GetSourceItem });
        }
    }
}
