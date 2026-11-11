namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>How a Spark starts and how long it waits (#1472, #1519).</summary>
/// <param name="Start">The animation and behaviour it starts with, one of <see cref="SparkMovementDefinitions"/>'s three.</param>
/// <param name="BaseFunctionTime">The updates it waits between functions, 0 through 32767, when it does not randomize them.</param>
/// <param name="RandomizesDelay">Whether it waits a random number of updates between functions instead (#1607).</param>
internal sealed record SparkSpawnSettings(SparkMovementDefinition Start, int BaseFunctionTime, bool RandomizesDelay = false) : EnemySpawnSettings;
