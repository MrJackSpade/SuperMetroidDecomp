namespace SuperMetroid.Core.Map;

/// <summary>
/// How one area-map cell looks (#1513): the kind of place it shows, what bounds each of its edges, and the piece of a
/// sloped hallway it shows, if any. The map screens show it in the shade its exploration selects, and the HUD minimap in
/// its own shades; both picture it by this identity. The import pipeline decides each cell once from the map it imports.
/// </summary>
/// <param name="Kind">The kind of place the cell shows.</param>
/// <param name="Edges">What bounds each of the cell's edges.</param>
/// <param name="Slope">The piece of a sloped hallway the cell shows; <see cref="MapCellSlope.None"/> for any other cell.</param>
public readonly record struct MapCell(MapCellKind Kind, MapCellEdges Edges, MapCellSlope Slope = MapCellSlope.None)
{
    /// <summary>The cell where no place is mapped, which shows nothing; the map screens show it where nothing is revealed.</summary>
    public static MapCell Empty => new(MapCellKind.Empty, MapCellEdges.Open);

    /// <summary>
    /// The cell as its pictures' names spell it: its kind, then its slope piece if it shows one, then each edge that is not
    /// open (<see cref="MapCellEdges.Name"/>), each segment a name.
    /// </summary>
    public string Name => string.Concat(Kind.ToString(), Slope == MapCellSlope.None ? "" : $".{Slope}", Edges.Name);
}
