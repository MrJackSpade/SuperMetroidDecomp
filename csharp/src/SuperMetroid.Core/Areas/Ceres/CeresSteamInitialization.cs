namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The way one Ceres steam vent blows, whose dormant wait it starts with, and its action.</summary>
internal readonly record struct CeresSteamInitialization(
    CeresSteamWay Way,
    CeresSteamAction Action);
