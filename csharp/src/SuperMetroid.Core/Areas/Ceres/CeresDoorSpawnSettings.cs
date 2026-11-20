using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which way an ordinary Ceres door a placement places faces (#1472, #1989).</summary>
/// <param name="Facing">The way the door faces, which selects its facing's animations.</param>
internal sealed record CeresDoorSpawnSettings(HorizontalDirection Facing) : EnemySpawnSettings;
