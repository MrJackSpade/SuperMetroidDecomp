namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Whether a gunship bottom placement is the entrance pad (#1472).</summary>
/// <param name="Part">Which part of the gunship it is.</param>
internal sealed record GunshipSpawnSettings(GunshipBottomPiece Part) : EnemySpawnSettings;
