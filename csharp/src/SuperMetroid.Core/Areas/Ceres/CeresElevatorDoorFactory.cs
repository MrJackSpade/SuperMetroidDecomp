using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Places the elevator room's blocked door (#2426): built only for a room that shows a turning floor, whose elevator
/// platform the door cycles. The door's animations, look, escape and quake are those of the room's owner of the Ceres
/// doors and walls.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="floor">The room's turning floor.</param>
[RoomLifetime]
internal sealed partial class CeresElevatorDoorFactory(CeresDoorBehavior doors, RoomFloor floor) : IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <summary>The room's owner of the Ceres doors and walls.</summary>
    private readonly CeresDoorBehavior _doors = Ensure.NotNull(doors);

    /// <summary>The room's turning floor, whose elevator platform the door cycles.</summary>
    private readonly RoomFloor _floor = Ensure.NotNull(floor);

    /// <inheritdoc/>
    /// <remarks>Only a room with a turning floor can place these.</remarks>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CeresElevatorDoor];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Of(new CeresElevatorDoorEntity(_doors, occupant, Ensure.NotNull(place), common, _floor));
}
