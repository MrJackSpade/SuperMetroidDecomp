using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// Editable Chozo hand and slope-access appearances. The calculated draw geometry and complete
/// terrain blocks remain independent of installed block appearances.
/// </summary>
public sealed class RoomObjectChozoStatueVisualCatalog
{
    /// <summary>Visual-only overrides keyed by draw; absent when every selected block uses its compiled appearance.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customBlocks">The changed layouts' flattened appearances, keyed by draw.</param>
    internal RoomObjectChozoStatueVisualCatalog(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected Chozo-layout appearance, projecting run-local indices into the flattened authored payload while preserving compiled mechanics.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a room coordinate or flattened entry index.</param>
    /// <returns>The block and its mirroring; unchanged layouts take them from the compiled stock blocks.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Chozo layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        ChozoStatueArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
