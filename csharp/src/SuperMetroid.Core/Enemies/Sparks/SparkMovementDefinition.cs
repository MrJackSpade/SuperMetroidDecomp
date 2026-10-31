namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Animation and AI-function pair selected by a Spark population parameter.</summary>
/// <param name="Program">The animation the Spark starts.</param>
/// <param name="Function">The AI function the Spark starts in.</param>
internal readonly record struct SparkMovementDefinition(
    SparkProgram Program,
    SparkEnemyFunction Function);
