namespace BuildPolicy;

/// <summary>CA1822 compliant fixture with a helper that uses only its argument.</summary>
internal static class Ca1822
{
    /// <summary>Doubles the supplied value without accessing instance state, exercising the static-member rule.</summary>
    internal static int Twice(int value) => value * 2;
}
