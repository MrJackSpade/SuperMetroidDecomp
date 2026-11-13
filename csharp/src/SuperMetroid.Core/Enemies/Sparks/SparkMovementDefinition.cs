namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The animation and AI function a Spark starts with, as its placement names them.</summary>
/// <param name="Animation">The animation the Spark starts.</param>
/// <param name="Function">The AI function the Spark starts in.</param>
internal readonly record struct SparkMovementDefinition(
    SparkSegment Animation,
    SparkEnemyFunction Function);
