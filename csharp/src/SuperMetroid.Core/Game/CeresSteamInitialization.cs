namespace SuperMetroid.Core.Game;

/// <summary>One compiled Ceres steam instruction/function selection.</summary>
internal readonly record struct CeresSteamInitialization(
    ushort InstructionList,
    CeresSteamFunction Function);
