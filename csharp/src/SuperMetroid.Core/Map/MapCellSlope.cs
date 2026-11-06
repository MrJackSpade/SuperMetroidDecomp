namespace SuperMetroid.Core.Map;

/// <summary>
/// Which piece of a sloped hallway a room cell shows (#1513). The map draws a hallway's slope over steps of two cells
/// across and two down: its ceiling runs diagonally across the step's upper cells and its floor across the lower ones,
/// each crossing the lower half of the cell it starts in and the upper half of the next. A piece is named for the way the
/// hallway rises, whether it shows the ceiling or the floor, and the half of the cell its diagonal crosses.
/// </summary>
public enum MapCellSlope
{
    /// <summary>Not part of a sloped hallway.</summary>
    None,

    /// <summary>A hallway rising to the right: its floor crossing the cell's lower half, the hallway filling the cell above it.</summary>
    RightwardFloorLow,

    /// <summary>A hallway rising to the right: its floor crossing the cell's upper half, the hallway filling the cell above it.</summary>
    RightwardFloorHigh,

    /// <summary>A hallway rising to the right: its ceiling crossing the cell's lower half, the hallway filling the cell below it.</summary>
    RightwardCeilingLow,

    /// <summary>A hallway rising to the right: its ceiling crossing the cell's upper half, the hallway filling the cell below it.</summary>
    RightwardCeilingHigh,

    /// <summary>A hallway rising to the left: its floor crossing the cell's lower half, the hallway filling the cell above it.</summary>
    LeftwardFloorLow,

    /// <summary>A hallway rising to the left: its floor crossing the cell's upper half, the hallway filling the cell above it.</summary>
    LeftwardFloorHigh,

    /// <summary>A hallway rising to the left: its ceiling crossing the cell's lower half, the hallway filling the cell below it.</summary>
    LeftwardCeilingLow,

    /// <summary>A hallway rising to the left: its ceiling crossing the cell's upper half, the hallway filling the cell below it.</summary>
    LeftwardCeilingHigh,
}
