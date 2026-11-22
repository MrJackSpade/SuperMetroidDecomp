namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's flames start: the rage waves' and the spiral's circles, the intro's flames and the rain columns.</summary>
public static partial class PhantoonFlameSpawns
{
    /// <summary>The flames of an even rage wave, in the order they spawn.</summary>
    internal static IReadOnlyList<PhantoonRageFlameStart> EvenRageWave => PhantoonFlameSpawnDefinitions.EvenRageWave;

    /// <summary>The flames of an odd rage wave, in the order they spawn.</summary>
    internal static IReadOnlyList<PhantoonRageFlameStart> OddRageWave => PhantoonFlameSpawnDefinitions.OddRageWave;

    /// <summary>The spiral flames' start angles, in the order they spawn.</summary>
    internal static IReadOnlyList<TrigonometryAngle> SpiralStarts => PhantoonFlameSpawnDefinitions.SpiralStarts;

    /// <summary>The intro's starting flames' start angles, in the order they appear.</summary>
    internal static IReadOnlyList<TrigonometryAngle> StartingFlameStarts => PhantoonFlameSpawnDefinitions.StartingFlameStarts;

    /// <summary>The rain columns' screen Xs, left to right.</summary>
    internal static IReadOnlyList<int> RainXs => PhantoonFlameSpawnDefinitions.RainXs;

    /// <summary>The rain column at <paramref name="place"/> of <see cref="RainXs"/>, counting from the left from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The rain has no such column.</exception>
    internal static ListStep<int> RainColumn(int place) => ListStep<int>.At(RainXs, place);

    /// <summary>
    /// The rain column after the one at <paramref name="bodyX"/>, the first after the last (#2556): where a rain whose
    /// gap is his body's column starts.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No rain column lies at <paramref name="bodyX"/>.</exception>
    internal static ListStep<int> RainColumnAfter(int bodyX)
    {
        for (int column = 0; column < RainXs.Count; column++)
        {
            if (RainXs[column] == bodyX)
                return RainColumn(column).NextInCycle;
        }
        throw new ArgumentOutOfRangeException(nameof(bodyX), bodyX, "No rain column lies at his body's X.");
    }

    /// <summary>
    /// The turn a rage flame starting at <paramref name="start"/> makes each update: the rage turn clockwise, or the same
    /// turn back, a full circle less it, counterclockwise. The flame's angle grows clockwise round the body.
    /// </summary>
    internal static TrigonometryAngle RageTurnPerUpdate(PhantoonRageFlameStart start) => start.Turn == RotationDirection.Clockwise
        ? PhantoonFlameMotionDefinitions.RageTurnPerUpdate
        : TrigonometryAngle.Zero - PhantoonFlameMotionDefinitions.RageTurnPerUpdate;
}
