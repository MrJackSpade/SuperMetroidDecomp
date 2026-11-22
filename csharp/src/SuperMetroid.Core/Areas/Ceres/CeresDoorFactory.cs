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

    /// <summary>
    /// The door and wall actors, each facing or standing where its placement says: an ordinary door, the door of Ridley's
    /// room, the elevator room's wall and a wall of Ridley's getaway chamber, which the escape spawns
    /// (<see cref="CreateGetawayChamberWall"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    [field: NonSerialized]
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<CeresDoorSpawnSettings>([EnemyDefinitionId.CeresDoor], CreateDoor);
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.CeresRidleyRoomDoor], CreateRidleyRoomDoor);
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.CeresElevatorRoomWall], CreateElevatorRoomWall);
        registrar.Spawns<CeresGetawayChamberWallSpawnSettings>([EnemyDefinitionId.CeresGetawayChamberWall], CreateGetawayChamberWall);
    }

    /// <summary>The door of Ridley's room.</summary>
    private EnemyOccupant CreateRidleyRoomDoor(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Of(new CeresRidleyRoomDoorEntity(_doors, occupant, place, common));

    /// <summary>The elevator room's wall.</summary>
    private EnemyOccupant CreateElevatorRoomWall(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new CeresElevatorRoomWallEntity(_doors, occupant, place, common));

    /// <summary>An ordinary door, facing where its placement says.</summary>
    /// <exception cref="InvalidDataException">The placement names no facing.</exception>
    private EnemyOccupant CreateDoor(RoomEnemyOccupant occupant, CeresDoorSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        settings.Facing is HorizontalDirection.Right or HorizontalDirection.Left
            ? EnemyOccupant.Running(new CeresDoorEntity(_doors, occupant, place, common, settings.Facing))
            : throw new InvalidDataException($"No Ceres door faces {settings.Facing}.");

    /// <summary>A wall of Ridley's getaway chamber, standing on the side its placement says.</summary>
    /// <param name="occupant">The occupant the entity is created for.</param>
    /// <param name="settings">The side the wall stands on.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <exception cref="InvalidDataException">The placement names no side.</exception>
    internal EnemyOccupant CreateGetawayChamberWall(RoomEnemyOccupant occupant, CeresGetawayChamberWallSpawnSettings settings, RoomEnemyPlace place,
        EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new CeresGetawayChamberWallEntity(_doors, occupant, place, common, Ensure.NotNull(settings).Side switch
        {
            HorizontalDirection.Left => CeresDoorSegment.GetawayChamberLeftWall,
            HorizontalDirection.Right => CeresDoorSegment.GetawayChamberRightWall,
            var other => throw new InvalidDataException($"No getaway chamber wall stands on side {other}."),
        }));
}
