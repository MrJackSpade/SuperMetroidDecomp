namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Fixed Shaktool segment geometry, not editable animation data.</summary>
public static class ShaktoolOrbitTables
{
    /// <summary>The orbit's sine at <paramref name="angle"/>, in 256ths of a pixel.</summary>
    /// <remarks>
    /// Independently verified for #1165: trunc(3072*sin(angle*3.14159/128)) toward zero. Reduced pi and full-cycle
    /// evaluation preserve the asymmetric values and +/-3071 peaks without corrective samples. Reflection, true pi or
    /// direct negative cosine differ. This exact convention does not identify the authoring software.
    /// </remarks>
    internal static int SampleAt(TrigonometryAngle angle) => (int)UnquantizedSampleAt(angle);

    /// <summary>Evaluates Shaktool's sine, in 256ths of a turn, with 24 decimal Taylor terms instead of platform math functions.</summary>
    /// <remarks>Terms through x^47/47! keep full-cycle remainder and decimal roundoff below 1e-19 before scaling; the authored-table proof checks both bounds at every angle.</remarks>
    internal static decimal UnquantizedSampleAt(TrigonometryAngle angle)
    {
        decimal x = angle.RawValue * 3.14159m / 128;
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 24; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return 3072 * sum;
    }
    /// <summary>
    /// A segment's place from the one before it at <paramref name="angle"/>: the orbit's sine at the angle across, and its
    /// sine a quarter turn behind the angle (the negative cosine) down, each in 256ths of a pixel.
    /// </summary>
    public static FixedVector<RoomPixels> Displacement(TrigonometryAngle angle) => new(
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(SampleAt(angle)),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(SampleAt(angle.TurnedBy(TrigonometryAngle.HalfTurn).TurnedBy(TrigonometryAngle.QuarterTurn))));
}
