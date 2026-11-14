namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The turning floor a room state shows (#1649), or that it shows none. Only two Ceres rooms ever turn their picture: the
/// elevator shaft, entered through its elevator door, and Ceres Ridley's arena while Ridley flies away with the Baby. Every
/// other room, the rest of Ceres among them, shows no turning floor, installs none and draws none. Its values are arbitrary.
/// </summary>
public enum RoomTurningFloor
{
    /// <summary>The room shows no turning floor.</summary>
    None,

    /// <summary>
    /// The Ceres elevator shaft's floor: the whole shaft, shown turned while the room was entered through its elevator door,
    /// with the elevator platform's strip drawn over it.
    /// </summary>
    ElevatorShaft,

    /// <summary>The floor Ceres Ridley's getaway flies over: Ridley carrying the Baby's capsule, with his wings and the capsule drawn over it.</summary>
    RidleyGetaway,
}
