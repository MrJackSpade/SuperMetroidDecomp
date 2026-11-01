namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Pose handlers named by the directional grapple connection records.</summary>
public enum GrappleConnectionHandler
{
    /// <summary>Clockwise swing-pose connection handler.</summary>
    SwingClockwise,
    /// <summary>Anticlockwise swing-pose connection handler.</summary>
    SwingAnticlockwise,
    /// <summary>Standing up-right connection handler.</summary>
    StandingUpRight,
    /// <summary>Standing right connection handler.</summary>
    StandingRight,
    /// <summary>Standing down connection handler.</summary>
    StandingDown,
    /// <summary>Standing up-left connection handler.</summary>
    StandingUpLeft,
    /// <summary>Crouching up-right connection handler.</summary>
    CrouchingUpRight,
    /// <summary>Crouching right connection handler.</summary>
    CrouchingRight,
    /// <summary>Crouching down-left connection handler.</summary>
    CrouchingDownLeft,
    /// <summary>Crouching up-left connection handler.</summary>
    CrouchingUpLeft,
}
