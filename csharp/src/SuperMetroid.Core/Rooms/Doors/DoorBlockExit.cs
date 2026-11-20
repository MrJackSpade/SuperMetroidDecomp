namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Where a door block leads (#2090): through a physical door definition, or to the elevator's pseudo-door
/// (<see cref="ElevatorRide"/>).
/// </summary>
/// <remarks>
/// An elevator's door blocks stop Samus and arm the elevator; they lead to no room, and every elevator's door blocks
/// lead to the same elevator ride.
/// </remarks>
public readonly record struct DoorBlockExit
{
    /// <summary>Holds a physical door, or none for the elevator ride.</summary>
    private DoorBlockExit(DoorDefinition? door) => Door = door;

    /// <summary>The physical door, or null for the elevator ride.</summary>
    public DoorDefinition? Door { get; }

    /// <summary>True for the elevator ride, which leads to no room.</summary>
    public bool IsElevatorRide => Door is null;

    /// <summary>A door block leading through <paramref name="door"/>.</summary>
    public static DoorBlockExit Physical(DoorDefinition door) => new(Ensure.NotNull(door));

    /// <summary>A door block leading to the elevator ride.</summary>
    public static DoorBlockExit ElevatorRide { get; } = new(null);

    /// <summary>The door's name, or "elevator ride".</summary>
    public override string ToString() => Door?.Name ?? "elevator ride";
}
