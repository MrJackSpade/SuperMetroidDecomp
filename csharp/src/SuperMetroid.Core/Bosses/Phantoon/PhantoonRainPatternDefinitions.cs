namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The patterns of Phantoon's flame rain, by where his body reappears (#2258).</summary>
public static class PhantoonRainPatternDefinitions
{
    /// <summary>He reappears at the room's centre.</summary>
    public static PhantoonRainPattern Centre { get; } = new(1, new(128, 96), PhantoonFlameSpawns.RainColumn(5));

    /// <summary>He reappears high, right of centre.</summary>
    public static PhantoonRainPattern UpperRight { get; } = new(71, new(168, 64), PhantoonFlameSpawns.RainColumn(7));

    /// <summary>He reappears at the room's right.</summary>
    public static PhantoonRainPattern Right { get; } = new(PhantoonPathDefinitions.RightAppearance, PhantoonPathDefinitions.RightAppearancePosition, PhantoonFlameSpawns.RainColumn(0));

    /// <summary>He reappears low, right of centre.</summary>
    public static PhantoonRainPattern LowerRight { get; } = new(201, new(168, 128), PhantoonFlameSpawns.RainColumn(7));

    /// <summary>He reappears high, left of centre.</summary>
    public static PhantoonRainPattern UpperLeft { get; } = new(334, new(88, 64), PhantoonFlameSpawns.RainColumn(3));

    /// <summary>He reappears at the room's left.</summary>
    public static PhantoonRainPattern Left { get; } = new(PhantoonPathDefinitions.LeftAppearance, PhantoonPathDefinitions.LeftAppearancePosition, PhantoonFlameSpawns.RainColumn(1));

    /// <summary>He reappears low, left of centre.</summary>
    public static PhantoonRainPattern LowerLeft { get; } = new(465, new(88, 128), PhantoonFlameSpawns.RainColumn(3));

    /// <summary>
    /// The patterns a later rain chooses from, by its random draw: each is as likely as the others but the centre, which
    /// the draw chooses twice as often.
    /// </summary>
    public static IReadOnlyList<PhantoonRainPattern> Choices { get; } =
        [Centre, UpperRight, Right, LowerRight, Centre, UpperLeft, Left, LowerLeft];

    /// <summary>The first rain's pattern while his body is in the room's left half.</summary>
    public static PhantoonRainPattern FirstRainFromLeftHalf => Centre;

    /// <summary>The first rain's pattern while his body is in the room's right half.</summary>
    public static PhantoonRainPattern FirstRainFromRightHalf => Right;
}
