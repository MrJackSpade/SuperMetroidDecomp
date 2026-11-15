namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What one door cap draw shows (#1994): the cleared opening a cap leaves, the closed cap, or the cap partway open as
/// it slides aside. The blue cap also has its closed image drawn solid, the cap an eye door flashes to. Each cap's
/// family draws the looks it has.
/// </summary>
internal enum DoorCapLook
{
    /// <summary>The cleared opening a cap leaves once it has opened: no cap at all.</summary>
    Cleared,
    /// <summary>The closed cap, its first block shootable.</summary>
    Closed,
    /// <summary>The closed blue cap drawn with its first block solid: the cap an eye door flashes to.</summary>
    ClosedSolid,
    /// <summary>The cap just starting to slide aside.</summary>
    Ajar,
    /// <summary>The cap half slid aside.</summary>
    HalfOpen,
    /// <summary>The cap nearly slid aside, a sliver left.</summary>
    NearlyOpen,
}
