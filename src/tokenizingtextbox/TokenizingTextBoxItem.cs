using System.Windows;
using System.Windows.Controls;

namespace tokenizingtextbox
{
    public class TokenizingTextBoxItem : ListBoxItem
    {
        static TokenizingTextBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizingTextBoxItem), new FrameworkPropertyMetadata(typeof(TokenizingTextBoxItem)));
        }
    }
}
