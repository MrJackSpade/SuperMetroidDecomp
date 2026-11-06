namespace SuperMetroid.Core.Spatial;

/// <summary>The game screen's geometry: its size, the HUD band across its top, and the tiles backgrounds are laid out in.</summary>
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
    /// The rows the room's layers are drawn above the sprites placed from the same camera (#1514): the screen's top row
    /// shows the layers' row one below the camera's top row, while a sprite is drawn at its row less the camera's. The
    /// gameplay view publishes the row each layer shows at the screen's top, so this lead is applied once, where the
    /// camera becomes the view. Verified against #516's captured frames.
    /// </summary>
    public const int LayerRowsAboveSprites = 1;

    /// <summary>The height of the gameplay HUD band across the top of the screen.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Pixels on either axis of one background tile.</summary>
    public const int BackgroundTileSizePixels = 8;
}
