using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The oam and bg2 of one read boss display oracle result.</summary>
/// <param name="Oam">The oam.</param>
/// <param name="Bg2">The bg2.</param>
internal readonly record struct ReadBossDisplayOracleResult(EnemyExtendedDrawComponent[] Oam, EnemyBg2TilemapWrite[] Bg2);
