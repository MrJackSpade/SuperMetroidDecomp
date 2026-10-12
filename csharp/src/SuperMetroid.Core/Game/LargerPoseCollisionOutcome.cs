namespace SuperMetroid.Core.Game;

/// <summary>How a pose change that enlarges Samus's collision box resolves against the surrounding terrain.</summary>
internal enum LargerPoseCollisionOutcome
{
    /// <summary>Identifies allowed.</summary>
    Allowed,
    /// <summary>Identifies retain source.</summary>
    RetainSource,
    /// <summary>Identifies crouch fallback.</summary>
    CrouchFallback,
}
