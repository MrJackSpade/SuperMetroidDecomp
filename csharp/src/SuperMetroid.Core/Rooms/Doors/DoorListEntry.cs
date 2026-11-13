namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One resolved entry of a room's door list: a physical door definition, or one of the
/// elevator pseudo-doors (<see cref="ElevatorPseudoDoor.Shared"/>,
/// <see cref="ElevatorPseudoDoor.MaridiaTourian"/>).
/// </summary>
/// <remarks>
/// The game tells them apart by the entry's destination: a pseudo-door leads to no room, so collision treats the
/// block as solid and arms the elevator instead of publishing a transition. Nothing else of a pseudo-door is ever
/// read, so no definition is modeled for it.
/// </remarks>
public readonly record struct DoorListEntry
{
    /// <summary>Pairs a physical door, or a pseudo-door kind for an elevator entry.</summary>
    private DoorListEntry(DoorDefinition? door, ElevatorPseudoDoor? pseudoDoor)
    {
        Door = door;
        PseudoDoor = pseudoDoor;
    }

    /// <summary>The physical door, or null for an elevator pseudo-door.</summary>
    public DoorDefinition? Door { get; }

    /// <summary>The elevator pseudo-door, or null for a physical door.</summary>
    public ElevatorPseudoDoor? PseudoDoor { get; }

    /// <summary>True for an elevator pseudo-door (zero destination).</summary>
    public bool IsElevatorPseudoDoor => Door is null;

    /// <summary>A physical door.</summary>
    public static DoorListEntry Physical(DoorDefinition door) => new(Ensure.NotNull(door), null);

    /// <summary>An elevator pseudo-door entry.</summary>
    public static DoorListEntry ElevatorPseudoDoor(ElevatorPseudoDoor pseudoDoor)
    {
        if (!Enum.IsDefined(pseudoDoor))
            throw new ArgumentOutOfRangeException(nameof(pseudoDoor), pseudoDoor, "Not an elevator pseudo-door.");
        return new(null, pseudoDoor);
    }
}
