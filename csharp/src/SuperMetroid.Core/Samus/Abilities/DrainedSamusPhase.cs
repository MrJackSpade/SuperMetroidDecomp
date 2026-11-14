namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The phases of Samus's drained pose and movement lock.</summary>
public enum DrainedSamusPhase
{
    /// <summary>The drain holds neither Samus's pose nor her movement, either before setup, after release art completes, or after another owner takes her movement.</summary>
    Inactive,
    /// <summary>Either drain setup, able or unable to stand, has installed left-facing knockback <see cref="Poses.SamusPoseId.KnockbackLeftPose"/> and locked input via the shared rainbow-beam setup.</summary>
    RainbowBeamLocked,
    /// <summary>Controller zero has selected drained crouching <see cref="Poses.SamusPoseId.DrainedCrouchingRightPose"/> and <see cref="Poses.SamusPoseId.DrainedCrouchingLeftPose"/> at animation index two with zero base/Y speed; the drained-fall animation cue has not yet started her fall.</summary>
    WaitingForFallingCue,
    /// <summary>The drained-fall animation cue has started her fall, which advances shared gravity/Y movement and block collision until a solid collision ends it.</summary>
    Falling,
    /// <summary>The fall encountered a solid collision, cleared Y speed, and selected animation index seven with an eight-update timer without changing pose or Y direction.</summary>
    OnFloor,
    /// <summary>Controller one has selected facing-dependent standing drained <see cref="Poses.SamusPoseId.DrainedStandingRightPose"/> and <see cref="Poses.SamusPoseId.DrainedStandingLeftPose"/> at animation index zero with a sixteen-update timer.</summary>
    Standing,
    /// <summary>Controller four has selected facing-dependent crouching drained <see cref="Poses.SamusPoseId.DrainedCrouchingRightPose"/> and <see cref="Poses.SamusPoseId.DrainedCrouchingLeftPose"/> at animation index eight with a sixteen-update timer.</summary>
    Crouching,
    /// <summary>Controller two has started drained release art, refreshed collision radii, and cleared base/Y speed; the drain lock remains until the release animation's closing pose transition completes.</summary>
    Releasing,
}
