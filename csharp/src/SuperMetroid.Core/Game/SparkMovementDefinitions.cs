namespace SuperMetroid.Core.Game;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{
    /// <summary>$A8:E688, the first function pointer observed past the three instruction-list entries.</summary>
    private const ushort AdjacentInstructionWord = (ushort)SparkEnemyFunction.AlwaysActive;

    /// <summary>$A8:E68E, the first two bytes of MainAI_Spark's LDX $0E54, observed past the three function entries.</summary>
    private const SparkEnemyFunction AdjacentFunctionWord = (SparkEnemyFunction)0x54ae;

    /// <summary>
    /// $A8:E682/$A8:E688: the three authored list/function pairs plus selector
    /// three's native adjacent-word observations. Its instruction-list lookup
    /// observes the first function word $E694; its function lookup observes the
    /// following LDX opcode/operand word $54AE.
    /// </summary>
    internal static SparkMovementDefinition InitialState(ushort populationParameter) =>
        (populationParameter & 3) switch
        {
            0 => new(SparkInstructionProgramDefinitions.Active, SparkEnemyFunction.AlwaysActive),
            1 => new(SparkInstructionProgramDefinitions.Active, SparkEnemyFunction.IntermittentActive),
            2 => new(SparkInstructionProgramDefinitions.Emitter, SparkEnemyFunction.EmitFallingSparks),
            _ => new(AdjacentInstructionWord, AdjacentFunctionWord),
        };
}

/// <summary>One population-selected Spark movement state, pairing the instruction list to execute with its initial enemy function.</summary>
/// <param name="InstructionList">Bank-$A8 instruction-list pointer selected for the Spark's population parameter.</param>
/// <param name="Function">Initial enemy function paired with that instruction list, including the native adjacent-word observation for selector three.</param>
internal readonly record struct SparkMovementDefinition(
    ushort InstructionList,
    SparkEnemyFunction Function);
