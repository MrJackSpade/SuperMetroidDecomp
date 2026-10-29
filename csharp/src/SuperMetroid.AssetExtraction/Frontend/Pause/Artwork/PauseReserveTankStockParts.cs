using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>The stock single-character reserve-strip compositions the pause sheet's reserve frames are recognised from.</summary>
internal static class PauseReserveTankStockParts
{
    /// <summary>Reserve spritemaps $82:C35B/C369/C3D9..C410 are one stationary small sprite: partial levels 1..6 advance
    /// through sheet tiles 47..4C, level 7 uses the full tile 4E, 4D is empty and 4F is the end cap.</summary>
    private static readonly IReadOnlyDictionary<PauseReserveTankVisual, int> StockTiles = new Dictionary<PauseReserveTankVisual, int>
    {
        [PauseReserveTankVisual.Full] = 0x4e,
        [PauseReserveTankVisual.EndCap] = 0x4f,
        [PauseReserveTankVisual.Empty] = 0x4d,
        [PauseReserveTankVisual.Fill1] = 0x47,
        [PauseReserveTankVisual.Fill2] = 0x48,
        [PauseReserveTankVisual.Fill3] = 0x49,
        [PauseReserveTankVisual.Fill4] = 0x4a,
        [PauseReserveTankVisual.Fill5] = 0x4b,
        [PauseReserveTankVisual.Fill6] = 0x4c,
        [PauseReserveTankVisual.Fill7] = 0x4e,
    };

    /// <summary>The sheet tile drawn for a reserve-strip spritemap identity; palette is caller-owned.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The identity is not one of the ten reserve-strip spritemaps.</exception>
    internal static int StockTile(PauseReserveTankVisual identity) => Ensure.Listed(StockTiles, identity);

    /// <summary>Builds the native eight-pixel reserve-tank part with its tile identity and caller-owned palette.</summary>
    internal static SpriteVisualPart StockPart(PauseReserveTankVisual identity)
    {
        int tile = StockTile(identity);
        return new() { OffsetX = 0, OffsetY = 0, Size = 8, Priority = 3, Palette = null,
            FlipX = false, FlipY = false, TileColumn = tile % MapSpriteFormat.TileColumns, TileRow = tile / MapSpriteFormat.TileColumns };
    }
}
