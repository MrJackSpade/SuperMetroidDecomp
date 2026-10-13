namespace BuildPolicy;

/// <summary>Checks that the build policy preserves intentional parameter flow through a private helper.</summary>
internal static class Ide0060
{
    /// <summary>Passes seven through the helper, yielding ten.</summary>
    internal static int Call() => Constant(7);

    /// <summary>Offsets the caller's value by three; the parameter participates in the result.</summary>
    private static int Constant(int value) => value + 3;
}
