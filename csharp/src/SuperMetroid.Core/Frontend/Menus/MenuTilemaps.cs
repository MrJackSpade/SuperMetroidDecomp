using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The tilemaps a menu screen shows (#1358): BG1, the room map's two-screen area map over BG1's screen and the one
/// after it, BG2, and the world map's area background. The screen owns their tiles; its capture places them.
/// </summary>
internal sealed class MenuTilemaps
{
    /// <summary>Tiles in one 32x32 screen.</summary>
    internal const int ScreenWords = ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The four screens, in their order: BG1, the area map's second screen, BG2, the area background.</summary>
    internal readonly BackgroundTile[] tiles = new BackgroundTile[4 * ScreenWords];

    /// <summary>The menu BG1 screen.</summary>
    internal Span<BackgroundTile> Bg1 => tiles.AsSpan(0, ScreenWords);

    /// <summary>The room map's 64x32 area map: BG1's screen and the one after it.</summary>
    internal Span<BackgroundTile> AreaMap => tiles.AsSpan(0, 2 * ScreenWords);

    /// <summary>The menu BG2 screen.</summary>
    internal Span<BackgroundTile> Bg2 => tiles.AsSpan(2 * ScreenWords, ScreenWords);

    /// <summary>The world map's area-background screen.</summary>
    internal Span<BackgroundTile> AreaBackground => tiles.AsSpan(3 * ScreenWords, ScreenWords);

    /// <summary>Copies <paramref name="source"/>, a whole tilemap, into <paramref name="destination"/>.</summary>
    internal static void Load(Span<BackgroundTile> destination, ReadOnlySpan<BackgroundTile> source)
    {
        Ensure.LengthEqual(source, destination.Length);
        source.CopyTo(destination);
    }
}
