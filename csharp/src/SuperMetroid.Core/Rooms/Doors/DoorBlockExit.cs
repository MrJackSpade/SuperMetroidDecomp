namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Where a door block leads (#2090): through a physical door definition, or to the elevator's pseudo-door
/// (<see cref="ElevatorRide"/>).
/// </summary>
/// <remarks>
/// An elevator's door blocks stop Samus and arm the elevator; they lead to no room, and every elevator's door blocks
/// lead to the same elevator ride.
/// </remarks>
public abstract partial record DoorBlockExit
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
    public sealed partial record ThroughDoor(DoorDefinition Door) : DoorBlockExit
    {
        /// <summary>The door's name.</summary>
        public override string ToString() => Door.Name;
    }

    /// <summary>A door block leading to the elevator's pseudo-door, the elevator ride.</summary>
    public sealed partial record ToElevator : DoorBlockExit
    {
        /// <summary>"elevator ride".</summary>
        public override string ToString() => "elevator ride";
    }
}
