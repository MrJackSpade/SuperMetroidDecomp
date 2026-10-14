namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Instruction-list and AI-function pair selected by a Spark population parameter.</summary>
internal readonly record struct SparkMovementDefinition(
    ushort InstructionList,
    SparkEnemyFunction Function);
