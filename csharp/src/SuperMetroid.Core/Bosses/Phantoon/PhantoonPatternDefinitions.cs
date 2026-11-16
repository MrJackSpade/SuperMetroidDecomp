namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where Phantoon reappears for his flame rain, the rain's columns and where his eye looks.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>
    /// For each of the eight random picks, how far along the figure eight Phantoon reappears and where his body
    /// stands. Picks zero and four are the same place.
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

    /// <summary>The eye's gaze animation for each direction it looks toward Samus in.</summary>
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
