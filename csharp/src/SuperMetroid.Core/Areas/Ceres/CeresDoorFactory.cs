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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PartFactories.Keys];

    /// <summary>The door or wall actor <paramref name="place"/>'s definition names, facing or standing where its placement says.</summary>
    /// <exception cref="KeyNotFoundException">The definition is no Ceres door or wall.</exception>
    /// <exception cref="InvalidDataException">The placement names no facing or side.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        PartFactories[Ensure.NotNull(occupant).DefinitionId](this, occupant, place, common);

    /// <summary>
    /// The factory of each part the species creates, by the definition that places it: the species serves these
    /// definitions, and each builds its own part from the species it is given.
    /// </summary>
    private static readonly Dictionary<EnemyDefinitionId, EnemyPartFactory<CeresDoorFactory>> PartFactories =
        new()
        {
            [EnemyDefinitionId.CeresDoor] = static (species, occupant, place, common) =>
            {
                HorizontalDirection facing = occupant.SpawnSettings<CeresDoorSpawnSettings>().Facing;
                if (facing is not (HorizontalDirection.Right or HorizontalDirection.Left))
                    throw new InvalidDataException($"No Ceres door faces {facing}.");
                return EnemyOccupant.Of(new CeresDoorEntity(species._doors, occupant, place, common, facing));
            },
            [EnemyDefinitionId.CeresRidleyRoomDoor] = static (species, occupant, place, common) => EnemyOccupant.Of(new CeresRidleyRoomDoorEntity(species._doors, occupant, place, common)),
            [EnemyDefinitionId.CeresElevatorRoomWall] = static (species, occupant, place, common) => EnemyOccupant.Of(new CeresElevatorRoomWallEntity(species._doors, occupant, place, common)),
            [EnemyDefinitionId.CeresGetawayChamberWall] = static (species, occupant, place, common) => EnemyOccupant.Of(new CeresGetawayChamberWallEntity(species._doors, occupant, place, common, occupant.SpawnSettings<CeresGetawayChamberWallSpawnSettings>().Side switch
                {
                HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
                HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
                var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
                })),
        };
}
