namespace SuperMetroid.Core.Map;

/// <summary>
/// How a map screen shows one area-map cell (#1513): the cell, in the shade its exploration selects.
/// </summary>
/// <param name="Cell">How the cell looks.</param>
/// <param name="Shade">The shade the screen shows the cell in.</param>
public readonly record struct ShownMapCell(MapCell Cell, MapCellShade Shade)
{
    /// <summary><paramref name="cell"/> as a screen shows it once Samus has explored it.</summary>
    public static ShownMapCell Explored(MapCell cell) => new(cell, MapCellShade.Explored);

    /// <summary><paramref name="cell"/> as a screen shows it before Samus has explored it.</summary>
    public static ShownMapCell Unexplored(MapCell cell) => new(cell, MapCellShade.Unexplored);
}
