namespace BuildPolicy;

/// <summary>CA1826 violation fixture for LINQ indexing on an indexable collection.</summary>
internal static class Ca1826
{
    /// <summary>Calls LINQ First on an indexed list so CA1826 can require direct index access.</summary>
    internal static int Head(IReadOnlyList<int> values) => values.First();
}
