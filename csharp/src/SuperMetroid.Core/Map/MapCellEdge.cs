namespace SuperMetroid.Core.Map;

/// <summary>What bounds one edge of an area-map cell (#1513).</summary>
public enum MapCellEdge
{
    /// <summary>Nothing: the place continues into the next cell.</summary>
    Open,

    /// <summary>A wall the map draws along the edge.</summary>
    Wall,

    /// <summary>The edge an elevator's shaft joins the cell at.</summary>
    ElevatorShaft,
}
