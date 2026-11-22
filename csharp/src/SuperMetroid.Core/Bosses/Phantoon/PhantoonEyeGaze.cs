namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Turns Phantoon's eye toward Samus.</summary>
internal static partial class PhantoonEyeGaze
{
    /// <summary>The eye's gaze animation looking in <paramref name="direction"/>.</summary>
    /// <param name="direction">The direction from Phantoon in which Samus lies.</param>
    /// <returns>The eye's gaze animation; Phantoon's eye starts it and resets its frame timer.</returns>
    internal static PhantoonPartSegment EyeAnimation(CompassDirection direction) =>
        PhantoonPatternDefinitions.EyeGazeAnimations[direction];
}
