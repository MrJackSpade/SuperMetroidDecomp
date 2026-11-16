using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// Checks the Yapping Maw's bend chain (#1620) against the original's chained addition.
/// </summary>
internal static class YappingMawBendChainVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>Words at and around the ends and the middle of the sixteen-bit range, and small values either way.</summary>
    private static readonly int[] WordClasses =
        [0x0000, 0x0001, 0x0002, 0x007f, 0x0080, 0x00ff, 0x0100, 0x7ffe, 0x7fff, 0x8000, 0x8001, 0xff00, 0xff80, 0xfffe, 0xffff];

    /// <summary>
    /// Every offset word with every correction class, with and without the previous offset passing the end: each offset
    /// lands on the chained addition's word, and the next offset moves one pixel further exactly when that addition
    /// leaves its carry.
    /// </summary>
    internal static void VerifyYappingMawBendChain()
    {
        foreach (int correction in WordClasses)
        foreach (bool previousPassed in new[] { false, true })
        {
            int carryIn = previousPassed ? 1 : 0;
            for (int offset = 0; offset < WordRange; offset++)
            {
                var chain = new YappingMawBendChain();
                if (previousPassed && chain.Corrected(0xffff, 0x0001) != 0)
                    throw new InvalidOperationException("The chain's priming offset did not land on zero.");
                int sum = offset + correction + carryIn;
                int corrected = chain.Corrected(offset, correction);
                int next = chain.Corrected(0, 0);
                if (corrected != (sum & ushort.MaxValue) || next != (sum > ushort.MaxValue ? 1 : 0))
                    throw new InvalidOperationException(
                        $"Offset {offset:X4} corrected by {correction:X4} after a pass {previousPassed}: {corrected:X4} then {next}, not the chained addition.");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => new YappingMawBendChain().Corrected(WordRange, 0), "an offset past the coordinate space is refused");
        Console.WriteLine("  Yapping Maw bend chain: every offset and correction class matches the chained addition.");
    }
}
