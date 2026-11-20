namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>How long a Hibashi rests and whether it is the collision part (#1472, #1519).</summary>
/// <param name="InactiveTime">The updates it rests between flames.</param>
/// <param name="Part">Which part of the Hibashi it is.</param>
internal sealed record HibashiSpawnSettings(int InactiveTime, HibashiPart Part) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
