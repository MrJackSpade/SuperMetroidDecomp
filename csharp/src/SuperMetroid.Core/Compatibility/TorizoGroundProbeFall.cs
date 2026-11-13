namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Torizo's air transition drops it each update while it probes for ground (#1556): one pixel more than its
/// sideways velocity's 256ths of a pixel, read as whole pixels, at most fifteen. After a walking step that velocity is the
/// one the step leaves (<see cref="TorizoWalkLeftVelocity"/>), so the probe drops one pixel more than the step moved;
/// after a jump it drops at the fifteen-pixel limit, and after a fall's reset by one pixel.
/// </summary>
/// <remarks>
/// Evidence: the air transition's probe adds one to the magnitude of the Torizo's horizontal velocity word and clamps it
/// at fifteen, then moves the Torizo down by that many whole pixels.
/// </remarks>
[CompatibilityService("a Torizo probing for ground falls one pixel more than its sideways velocity's 256ths of a pixel, read as whole pixels, at most fifteen")]
public static class TorizoGroundProbeFall
{
    /// <summary>The fastest the probe drops the Torizo, in whole pixels an update.</summary>
    public const int FastestPixels = 15;

    /// <summary>The whole pixels the probe drops the Torizo this update.</summary>
    /// <param name="sidewaysVelocity">The Torizo's sideways velocity.</param>
    public static int Pixels(SignedEightEight sidewaysVelocity) =>
        Math.Min(Math.Abs(sidewaysVelocity.InTwoHundredFiftySixthsOfPixel) + 1, FastestPixels);
}
