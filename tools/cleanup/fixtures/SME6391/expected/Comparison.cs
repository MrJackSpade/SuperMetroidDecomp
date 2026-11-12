namespace Fixture;

/// <summary>Compares value sequences where the rule applies.</summary>
public static class Comparison
{
    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/> hold the same values.</summary>
    public static bool Same(int[] left, int[] right) => left.Length == right.Length && left.SequenceEqual(right);

    /// <summary>Whether the first values of <paramref name="left"/> are <paramref name="right"/>.</summary>
    public static bool Starts(int[] left, int[] right) => left.Length >= right.Length && left.AsSpan(0, right.Length).SequenceEqual(right);
}
