using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce curve: the sample its multiplier scales at each curve index.</summary>
public static class BoyonBounceCurve
{
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

    /// <summary>The sample at <paramref name="index"/> scaled by <paramref name="multiplier"/>, 0 through 255; the product is 0 through 65025.</summary>
    public static int Multiply(int index, int multiplier) => Sample(index) * Ensure.BetweenInclusive(0, byte.MaxValue, multiplier);
}
