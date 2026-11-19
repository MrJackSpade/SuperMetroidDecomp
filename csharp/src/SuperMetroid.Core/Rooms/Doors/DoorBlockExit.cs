namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Where a door block leads (#2090): through a physical door definition, or to the elevator's pseudo-door
/// (<see cref="ElevatorRide"/>).
/// </summary>
/// <remarks>
/// The game tells them apart by the destination: the pseudo-door leads to no room, so collision treats the block as solid
/// and arms the elevator instead of publishing a transition. Nothing else of the pseudo-door is ever read, so no definition
/// is kept for it, and every elevator's door blocks lead to the same one.
/// </remarks>
public abstract record DoorBlockExit
{
    /// <summary>Only the nested cases derive.</summary>
    private DoorBlockExit()
    {
    }

    /// <summary>True for the elevator ride, which leads to no room.</summary>
    public bool IsElevatorRide => this is ToElevator;

    /// <summary>A door block leading through <paramref name="door"/>.</summary>
    public static DoorBlockExit Physical(DoorDefinition door) => new ThroughDoor(Ensure.NotNull(door));

    /// <summary>A door block leading to the elevator ride.</summary>
    public static DoorBlockExit ElevatorRide { get; } = new ToElevator();

    /// <summary>A door block leading through a physical door.</summary>
    /// <param name="Door">The door.</param>
    public sealed record ThroughDoor(DoorDefinition Door) : DoorBlockExit
    {
        /// <summary>The door's name.</summary>
        public override string ToString() => Door.Name;
    }

    /// <summary>A door block leading to the elevator's pseudo-door, the elevator ride.</summary>
    public sealed record ToElevator : DoorBlockExit
    {
        /// <summary>"elevator ride".</summary>
        public override string ToString() => "elevator ride";
    }
}
