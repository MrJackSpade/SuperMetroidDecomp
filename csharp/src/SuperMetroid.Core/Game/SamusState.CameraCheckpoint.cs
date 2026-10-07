namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    private ushort? _poseCollisionPreviousYPosition;
    private int _poseAlignmentPreviousYDelta;
    private ushort? _previousXPositionWrite;

    /// <summary>
    /// Records a direct write of SamusPreviousXPosition ($0B10) by an owner that runs
    /// before MainScrollingRoutine, such as the landed gunship placing Samus at $A2:A925.
    /// Only the whole word changes; the previous X fraction ($0B12) is untouched.
    /// </summary>
    internal void WritePreviousXPosition(ushort xPosition) =>
        _previousXPositionWrite = xPosition;

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
    /// Records a direct write of SamusPreviousYPosition ($0B14), such as bank-$91
    /// changed-pose collision correction or MakeSamusFaceForward's lift. The previous
    /// fraction is deliberately not changed: native writes only $0B14, not $0B16.
    /// The write is absolute, discarding any earlier bottom-alignment shift this frame.
    /// </summary>
    internal void WritePreviousYPosition(ushort correctedY)
    {
        _poseCollisionPreviousYPosition = correctedY;
        _poseAlignmentPreviousYDelta = 0;
    }

    /// <summary>
    /// The live SamusPreviousXPosition/SamusPreviousYPosition words ($0B10/$0B14): the
    /// frame-start checkpoint with this frame's writes so far, without consuming them.
    /// </summary>
    internal (ushort X, ushort Y) PeekPreviousPositionWords(SamusCameraPoint previous) => (
        _previousXPositionWrite ?? previous.XPosition,
        unchecked((ushort)((_poseCollisionPreviousYPosition ?? previous.YPosition) + _poseAlignmentPreviousYDelta)));

    /// <summary>
    /// Applies this frame's previous-position word writes before $90:94EC calculates
    /// distance. Its normal tail subsequently replaces the complete checkpoint.
    /// </summary>
    internal SamusCameraPoint ApplyPreviousPositionWrites(SamusCameraPoint previous)
    {
        ushort correctedY = unchecked((ushort)((_poseCollisionPreviousYPosition ?? previous.YPosition)
            + _poseAlignmentPreviousYDelta));
        ushort xPosition = _previousXPositionWrite ?? previous.XPosition;
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        _previousXPositionWrite = null;
        return previous with { XPosition = xPosition, YPosition = correctedY };
    }
}
