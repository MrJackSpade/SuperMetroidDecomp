namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's flame rain: where the body appears for each random bucket and where the rain begins.</summary>
public static class PhantoonFlameRain
{
    /// <summary>Figure-eight cursor and world X/Y.</summary>
    /// <param name="pattern">Random-bucket index, 0..7; patterns zero and four share the same body placement.</param>
    /// <returns>The body's figure-eight path cursor and whole-pixel room coordinates; does not draw RNG or move the boss.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pattern is outside 0..7.</exception>
    public static PhantoonRainPlacementDefinition RainPlacement(int pattern) =>
        (uint)pattern < (uint)PhantoonPatternDefinitions.RainPlacements.Length
            ? PhantoonPatternDefinitions.RainPlacements[pattern]
            : throw new ArgumentOutOfRangeException(nameof(pattern));

    /// <summary>
    /// The flame rain starts the eight-flame rain one column after the body. Wrapping nine
    /// twenty-pixel columns leaves the gap precisely at the body's rain-placement X.
    /// </summary>
    public static PhantoonRainColumns FirstRainColumns => default;
}
