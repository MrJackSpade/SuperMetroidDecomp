namespace SuperMetroid.Core.Spatial;

/// <summary>The game screen's geometry: its size, the HUD band across its top, and the tiles and tilemap screens backgrounds are laid out in.</summary>
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
    /// The rows a background's scroll position sits above the screen's top row: the game's scroll values name the plane
    /// row one above the first row shown, so a layer shows its plane from one row below its scroll. Sprites are placed by
    /// screen row and take no such offset. Verified against #516's captured frames.
    /// </summary>
    public const int BackgroundRowOffset = 1;

    /// <summary>The height of the gameplay HUD band across the top of the screen.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Pixels on either axis of one background tile.</summary>
    public const int BackgroundTileSizePixels = 8;

    /// <summary>Tiles on either axis of one tilemap screen.</summary>
    public const int TilemapScreenWidthInTiles = 32;

    /// <summary>Cells in one 32-by-32 tilemap screen.</summary>
    public const int TilemapScreenCellCount = TilemapScreenWidthInTiles * TilemapScreenWidthInTiles;
}
