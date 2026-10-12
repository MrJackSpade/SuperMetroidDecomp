namespace BuildPolicy;

/// <summary>CA1859 violation fixture with a concrete list hidden behind an interface return type.</summary>
internal static class Ca1859
{
    /// <summary>Consumes the interface-typed list to expose the collection's virtual Count call.</summary>
    internal static int Count() => Values().Count;

    /// <summary>Returns a concrete list as IList so CA1859 can suggest a more specific return type.</summary>
    private static IList<int> Values() => new List<int> { 1, 2 };
}
