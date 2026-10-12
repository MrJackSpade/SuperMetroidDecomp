namespace BuildPolicy;

internal static class Ca1512
{
    internal static int Check(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
}
