namespace SuperMetroid.Core.Enemies;

/// <summary>Draws named selections from the enemy random-number word (#1461).</summary>
public static class RandomSampleFieldSelections
{
    /// <summary>Advances the random word and returns the value <paramref name="field"/> reads from the new sample.</summary>
    /// <param name="random">The random-number word.</param>
    /// <param name="field">The selection drawn.</param>
    public static int Next(this IEnemyRandomNumbers random, RandomSampleField field) =>
        field.Of(Ensure.NotNull(random).Generate());
}
