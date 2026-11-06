namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How a Cacatac patrols (#1472, #1519).</summary>
/// <param name="Direction">The way it starts.</param>
/// <param name="Mounting">Whether it hangs from a ceiling or stands on a floor.</param>
/// <param name="TravelDistance">How far, in pixels, it patrols to either side of where it starts.</param>
/// <param name="Speed">The speed it walks at, as a rightward displacement per update; it walks left at the same speed.</param>
internal sealed record CacatacSpawnSettings(CacatacDirection Direction, CacatacMounting Mounting, int TravelDistance, SignedSixteenSixteen Speed) : EnemySpawnSettings;
