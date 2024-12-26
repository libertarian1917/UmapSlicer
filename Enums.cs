using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmapSlicer
{
    /// <summary>
    /// Colors for <see cref="MainWindow.ColorDict">ColorDict</see> collection for easy access. Implemented in <see cref="MainWindow.InitializeColors">InitializeColors</see>
    /// </summary>
    public enum Materials
    {
        RedOutOfBorders,
        GrayDefault,
        BlueGreenHover,
        GreenSelected,
        White
    }

    /// <summary>
    /// Serves to determine the current axis in <see cref="ArrowHandler">ArrowHandler</see> class
    /// </summary>
    public enum Axis
    {
        X, Y, Z, NoAxis
    }

}
