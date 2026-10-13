namespace BuildPolicy;

/// <summary>Exercises the compact pattern form accepted by the IDE0078 build policy.</summary>
internal static class Ide0078
{
    /// <summary>Uses a disjunction pattern to recognize either of the two boundary values.</summary>
    internal static bool IsEdge(int value) => value is 1 or 2;
}
