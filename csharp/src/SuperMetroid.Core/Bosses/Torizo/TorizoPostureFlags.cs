namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Bomb or Golden Torizo's posture. The stepping cues replace all three latches and the turning cue sets its
/// latch alone, so they compose. The values here are arbitrary flags (#1413); where the game stores them is the
/// checks' knowledge.
/// </summary>
[Flags]
internal enum TorizoPostureFlags
{
    /// <summary>Facing left, the near leg forward.</summary>
    None = 0,
    /// <summary>
    /// The leg on the side away from the facing direction is forward: the right leg facing left, the left leg facing
    /// right.
    /// </summary>
    OppositeLegForward = 1,
    /// <summary>Turning around; the drool then sprays in every direction.</summary>
    TurningAround = 2,
    /// <summary>Facing right.</summary>
    FacingRight = 4,
}
