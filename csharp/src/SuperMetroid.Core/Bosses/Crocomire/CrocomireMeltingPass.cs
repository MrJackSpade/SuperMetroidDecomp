using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Fixed control and transfer metadata for one Crocomire dissolve pass. The graphics
/// chunks it copies are presentation data; where they come from in the cartridge is the
/// extraction pipeline's knowledge, not the simulation's.
/// </summary>
/// <param name="HeaderOffset">Melt-table cursor of the pass header.</param>
/// <param name="MaximumAdjustedDestinationY">Lowest BG2 source row the distortion stretches back to.</param>
/// <param name="DistortionEndY">Height at which the pass's distortion band stops.</param>
/// <param name="WordsToCopy">Words each graphics copy moves, one fewer than the native copy actually writes.</param>
/// <param name="TransferStartOffset">Melt-table cursor of the pass's first upload.</param>
/// <param name="TransferEndOffset">Melt-table cursor of the pass's upload terminator.</param>
/// <param name="NextHeaderOffset">Melt-table cursor just past the pass, where the next pass's header starts.</param>
/// <param name="ChunkCount">Graphics chunks the pass copies and uploads.</param>
internal readonly record struct CrocomireMeltingPass(
    int HeaderOffset,
    int MaximumAdjustedDestinationY,
    int DistortionEndY,
    int WordsToCopy,
    int TransferStartOffset,
    int TransferEndOffset,
    int NextHeaderOffset,
    int ChunkCount)
{
    /// <summary>
    /// The pass's graphics chunks in copy order, each landing one scratch stride after the last. The second pass's seventh
    /// chunk is palette and adjacent data rather than tiles; it is preserved, as is the caller's inclusive $0201-word copy.
    /// </summary>
    internal ImmutableArray<CrocomireMeltingCopy> Copies
    {
        get
        {
            CrocomireMeltingHeader header = HeaderOffset == (ushort)CrocomireMeltingHeader.First
                ? CrocomireMeltingHeader.First
                : CrocomireMeltingHeader.Second;
            var copies = ImmutableArray.CreateBuilder<CrocomireMeltingCopy>(ChunkCount);
            for (int index = 0; index < ChunkCount; index++)
            {
                copies.Add(new(header, index,
                    CrocomireMeltingTransferDefinitions.ScratchStart + index * CrocomireMeltingTransferDefinitions.ChunkByteStride));
            }
            return copies.MoveToImmutable();
        }
    }
    /// <summary>Scratch-to-VRAM transfers for the same chunks after the copy sentinel.</summary>
    internal CrocomireMeltingUploadSequence Uploads => new(this);
}
