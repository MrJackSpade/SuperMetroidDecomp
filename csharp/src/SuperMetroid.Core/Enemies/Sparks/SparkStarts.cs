namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>Chooses a Wrecked Ship Spark's starting program and function.</summary>
internal static class SparkStarts
{
    /// <summary>The program and function <paramref name="initialState"/> selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialState"/> is not one of the three authored states.</exception>
    internal static SparkMovementDefinition InitialState(int initialState) =>
        Ensure.Element(SparkMovementDefinitions.InitialStates, initialState);
}
