using System.Buffers.Binary;

namespace SuperMetroid.Core.Game;

/// <summary>Shared exploration projection for $82:943D pause and $82:9517 room-select maps.</summary>
public static class AreaMapTilemapBuilder
{
    /// <summary>
    /// Projects immutable cartridge cells into native two-page VRAM order. The caller
    /// supplies its screen's hidden-cell character; reveal overrides never modify SRAM.
    /// </summary>
    public static byte[] Build(IAreaMapView map, Bank80SystemState system,
        MapTileWord hiddenCell, MapRevealMode revealMode = MapRevealMode.None)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(system);
        var bytes = new byte[AreaMapRomData.TilemapByteCount];
        bool downloaded = system.HasAreaMap(map.Area);
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            MapTileWord word = map.GetTile(x, y);
            bool explored = system.IsMapTileExplored(map.Area, x, y);
            if (explored)
                word = word.AsExplored();
            else if (!AreaMapVisibility.IsVisible(explored, downloaded,
                         map.IsRevealedByMapStation(x, y), map.IsDiscoverable(x, y), revealMode))
                word = hiddenCell;
            BinaryPrimitives.WriteUInt16LittleEndian(
                bytes.AsSpan(AreaMapLayout.GetTilemapWordIndex(x, y) * 2), word.Raw);
        }
        return bytes;
    }
}
