namespace BuildPolicy;

/// <summary>Exercises CA1826's direct-indexing form on a read-only list.</summary>
internal static class Ca1826
{
    /// <summary>Uses index zero so the compliant fixture does not enumerate the list to select its head.</summary>
    internal static int Head(IReadOnlyList<int> values) => values[0];
}
