namespace SuperMetroid.Core.Map;

/// <summary>
/// How a map screen shows one area-map cell (#1466): the cell, in the colour variant its exploration selects.
/// </summary>
public readonly record struct ShownMapCell
{
    /// <summary>Keeps the cell and the colour variant it is shown in.</summary>
    private ShownMapCell(MapCell cell, int colourVariant)
    {
        Cell = cell;
        ColourVariant = colourVariant;
    }

    /// <summary>How the cell looks.</summary>
    public MapCell Cell { get; }

    /// <summary>The colour variant the screen shows the cell in.</summary>
    public int ColourVariant { get; }

    /// <summary><paramref name="cell"/> as a screen shows it once Samus has explored it.</summary>
    public static ShownMapCell Explored(MapCell cell) => new(cell, cell.ExploredColourVariant);

    /// <summary><paramref name="cell"/> as a screen shows it before Samus has explored it.</summary>
    public static ShownMapCell Unexplored(MapCell cell) => new(cell, cell.ColourVariant);
}
