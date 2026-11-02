using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Editable appearance for the glass PLM. The compiled draw geometry, full
/// terrain blocks, hit thresholds, shards, and event timing remain fixed.
/// </summary>
public sealed class RoomPlmMotherBrainGlassVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
    public string ContentIdentity => MotherBrainGlassArtwork.ContentIdentity(this);

    /// <summary>Copied visual overrides by compiled glass-layout pointer; unchanged layouts use stock appearances.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customBlocks">The changed layouts' flattened appearances, keyed by draw.</param>
    internal RoomPlmMotherBrainGlassVisualCatalog(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one glass-layout visual block by translating run-local indices into the flattened authored frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based run ordinal; supported layouts contain one through four runs.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored block and mirroring, or the compiled terrain block's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported glass layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        MotherBrainGlassArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
