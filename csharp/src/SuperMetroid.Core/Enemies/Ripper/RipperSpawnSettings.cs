namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>How fast a Ripper flies and which way it starts (#1472, #1519).</summary>
/// <param name="Speed">Its flying speed in pixels per update; it flies equally fast either way.</param>
/// <param name="Direction">The way it starts.</param>
internal sealed record RipperSpawnSettings(SignedSixteenSixteen Speed, HorizontalDirection Direction) : EnemySpawnSettings;
