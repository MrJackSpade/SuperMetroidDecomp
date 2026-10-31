namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Compiled initial program/function selections for Wrecked Ship Spark.</summary>
internal static class SparkMovementDefinitions
{

    /// <summary>The three authored animation and function pairs a Spark's initial state selects.</summary>
    internal static readonly SparkMovementDefinition[] InitialStates =
    [
        new(SparkProgram.Active, SparkEnemyFunction.AlwaysActive),
        new(SparkProgram.Active, SparkEnemyFunction.IntermittentActive),
        new(SparkProgram.Emitter, SparkEnemyFunction.EmitFallingSparks),
    ];
}
