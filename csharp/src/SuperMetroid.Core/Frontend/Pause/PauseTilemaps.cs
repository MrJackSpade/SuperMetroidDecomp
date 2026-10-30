using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The equipment page the pause screen shows (#1358). The map page is a grid of the area map's cell pictures and the frame
/// is the area's backdrop and button pictures (#1467); the retained gameplay HUD is the screen's own images (#1466).
/// </summary>
internal sealed class PauseTilemaps
{
    /// <summary>The equipment page's tiles, row by row.</summary>
    private readonly BackgroundTile[] equipment = new BackgroundTile[ScreenGeometry.TilemapScreenCellCount];

    /// <summary>The equipment page.</summary>
    internal Span<BackgroundTile> Equipment => equipment;
}
