namespace BuildPolicy;

/// <summary>Represents ca1512.</summary>
internal static class Ca1512
{
    /// <summary>check.</summary>
    internal static int Check(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return value;
    }
}
