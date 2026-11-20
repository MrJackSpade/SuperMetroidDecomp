namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How a Choot jumps and falls (#1472, #1519).</summary>
/// <param name="FallingPath">The path it falls along.</param>
/// <param name="PatternLoops">The loops of its falling path it falls through; zero falls through it once, as one does.</param>
/// <param name="JumpDelay">Updates before its first jump.</param>
internal sealed record ChootSpawnSettings(ChootFallingPath FallingPath, int PatternLoops, int JumpDelay) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
