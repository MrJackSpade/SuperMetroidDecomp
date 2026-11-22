using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one windup or active shinespark update did.</summary>
/// <param name="WindupTimedOut">True when this update's windup countdown ran out and selected the automatic vertical launch.</param>
/// <param name="CrashSequenceFinished">True when crash cleanup completed, requesting the post-animation standing-pose commit; does not imply both released echoes were allocated.</param>
/// <param name="PendingLaunchPose">Directional launch pose a timed-out windup leaves for the frame's pose update to commit; otherwise null.</param>
public readonly partial record struct ShinesparkMovementResult(
    bool WindupTimedOut,
    bool CrashSequenceFinished = false,
    SamusPoseId? PendingLaunchPose = null);
