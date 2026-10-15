namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Map-object cell runs in the menu atlas and the marker cells drawn by calculation (<see cref="MapMarkerCell"/>).</summary>
internal static class MapMarkerCellDefinitions
{
    /// <summary>The calculated marker cells drawn by <see cref="MapMarkerTileArtwork"/>, in atlas order.</summary>
    internal static readonly MapMarkerCell[] CalculatedCells =
        [MapMarkerCell.VerticalArrowHalf, MapMarkerCell.HorizontalArrowHalf, MapMarkerCell.DefeatedBossCorner, MapMarkerCell.IndicatorCorner];

    /// <summary>$47 ($B6:C8E0): the first of the nine reserve-tank cells (six partial bars, empty, full and cap).</summary>
    internal const int FirstReserveCell = 0x47;

    /// <summary>$5B ($B6:CB60): the first of the four selector highlight cells (left corner, line and right ends).</summary>
    internal const int FirstHighlightCell = 0x5b;

    /// <summary>$46 ($B6:C8C0): the reserve cursor, reused in the selector's left corner with colors 1 and 2 swapped.</summary>
    internal const int ReserveCursorCell = 0x46;
}
