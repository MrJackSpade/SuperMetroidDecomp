using System.Collections.Frozen;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Map-object cell runs in the menu atlas and the marker cells drawn by calculation (<see cref="MapMarkerCell"/>).</summary>
internal static class MapMarkerCellDefinitions
{
    /// <summary>The calculated marker cells the import pipeline draws into the menu atlas, in atlas order.</summary>
    internal static readonly MapMarkerCell[] CalculatedCells =
        [MapMarkerCell.VerticalArrowHalf, MapMarkerCell.HorizontalArrowHalf, MapMarkerCell.DefeatedBossCorner, MapMarkerCell.IndicatorCorner];

    /// <summary>The menu atlas tile each marker cell occupies (bank $82:C232..C262, C298, C38C..C3A8, CF7C..CFE0).</summary>
    internal static readonly FrozenDictionary<MapMarkerCell, int> Tiles = new Dictionary<MapMarkerCell, int>
    {
        [MapMarkerCell.IndicatorBacking] = 137,
        [MapMarkerCell.Boss] = 138,
        [MapMarkerCell.MissileStation] = 139,
        [MapMarkerCell.EnergyStation] = 140,
        [MapMarkerCell.MapStation] = 142,
        [MapMarkerCell.GunshipHalf] = 143,
        [MapMarkerCell.VerticalArrowHalf] = 157,
        [MapMarkerCell.HorizontalArrowHalf] = 158,
        [MapMarkerCell.DefeatedBossCorner] = 159,
        [MapMarkerCell.IndicatorCorner] = 175,
    }.ToFrozenDictionary();

    /// <summary>The marker cell each of those atlas tiles holds.</summary>
    internal static readonly FrozenDictionary<int, MapMarkerCell> CellsByTile =
        Tiles.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>$47 ($B6:C8E0): the first of the nine reserve-tank cells (six partial bars, empty, full and cap).</summary>
    internal const int FirstReserveCell = 71;

    /// <summary>$5B ($B6:CB60): the first of the four selector highlight cells (left corner, line and right ends).</summary>
    internal const int FirstHighlightCell = 91;

    /// <summary>$46 ($B6:C8C0): the reserve cursor, reused in the selector's left corner with colors 1 and 2 swapped.</summary>
    internal const int ReserveCursorCell = 70;
}
