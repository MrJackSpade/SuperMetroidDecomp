namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One compiled Ceres steam instruction/function selection.</summary>
internal readonly record struct CeresSteamInitialization(
    ushort InstructionList,
    CeresSteamFunction Function);
