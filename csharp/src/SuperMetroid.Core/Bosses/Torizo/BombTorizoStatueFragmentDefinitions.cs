namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed projectile definitions for Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>The fragments the breaking statue hand emits.</summary>
    internal const int FragmentCount = 16;

    /// <summary>Every fragment's initial downward velocity.</summary>
    internal const int YVelocity = 256;

    /// <summary>Every fragment's downward acceleration.</summary>
    internal static readonly SignedEightEight Acceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);
}
