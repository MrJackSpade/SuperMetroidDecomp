namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The fall of Kraid's ceiling rocks and Crocomire's bridge fragments: gravity raises the fall speed each update, and a
/// speed that reaches <see cref="RestartSpeed"/>, 64 pixels an update, starts again from rest, keeping only what lies
/// past it. Evidence: both falls keep only the speed's lower fourteen bits after adding their gravity. A rock or fragment
/// starts falling at under half a pixel an update and is removed when its terrain probe finds a block, at the latest as it
/// leaves its room, so in the retail rooms the restart is never reached; it is kept because the fall states it.
/// </summary>
internal static class RestartingFallSpeed
{
    /// <summary>The fall speed, 64 pixels an update in 256ths of a pixel, at which a fall starts again from rest.</summary>
    internal const int RestartSpeed = 64 * 256;

    /// <summary>The fall speed after one update's <paramref name="gravity"/>, starting again from rest at <see cref="RestartSpeed"/>.</summary>
    /// <param name="speed">The downward fall speed, zero or more.</param>
    /// <param name="gravity">The downward acceleration, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The speed or the gravity points upward.</exception>
    internal static SignedEightEight Accelerate(SignedEightEight speed, SignedEightEight gravity) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            (Ensure.AtLeastZero(speed.InTwoHundredFiftySixthsOfPixel) + Ensure.AtLeastZero(gravity.InTwoHundredFiftySixthsOfPixel)) % RestartSpeed);
}
