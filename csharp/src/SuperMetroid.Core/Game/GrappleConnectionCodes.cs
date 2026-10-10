namespace SuperMetroid.Core.Game;

/// <summary>Bank-$9B grapple functions a connection record installs after the pose handler runs.</summary>
public enum GrappleConnectionFunction : ushort
{
    /// <summary>Bank-$9B locked-in-place connection handler at $9B:C77E.</summary>
    LockedInPlace = 0xc77e,
    /// <summary>Bank-$9B ordinary swinging connection handler at $9B:C79D.</summary>
    Swinging = 0xc79d,
    /// <summary>Bank-$9B wall-grab connection handler at $9B:C814.</summary>
    WallGrab = 0xc814,
}

/// <summary>Bank-$9B pose handlers named by the directional grapple connection records.</summary>
public enum GrappleConnectionHandler : ushort
{
    /// <summary>Bank-$9B clockwise swing-pose connection handler at $9B:B9D9.</summary>
    SwingClockwise = 0xb9d9,
    /// <summary>Bank-$9B anticlockwise swing-pose connection handler at $9B:B9E2.</summary>
    SwingAnticlockwise = 0xb9e2,
    /// <summary>Bank-$9B standing up-right connection handler at $9B:B9EA.</summary>
    StandingUpRight = 0xb9ea,
    /// <summary>Bank-$9B standing right connection handler at $9B:B9F3.</summary>
    StandingRight = 0xb9f3,
    /// <summary>Bank-$9B standing down connection handler at $9B:B9FC.</summary>
    StandingDown = 0xb9fc,
    /// <summary>Bank-$9B standing up-left connection handler at $9B:BA05.</summary>
    StandingUpLeft = 0xba05,
    /// <summary>Bank-$9B crouching up-right connection handler at $9B:BA0E.</summary>
    CrouchingUpRight = 0xba0e,
    /// <summary>Bank-$9B crouching right connection handler at $9B:BA17.</summary>
    CrouchingRight = 0xba17,
    /// <summary>Bank-$9B crouching down-left connection handler at $9B:BA20.</summary>
    CrouchingDownLeft = 0xba20,
    /// <summary>Bank-$9B crouching up-left connection handler at $9B:BA29.</summary>
    CrouchingUpLeft = 0xba29,
}
