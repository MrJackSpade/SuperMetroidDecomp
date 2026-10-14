namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{
    /// <summary>$A8:E68E, the first two bytes of MainAI_Spark's LDX $0E54, observed past the three function entries.</summary>
    private const SparkEnemyFunction AdjacentFunctionWord = (SparkEnemyFunction)0x54ae;

    /// <summary>
    /// $A8:E682/$A8:E688: the three authored list/function pairs plus selector
    /// three's native adjacent-word observations. Its instruction-list lookup
    /// observes the first function word $E694, which is code rather than an animation
    /// list; its function lookup observes the following LDX opcode/operand word $54AE.
    /// </summary>
    internal static SparkMovementDefinition InitialState(ushort populationParameter) =>
        (populationParameter & 3) switch
        {
            0 => new(SparkProgram.Active, SparkEnemyFunction.AlwaysActive),
            1 => new(SparkProgram.Active, SparkEnemyFunction.IntermittentActive),
            2 => new(SparkProgram.Emitter, SparkEnemyFunction.EmitFallingSparks),
            _ => new(null, AdjacentFunctionWord),
        };
}
