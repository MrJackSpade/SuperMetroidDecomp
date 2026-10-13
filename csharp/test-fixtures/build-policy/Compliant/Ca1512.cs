namespace BuildPolicy;

/// <summary>Exercises the argument guard accepted by the CA1512 build policy.</summary>
internal static class Ca1512
{
    /// <summary>Rejects negative input through the framework guard and preserves other values.</summary>
    internal static int Check(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return value;
    }
}
