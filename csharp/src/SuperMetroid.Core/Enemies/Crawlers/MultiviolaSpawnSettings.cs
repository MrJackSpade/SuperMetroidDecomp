namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Which way a Multiviola first bounces and how fast (#1472, #1519).</summary>
/// <param name="Heading">Its first heading.</param>
/// <param name="Speed">Its speed, the magnitude its heading's sine and cosine scale.</param>
internal sealed record MultiviolaSpawnSettings(TrigonometryAngle Heading, int Speed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
