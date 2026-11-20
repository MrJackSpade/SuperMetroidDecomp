namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay: the hand that follows the pillar's top and throws the lava.</summary>
internal sealed record MagdolliteOverlaySpawnSettings : MagdolliteSpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
