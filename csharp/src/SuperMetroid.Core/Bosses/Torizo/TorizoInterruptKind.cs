namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which attack or stagger a <see cref="TorizoCue.Interrupt"/> cue is (#2028): one that interrupts the walk when its test
/// passes, keeping the walk's place, and whose animation resumes the walk there when it ends.
/// </summary>
internal enum TorizoInterruptKind
{
    /// <summary>Bomb Torizo swipes behind it at Samus close behind it.</summary>
    BombSwipeAtSamusBehind,
    /// <summary>Golden Torizo staggers while it is stunned and its health is high.</summary>
    GoldenStaggerWhenStunned,
    /// <summary>Golden Torizo fires its eye beam, at random, at Samus at medium range in front of it.</summary>
    GoldenEyeBeamAtMediumRange,
    /// <summary>Golden Torizo releases its eggs, at random, while its health is low.</summary>
    GoldenEggsAtLowHealth,
}
