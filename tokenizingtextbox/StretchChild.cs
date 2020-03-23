namespace tokenizingtextbox
{
    /// <summary>
    /// Options for how to calculate the layout of <see cref="Windows.UI.Xaml.Controls.WrapGrid"/> items.
    /// </summary>
    public enum StretchChild
    {
        /// <summary>
        /// Don't apply any additional stretching logic
        /// </summary>
        None,

        /// <summary>
        /// Make the last child stretch to fill the available space
        /// </summary>
        Last
    }
}
