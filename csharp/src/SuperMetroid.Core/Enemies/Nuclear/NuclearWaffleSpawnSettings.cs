namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>How a nuclear waffle orbits (#1472, #1519).</summary>
/// <param name="AngularSpeed">Its sweep speed, in units of 1/256 turn per update.</param>
/// <param name="OrbitRadius">Its orbit radius in pixels.</param>
/// <param name="Direction">The way it sweeps.</param>
/// <param name="WaitTime">The updates it waits between sweeps.</param>
internal sealed record NuclearWaffleSpawnSettings(SignedSixteenSixteen AngularSpeed, int OrbitRadius, NuclearWaffleSweepDirection Direction, int WaitTime) : EnemySpawnSettings;
