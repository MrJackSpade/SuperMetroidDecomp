namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for, which way it grows and how fast (#1471, #1472, #1519).</summary>
/// <param name="Wait">What it waits for, with the updates or distance its wait reads (#2560).</param>
/// <param name="Growth">The way it grows.</param>
/// <param name="GrowthSpeed">How far it grows each update, in pixels.</param>
internal sealed partial record GrowingShutterSpawnSettings(
    GrowingShutterWait Wait, VerticalDirection Growth, SignedSixteenSixteen GrowthSpeed)
    : EnemySpawnSettings;
