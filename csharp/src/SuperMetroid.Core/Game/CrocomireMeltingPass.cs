namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed control and transfer metadata for one Crocomire dissolve pass. The graphics
/// bytes at the selected bank-$A4 sources remain presentation data, not mechanics.
/// </summary>
internal readonly record struct CrocomireMeltingPass(
    ushort HeaderOffset,
    ushort MaximumAdjustedDestinationY,
    ushort DistortionEndY,
    ushort WordsToCopy,
    byte SourceBank,
    ushort TransferStartOffset,
    ushort TransferEndOffset,
    ushort NextHeaderOffset,
    int ChunkCount)
{
    /// <summary>Consecutive source-to-scratch pairs for this pass's six or seven chunks.</summary>
    internal CrocomireMeltingCopySequence Copies => new(this);
    /// <summary>Scratch-to-VRAM transfers for the same chunks after the copy sentinel.</summary>
    internal CrocomireMeltingUploadSequence Uploads => new(this);
}
