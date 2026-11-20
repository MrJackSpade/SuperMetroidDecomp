namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>How many hits a Kago nest takes (#1472, #1519).</summary>
/// <param name="HitCount">The hits it takes.</param>
internal sealed record KagoSpawnSettings(int HitCount) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
