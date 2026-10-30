using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// Editable Chozo hand and slope-access appearances. The calculated draw geometry and complete
/// terrain blocks remain independent of installed block appearances.
/// </summary>
public sealed class RoomPlmChozoStatueVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => ChozoStatueArtwork.ContentIdentity(this);

    /// <summary>Visual-only overrides keyed by draw; absent when every selected block uses its compiled appearance.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete Chozo-layout coverage and copies authored visual differences without replacing terrain blocks or calculated run geometry.</summary>
    /// <param name="entries">One entry per compiled layout with its exact flattened block count and appearance payload.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, duplicates an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmChozoStatueVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customBlocks = new PlmVisualOverrides(entries, "Chozo statue", ChozoStatueDraws.TryGetByVisualId,
            ChozoStatueDraws.All.Count());

    /// <summary>Resolves one selected Chozo-layout appearance, projecting run-local indices into the flattened authored payload while preserving compiled mechanics.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a room coordinate or flattened entry index.</param>
    /// <returns>The block and its mirroring; unchanged layouts take them from the compiled stock blocks.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Chozo layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        ChozoStatueArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
