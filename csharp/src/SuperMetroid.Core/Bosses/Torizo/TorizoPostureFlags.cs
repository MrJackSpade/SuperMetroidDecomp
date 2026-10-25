namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Bomb or Golden Torizo's posture. The stepping commands replace all three latches and the turning command sets its
/// latch alone, so they compose. The cartridge keeps them in the top three bits of enemy parameter 1; where, is
/// <see cref="TorizoParameterFlagCodec"/>'s knowledge, and the values here are arbitrary bits (#1413).
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
    /// <summary>Every posture latch.</summary>
    All = OppositeLegForward | TurningAround | FacingRight,
}
