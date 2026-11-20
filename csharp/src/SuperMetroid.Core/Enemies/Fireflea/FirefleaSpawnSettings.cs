namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>How a Fireflea flies (#1472, #1519, #2023).</summary>
/// <param name="Flight">Whether it bobs at a speed or circles at a turn per update.</param>
/// <param name="Radius">The radius of its circle, or how far it bobs to either side of where it starts, in pixels.</param>
internal sealed record FirefleaSpawnSettings(FirefleaFlight Flight, int Radius) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
