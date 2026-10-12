namespace BuildPolicy;

/// <summary>Represents ca1512.</summary>
internal static class Ca1512
{
    /// <summary>check.</summary>
    internal static int Check(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
}
