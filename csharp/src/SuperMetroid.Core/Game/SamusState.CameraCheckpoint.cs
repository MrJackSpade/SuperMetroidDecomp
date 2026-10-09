namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>Absolute previous-Y checkpoint written by collision correction, when one has occurred this frame.</summary>
    private ushort? _poseCollisionPreviousYPosition;
    /// <summary>Accumulated previous-Y shift caused by bottom alignment after pose changes.</summary>
    private int _poseAlignmentPreviousYDelta;
    /// <summary>Pending whole-word write to the previous X checkpoint.</summary>
    private ushort? _previousXPositionWrite;
    // Pending writes to the previous fractions ($0B12/$0B16): bits set in a mask are replaced
    // by the matching value bits when the checkpoint is applied. A zero mask means no write.
    /// <summary>Bits of the previous X fraction word that should be replaced at checkpoint application.</summary>
    private ushort _previousXSubpositionWriteMask;
    /// <summary>Replacement bits paired with <see cref="_previousXSubpositionWriteMask"/>.</summary>
    private ushort _previousXSubpositionWriteValue;
    /// <summary>Bits of the previous Y fraction word that should be replaced at checkpoint application.</summary>
    private ushort _previousYSubpositionWriteMask;
    /// <summary>Replacement bits paired with <see cref="_previousYSubpositionWriteMask"/>.</summary>
    private ushort _previousYSubpositionWriteValue;

    /// <summary>
    /// Records a direct write of SamusPreviousXPosition ($0B10) by an owner that runs
    /// before MainScrollingRoutine, such as the landed gunship placing Samus at $A2:A925.
    /// Only the whole word changes; the previous X fraction ($0B12) is untouched.
    /// </summary>
    internal void WritePreviousXPosition(ushort xPosition) =>
        _previousXPositionWrite = xPosition;

    /// <summary>
    /// Records a write to bits of SamusPreviousXSubPosition ($0B12): a byte store sets mask
    /// $FF00, a word store $FFFF. Later writes this frame replace the bits they cover.
    /// </summary>
    internal void WritePreviousXSubposition(ushort mask, ushort value)
    {
        _previousXSubpositionWriteValue = (ushort)((_previousXSubpositionWriteValue & ~mask) | (value & mask));
        _previousXSubpositionWriteMask |= mask;
    }

    /// <summary>Records a write to bits of SamusPreviousYSubPosition ($0B16), as for X.</summary>
    internal void WritePreviousYSubposition(ushort mask, ushort value)
    {
        _previousYSubpositionWriteValue = (ushort)((_previousYSubpositionWriteValue & ~mask) | (value & mask));
        _previousYSubpositionWriteMask |= mask;
    }

    /// <summary>Replaces only the masked bits of a checkpoint word, preserving all other bits.</summary>
    /// <param name="mask">Bits selected for replacement.</param>
    /// <param name="value">Replacement bits, aligned to their positions in the word.</param>
    /// <param name="word">Original checkpoint word.</param>
    /// <returns>The word with selected bits taken from <paramref name="value"/>.</returns>
    private static ushort ApplyBits(ushort mask, ushort value, ushort word) =>
        (ushort)((word & ~mask) | value);

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
        ushort xSubposition = ApplyBits(_previousXSubpositionWriteMask, _previousXSubpositionWriteValue, previous.XSubposition);
        ushort ySubposition = ApplyBits(_previousYSubpositionWriteMask, _previousYSubpositionWriteValue, previous.YSubposition);
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        _previousXPositionWrite = null;
        _previousXSubpositionWriteMask = _previousXSubpositionWriteValue = 0;
        _previousYSubpositionWriteMask = _previousYSubpositionWriteValue = 0;
        return previous with
        {
            XPosition = xPosition,
            XSubposition = xSubposition,
            YPosition = correctedY,
            YSubposition = ySubposition,
        };
    }
}
