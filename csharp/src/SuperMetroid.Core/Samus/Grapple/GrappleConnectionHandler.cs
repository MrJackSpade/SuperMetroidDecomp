namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Bank-$9B pose handlers named by the directional grapple connection records.</summary>
public enum GrappleConnectionHandler : ushort
{
    /// <summary>Bank-$9B clockwise swing-pose connection handler at $9B:B9D9.</summary>
    SwingClockwise,
    /// <summary>Bank-$9B anticlockwise swing-pose connection handler at $9B:B9E2.</summary>
    SwingAnticlockwise,
    /// <summary>Bank-$9B standing up-right connection handler at $9B:B9EA.</summary>
    StandingUpRight,
    /// <summary>Bank-$9B standing right connection handler at $9B:B9F3.</summary>
    StandingRight,
    /// <summary>Bank-$9B standing down connection handler at $9B:B9FC.</summary>
    StandingDown,
    /// <summary>Bank-$9B standing up-left connection handler at $9B:BA05.</summary>
    StandingUpLeft,
    /// <summary>Bank-$9B crouching up-right connection handler at $9B:BA0E.</summary>
    CrouchingUpRight,
    /// <summary>Bank-$9B crouching right connection handler at $9B:BA17.</summary>
    CrouchingRight,
    /// <summary>Bank-$9B crouching down-left connection handler at $9B:BA20.</summary>
    CrouchingDownLeft,
    /// <summary>Bank-$9B crouching up-left connection handler at $9B:BA29.</summary>
    CrouchingUpLeft,
}
