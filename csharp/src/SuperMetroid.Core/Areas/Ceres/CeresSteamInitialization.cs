namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One compiled Ceres steam animation/function selection.</summary>
internal readonly record struct CeresSteamInitialization(
    CeresSteamSegment Animation,
    CeresSteamAction Action);
