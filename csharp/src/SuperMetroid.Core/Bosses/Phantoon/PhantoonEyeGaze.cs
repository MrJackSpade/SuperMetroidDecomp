namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Turns Phantoon's eye toward Samus.</summary>
internal static class PhantoonEyeGaze
{
    /// <summary>The eye's gaze program looking in <paramref name="direction"/>.</summary>
    /// <param name="direction">The direction from Phantoon in which Samus lies.</param>
    /// <returns>The eye's gaze program; the caller owns starting it and resetting its frame timer.</returns>
    /// <exception cref="InvalidDataException">The direction is not one of the eight the eye looks in.</exception>
    internal static PhantoonPartProgram EyeProgram(CompassDirection direction) =>
        PhantoonPatternDefinitions.EyeGazePrograms.TryGetValue(direction, out PhantoonPartProgram program)
            ? program
            : throw new InvalidDataException($"Phantoon's eye has no gaze toward {direction}.");
}
