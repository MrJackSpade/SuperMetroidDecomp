using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which glass tube piece falls when Mother Brain's room shakes (#1472, #1519); the main tube's wait before it falls is its
/// own (<see cref="MotherBrainFallingTubePopulationDefinitions.MainFallWaitUpdates"/>).
/// </summary>
/// <param name="Piece">The tube piece.</param>
internal sealed record MotherBrainTubeSpawnSettings(MotherBrainTubePiece Piece) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
