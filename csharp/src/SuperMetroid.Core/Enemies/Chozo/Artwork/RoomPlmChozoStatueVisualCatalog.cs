using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// Editable Chozo hand and slope-access appearances. The calculated draw geometry and full
/// level words remain independent of installed visual block references.
/// </summary>
public sealed class RoomPlmChozoStatueVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => ChozoStatueArtwork.ContentIdentity(this);

    /// <summary>Visual-only overrides keyed by draw; absent when every selected block uses its compiled visual word.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete Chozo-layout coverage and copies authored visual differences without replacing physical level words or calculated run geometry.</summary>
    /// <param name="entries">One entry per compiled layout with its exact flattened block count and visual-only word payload.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, duplicates an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmChozoStatueVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customBlocks = new PlmVisualOverrides(entries, "Chozo statue", ChozoStatueDraws.TryGetByVisualId,
            ChozoStatueDraws.All.Count());

    /// <summary>Resolves one selected Chozo-layout visual word, projecting run-local indices into the flattened authored payload while preserving compiled mechanics.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a room coordinate or flattened entry index.</param>
    /// <returns>The metatile reference and parent flips; unchanged layouts derive these bits from compiled stock words.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Chozo layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex) =>
        ChozoStatueArtwork.Word(customBlocks, draw, runIndex, blockIndex);
}
