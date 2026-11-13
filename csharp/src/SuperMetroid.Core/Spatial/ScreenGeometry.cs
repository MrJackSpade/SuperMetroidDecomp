namespace SuperMetroid.Core.Spatial;

/// <summary>The game screen's geometry: its size, the HUD band across its top, and the eight-pixel cells backgrounds are laid out in.</summary>
/// <remarks>
/// Only screen-wide sizes live here. Dimensions belonging to one animation, temporary buffer, or algorithm remain beside
/// that code even when their numeric value happens to match one of these constants.
/// </remarks>
public static class ScreenGeometry
{
    /// <summary>The screen's width in pixels.</summary>
    public const int ScreenWidthPixels = 256;

    /// <summary>The screen's height in pixels.</summary>
    public const int ScreenHeightPixels = 224;

    /// <summary>The screen: <see cref="ScreenWidthPixels"/> by <see cref="ScreenHeightPixels"/>.</summary>
    public static Extent16<ScreenPixels> ScreenSize => new(ScreenWidthPixels, ScreenHeightPixels);

    /// <summary>
    /// The camera rows the screen hides above its top row when it shows the room's layers (#1514, #1705): the screen's
    /// top row shows each layer's row below the camera's top row, while a sprite is placed at its position less the
    /// camera's, from the camera's top row itself. The game keeps this one-row offset between the layers and the sprites
    /// so every sprite shows against the room exactly where the original shows it. The gameplay view publishes the row
    /// each layer shows at the screen's top, so the offset is applied once, where the camera becomes the view. Verified
    /// against #516's captured frames.
    /// </summary>
    public const int LayerRowsHiddenAboveScreen = 1;

    /// <summary>The height of the gameplay HUD band across the top of the screen.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Pixels on either axis of one background cell.</summary>
    public const int BackgroundTileSizePixels = 8;
}
