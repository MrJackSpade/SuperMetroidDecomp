namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>A GRipper's patrol speed, starting direction and patrol bounds (#1471, #1472, #1519).</summary>
/// <param name="Speed">Its patrol speed in pixels per update; it patrols equally fast either way.</param>
/// <param name="Direction">The direction it starts moving in.</param>
/// <param name="MinimumX">The room column it turns back at moving left.</param>
/// <param name="MaximumX">The room column it turns back at moving right.</param>
internal sealed record GRipperSpawnSettings(SignedSixteenSixteen Speed, GRipperStartDirection Direction, int MinimumX, int MaximumX) : EnemySpawnSettings;
