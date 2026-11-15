using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which wall of Ceres Ridley's getaway chamber his getaway places (#1989).</summary>
/// <param name="Side">The side of the chamber the wall stands on.</param>
internal sealed record CeresGetawayChamberWallSpawnSettings(HorizontalDirection Side) : EnemySpawnSettings;
