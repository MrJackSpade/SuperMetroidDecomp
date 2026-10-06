internal static partial class Program
{
    /// <summary>
    /// The 16-bit inputs that distinguish fixed-point table arithmetic: every low byte at the
    /// bottom of the range (all truncation remainders), both sides of the signed boundary, the
    /// top of the range (unsigned wraparound), and an evenly spread middle stride that walks
    /// every high byte. Integer products with a verified table entry cannot differ at other
    /// words, so this replaces exhaustive 65,536-word sweeps.
    /// </summary>
    private static IEnumerable<int> WordBoundarySamples()
    {
        for (int word = 0; word < 0x200; word++) yield return word;
        for (int word = 0x7f00; word < 0x8100; word++) yield return word;
        for (int word = 0xff00; word <= 0xffff; word++) yield return word;
        for (int word = 0x200; word < 0xff00; word += 0x101) yield return word;
    }

    /// <summary>
    /// Signed bytes that distinguish an added or subtracted signed offset: zero, +/-1, a small
    /// value of each sign and both signed extremes.
    /// </summary>
    private static readonly byte[] SignedByteClasses = [0x00, 0x01, 0xff, 0x05, 0xfb, 0x7f, 0x80];

    /// <summary>
    /// Angle words for readers that only consume the high byte: each high byte with a zero,
    /// a mid and a saturated fractional byte, proving the fraction is ignored.
    /// </summary>
    private static IEnumerable<int> AngleHighByteSamples()
    {
        for (int high = 0; high < 0x100; high++)
        {
            yield return high << 8;
            yield return high << 8 | 0x7f;
            yield return high << 8 | 0xff;
        }
    }
}
