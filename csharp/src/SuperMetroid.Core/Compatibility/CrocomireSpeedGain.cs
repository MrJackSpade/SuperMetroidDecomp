namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Crocomire's sinking and rising body and the pieces of his falling spike wall gather and lose speed (#2299, #2462).
/// Each update the speed changes by its acceleration counted in whole 256ths of a pixel an update; what the acceleration
/// holds finer than that builds up in the acceleration alone until it makes up another 256th. A speed gained past the
/// fastest it may go keeps the fastest's whole pixels and its own fraction, so once at its fastest it climbs through that
/// pixel and starts over rather than holding still.
/// </summary>
/// <remarks>
/// Consulted each update his body sinks, before it moves, each update it rises, before it moves, and each update a
/// spike-wall piece flies, before it moves sideways. His sinking acceleration grows by three and a half 256ths of a pixel
/// an update each update, so its half carries into the speed every other update, and his sinking speed can reach its
/// fastest, three pixels an update; each spike-wall piece's acceleration grows by a step of its own, not a whole number of
/// 256ths, and its speed reaches its piece's fastest. His rising speed loses its acceleration's whole 256ths the same way:
/// a rise's acceleration grows by whole 256ths, and a part finer than a 256th it carries (the cartridge's body keeps one
/// from its sunk wait through a melt) never reaches the speed. Evidence: the cartridge keeps these speeds in 256ths of a
/// pixel an update and their accelerations in 65536ths of a pixel an update an update. The sink
/// (<c>$A4:9224</c>-<c>$A4:924D</c>) adds the acceleration's upper byte to the speed's fraction byte with the carry into
/// its whole byte, and stores the fastest's whole byte over a whole byte past it, keeping the fraction byte; the rise
/// (<c>$A4:9306</c>-<c>$A4:9320</c>) subtracts the acceleration's upper byte from the speed's fraction byte with the
/// borrow from its whole byte. The
/// Crocomire state-quantity check runs a resting and a clamped sink, and a resting, a stalled and a finer-carrying rise,
/// beside that byte arithmetic every update.
/// </remarks>
internal static partial class CrocomireSpeedGain
{
    /// <summary>
    /// The speed after one update's gain on <paramref name="speed"/>: its <paramref name="acceleration"/>'s whole 256ths of a
    /// pixel an update added, its whole pixels at most <paramref name="fastest"/>'s with its own fraction kept.
    /// </summary>
    /// <param name="speed">The speed before the update's gain, in pixels an update.</param>
    /// <param name="acceleration">The acceleration, in pixels an update an update.</param>
    /// <param name="fastest">The fastest the speed may go, whose whole pixels the speed's never pass.</param>
    internal static SignedEightEight Gained(SignedEightEight speed, SignedSixteenSixteen acceleration, SignedEightEight fastest) =>
        (speed + WholeTwoFiftySixths(acceleration)).WithWholePixelsAtMost(fastest.WholePixels);

    /// <summary>
    /// The speed after one update's loss on <paramref name="speed"/>: its <paramref name="acceleration"/>'s whole 256ths of a
    /// pixel an update taken away. A loss past nothing leaves the speed below zero for the caller to stop.
    /// </summary>
    /// <param name="speed">The speed before the update's loss, in pixels an update.</param>
    /// <param name="acceleration">The deceleration, in pixels an update an update.</param>
    internal static SignedEightEight Lost(SignedEightEight speed, SignedSixteenSixteen acceleration) =>
        speed - WholeTwoFiftySixths(acceleration);

    /// <summary>The part of <paramref name="acceleration"/> a speed takes each update: its whole 256ths of a pixel an update an update.</summary>
    private static SignedEightEight WholeTwoFiftySixths(SignedSixteenSixteen acceleration) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration.InTwoFiftySixths);
}
