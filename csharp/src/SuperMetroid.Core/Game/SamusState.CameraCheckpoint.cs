namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    private ushort? _poseCollisionPreviousYPosition;
    private int _poseAlignmentPreviousYDelta;

    /// <summary>
    /// Applies $90:EC7E Samus_AlignBottomWithPrevPose after installing a new pose:
    /// preserve the feet and shift the previous whole-Y checkpoint by the same delta.
    /// Neither fractional word changes.
    /// </summary>
    internal void AlignBottomAfterPoseChange(ushort previousRadius, ushort targetRadius)
    {
        int delta = previousRadius - targetRadius;
        YPosition = unchecked((ushort)(YPosition + delta));
        _poseAlignmentPreviousYDelta += delta;
    }

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
        ushort correctedY = unchecked((ushort)((_poseCollisionPreviousYPosition ?? previous.YPosition)
            + _poseAlignmentPreviousYDelta));
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        return previous with { YPosition = correctedY };
    }
}
