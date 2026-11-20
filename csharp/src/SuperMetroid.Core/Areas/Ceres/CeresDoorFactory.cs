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
internal sealed class CeresDoorFactory(CeresDoorBehavior doors) : IEnemyEntityFactory, IEnemySettingsFactory<CeresDoorSpawnSettings>,
    IEnemySettingsFactory<CeresGetawayChamberWallSpawnSettings>
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
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    {
        EnemyDefinitionId actor = Ensure.NotNull(place).DefinitionId;
        if (actor == EnemyDefinitionId.CeresRidleyRoomDoor)
            return new CeresRidleyRoomDoorEntity(_doors, occupant, place, common);
        if (actor == EnemyDefinitionId.CeresElevatorRoomWall)
            return new CeresElevatorRoomWallEntity(_doors, place, common);
        throw new InvalidDataException($"{actor} is no Ceres door or wall told nothing.");
    }

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry)
    {
        IEnemyEntityFactory untold = this;
        Ensure.NotNull(registry).Add(untold);
        registry.Add<CeresDoorSpawnSettings>(this);
        registry.Add<CeresGetawayChamberWallSpawnSettings>(this);
    }

    /// <summary>A Ceres door, facing as its placement names.</summary>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, CeresDoorSpawnSettings settings) =>
        new CeresDoorEntity(_doors, place, common, Ensure.NotNull(settings).Facing switch
        {
            HorizontalDirection.Right => CeresDoorSegment.DoorFacingRight,
            HorizontalDirection.Left => CeresDoorSegment.DoorFacingLeft,
            var other => throw new InvalidDataException($"No Ceres door faces {other}."),
        });

    /// <summary>A getaway chamber wall, standing on the side its placement names.</summary>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, CeresGetawayChamberWallSpawnSettings settings) =>
        new CeresGetawayChamberWallEntity(_doors, place, common, Ensure.NotNull(settings).Side switch
        {
            HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
            HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
            var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
        });
}
