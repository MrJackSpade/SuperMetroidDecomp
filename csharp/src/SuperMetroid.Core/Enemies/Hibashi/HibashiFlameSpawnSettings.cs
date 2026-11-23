namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>A Hibashi's flame, and how long it rests between flames (#1472, #1519).</summary>
/// <param name="InactiveTime">The updates it rests between flames.</param>
internal sealed partial record HibashiFlameSpawnSettings(int InactiveTime) : HibashiSpawnSettings;
