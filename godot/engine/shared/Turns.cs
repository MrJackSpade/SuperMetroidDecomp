namespace Engine.Shared;

/// <summary>
/// Angles and the sine of an angle, in whole numbers only, so that anything moving in a wave or a circle moves the same
/// way on every machine. An angle is counted in 256ths of a full turn.
/// </summary>
public static class Turns
{
    /// <summary>How many angle steps make a full turn.</summary>
    public const int PerTurn = 256;

    /// <summary>How finely a sine is taken.</summary>
    public enum Fineness
    {
        /// <summary>To the nearest 65536th.</summary>
        Fine,

        /// <summary>
        /// In whole 256ths, cut off toward nothing, and never the whole size except at the quarter turn itself: the
        /// sine of a game that keeps its sines in a small table.
        /// </summary>
        Coarse,
    }

    /// <summary>The sine of each step of the first half turn, in 65536ths, to the nearest.</summary>
    private static readonly int[] FineHalfWave = [.. Enumerable.Range(0, PerTurn / 2).Select(step => (int)Math.Round(65536m * UnitHalfWave(step)))];

    /// <summary>The sine of each step of the first half turn, in 256ths, cut off, and short of whole except at the quarter turn.</summary>
    private static readonly int[] CoarseHalfWave =
        [.. Enumerable.Range(0, PerTurn / 2).Select(step => step == PerTurn / 4 ? 256 : Math.Min(255, (int)(256m * UnitHalfWave(step))))];

    /// <summary><paramref name="size"/> times the sine of <paramref name="angle"/>, to the nearest part of a pixel the game holds.</summary>
    public static Pixels Sine(int angle, Pixels size) => Sine(angle, size, Fineness.Fine);

    /// <summary><paramref name="size"/> times the sine of <paramref name="angle"/>, the sine taken as finely as asked.</summary>
    public static Pixels Sine(int angle, Pixels size, Fineness fineness)
    {
        int wrapped = Wrap(angle);
        int half = PerTurn / 2;
        bool negative = wrapped >= half;
        int within = negative ? wrapped - half : wrapped;
        long scaled = fineness == Fineness.Coarse
            ? FractionsOf(size) * CoarseHalfWave[within] / 256
            : FractionsOf(size) * FineHalfWave[within] / Pixels.FractionsPerPixel;
        return FromFractions(negative ? -scaled : scaled);
    }

    /// <summary><paramref name="size"/> times the cosine of <paramref name="angle"/>.</summary>
    public static Pixels Cosine(int angle, Pixels size) => Sine(angle + PerTurn / 4, size, Fineness.Fine);

    /// <summary><paramref name="size"/> times the cosine of <paramref name="angle"/>, the cosine taken as finely as asked.</summary>
    public static Pixels Cosine(int angle, Pixels size, Fineness fineness) => Sine(angle + PerTurn / 4, size, fineness);

    /// <summary>The sine of <paramref name="angle"/> in 256ths, as a coarse sine takes it: -256 through 256.</summary>
    public static int CoarseSine(int angle)
    {
        int wrapped = Wrap(angle);
        int half = PerTurn / 2;
        return wrapped >= half ? -CoarseHalfWave[wrapped - half] : CoarseHalfWave[wrapped];
    }

    /// <summary>
    /// The angle of the direction that lies <paramref name="across"/> to the right and <paramref name="down"/> below: 0
    /// straight right, a quarter turn straight down, to the nearest step.
    /// </summary>
    public static int AngleOf(long across, long down)
    {
        if (across == 0 && down == 0)
            return 0;
        // The step whose direction lies closest to the point's, found with whole numbers only.
        int best = 0;
        long bestAlong = long.MinValue;
        for (int step = 0; step < PerTurn; step++)
        {
            long along = across * FineSigned(step + PerTurn / 4) + down * FineSigned(step);
            if (along > bestAlong)
            {
                bestAlong = along;
                best = step;
            }
        }

        return best;
    }

    /// <summary>An angle brought into 0 through one step short of a full turn.</summary>
    public static int Wrap(int angle) => ((angle % PerTurn) + PerTurn) % PerTurn;

    /// <summary>
    /// The sine of a step of the first half turn as a share of <paramref name="peak"/>, what is finer than one of the
    /// peak's parts dropped: for a game whose own table of sines is counted another way than these.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is not one of the half turn's, or the peak is below nothing.</exception>
    public static int HalfWave(int step, int peak)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(peak);
        if ((uint)step >= PerTurn / 2)
            throw new ArgumentOutOfRangeException(nameof(step), step, "A half turn has steps 0 to 127.");
        return (int)(peak * UnitHalfWave(step));
    }

    /// <summary>The fine sine of an angle in 65536ths, signed.</summary>
    private static long FineSigned(int angle)
    {
        int wrapped = Wrap(angle);
        int half = PerTurn / 2;
        return wrapped >= half ? -FineHalfWave[wrapped - half] : FineHalfWave[wrapped];
    }

    /// <summary>
    /// The sine of a step of the first half turn, worked out in exact decimal arithmetic so that every machine fills
    /// the tables alike.
    /// </summary>
    private static decimal UnitHalfWave(int step)
    {
        int phase = Math.Min(step, PerTurn / 2 - step);
        if (phase == 0)
            return 0;
        if (phase == PerTurn / 4)
            return 1;
        decimal x = phase * 3.1415926535897932384626433833m / (PerTurn / 2);
        decimal squared = x * x;
        decimal term = x, sum = x;
        for (int k = 1; k < 12; k++)
        {
            term = -term * squared / ((2 * k) * (2 * k + 1));
            sum += term;
        }

        return sum;
    }

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
