namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>How a nuclear waffle orbits (#1472, #1519).</summary>
/// <param name="AngularSpeed">How fast it sweeps, each update, toward increasing angles.</param>
/// <param name="OrbitRadius">Its orbit radius in pixels.</param>
/// <param name="Direction">The way it sweeps.</param>
/// <param name="WaitTime">The updates it waits between sweeps.</param>
internal sealed record NuclearWaffleSpawnSettings(AngularSpeed AngularSpeed, int OrbitRadius, NuclearWaffleSweepDirection Direction, int WaitTime) : EnemySpawnSettings;
