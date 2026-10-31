namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{

    /// <summary>
    /// $A8:E682/$A8:E688: the three authored list/function pairs plus selector
    /// three's native adjacent-word observations. Its instruction-list lookup
    /// observes the first function word $E694, which is code rather than an animation
    /// list; its function lookup observes the following LDX opcode/operand word $54AE.
    /// </summary>
    internal static readonly SparkMovementDefinition[] InitialStates =
    [
        new(SparkProgram.Active, SparkEnemyFunction.AlwaysActive),
        new(SparkProgram.Active, SparkEnemyFunction.IntermittentActive),
        new(SparkProgram.Emitter, SparkEnemyFunction.EmitFallingSparks),
        new(null, SparkEnemyFunction.AdjacentCode),
    ];
}
