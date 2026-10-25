namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The enemies' aim-angle division (#1420): an unsigned word dividend over a byte divisor. A zero divisor gives the
/// largest word instead of failing, and a distance that does not fit the byte divisor is truncated by its caller, so
/// the aimed angle bends exactly as it originally did.
/// </summary>
[CompatibilityService("the aim-angle division: a word over a byte, with a zero divisor giving the largest word, so distances beyond a byte bend the aimed angle")]
public static class UnsignedDivision
{
    /// <summary>
    /// The unsigned quotient of <paramref name="dividend"/> over <paramref name="divisor"/>; a zero divisor gives
    /// 65535, including when the dividend is also zero. The dividend is a word, 0 through 65535, and the divisor a
    /// byte, 0 through 255.
    /// </summary>
    public static ushort Quotient(int dividend, int divisor)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, dividend);
        return Ensure.BetweenInclusive(0, byte.MaxValue, divisor) == 0 ? ushort.MaxValue : (ushort)(dividend / divisor);
    }
}
