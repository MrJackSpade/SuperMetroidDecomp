namespace Fixture;

/// <summary>Compares value sequences in tools, where csharp/.editorconfig turns the SME rules off.</summary>
public static class OutsideComparison
{
    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/> hold the same values.</summary>
    public static bool Same(int[] left, int[] right) => left.Length == right.Length && left.AsSpan().SequenceEqual(right);
}
