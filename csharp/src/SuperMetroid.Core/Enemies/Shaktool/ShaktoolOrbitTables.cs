namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed Shaktool segment geometry, not editable animation data.</summary>
public static class ShaktoolOrbitTables
{
    /// <summary>$AA:E03D through E2BC, negative-cosine prefix and signed sine words.</summary>
    /// <remarks>
    /// Independently verified for #1165: index 0..319 maps to angle=(index+192)%256,
    /// then trunc(3072*sin(angle*3.14159/128)) toward zero. Reduced pi and full-cycle
    /// evaluation preserve the native asymmetric values and +/-3071 peaks without
    /// corrective samples. Reflection, true pi or direct negative cosine differ.
    /// This exact convention does not identify the original authoring software.
    /// </remarks>
    internal static int Sample(int index) => WordWrap.Signed(UnquantizedSample(index));

    /// <summary>Evaluates Shaktool's native-angle sine with 24 decimal Taylor terms instead of platform math functions.</summary>
    /// <remarks>Terms through x^47/47! keep full-cycle remainder and decimal roundoff below 1e-19 before scaling; the original-table proof checks both bounds at every index.</remarks>
    internal static decimal UnquantizedSample(int index)
    {
        if ((uint)index >= 320) throw new ArgumentOutOfRangeException(nameof(index));
        int angle = (index + 192) % 256;
        decimal x = angle * 3.14159m / 128;
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 24; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return 3072 * sum;
    }
    /// <summary>$AA:DC2A segment placement selects negative cosine for Y and
    /// the 64-word-shifted sine for X. Both are 8.8 displacements promoted to 16.16
    /// before addition to the preceding segment's position.</summary>
    public static FixedVector<RoomPixels> Displacement(ByteAngle angle) => new(
        SignedEightEight.FromSignedRaw(Sample(angle.RawValue + 64)).ToSixteenSixteenDelta(),
        SignedEightEight.FromSignedRaw(Sample(angle.RawValue)).ToSixteenSixteenDelta());
}
