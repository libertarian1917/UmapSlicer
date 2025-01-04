using UmapSlicer.Interaction;

namespace UmapSlicer.Enums
{
    /// <summary>
    /// Colors for <see cref="MainWindow.ColorDict">ColorDict</see> collection for easy access. Implemented in <see cref="MainWindow.InitializeColors">InitializeColors</see>
    /// </summary>
    public enum Materials
    {
        OutOfBorders,
        ModelDefault,
        OnModelHover,
        ModelSelected,
        PlateDefault,
        NoMaterial
    }

    /// <summary>
    /// Serves to determine the current axis in <see cref="ArrowHandler">ArrowHandler</see> class
    /// </summary>
    public enum Axis
    {
        X, Y, Z, NoAxis
    }

}
