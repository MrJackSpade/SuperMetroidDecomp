namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Whether a gunship bottom placement is the entrance pad (#1472).</summary>
/// <param name="EntrancePad">Whether it is the entrance pad rather than the hull.</param>
internal sealed record GunshipSpawnSettings(bool EntrancePad) : EnemySpawnSettings;
