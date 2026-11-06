namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>How a Kzan falls (#1472, #1519).</summary>
/// <param name="FallSpeed">The speed, in pixels per update, at which each fall starts; the fall then accelerates.</param>
/// <param name="FallWait">The updates it waits between falls.</param>
/// <param name="FallDistance">The pixels it falls.</param>
internal sealed record KzanSpawnSettings(SignedSixteenSixteen FallSpeed, int FallWait, int FallDistance) : EnemySpawnSettings;
