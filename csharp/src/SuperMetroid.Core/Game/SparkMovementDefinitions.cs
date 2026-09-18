namespace SuperMetroid.Core.Game;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{
    /// <summary>
    /// $A8:E682/$A8:E688: the three authored list/function pairs plus selector
    /// three's native adjacent-word observations. Its instruction-list lookup
    /// observes the first function word $E694; its function lookup observes the
    /// following PHB/TCS opcode word $54AE.
    /// </summary>
    private static readonly SparkMovementDefinition[] InitialStates =
    [
        new(0xe5d1, SparkEnemyFunction.AlwaysActive),
        new(0xe5d1, SparkEnemyFunction.IntermittentActive),
        new(0xe609, SparkEnemyFunction.EmitFallingSparks),
        new(0xe694, (SparkEnemyFunction)0x54ae),
    ];

    internal static SparkMovementDefinition InitialState(ushort populationParameter) =>
        InitialStates[populationParameter & 3];
}

internal readonly record struct SparkMovementDefinition(
    ushort InstructionList,
    SparkEnemyFunction Function);
