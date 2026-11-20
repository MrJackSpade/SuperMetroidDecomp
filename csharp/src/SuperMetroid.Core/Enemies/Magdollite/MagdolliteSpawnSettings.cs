namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Which part of a Magdollite a placement is (#1472, #1519, #2023): its head, its rising body or its tracking overlay,
/// each its own settings (#2474), so only the head's carry how it rises.
/// </summary>
internal abstract record MagdolliteSpawnSettings : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
