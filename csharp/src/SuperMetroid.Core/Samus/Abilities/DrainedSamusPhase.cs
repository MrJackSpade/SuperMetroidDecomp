namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The phases of Samus's drained pose and movement lock.</summary>
public enum DrainedSamusPhase
{
    /// <summary>No drained movement/pose lock is owned by this state, either before setup, after release art completes, or after another owner replaces the movement handler.</summary>
    Inactive,
    /// <summary>Either drain setup, able or unable to stand, has installed left-facing knockback <c>KnockbackLeftPose</c> and locked input via the shared rainbow-beam setup.</summary>
    RainbowBeamLocked,
    /// <summary>Controller zero has selected drained crouching <c>DrainedCrouchingRightPose</c> and <c>DrainedCrouchingLeftPose</c> at animation index two with zero base/Y speed; the drained-fall animation cue has not yet installed falling movement.</summary>
    WaitingForFallingCommand,
    /// <summary>The drained-fall animation cue has installed the drained falling handler, which advances shared gravity/Y movement and block collision until a solid collision restores normal movement.</summary>
    Falling,
    /// <summary>The falling handler encountered a solid collision, cleared Y speed, and selected animation index seven with an eight-update timer without changing pose or Y direction.</summary>
    OnFloor,
    /// <summary>Controller one has selected facing-dependent standing drained <c>DrainedStandingRightPose</c> and <c>DrainedStandingLeftPose</c> at animation index zero with a sixteen-update timer.</summary>
    Standing,
    /// <summary>Controller four has selected facing-dependent crouching drained <c>DrainedCrouchingRightPose</c> and <c>DrainedCrouchingLeftPose</c> at animation index eight with a sixteen-update timer.</summary>
    Crouching,
    /// <summary>Controller two has started drained release art, refreshed collision radii, and cleared base/Y speed; the drain lock remains until the release animation's closing pose transition completes.</summary>
    Releasing,
}
