namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The damage that ends a round of Phantoon's vulnerable eye or swoop, and the eye's gaze animations.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>
    /// The damage that ends a round of his vulnerable eye or swoop: a round's damage reaching it closes his eye, and a
    /// single Super Missile doing as much sends him into his rage.
    /// </summary>
    internal const int RoundDamageThreshold = 300;

    /// <summary>The eye's gaze animation for each direction it looks toward Samus in.</summary>
    internal static readonly IdentityTable<CompassDirection, PhantoonPartSegment> EyeGazeAnimations = new(new Dictionary<CompassDirection, PhantoonPartSegment>
        {
            [CompassDirection.Up] = PhantoonPartSegment.EyeLookingUp,
            [CompassDirection.UpRight] = PhantoonPartSegment.EyeLookingUpRight,
            [CompassDirection.Right] = PhantoonPartSegment.EyeLookingRight,
            [CompassDirection.DownRight] = PhantoonPartSegment.EyeLookingDownRight,
            [CompassDirection.Down] = PhantoonPartSegment.EyeLookingDown,
            [CompassDirection.DownLeft] = PhantoonPartSegment.EyeLookingDownLeft,
            [CompassDirection.Left] = PhantoonPartSegment.EyeLookingLeft,
            [CompassDirection.UpLeft] = PhantoonPartSegment.EyeLookingUpLeft,
        });
}
