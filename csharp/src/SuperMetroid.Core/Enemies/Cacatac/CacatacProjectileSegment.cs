namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike programs, one per launch direction (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="CacatacProjectileAnimations"/>.
/// </summary>
internal enum CacatacProjectileSegment
{
    /// <summary>The left facing up animation.</summary>
    LeftFacingUp,
    /// <summary>The up left animation.</summary>
    UpLeft,
    /// <summary>The up animation.</summary>
    Up,
    /// <summary>The up right animation.</summary>
    UpRight,
    /// <summary>The right facing up animation.</summary>
    RightFacingUp,
    /// <summary>The left facing down animation.</summary>
    LeftFacingDown,
    /// <summary>The down left animation.</summary>
    DownLeft,
    /// <summary>The down animation.</summary>
    Down,
    /// <summary>The down right animation.</summary>
    DownRight,
    /// <summary>The right facing down animation.</summary>
    RightFacingDown,
}
