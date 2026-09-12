namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    private ushort? _poseCollisionPreviousYPosition;

    /// <summary>
    /// Records the previous-Y word written by bank-$91 changed-pose collision
    /// correction. The previous fraction is deliberately not changed: native
    /// writes only $0B14, not the neighboring $0B16 word.
    /// </summary>
    private void RecordPoseCollisionCameraY(ushort correctedY) =>
        _poseCollisionPreviousYPosition = correctedY;

    /// <summary>
    /// Applies pose-collision checkpoint writes before $90:94EC calculates
    /// distance. Its normal tail subsequently replaces the complete checkpoint.
    /// </summary>
    internal SamusCameraPoint ApplyPoseCollisionCameraCheckpoint(SamusCameraPoint previous)
    {
        if (_poseCollisionPreviousYPosition is not { } correctedY) return previous;
        _poseCollisionPreviousYPosition = null;
        return previous with { YPosition = correctedY };
    }
}
