namespace SuperMetroid.Core.Samus.Poses;

/// <summary>How a pose change that enlarges Samus's collision box resolves against the surrounding terrain.</summary>
internal enum LargerPoseCollisionOutcome
{
    /// <summary>The enlarged target pose fits after the center adjustment.</summary>
    Allowed,
    /// <summary>Terrain blocks the target pose, so Samus keeps her source pose.</summary>
    RetainSource,
    /// <summary>Terrain blocks the target pose and the game's collision handling selects an ordinary crouch.</summary>
    CrouchFallback,
}
