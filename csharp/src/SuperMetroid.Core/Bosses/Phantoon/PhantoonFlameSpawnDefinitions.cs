namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored Phantoon flame starts and rain columns.</summary>
public static class PhantoonFlameSpawnDefinitions
{
    /// <summary>
    /// The seven flames of an even rage wave in the order they spawn: down the right of the body from just short of
    /// straight down to just past straight up, each circling clockwise.
    /// </summary>
    internal static readonly PhantoonRageFlameStart[] EvenRageWave = RageWave(new(112), -RageWaveSpacing, 7, RotationDirection.Clockwise);

    /// <summary>
    /// The eight flames of an odd rage wave in the order they spawn: from straight down up the left of the body to just
    /// short of straight up, each circling counterclockwise.
    /// </summary>
    internal static readonly PhantoonRageFlameStart[] OddRageWave = RageWave(TrigonometryAngle.HalfTurn, RageWaveSpacing, 8, RotationDirection.Counterclockwise);

    /// <summary>The rain's <see cref="RainColumns"/> screen-X columns from <see cref="FirstRainX"/>, <see cref="RainSpacing"/> pixels apart.</summary>
    internal static IReadOnlyList<int> RainXs { get; } = [.. Enumerable.Range(0, RainColumns).Select(column => FirstRainX + RainSpacing * column)];

    /// <summary>
    /// The spiral's eight flames' start angles in the order they spawn, an eighth of a turn apart: from up and to the
    /// left anticlockwise round to straight up.
    /// </summary>
    internal static readonly TrigonometryAngle[] SpiralStarts = EighthsApart(new(224), -1);

    /// <summary>
    /// The intro's eight starting flames' start angles in the order they appear, an eighth of a turn apart: from straight
    /// up clockwise round the body.
    /// </summary>
    internal static IReadOnlyList<TrigonometryAngle> StartingFlameStarts { get; } = EighthsApart(TrigonometryAngle.Zero, 1);

    /// <summary>How many columns the rain falls in.</summary>
    private const int RainColumns = 9;

    /// <summary>The screen X of the rain's leftmost column.</summary>
    private const int FirstRainX = 48;

    /// <summary>How far apart the rain's columns lie, in pixels.</summary>
    private const int RainSpacing = 20;

    /// <summary>How far apart a rage wave's flames start: a sixteenth of a turn.</summary>
    private const int RageWaveSpacing = 16;

    /// <summary>An eighth of a turn, how far apart the spiral's and the intro's flames start.</summary>
    private const int EighthTurn = 32;

    /// <summary>
    /// A rage wave of <paramref name="count"/> flames in the order they spawn, the first at <paramref name="first"/> and each
    /// next <paramref name="spacing"/> steps on, every one circling <paramref name="turn"/>.
    /// </summary>
    private static PhantoonRageFlameStart[] RageWave(TrigonometryAngle first, int spacing, int count, RotationDirection turn) =>
        [.. Enumerable.Range(0, count).Select(flame => new PhantoonRageFlameStart(first.TurnedBySteps(spacing * flame), turn))];

    /// <summary>Eight start angles an eighth of a turn apart from <paramref name="first"/>, each next one <paramref name="sense"/> eighths on.</summary>
    private static TrigonometryAngle[] EighthsApart(TrigonometryAngle first, int sense) =>
        [.. Enumerable.Range(0, 8).Select(flame => first.TurnedBySteps(sense * EighthTurn * flame))];
}
