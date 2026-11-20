namespace SuperMetroid.Core.Enemies;

/// <summary>Occupants a check hands a species behaviour for an enemy no place of its fixture holds.</summary>
internal static class EnemyOccupantFixtures
{
    /// <summary>An enemy of <paramref name="species"/>, as a check that spawns its objects from an unbound place hands it.</summary>
    internal static RoomEnemyOccupant Of(EnemyDefinitionId species) =>
        new(RoomEnemyPlaceSpawnParameters.PlacementFor(species, EnemySpawnSettings.None));
}
