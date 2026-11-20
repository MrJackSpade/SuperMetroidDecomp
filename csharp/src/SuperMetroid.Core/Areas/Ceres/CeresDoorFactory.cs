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

    /// <summary>
    /// The door and wall actors, each facing or standing where its placement says: an ordinary door, the door of Ridley's
    /// room, the elevator room's wall and a wall of Ridley's getaway chamber, which the escape spawns
    /// (<see cref="CreateGetawayChamberWall"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void Register(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<CeresDoorSpawnSettings>([EnemyDefinitionId.CeresDoor], CreateDoor);
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.CeresRidleyRoomDoor], (occupant, _, place, common) =>
            EnemyOccupant.Of(new CeresRidleyRoomDoorEntity(_doors, occupant, place, common)));
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.CeresElevatorRoomWall], (occupant, _, place, common) =>
            EnemyOccupant.Running(new CeresElevatorRoomWallEntity(_doors, occupant, place, common)));
        registrar.Spawns<CeresGetawayChamberWallSpawnSettings>([EnemyDefinitionId.CeresGetawayChamberWall], CreateGetawayChamberWall);
    }

    /// <summary>An ordinary door, facing where its placement says.</summary>
    /// <exception cref="InvalidDataException">The placement names no facing.</exception>
    private EnemyOccupant CreateDoor(RoomEnemyOccupant occupant, CeresDoorSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new CeresDoorEntity(_doors, occupant, place, common, settings.Facing switch
        {
            HorizontalDirection.Right => CeresDoorSegment.DoorFacingRight,
            HorizontalDirection.Left => CeresDoorSegment.DoorFacingLeft,
            var other => throw new InvalidDataException($"No Ceres door faces {other}."),
        }));

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
