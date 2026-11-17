namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Crocomire's sinking body and the pieces of his falling spike wall gather speed (#2299). Each update the speed gains
/// its acceleration counted in whole 256ths of a pixel an update; what the acceleration holds finer than that builds up in
/// the acceleration alone until it makes up another 256th. A speed whose whole pixels pass the fastest it may go keeps the
/// fastest's whole pixels and its own fraction, so once at its fastest it climbs through that pixel and starts over rather
/// than holding still.
/// </summary>
/// <remarks>
/// Consulted each update his body sinks, before it moves, and each update a spike-wall piece flies, before it moves
/// sideways. His sinking acceleration grows by three and a half 256ths of a pixel an update each update, so its half
/// carries into the speed every other update, and his sinking speed can reach its fastest, three pixels an update; each
/// spike-wall piece's acceleration grows by a step of its own, not a whole number of 256ths, and its speed reaches its
/// piece's fastest. Evidence: the cartridge keeps both speeds in 256ths of a pixel an update and their accelerations in
/// 65536ths of a pixel an update an update, adds the acceleration's upper byte to the speed with the carry into its whole
/// byte, and stores the fastest's whole byte over a whole byte past it, keeping the low byte. The Crocomire state-quantity
/// check runs a resting and a clamped sink beside that byte arithmetic every update.
/// </remarks>
[CompatibilityService("Crocomire's sinking body and spike-wall pieces gain their acceleration's whole 256ths of a pixel each update, and a speed past its fastest keeps the fastest's whole pixels with its own fraction")]
internal static class CrocomireSpeedGain
{
    /// <summary>
    /// The speed after one update's gain on <paramref name="speed"/>: its <paramref name="acceleration"/>'s whole 256ths of a
    /// pixel an update added, its whole pixels at most <paramref name="fastest"/>'s with its own fraction kept.
    /// </summary>
    /// <param name="speed">The speed before the update's gain, in pixels an update.</param>
    /// <param name="acceleration">The acceleration, in pixels an update an update.</param>
    /// <param name="fastest">The fastest the speed may go, whose whole pixels the speed's never pass.</param>
    internal static SignedEightEight Gained(SignedEightEight speed, SignedSixteenSixteen acceleration, SignedEightEight fastest) =>
        (speed + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration.InTwoFiftySixths)).WithWholePixelsAtMost(fastest.WholePixels);
}
