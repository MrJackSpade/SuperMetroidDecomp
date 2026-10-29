using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Map;

/// <summary>
/// How one area-map cell looks (#1466): the shape it shows, the colour variant a map screen shows it in before and after
/// Samus explores it, its mirroring, and whether it sits in the map layer's upper tier. The HUD minimap shows the same
/// shape and mirroring in its own explored, unexplored and Samus-location shades.
/// </summary>
/// <param name="Shape">The shape the cell shows.</param>
/// <param name="ColourVariant">The colour variant a map screen shows the cell in until it is explored.</param>
/// <param name="ExploredColourVariant">The colour variant a map screen shows the cell in once it is explored.</param>
/// <param name="Mirroring">How the shape is mirrored.</param>
/// <param name="InFront">Whether the cell sits in the map layer's upper tier.</param>
public readonly record struct MapCell(MapCellShape Shape, int ColourVariant, int ExploredColourVariant, TileFlip Mirroring, bool InFront)
{
    /// <summary>The tile a map screen places for this cell, explored or not.</summary>
    /// <param name="explored">Whether Samus has explored the cell.</param>
    public BackgroundTile Tile(bool explored) =>
        new(Shape.Picture, explored ? ExploredColourVariant : ColourVariant, InFront, Mirroring);
}
