using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Places the Ceres station's doors and walls every room can hold (#1953, #2426): an ordinary door, the door of Ridley's
/// room, the elevator room's wall and a wall of Ridley's getaway chamber, each its own actor over the room's owner of the
/// Ceres doors and walls.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
[RoomLifetime]
internal sealed partial class CeresDoorFactory(CeresDoorBehavior doors) : IEnemyEntityFactory
{
    /// <summary>The room's owner of the Ceres doors and walls.</summary>
    private readonly CeresDoorBehavior _doors = Ensure.NotNull(doors);

    /// <inheritdoc/>
    [field: NonSerialized]
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
        {
            [EnemyDefinitionId.CeresDoor] = (occupant, place, common) =>
            {
                HorizontalDirection facing = occupant.SpawnSettings<CeresDoorSpawnSettings>().Facing;
                if (facing is not (HorizontalDirection.Right or HorizontalDirection.Left))
                    throw new InvalidDataException($"No Ceres door faces {facing}.");
                return EnemyOccupant.Of(new CeresDoorEntity(_doors, occupant, place, common, facing));
            },
            [EnemyDefinitionId.CeresRidleyRoomDoor] = (occupant, place, common) => EnemyOccupant.Of(new CeresRidleyRoomDoorEntity(_doors, occupant, place, common)),
            [EnemyDefinitionId.CeresElevatorRoomWall] = (occupant, place, common) => EnemyOccupant.Of(new CeresElevatorRoomWallEntity(_doors, occupant, place, common)),
            [EnemyDefinitionId.CeresGetawayChamberWall] = (occupant, place, common) => EnemyOccupant.Of(new CeresGetawayChamberWallEntity(_doors, occupant, place, common, occupant.SpawnSettings<CeresGetawayChamberWallSpawnSettings>().Side switch
                {
                HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
                HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
                var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
                })),
        };
}
