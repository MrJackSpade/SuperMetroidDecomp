using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Map-object cell runs in the menu atlas and the marker cells drawn by calculation (<see cref="MapMarkerCell"/>).</summary>
internal static class MapMarkerCellDefinitions
{
    /// <summary>The calculated marker cells the import pipeline draws into the menu atlas, in atlas order.</summary>
    internal static readonly MapMarkerCell[] CalculatedCells =
        [MapMarkerCell.VerticalArrowHalf, MapMarkerCell.HorizontalArrowHalf, MapMarkerCell.DefeatedBossCorner, MapMarkerCell.IndicatorCorner];

    /// <summary>The menu atlas tile each marker cell occupies (bank $82:C232..C262, C298, C38C..C3A8, CF7C..CFE0).</summary>
    internal static readonly FrozenDictionary<MapMarkerCell, int> Tiles = new Dictionary<MapMarkerCell, int>
    {
        [MapMarkerCell.IndicatorBacking] = 0x89,
        [MapMarkerCell.Boss] = 0x8a,
        [MapMarkerCell.MissileStation] = 0x8b,
        [MapMarkerCell.EnergyStation] = 0x8c,
        [MapMarkerCell.MapStation] = 0x8e,
        [MapMarkerCell.GunshipHalf] = 0x8f,
        [MapMarkerCell.VerticalArrowHalf] = 0x9d,
        [MapMarkerCell.HorizontalArrowHalf] = 0x9e,
        [MapMarkerCell.DefeatedBossCorner] = 0x9f,
        [MapMarkerCell.IndicatorCorner] = 0xaf,
    }.ToFrozenDictionary();

    /// <summary>The marker cell each of those atlas tiles holds.</summary>
    internal static readonly FrozenDictionary<int, MapMarkerCell> CellsByTile =
        Tiles.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>$47 ($B6:C8E0): the first of the nine reserve-tank cells (six partial bars, empty, full and cap).</summary>
    internal const int FirstReserveCell = 0x47;

    /// <summary>$5B ($B6:CB60): the first of the four selector highlight cells (left corner, line and right ends).</summary>
    internal const int FirstHighlightCell = 0x5b;

    /// <summary>$46 ($B6:C8C0): the reserve cursor, reused in the selector's left corner with colors 1 and 2 swapped.</summary>
    internal const int ReserveCursorCell = 0x46;
}
