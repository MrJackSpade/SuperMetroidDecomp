namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>A gauge over a list's level.</summary>
internal sealed class ListGauge
{
    /// <summary>A gauge at <paramref name="level"/>.</summary>
    /// <param name="level">The list's level.</param>
    internal ListGauge(int level)
    {
        Level = level;
    }

    /// <summary>The list's level.</summary>
    internal int Level { get; }

    /// <summary>The level as production reads it, scaled.</summary>
    internal int Reading => Scaled(Level);

    /// <summary>Twice <paramref name="value"/>.</summary>
    /// <param name="value">The value to scale.</param>
    private static int Scaled(int value) => value * 2;
}
