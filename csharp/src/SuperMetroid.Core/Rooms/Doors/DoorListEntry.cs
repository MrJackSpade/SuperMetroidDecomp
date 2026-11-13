namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One resolved entry of a room's door list: a physical door definition, or the elevator pseudo-door
/// (<see cref="ElevatorPseudoDoor"/>).
/// </summary>
/// <remarks>
/// The game tells them apart by the entry's destination: a pseudo-door leads to no room, so collision treats the
/// block as solid and arms the elevator instead of publishing a transition. Nothing else of a pseudo-door is ever
/// read, so no definition is modeled for it, and the lists that keep it in one of two places all hold the same entry.
/// </remarks>
public readonly record struct DoorListEntry
{
    /// <summary>Holds a physical door, or none for the elevator pseudo-door.</summary>
    private DoorListEntry(DoorDefinition? door) => Door = door;

    /// <summary>The physical door, or null for an elevator pseudo-door.</summary>
    public DoorDefinition? Door { get; }

    /// <summary>True for an elevator pseudo-door (zero destination).</summary>
    public bool IsElevatorPseudoDoor => Door is null;

    /// <summary>A physical door.</summary>
    public static DoorListEntry Physical(DoorDefinition door) => new(Ensure.NotNull(door));

    /// <summary>The elevator pseudo-door entry.</summary>
    public static DoorListEntry ElevatorPseudoDoor { get; } = new(null);
}
