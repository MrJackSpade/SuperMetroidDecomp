namespace SuperMetroid.Core.Map;

/// <summary>
/// How a map screen shows one area-map cell (#1466): the cell, and whether Samus has explored it, which selects the colour
/// variant the screen shows it in.
/// </summary>
/// <param name="Cell">How the cell looks.</param>
/// <param name="Explored">Whether the screen shows the cell as explored.</param>
public readonly record struct ShownMapCell(MapCell Cell, bool Explored)
{
    /// <summary>The colour variant the screen shows the cell in.</summary>
    public int ColourVariant => Explored ? Cell.ExploredColourVariant : Cell.ColourVariant;
}
