namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Fixed control and transfer metadata for one Crocomire dissolve pass. The graphics
/// bytes at the selected bank-$A4 sources remain presentation data, not mechanics.
/// </summary>
/// <param name="HeaderOffset">Melt-table cursor of the pass header.</param>
/// <param name="MaximumAdjustedDestinationY">Lowest BG2 source row the distortion stretches back to.</param>
/// <param name="DistortionEndY">Height at which the pass's distortion band stops.</param>
/// <param name="WordsToCopy">Words each graphics copy moves, one fewer than the native copy actually writes.</param>
/// <param name="SourceBank">Cartridge bank of the pass's graphics sources.</param>
/// <param name="TransferStartOffset">Melt-table cursor of the pass's first upload.</param>
/// <param name="TransferEndOffset">Melt-table cursor of the pass's upload terminator.</param>
/// <param name="NextHeaderOffset">Melt-table cursor just past the pass, where the next pass's header starts.</param>
/// <param name="ChunkCount">Graphics chunks the pass copies and uploads.</param>
internal readonly record struct CrocomireMeltingPass(
    int HeaderOffset,
    int MaximumAdjustedDestinationY,
    int DistortionEndY,
    int WordsToCopy,
    byte SourceBank,
    int TransferStartOffset,
    int TransferEndOffset,
    int NextHeaderOffset,
    int ChunkCount)
{
    /// <summary>Consecutive source-to-scratch pairs for this pass's six or seven chunks.</summary>
    internal CrocomireMeltingCopySequence Copies => new(this);
    /// <summary>Scratch-to-VRAM transfers for the same chunks after the copy sentinel.</summary>
    internal CrocomireMeltingUploadSequence Uploads => new(this);
}
