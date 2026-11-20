namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What an escaping Etecoon does (#1472).</summary>
/// <param name="Role">Its role.</param>
internal sealed record EscapeEtecoonSpawnSettings(EscapeEtecoonRole Role) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
