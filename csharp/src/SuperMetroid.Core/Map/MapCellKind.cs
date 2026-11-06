namespace SuperMetroid.Core.Map;

/// <summary>What an area-map cell shows (#1513): nothing, or which kind of place the map draws there.</summary>
public enum MapCellKind
{
    /// <summary>Nothing: no place is mapped in the cell, and it shows nothing.</summary>
    Empty,

    /// <summary>Part of a room, filled and bounded by the walls on its edges.</summary>
    Room,

    /// <summary>Part of a room that holds an item, marked by a dot.</summary>
    Item,

    /// <summary>A save room, marked by its letter.</summary>
    SaveStation,

    /// <summary>A room an elevator stops in, marked by the elevator's platform; the elevator's shaft joins it at one edge.</summary>
    Elevator,

    /// <summary>Part of an elevator's shaft between the rooms it stops in, drawn as a dotted column.</summary>
    ElevatorShaft,

    /// <summary>The arrow at the end of an elevator's shaft that leads out of the area, pointing away from the shaft.</summary>
    ElevatorExit,

    /// <summary>Part of a passage drawn as a dotted corridor without walls, crossing between rooms or out of the area.</summary>
    Passage,
}
