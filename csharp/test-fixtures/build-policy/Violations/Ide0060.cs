namespace BuildPolicy;

/// <summary>IDE0060 violation fixture with an unused private method parameter.</summary>
internal static class Ide0060
{
    /// <summary>Passes an argument to the intentionally unused-parameter helper.</summary>
    internal static int Call() => Constant(7);

    /// <summary>Ignores its argument so the unused-parameter diagnostic has a stable target.</summary>
    private static int Constant(int unused) => 3;
}
