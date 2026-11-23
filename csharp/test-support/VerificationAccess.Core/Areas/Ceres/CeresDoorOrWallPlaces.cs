using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Places a Ceres door or wall in a fixture slot as a placement of it would (#1953, #1989).</summary>
internal static class CeresDoorOrWallPlaces
{
    /// <summary>Binds <paramref name="place"/> to the Ceres door or wall <paramref name="actor"/> placed with <paramref name="settings"/>, without starting it.</summary>
    internal static void BindCeresDoorOrWall(this RoomEnemyPlace place, EnemyDefinitionId actor, EnemySpawnSettings settings)
    {
        place.BindPlaced(actor, settings);
    }

    /// <summary>
    /// Places the Ceres door or wall <paramref name="actor"/> with <paramref name="settings"/> at <paramref name="at"/> as
    /// <paramref name="enemies"/>' only enemy, as its room's population load places and sets it up (#2554).
    /// </summary>
    /// <returns>The place holding it.</returns>
    internal static RoomEnemyPlace StartCeresDoorOrWall(this World.RoomEnemySystem enemies, EnemyDefinitionId actor, EnemySpawnSettings settings,
        Position16<RoomPixels> at = default) =>
        enemies.LoadPopulation(PopulationPlacementAccess.Entry(actor, settings, at))[0];

    /// <summary>The elevator door <paramref name="place"/> holds.</summary>
    internal static CeresElevatorDoorEntity ElevatorDoor(this RoomEnemyPlace place) =>
        place.Entity as CeresElevatorDoorEntity
            ?? throw new InvalidOperationException($"Enemy slot {place.Ordinal} holds no Ceres elevator door.");
}
