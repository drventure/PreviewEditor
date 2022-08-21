using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewEditor
{
    internal static class ColorExtensions
    {
        internal static Color ToDrawColor(this System.Windows.Media.Color c)
        {
            return Color.FromArgb(c.R, c.G, c.B);
        }
    }
}
