namespace SuperMetroid.Core.Spatial;

/// <summary>The game screen's geometry: its size, the HUD band across its top, and the eight-pixel cells backgrounds are laid out in.</summary>
/// <remarks>
/// Only screen-wide sizes live here. Dimensions belonging to one animation, temporary buffer, or algorithm remain beside
/// that code even when their numeric value happens to match one of these constants.
/// </remarks>
public static partial class ScreenGeometry
{
    /// <summary>The screen's width in pixels.</summary>
    public const int ScreenWidthPixels = 256;

    /// <summary>The screen's height in pixels.</summary>
    public const int ScreenHeightPixels = 224;

    /// <summary>The screen: <see cref="ScreenWidthPixels"/> by <see cref="ScreenHeightPixels"/>.</summary>
    public static Extent16<ScreenPixels> ScreenSize => new(ScreenWidthPixels, ScreenHeightPixels);

    /// <summary>The height of the gameplay HUD band across the top of the screen.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Pixels on either axis of one background cell.</summary>
    public const int BackgroundCellSizePixels = 8;
}
