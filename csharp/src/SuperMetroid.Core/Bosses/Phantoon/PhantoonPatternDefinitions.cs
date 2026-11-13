namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Fixed Phantoon rain placements and shot-response markers.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>
    /// The figure-eight cursor and world X/Y of each of the eight
    /// random buckets. Patterns zero and four share the same body
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

    /// <summary>The eye's gaze program for each direction it looks toward Samus in.</summary>
    internal static readonly IReadOnlyDictionary<CompassDirection, PhantoonPartSegment> EyeGazeAnimations =
        new Dictionary<CompassDirection, PhantoonPartSegment>
        {
            [CompassDirection.Up] = PhantoonPartSegment.EyeLookingUp,
            [CompassDirection.UpRight] = PhantoonPartSegment.EyeLookingUpRight,
            [CompassDirection.Right] = PhantoonPartSegment.EyeLookingRight,
            [CompassDirection.DownRight] = PhantoonPartSegment.EyeLookingDownRight,
            [CompassDirection.Down] = PhantoonPartSegment.EyeLookingDown,
            [CompassDirection.DownLeft] = PhantoonPartSegment.EyeLookingDownLeft,
            [CompassDirection.Left] = PhantoonPartSegment.EyeLookingLeft,
            [CompassDirection.UpLeft] = PhantoonPartSegment.EyeLookingUpLeft,
        };
}
