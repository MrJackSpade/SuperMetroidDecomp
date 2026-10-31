using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which authored Ceres door or wall a placement starts as (#1472).</summary>
/// <param name="Variant">The door or wall the placement starts as.</param>
internal sealed record CeresDoorSpawnSettings(CeresDoorVariant Variant) : EnemySpawnSettings;
