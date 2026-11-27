namespace Engine.Shared;

/// <summary>
/// Angles and the sine of an angle, in whole numbers only, so that anything that turns does so the same way on every
/// machine. An angle is counted in 256ths of a full turn.
/// </summary>
public static class Turns
{
    /// <summary>How many angle steps make a full turn.</summary>
    public const int PerTurn = 256;

    /// <summary>
    /// The sine of each step of the first half turn, in 256ths, cut off toward nothing, and short of whole except at
    /// the quarter turn itself: the sine of a game that keeps its sines in a small table.
    /// </summary>
    private static readonly int[] CoarseHalfWave =
        [.. Enumerable.Range(0, PerTurn / 2).Select(step => step == PerTurn / 4 ? 256 : Math.Min(255, (int)(256m * UnitHalfWave(step))))];

    /// <summary>The sine of <paramref name="angle"/> in 256ths, as a coarse sine takes it: -256 through 256.</summary>
    public static int CoarseSine(int angle)
    {
        int wrapped = Wrap(angle);
        int half = PerTurn / 2;
        return wrapped >= half ? -CoarseHalfWave[wrapped - half] : CoarseHalfWave[wrapped];
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
}
