namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed projectile definitions for Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>
    /// The fragments the breaking statue hand emits (#1941): the hand's crumble drops eight, one before each of its
    /// shortening waits (<see cref="BombTorizoHandCrumbleDefinitions.Crumble"/>).
    /// </summary>
    internal const int FragmentCount = 8;

    /// <summary>Every fragment's initial fall speed, one pixel an update.</summary>
    internal static readonly SignedEightEight InitialFallSpeed = SignedEightEight.FromPixels(1);

    /// <summary>Every fragment's downward acceleration.</summary>
    internal static readonly SignedEightEight Acceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);
}
