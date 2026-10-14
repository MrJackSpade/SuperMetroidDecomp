namespace SuperMetroid.Core.Game;

/// <summary>Initial main-function and instruction-list selection for one Ceres-door actor.</summary>
internal readonly record struct CeresDoorInitializationDefinition(
    CeresDoorFunction MainFunction,
    ushort InstructionList);
