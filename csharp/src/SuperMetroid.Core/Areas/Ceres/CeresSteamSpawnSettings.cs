using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which way a Ceres steam vent blows (#1472).</summary>
/// <param name="Variant">The vent and the way it blows.</param>
internal sealed record CeresSteamSpawnSettings(CeresSteamVariant Variant) : EnemySpawnSettings;
