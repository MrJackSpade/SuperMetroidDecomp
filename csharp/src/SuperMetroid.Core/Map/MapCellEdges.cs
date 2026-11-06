namespace SuperMetroid.Core.Map;

/// <summary>What bounds each of an area-map cell's four edges (#1513), as the map draws the cell.</summary>
/// <param name="Top">What bounds the cell's top edge.</param>
/// <param name="Bottom">What bounds the cell's bottom edge.</param>
/// <param name="Left">What bounds the cell's left edge.</param>
/// <param name="Right">What bounds the cell's right edge.</param>
public readonly record struct MapCellEdges(MapCellEdge Top, MapCellEdge Bottom, MapCellEdge Left, MapCellEdge Right)
{
    /// <summary>A cell open on every edge.</summary>
    public static MapCellEdges Open => default;

    /// <summary>
    /// The edges as a picture's name spells them: each edge that is not open, top, bottom, left then right, as a
    /// <c>.</c>-led segment naming the side and what bounds it; empty when every edge is open.
    /// </summary>
    public string Name => string.Concat(Segment("Top", Top), Segment("Bottom", Bottom), Segment("Left", Left), Segment("Right", Right));

    /// <summary>The name segment of the edge on <paramref name="side"/>; empty for an open edge.</summary>
    private static string Segment(string side, MapCellEdge edge) => edge == MapCellEdge.Open ? "" : $".{side}{edge}";
}
