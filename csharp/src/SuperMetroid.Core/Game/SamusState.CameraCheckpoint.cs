namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    private ushort? _poseCollisionPreviousYPosition;
    private int _poseAlignmentPreviousYDelta;
    private ushort? _previousXPositionWrite;
    // Pending writes to the previous fractions ($0B12/$0B16): bits set in a mask are replaced
    // by the matching value bits when the checkpoint is applied. A zero mask means no write.
    private PreviousFractionWrites _previousFractionWrites;

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
    internal void WritePreviousXSubposition(ushort mask, ushort value) =>
        _previousFractionWrites = _previousFractionWrites with { X = _previousFractionWrites.X.Write(mask, value) };

    /// <summary>Records a write to bits of SamusPreviousYSubPosition ($0B16), as for X.</summary>
    internal void WritePreviousYSubposition(ushort mask, ushort value) =>
        _previousFractionWrites = _previousFractionWrites with { Y = _previousFractionWrites.Y.Write(mask, value) };

    /// <summary>
    /// Applies $90:EC7E Samus_AlignBottomWithPrevPose after installing a new pose:
    /// preserve the feet and shift the previous whole-Y checkpoint by the same delta.
    /// Neither fractional word changes.
    /// </summary>
    internal void AlignBottomAfterPoseChange(ushort previousRadius, ushort targetRadius)
    {
        int delta = previousRadius - targetRadius;
        Position += new Offset16<RoomPixels>(0, unchecked((short)delta));
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
        ushort xSubposition = _previousFractionWrites.X.ApplyTo(previous.XSubposition);
        ushort ySubposition = _previousFractionWrites.Y.ApplyTo(previous.YSubposition);
        _poseCollisionPreviousYPosition = null;
        _poseAlignmentPreviousYDelta = 0;
        _previousXPositionWrite = null;
        _previousFractionWrites = default;
        return previous with
        {
            XPosition = xPosition,
            XSubposition = xSubposition,
            YPosition = correctedY,
            YSubposition = ySubposition,
        };
    }

    /// <summary>
    /// Bit-masked pending writes to one previous-fraction word: bits set in
    /// <see cref="Mask"/> are replaced by the matching <see cref="Value"/> bits on apply.
    /// </summary>
    private readonly record struct MaskedWordWrite(ushort Mask, ushort Value)
    {
        /// <summary>Records a byte ($FF00) or word ($FFFF) store; later stores replace the bits they cover.</summary>
        public MaskedWordWrite Write(ushort mask, ushort value) =>
            new((ushort)(Mask | mask), (ushort)((Value & ~mask) | (value & mask)));

        public ushort ApplyTo(ushort word) => (ushort)((word & ~Mask) | Value);
    }

    /// <summary>The pending masked writes to both previous-fraction words as one value.</summary>
    [SpatialComponents("Spatial value type: the pending masked writes to the previous X/Y fraction words $0B12/$0B16, one per axis.")]
    private readonly record struct PreviousFractionWrites(MaskedWordWrite X, MaskedWordWrite Y);
}
