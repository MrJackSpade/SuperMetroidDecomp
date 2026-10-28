namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed projectile definitions for Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>The fragments the breaking statue hand emits.</summary>
    internal const int FragmentCount = 16;

    /// <summary>$86:A7FB: every fragment's initial downward velocity.</summary>
    internal const int YVelocity = 0x0100;

    /// <summary>$86:A80B: every fragment's downward acceleration.</summary>
    internal const int Acceleration = 0x0010;
}
