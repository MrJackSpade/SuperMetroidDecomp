using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which of the Ceres station's doors and walls a placement places (#1472, #1953).</summary>
/// <param name="Actor">The door or wall the placement places.</param>
internal sealed record CeresDoorOrWallSpawnSettings(CeresDoorOrWall Actor) : EnemySpawnSettings;
