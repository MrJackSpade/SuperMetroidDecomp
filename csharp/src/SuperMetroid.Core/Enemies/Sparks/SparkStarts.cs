namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Chooses a Wrecked Ship Spark's starting program and function.</summary>
internal static class SparkStarts
{
    /// <summary>The program and function the population parameter's low two bits select.</summary>
    internal static SparkMovementDefinition InitialState(int populationParameter) =>
        SparkMovementDefinitions.InitialStates[populationParameter & 3];
}
