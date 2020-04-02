using System.Windows.Controls;

namespace tokenizingtextbox
{
    partial class WrapPanel
    {
        [System.Diagnostics.DebuggerDisplay("U = {U} V = {V}")]
        private struct UvMeasure
        {
            internal static readonly UvMeasure Zero = default;

            public UvMeasure(Orientation orientation, double width, double height)
            {
                if (orientation == Orientation.Horizontal)
                {
                    U = width;
                    V = height;
                }
                else
                {
                    U = height;
                    V = width;
                }
            }

            internal double U { get; set; }

            internal double V { get; set; }
        }
    }
}
