namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for, which way it grows and how fast (#1471, #1472, #1519).</summary>
/// <param name="Wait">What it waits for.</param>
/// <param name="Growth">The way it grows.</param>
/// <param name="ActivationDistance">
/// The horizontal distance in pixels Samus must come within, for a shutter that waits for her; zero for a timed one.
/// </param>
/// <param name="ActivationWait">The updates a timed shutter waits before it grows; zero for one that waits for Samus.</param>
/// <param name="GrowthSpeed">How far it grows each update, in pixels.</param>
internal sealed record GrowingShutterSpawnSettings(
    GrowingShutterWait Wait, GrowingShutterGrowth Growth, int ActivationDistance, int ActivationWait, SignedSixteenSixteen GrowthSpeed)
    : EnemySpawnSettings;
