namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// How far the grapple pulls a Powamp's balloon (#1472, #1519, #2474): the placement of the balloon, which heads its body,
/// names it; the body's placement names nothing (#2554).
/// </summary>
/// <param name="GrappleTravelDistance">The distance in pixels the grapple pulls the balloon.</param>
internal sealed record PowampSpawnSettings(int GrappleTravelDistance) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
