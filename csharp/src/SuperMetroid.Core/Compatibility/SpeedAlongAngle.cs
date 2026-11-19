namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a speed scales along an angle (#1422, #2572): a forward speed scales by the angle's sine, rounding toward zero;
/// a backward speed is taken as a forward speed 256 pixels an update faster than its own, scaled by the sine and then
/// wrapped back into a speed, so wherever the sine falls short of its peak a backward speed comes out a whole number of
/// pixels on from the product its size would give: Mother Brain's rainbow beam pushing Samus, Ridley's flight, Torizo's
/// leaps, the Baby Metroid's curved entrance and every enemy speed resolved along a heading move so.
/// </summary>
/// <remarks>
/// Consulted wherever a speed is resolved into its parts along an angle, in the update of the actor that moves by it.
/// Evidence: the original's sine and cosine multiplications take the speed as an unsigned sixteen-bit number, multiply it
/// by the sine's size, drop the 256ths and restore the sine's sign; the Baby Metroid's wrong-way entrance and the
/// Mother Brain and Ridley recorded movies replay those products.
/// </remarks>
[CompatibilityService("a backward speed resolved along an angle is taken as a forward speed 256 pixels an update faster, scaled by the sine and wrapped back into a speed")]
public static class SpeedAlongAngle
{
    /// <summary>The steps of a speed's 256ths across its whole range, which a backward speed is taken from.</summary>
    private const int SpeedSteps = 1 << 16;

    /// <summary>
    /// <paramref name="speed"/> scaled by <paramref name="angle"/>'s sine; a cosine is this at the angle a quarter-turn on.
    /// </summary>
    /// <param name="speed">The speed, forward or backward.</param>
    /// <param name="angle">The angle whose sine scales it.</param>
    public static SignedEightEight Along(SignedEightEight speed, TrigonometryAngle angle)
    {
        int steps = speed.InTwoHundredFiftySixthsOfPixel;
        int taken = steps < 0 ? steps + SpeedSteps : steps;
        return SignedEightEight.Wrapped(UnitComponent.SineOf(angle).ReachOf(taken));
    }
}
