namespace SuperMetroid.Core.Game;

/// <summary>How a pose change that enlarges Samus's collision box resolves against the surrounding terrain.</summary>
internal enum LargerPoseCollisionOutcome
{
    Allowed,
    RetainSource,
    CrouchFallback,
}
