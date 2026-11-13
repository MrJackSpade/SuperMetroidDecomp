namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon's flames start: the rage waves' and the spiral's circles, the intro's flames and the rain columns.</summary>
public static class PhantoonFlameSpawns
{
    /// <summary>The flames of an even rage wave, in the order they spawn.</summary>
    internal static IReadOnlyList<PhantoonRageFlameStart> EvenRageWave => PhantoonFlameSpawnDefinitions.EvenRageWave;

    /// <summary>The flames of an odd rage wave, in the order they spawn.</summary>
    internal static IReadOnlyList<PhantoonRageFlameStart> OddRageWave => PhantoonFlameSpawnDefinitions.OddRageWave;

    /// <summary>The spiral flames' start angles, in the order they spawn.</summary>
    internal static IReadOnlyList<TrigonometryAngle> SpiralStarts => PhantoonFlameSpawnDefinitions.SpiralStarts;

    /// <summary>The start angle of the intro's starting flame <paramref name="order"/>, zero (the first to appear) through seven.</summary>
    internal static TrigonometryAngle StartingFlameStart(int order) =>
        At(PhantoonFlameSpawnDefinitions.StartingFlameStarts, order, nameof(order));

    /// <summary>The screen X of rain column <paramref name="column"/>, zero through eight.</summary>
    public static int RainX(int column) => At(PhantoonFlameSpawnDefinitions.RainXs, column, nameof(column));

    /// <summary>
    /// The turn a rage flame starting at <paramref name="start"/> makes each update: the rage turn clockwise, or the same
    /// turn back, a full circle less it, counterclockwise. The flame's angle grows clockwise round the body.
    /// </summary>
    internal static TrigonometryAngle RageTurnPerUpdate(PhantoonRageFlameStart start) => start.Turn switch
    {
        RotationDirection.Clockwise => PhantoonFlameMotionDefinitions.RageTurnPerUpdate,
        RotationDirection.Counterclockwise => TrigonometryAngle.Zero - PhantoonFlameMotionDefinitions.RageTurnPerUpdate,
        _ => throw new ArgumentOutOfRangeException(nameof(start), start.Turn, $"Undefined {nameof(RotationDirection)}."),
    };

    /// <summary>Reads one authored value, rejecting a position outside the table.</summary>
    private static T At<T>(T[] table, int position, string name) =>
        (uint)position < (uint)table.Length ? table[position] : throw new ArgumentOutOfRangeException(name);
}
