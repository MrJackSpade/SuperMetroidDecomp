namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How a Cacatac patrols (#1472, #1519).</summary>
/// <param name="Direction">
/// The way it patrols: from the start, or, for one placed to stop at first, once its first idle step starts it.
/// </param>
/// <param name="Start">Whether it patrols at once or stands still first (#2369).</param>
/// <param name="Mounting">Whether it hangs from a ceiling or stands on a floor.</param>
/// <param name="TravelDistance">How far, in pixels, it patrols to either side of where it starts.</param>
/// <param name="Speed">The speed it walks at, as a rightward displacement per update; it walks left at the same speed.</param>
internal sealed record CacatacSpawnSettings(HorizontalDirection Direction, CacatacStart Start, CacatacMounting Mounting, int TravelDistance, SignedSixteenSixteen Speed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
