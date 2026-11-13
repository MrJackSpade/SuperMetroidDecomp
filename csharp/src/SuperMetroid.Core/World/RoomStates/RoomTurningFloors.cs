using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Which turning floor a room state shows (#1649), decided once from what the state runs, for the import that installs
/// floors and the room load that binds one alike: a state that runs the elevator shaft's room main shows the shaft's floor,
/// since the shaft turns whenever its elevator door enters it and its elevator platform is drawn on the floor in every
/// state; a state whose enemy population is the Ceres Ridley fight's shows the floor his getaway flies over, since only his retreat starts
/// that scene. Ceres Ridley's arena after his escape runs the getaway's room main without him, so it shows none.
/// </summary>
public static class RoomTurningFloors
{
    /// <summary>The turning floor <paramref name="state"/> shows, <see cref="RoomTurningFloor.None"/> when it shows none.</summary>
    public static RoomTurningFloor Of(RoomStateSettings state)
    {
        Ensure.NotNull(state);
        if (ShowsShaftFloor(state.MainBehavior))
            return RoomTurningFloor.ElevatorShaft;
        // Only the Ceres Ridley fight's population starts his retreat, and with it the getaway's floor.
        return ReferenceEquals(state.EnemyPopulation, RoomEnemyPopulationDefinitions.CeresRidleyDefault)
            ? RoomTurningFloor.RidleyGetaway : RoomTurningFloor.None;
    }

    /// <summary>
    /// Whether a state running <paramref name="main"/> shows the elevator shaft's floor. Such a state draws none of its level's
    /// blocks: the shaft's screen is its turning floor alone (#1647).
    /// </summary>
    public static bool ShowsShaftFloor(RoomMainBehavior main) => main == RoomMainBehavior.CeresElevatorShaft;

    /// <summary>Whether <paramref name="state"/> shows a turning floor.</summary>
    public static bool ShowsOne(RoomStateSettings state) => Of(state) != RoomTurningFloor.None;
}
