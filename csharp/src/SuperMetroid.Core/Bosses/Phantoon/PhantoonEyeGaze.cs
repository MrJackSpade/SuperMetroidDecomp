namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Turns Phantoon's eye toward Samus.</summary>
internal static class PhantoonEyeGaze
{
    /// <summary>The eye's gaze animation looking in <paramref name="direction"/>.</summary>
    /// <param name="direction">The direction from Phantoon in which Samus lies.</param>
    /// <returns>The eye's gaze animation; the caller owns starting it and resetting its frame timer.</returns>
    /// <exception cref="InvalidDataException">The direction is not one of the eight the eye looks in.</exception>
    internal static PhantoonPartSegment EyeAnimation(CompassDirection direction) =>
        PhantoonPatternDefinitions.EyeGazeAnimations.TryGetValue(direction, out PhantoonPartSegment segment)
            ? segment
            : throw new InvalidDataException($"Phantoon's eye has no gaze toward {direction}.");
}
