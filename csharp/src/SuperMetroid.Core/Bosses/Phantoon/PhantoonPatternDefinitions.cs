namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Fixed Phantoon rain placements and shot-response markers.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>
    /// $A7:CDAD, Phantoon_FlameRain_PositionTable: the figure-eight cursor and world X/Y of each of the eight native
    /// random buckets; each native record also has an unused zero word. Patterns zero and four share the same body
    /// placement.
    /// </summary>
    internal static readonly PhantoonRainPlacementDefinition[] RainPlacements =
    [
        new(1, new(128, 96)),
        new(71, new(168, 64)),
        new(136, new(208, 96)),
        new(201, new(168, 128)),
        new(1, new(128, 96)),
        new(334, new(88, 64)),
        new(399, new(48, 96)),
        new(465, new(88, 128)),
    ];

    /// <summary>The rain's columns, which wrap from the last back to the first.</summary>
    internal const int RainColumnCount = 9;

    /// <summary>
    /// $A7:CDA5, Phantoon_Unknown0FEAValues: the shot reaction at $A7:DE5C-DE6A
    /// indexes these eight bytes with RNG &amp; 7 and writes the selected marker to eye variable B.
    /// </summary>
    /// <remarks>
    /// Issue1165 narrow nonsense exception: these are arbitrary random-bucket assignments,
    /// not direction, phase or geometric selectors. Native eye variable B has no reader;
    /// managed Phantoon reads of variable B address the other body/mouth/tentacle slots.
    /// The marker remains exposed state, so replacing this with a different 50/50 selection
    /// changes exact RNG-to-state behavior. Boolean cases would merely recite the same bucket
    /// choices. Retain only this mapping and preserve the existing RNG call/state write.
    /// </remarks>
    public static ReadOnlySpan<byte> ShotEyeMarkers => [6, 6, 8, 8, 6, 8, 6, 8];

    /// <summary>
    /// $A7:D40D-$D41E: the eye program of each relative-octant selector 0..8. The unused code 5 retains the native
    /// downward selection; all other values name a direction.
    /// </summary>
    internal static readonly PhantoonPartProgram[] EyePrograms =
    [
        PhantoonPartProgram.EyeLookingUp,
        PhantoonPartProgram.EyeLookingUpRight,
        PhantoonPartProgram.EyeLookingRight,
        PhantoonPartProgram.EyeLookingDownRight,
        PhantoonPartProgram.EyeLookingDown,
        PhantoonPartProgram.EyeLookingDown,
        PhantoonPartProgram.EyeLookingDownLeft,
        PhantoonPartProgram.EyeLookingLeft,
        PhantoonPartProgram.EyeLookingUpLeft,
    ];
}
