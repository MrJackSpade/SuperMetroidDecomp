namespace BuildPolicy;

internal static class Ca1512
{
    internal static int Check(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return value;
    }
}
