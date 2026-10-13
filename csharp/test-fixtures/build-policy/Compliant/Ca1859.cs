namespace BuildPolicy;

/// <summary>Compliant CA1859 fixture retaining a concrete list type throughout its count path.</summary>
internal static class Ca1859
{
    /// <summary>Counts the two entries produced by the concrete-list helper.</summary>
    internal static int Count() => Values().Count;

    /// <summary>Builds the concrete list used to exercise the rule's accepted form.</summary>
    private static List<int> Values() => [1, 2];
}
