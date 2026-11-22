using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>A check's enemies set up as the population load sets up ones whose entries name no setting of their own (#2554).</summary>
internal static class PopulatedEnemySetUp
{
    /// <summary>A check's room, whose enemies the check bound.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Sets the enemies the check bound in <paramref name="places"/> up as the room's population load sets up ones whose
        /// entries name no setting of their own: each entry gets the settings such an entry gives its species, keeping the
        /// definition the check bound, and the room loads the entries the check authored (<see cref="PopulationPlacementAccess"/>),
        /// its species building each enemy's entity, which runs its setup with a lone Samus.
        /// </summary>
        /// <exception cref="InvalidOperationException">The check has bound no enemy in one of the places.</exception>
        internal void SetUpAsPopulated(params RoomEnemyPlace[] places)
        {
            foreach (RoomEnemyPlace place in places)
            {
                EnemyDefinitionId species = place.HeldOccupant().DefinitionId;
                place.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, EnemySpawnSettingsCodec.Blank(species)));
            }
            _ = enemies.LoadAuthoredPopulation();
        }
    }
}
