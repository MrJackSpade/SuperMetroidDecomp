namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored Phantoon flame starts and rain columns.</summary>
public static class PhantoonFlameSpawnDefinitions
{
    /// <summary>
    /// The seven flames of an even rage wave in the order they spawn: down the right of the body from just short of
    /// straight down to just past straight up, each circling clockwise.
    /// </summary>
    internal static readonly PhantoonRageFlameStart[] EvenRageWave =
    [
        new(new(112), RotationDirection.Clockwise), new(new(96), RotationDirection.Clockwise),
        new(new(80), RotationDirection.Clockwise), new(new(64), RotationDirection.Clockwise),
        new(new(48), RotationDirection.Clockwise), new(new(32), RotationDirection.Clockwise),
        new(new(16), RotationDirection.Clockwise),
    ];

    /// <summary>
    /// The eight flames of an odd rage wave in the order they spawn: from straight down up the left of the body to just
    /// short of straight up, each circling counterclockwise.
    /// </summary>
    internal static readonly PhantoonRageFlameStart[] OddRageWave =
    [
        new(new(128), RotationDirection.Counterclockwise), new(new(144), RotationDirection.Counterclockwise),
        new(new(160), RotationDirection.Counterclockwise), new(new(176), RotationDirection.Counterclockwise),
        new(new(192), RotationDirection.Counterclockwise), new(new(208), RotationDirection.Counterclockwise),
        new(new(224), RotationDirection.Counterclockwise), new(new(240), RotationDirection.Counterclockwise),
    ];

    /// <summary>The rain's nine screen-X columns, 48 through 208, spaced twenty pixels apart.</summary>
    internal static readonly int[] RainXs = [48, 68, 88, 108, 128, 148, 168, 188, 208];

    /// <summary>
    /// The spiral's eight flames' start angles in the order they spawn, an eighth of a turn apart: from up and to the
    /// left anticlockwise round to straight up.
    /// </summary>
    internal static readonly TrigonometryAngle[] SpiralStarts = [new(224), new(192), new(160), new(128), new(96), new(64), new(32), new(0)];

    /// <summary>
    /// The intro's eight starting flames' start angles in the order they appear, an eighth of a turn apart: from straight
    /// up clockwise round the body.
    /// </summary>
    internal static readonly TrigonometryAngle[] StartingFlameStarts = [new(0), new(32), new(64), new(96), new(128), new(160), new(192), new(224)];
}
