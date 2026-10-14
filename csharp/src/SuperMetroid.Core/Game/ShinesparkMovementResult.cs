namespace SuperMetroid.Core.Game;

/// <summary>Debugger snapshot of one windup or active shinespark handler call.</summary>
/// <param name="WindupTimedOut">True when this call's windup countdown reached zero or wrapped negative and selected the automatic vertical launch.</param>
/// <param name="CrashSequenceFinished">True when crash cleanup completed, requesting the caller's post-animation standing-pose commit; does not imply both released echoes were allocated.</param>
/// <param name="PendingLaunchPose">Directional launch pose awaiting a post-animation commit when timeout pose changes were deferred; otherwise null.</param>
public readonly record struct ShinesparkMovementResult(
    bool WindupTimedOut,
    bool CrashSequenceFinished = false,
    SamusPoseId? PendingLaunchPose = null);
