namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How close Samus must come before a Boulder starts, how it starts and how it rolls (#1471, #1472, #1519, #2023).</summary>
/// <param name="TriggerHeight">The vertical distance, in pixels, Samus must be strictly within.</param>
/// <param name="TriggerReach">The horizontal distance, in pixels, ahead of it Samus must be within.</param>
/// <param name="Start">Whether it falls from a drop or rolls at once.</param>
/// <param name="Roll">How it rolls.</param>
internal sealed record BoulderSpawnSettings(int TriggerHeight, int TriggerReach, BoulderStart Start, BoulderRoll Roll) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
