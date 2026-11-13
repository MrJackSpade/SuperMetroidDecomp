namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Turns Phantoon's eye toward Samus's relative octant.</summary>
internal static class PhantoonEyeGaze
{
    /// <summary>
    /// The eye's gaze update selects the eye program from Samus's relative octant. The unused
    /// code5 retains the downward selection; all other values name a direction.
    /// </summary>
    /// <param name="direction">Relative-direction selector, 0..8, with four and the unused five both selecting down.</param>
    /// <returns>The eye's gaze program; the caller owns starting it and resetting its frame timer.</returns>
    /// <exception cref="InvalidDataException">The direction exceeds the nine authored selectors.</exception>
    internal static PhantoonPartSegment EyeAnimation(int direction) =>
        (uint)direction < (uint)PhantoonPatternDefinitions.EyeAnimations.Length
            ? PhantoonPatternDefinitions.EyeAnimations[direction]
            : throw new InvalidDataException($"Phantoon eye direction {direction} exceeds nine authored selectors.");
}
