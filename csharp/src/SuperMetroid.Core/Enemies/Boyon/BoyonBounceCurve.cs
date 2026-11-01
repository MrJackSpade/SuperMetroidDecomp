using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce: its authored multiplier and height, and the curve sample its hardware multiply scales.</summary>
public static class BoyonBounceCurve
{
    /// <summary>The hardware multiplier parameter one's low byte selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside zero through seven.</exception>
    public static int InitialMultiplier(int index) =>
        (uint)index < (uint)InitialMultipliers.Length
            ? InitialMultipliers[index]
            : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The 8.8 jump height parameter one's high byte selects.</summary>
    /// <remarks>
    /// #625 / #644 exact NTSC J/U v1.0 algorithm: for high-byte selector i in 0..8,
    /// the unsigned height is 12288 + i*4096. All nine words
    /// are the authored heights. The initializer doubles
    /// the selector for its word offset; selector 9 would read the adjacent curve
    /// and is outside this height domain. VerifyCompiledBoyonSpeeds exercises every
    /// authored height through the real initializer and rejects selector 9.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside zero through eight.</exception>
    public static int InitialHeight(int index) => (uint)index < InitialHeightCount
        ? CoordinateAxis.Wrap(LowestInitialHeight + index * InitialHeightStep)
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The curve is k(k+1)/2, with explicit byte saturation beyond its stored samples.</summary>
    /// <remarks>
    /// #625 / #643 exact proof: each of the 23 authored unsigned bytes
    /// equals k*(k+1)/2 for authored index k in 0..22, ending at 253.
    /// The caller reads first, then substitutes 255 for every index at or above
    /// 23 before its eight-bit multiply; the nominal overread at index 23 is
    /// discarded. This method models that effective caller result, not a larger table.
    /// VerifyCompiledBoyonSpeeds checks all authored bytes, all 65,536 input indices, and
    /// every low-byte multiplier through the real production calculation.
    /// </remarks>
    public static int Sample(int index) => index < StoredSampleCount
        ? WordWrap.LowByte(index * (index + 1) / 2)
        : byte.MaxValue;

    /// <summary>Only the multiplier's low byte reaches the multiply; the product is 0 through 65025.</summary>
    public static int Multiply(int index, int multiplier) => Sample(index) * WordWrap.LowByte(multiplier);
}
