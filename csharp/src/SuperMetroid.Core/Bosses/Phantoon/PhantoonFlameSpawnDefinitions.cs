namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored Phantoon flame start angles and rain columns.</summary>
public static class PhantoonFlameSpawnDefinitions
{
    /// <summary>The enraged flames' start angles: eight clockwise angles followed by eight counterclockwise angles.</summary>
    internal static readonly TrigonometryAngle[] RageAngles =
    [
        new(16), new(32), new(48), new(64), new(80), new(96), new(112), new(128),
        new(240), new(224), new(208), new(192), new(176), new(160), new(144), new(128),
    ];

    /// <summary>The rain's nine screen-X columns, 48 through 208, spaced twenty pixels apart.</summary>
    internal static readonly int[] RainXs = [48, 68, 88, 108, 128, 148, 168, 188, 208];

    /// <summary>The spiral flames' eight equally spaced start angles.</summary>
    internal static readonly TrigonometryAngle[] SpiralAngles = [new(0), new(32), new(64), new(96), new(128), new(160), new(192), new(224)];
}
