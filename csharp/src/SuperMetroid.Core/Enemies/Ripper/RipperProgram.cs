namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="RipperAnimations"/>.
/// </summary>
internal enum RipperProgram
{
    /// <summary>The g ripper moving left animation.</summary>
    GRipperMovingLeft,
    /// <summary>The g ripper moving right animation.</summary>
    GRipperMovingRight,
    /// <summary>The ripper 2 moving right animation.</summary>
    Ripper2MovingRight,
    /// <summary>The ripper 2 moving left animation.</summary>
    Ripper2MovingLeft,
    /// <summary>The ripper moving right animation.</summary>
    RipperMovingRight,
    /// <summary>The ripper moving left animation.</summary>
    RipperMovingLeft,
}
