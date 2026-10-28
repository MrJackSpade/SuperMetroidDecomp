namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Turns Phantoon's eye toward Samus's relative octant.</summary>
internal static class PhantoonEyeGaze
{
    /// <summary>
    /// $A7:D40D-$D41E selects the eye program from Samus's relative octant. The unused
    /// code5 retains the native downward selection; all other values name a direction.
    /// </summary>
    /// <param name="direction">Native relative-direction selector, 0..8, with four and the unused five both selecting down.</param>
    /// <returns>The eye's gaze program; the caller owns starting it and resetting its instruction timer.</returns>
    /// <exception cref="InvalidDataException">The direction exceeds the nine authored selectors.</exception>
    internal static PhantoonPartProgram EyeProgram(int direction) =>
        (uint)direction < (uint)PhantoonPatternDefinitions.EyePrograms.Length
            ? PhantoonPatternDefinitions.EyePrograms[direction]
            : throw new InvalidDataException($"Phantoon eye direction {direction} exceeds nine authored selectors.");
}
