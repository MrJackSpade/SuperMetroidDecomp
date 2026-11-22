using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>A check's enemy set up as the population load sets up one whose entry names no setting of its own (#2554).</summary>
internal static class PopulatedEnemySetUp
{
    /// <summary>The enemy a fixture place holds.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// Sets the enemy the check bound in the place up as the population load sets up one whose entry names no setting of
        /// its own: the entry is bound again with the settings such an entry gives its species, keeping the definition the
        /// check bound, and the species builds the enemy's entity, which runs its setup with a lone Samus.
        /// </summary>
        /// <exception cref="InvalidOperationException">The check has bound no enemy in the place.</exception>
        internal void SetUpAsPopulated()
        {
            EnemyDefinitionId species = self.HeldOccupant().DefinitionId;
            self.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, EnemySpawnSettingsCodec.Blank(species)));
            self.SetUpAsPlaced();
        }
    }
}
