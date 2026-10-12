namespace SuperMetroid.Core.Game;

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
    internal static short Sample(int index) => (short)UnquantizedSample(index);

    // Twenty-four decimal Taylor terms through x^47/47! avoid platform libm.
    // The full-cycle remainder and decimal roundoff fit inside 1e-19 before scaling;
    // the one original-table proof checks both ends of that interval at every index.
    internal static decimal UnquantizedSample(int index)
    {
        if ((uint)index >= 320) throw new IndexOutOfRangeException();
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
    /// the 64-word-shifted sine for X. Both are promoted from 8.8 to 16.16 by
    /// eight left shifts before addition to the preceding segment's position.</summary>
    public static (int X, int Y) Displacement(byte angle) =>
        (Sample(angle + 64) << 8,
         Sample(angle) << 8);
}
