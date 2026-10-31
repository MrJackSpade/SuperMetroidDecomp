using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Which part of the Tourian entrance statue a placement draws (#1472).</summary>
/// <param name="Statue">The statue part; the base decoration also spawns the statue's fixed screen pieces.</param>
internal sealed record TourianEntranceStatueSpawnSettings(TourianEntranceStatueProgram Statue) : EnemySpawnSettings;
