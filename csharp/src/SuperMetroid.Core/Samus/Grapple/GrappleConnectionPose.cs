namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The pose a grapple connection puts Samus in, named for her stance and the way she faces or swings.</summary>
public enum GrappleConnectionPose
{
    /// <summary>Swings clockwise in <see cref="Poses.SamusPoseId.GrappleSwingRightPose"/>.</summary>
    SwingClockwise,
    /// <summary>Swings anticlockwise in <see cref="Poses.SamusPoseId.GrappleSwingLeftPose"/>.</summary>
    SwingAnticlockwise,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleStandingRightPose"/>.</summary>
    StandingUpRight,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleStandingDownRightPose"/>.</summary>
    StandingRight,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleStandingDownLeftPose"/>.</summary>
    StandingDown,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleStandingLeftPose"/>.</summary>
    StandingUpLeft,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleCrouchingRightPose"/>.</summary>
    CrouchingUpRight,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleCrouchingDownRightPose"/>.</summary>
    CrouchingRight,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleCrouchingDownLeftPose"/>.</summary>
    CrouchingDownLeft,
    /// <summary>Held in place in <see cref="Poses.SamusPoseId.GrappleCrouchingLeftPose"/>.</summary>
    CrouchingUpLeft,
}
