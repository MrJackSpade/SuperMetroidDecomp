namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How a Boyon bounces and how near Samus must come (#1472, #1519).</summary>
/// <param name="SpeedMultiplier">The multiplier its bounce curve is scaled by.</param>
/// <param name="JumpHeight">The height of its arc, in whole pixels.</param>
/// <param name="TriggerDistance">The horizontal distance, in pixels, Samus must come within.</param>
internal sealed record BoyonSpawnSettings(int SpeedMultiplier, int JumpHeight, int TriggerDistance) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
