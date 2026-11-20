namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Whether a Powamp placement is its balloon, how far the grapple pulls it and whether the half refuses Samus's touch and
/// shots (#1472, #1519, #2474).
/// </summary>
/// <param name="Part">Which part of the Powamp it is.</param>
/// <param name="GrappleTravelDistance">The distance in pixels the grapple pulls the balloon.</param>
/// <param name="Contact">Whether Samus's touch and shots reach the half, as its placement says.</param>
internal sealed record PowampSpawnSettings(PowampPart Part, int GrappleTravelDistance, PowampContact Contact) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
