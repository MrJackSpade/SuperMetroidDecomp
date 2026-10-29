using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Where the cartridge's HUD routines write each energy tank, item icon and AUTO cell (#1466): the stock positions the
/// import writes into the editable HUD document, as cell indices in the HUD's three rows.
/// </summary>
internal static class GameplayHudStockCells
{
    /// <summary><c>HandleHUDTilemap_PausedAndRunning.etankIconOffsets</c> at $80:9CCE: two seven-cell rows, bottom first.</summary>
    /// <param name="tank">Zero-based tank cell 0..13, counting across the lower row before the upper row.</param>
    /// <returns>Cell index in the 32-by-3 mutable HUD tilemap (the native table holds twice this, a byte offset).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tank"/> is outside the fourteen tank cells.</exception>
    internal static int EnergyTankCell(int tank) => (uint)tank < GameplayHudDefinitions.EnergyTankCount
        ? 1 + tank % 7 + (tank < 7 ? GameplayHudDefinitions.Width : 0)
        : throw new ArgumentOutOfRangeException(nameof(tank));

    /// <summary><c>ToggleHUDItemHighlight.HUDItemOffsets</c> at $80:9D6E: three-cell missiles, then two-cell icons, each with a blank gap.</summary>
    /// <param name="item">Zero-based icon index 0..4 in <see cref="GameplayHudDefinitions.IconNames"/> order, rather than the live selection's one-based item code.</param>
    /// <returns>Cell index of the icon's upper-left word in the mutable HUD tilemap (the native table holds twice this).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="item"/> is outside the five item icons.</exception>
    internal static int ItemCell(int item) => (uint)item < GameplayHudDefinitions.ItemCount
        ? 10 + item * 3 + (item > 0 ? 1 : 0)
        : throw new ArgumentOutOfRangeException(nameof(item));

    /// <summary>Native AUTO stores at $80:9B64..9B87 occupy columns eight/nine across all three mutable HUD rows.</summary>
    /// <param name="cell">Zero-based composition cell 0..5, ordered left-to-right and top-to-bottom.</param>
    /// <returns>Word index in the 32-by-3 mutable HUD tilemap, not a byte offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the six AUTO cells.</exception>
    internal static int AutoReserveCellIndex(int cell) => (uint)cell < GameplayHudDefinitions.AutoReserveCellCount
        ? 8 + cell % 2 + cell / 2 * GameplayHudDefinitions.Width
        : throw new ArgumentOutOfRangeException(nameof(cell));
}
