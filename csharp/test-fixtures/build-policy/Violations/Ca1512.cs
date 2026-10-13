namespace BuildPolicy;

/// <summary>CA1512 violation fixture using an explicit range exception instead of the throw helper.</summary>
internal static class Ca1512
{
    /// <summary>Rejects negative input with the explicit throw form under test.</summary>
    internal static int Check(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
}
