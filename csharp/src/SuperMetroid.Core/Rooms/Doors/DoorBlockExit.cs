namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Where a door block leads (#2090): through a physical door definition, or to the elevator's pseudo-door
/// (<see cref="ElevatorPseudoDoor"/>).
/// </summary>
/// <remarks>
/// The game tells them apart by the destination: the pseudo-door leads to no room, so collision treats the block as solid
/// and arms the elevator instead of publishing a transition. Nothing else of the pseudo-door is ever read, so no definition
/// is kept for it, and every elevator's door blocks lead to the same one.
/// </remarks>
public readonly record struct DoorBlockExit
{
    /// <summary>Holds a physical door, or none for the elevator pseudo-door.</summary>
    private DoorBlockExit(DoorDefinition? door) => Door = door;

    /// <summary>The physical door, or null for the elevator pseudo-door.</summary>
    public DoorDefinition? Door { get; }

    /// <summary>True for the elevator pseudo-door, which leads to no room.</summary>
    public bool IsElevatorPseudoDoor => Door is null;

    /// <summary>A door block leading through <paramref name="door"/>.</summary>
    public static DoorBlockExit Physical(DoorDefinition door) => new(Ensure.NotNull(door));

    /// <summary>A door block leading to the elevator pseudo-door.</summary>
    public static DoorBlockExit ElevatorPseudoDoor { get; } = new(null);

    /// <summary>The door's name, or "elevator pseudo-door".</summary>
    public override string ToString() => Door?.Name ?? "elevator pseudo-door";
}
