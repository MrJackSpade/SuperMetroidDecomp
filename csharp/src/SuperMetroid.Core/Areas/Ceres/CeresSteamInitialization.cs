namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The animation and action one Ceres steam vent starts with.</summary>
internal readonly record struct CeresSteamInitialization(
    CeresSteamSegment Animation,
    CeresSteamAction Action);
