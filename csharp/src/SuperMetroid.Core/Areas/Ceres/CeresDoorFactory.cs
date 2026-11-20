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
internal sealed class CeresDoorFactory(CeresDoorBehavior doors) : IEnemyEntityFactory
{
    /// <summary>The room's owner of the Ceres doors and walls.</summary>
    private readonly CeresDoorBehavior _doors = Ensure.NotNull(doors);

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [
        EnemyDefinitionId.CeresDoor, EnemyDefinitionId.CeresRidleyRoomDoor,
        EnemyDefinitionId.CeresElevatorRoomWall, EnemyDefinitionId.CeresGetawayChamberWall,
    ];

    /// <summary>The door or wall actor <paramref name="place"/>'s definition names, facing or standing where its placement says.</summary>
    /// <exception cref="InvalidDataException">The definition is no Ceres door or wall, or the placement names no facing or side.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId actor = Ensure.NotNull(occupant).DefinitionId;
        if (actor == EnemyDefinitionId.CeresDoor)
        {
            HorizontalDirection facing = occupant.SpawnSettings<CeresDoorSpawnSettings>().Facing;
            if (facing is not (HorizontalDirection.Right or HorizontalDirection.Left))
                throw new InvalidDataException($"No Ceres door faces {facing}.");
            return EnemyOccupant.Of(new CeresDoorEntity(_doors, occupant, place, common, facing));
        }
        if (actor == EnemyDefinitionId.CeresRidleyRoomDoor)
            return EnemyOccupant.Of(new CeresRidleyRoomDoorEntity(_doors, occupant, place, common));
        if (actor == EnemyDefinitionId.CeresElevatorRoomWall)
            return EnemyOccupant.Of(new CeresElevatorRoomWallEntity(_doors, occupant, place, common));
        if (actor == EnemyDefinitionId.CeresGetawayChamberWall)
        {
            return EnemyOccupant.Of(new CeresGetawayChamberWallEntity(_doors, occupant, place, common, occupant.SpawnSettings<CeresGetawayChamberWallSpawnSettings>().Side switch
            {
                HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
                HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
                var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
            }));
        }
        throw new InvalidDataException($"{actor} is no Ceres door or wall.");
    }
}
