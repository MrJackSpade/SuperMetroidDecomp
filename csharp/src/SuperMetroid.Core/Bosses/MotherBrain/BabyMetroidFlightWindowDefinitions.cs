namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The window about the screen that the Baby Metroid, in Mother Brain's room and as the Tourian Baby Metroid, counts as on
/// screen when it reverses toward a point (#2254): the screen, widened far above and below and a little to each side. A
/// reversal outside it takes the off-screen kick.
/// </summary>
internal static partial class BabyMetroidFlightWindowDefinitions
{
    /// <summary>How far above and below the screen the window reaches.</summary>
    internal const int VerticalMargin = 96;

    /// <summary>How far left and right of the screen the window reaches.</summary>
    internal const int HorizontalMargin = 16;

    /// <summary>The window's height: the screen's, widened by its margin above and below.</summary>
    internal const int Height = ScreenGeometry.ScreenHeightPixels + 2 * VerticalMargin;

    /// <summary>The window's width: the screen's, widened by its margin on both sides.</summary>
    internal const int Width = ScreenGeometry.ScreenWidthPixels + 2 * HorizontalMargin;
}
