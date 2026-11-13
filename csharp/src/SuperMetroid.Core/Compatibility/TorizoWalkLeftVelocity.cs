namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The sideways velocity a Torizo's walking step leaves (#1556): the step's signed whole pixels as 256ths of a pixel an
/// update. The step itself moves the Torizo by its whole pixels; a jump that starts without setting its own sideways
/// velocity carries the Torizo by this one, and the ground probe falls by it (<see cref="TorizoGroundProbeFall"/>).
/// </summary>
/// <remarks>
/// Evidence: the walking instructions store their signed whole-pixel delta in the Torizo's horizontal velocity word,
/// which the jump and the air transition's ground probe read as that velocity.
/// </remarks>
[CompatibilityService("a Torizo's walking step leaves its whole pixels as its sideways velocity in 256ths of a pixel, which a following jump and the ground probe read")]
public static class TorizoWalkLeftVelocity
{
    /// <summary>The sideways velocity a step of <paramref name="stepPixels"/> leaves: its pixels as 256ths of a pixel an update.</summary>
    /// <param name="stepPixels">The step's signed whole pixels.</param>
    public static SignedEightEight Of(int stepPixels) => SignedEightEight.FromTwoHundredFiftySixthsOfPixel(stepPixels);
}
