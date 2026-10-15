namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One compiled Ceres steam instruction/function selection.</summary>
internal readonly record struct CeresSteamInitialization(
    CeresSteamProgram Program,
    CeresSteamFunction Function);
