namespace Engine.Shared;

/// <summary>
/// Angles and the sine of an angle, in whole numbers only, so that anything moving in a wave or a circle moves the same
/// way on every machine. An angle is counted in 256ths of a full turn.
/// </summary>
public static class Turns
{
    /// <summary>How many angle steps make a full turn.</summary>
    public const int PerTurn = 256;

    /// <summary>
    /// <paramref name="size"/> times the sine of <paramref name="angle"/>, to the nearest part of a pixel the game holds.
    /// The sine is a close whole-number approximation, exact at each quarter turn.
    /// </summary>
    public static Pixels Sine(int angle, Pixels size)
    {
        int wrapped = ((angle % PerTurn) + PerTurn) % PerTurn;
        int half = PerTurn / 2;
        bool negative = wrapped >= half;
        long within = negative ? wrapped - half : wrapped;
        long product = within * (half - within);
        // Bhaskara's approximation of the sine over a half turn, scaled to 65536 for one.
        long sine = 4 * product * Pixels.FractionsPerPixel / (20480 - product);
        long scaled = sine * FractionsOf(size) / Pixels.FractionsPerPixel;
        return FromFractions(negative ? -scaled : scaled);
    }

    /// <summary><paramref name="size"/> times the cosine of <paramref name="angle"/>.</summary>
    public static Pixels Cosine(int angle, Pixels size) => Sine(angle + PerTurn / 4, size);

    /// <summary>A distance as a count of fractions of a pixel.</summary>
    private static long FractionsOf(Pixels distance) => (long)distance.Whole * Pixels.FractionsPerPixel + distance.Fraction;

    /// <summary>A distance from a count of fractions of a pixel.</summary>
    private static Pixels FromFractions(long fractions)
    {
        long whole = Math.DivRem(fractions, Pixels.FractionsPerPixel, out long fraction);
        if (fraction < 0)
        {
            fraction += Pixels.FractionsPerPixel;
            whole--;
        }

        return Pixels.FromParts(checked((int)whole), (int)fraction);
    }
}
