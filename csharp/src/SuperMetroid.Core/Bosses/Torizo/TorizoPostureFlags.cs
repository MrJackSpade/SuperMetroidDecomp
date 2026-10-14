namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The posture bits of Bomb and Golden Torizo's enemy parameter 1, above its low thirteen bits. The stepping commands
/// replace all three, and the turning command sets its bit alone, so they compose.
/// </summary>
[Flags]
internal enum TorizoPostureFlags : ushort
{
    /// <summary>Facing left, the near leg forward.</summary>
    None = 0,
    /// <summary>
    /// Bit 13: the leg on the side away from the facing direction is forward: the right leg facing left, the left leg
    /// facing right.
    /// </summary>
    OppositeLegForward = 0x2000,
    /// <summary>Bit 14: turning around; the drool then sprays in every direction.</summary>
    TurningAround = 0x4000,
    /// <summary>Bit 15: facing right.</summary>
    FacingRight = 0x8000,
    /// <summary>Every posture bit.</summary>
    All = OppositeLegForward | TurningAround | FacingRight,
}
