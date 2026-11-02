using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete terrain blocks, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    /// <summary>Copied visual-only differences for the five compiled Crocomire arena layouts.</summary>
    private readonly PlmVisualOverrides customAppearances;

    /// <summary>Holds the import-validated appearances of the layouts that differ from the compiled layouts.</summary>
    /// <param name="customAppearances">The changed layouts' flattened appearances, keyed by draw.</param>
    internal RoomPlmCrocomireVisualCatalog(PlmVisualOverrides customAppearances) =>
        this.customAppearances = Ensure.NotNull(customAppearances);

    /// <summary>Resolves one arena-layout block, translating run-local indices into the flattened visual override.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the compiled draw run.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The selected block and mirroring, taken from the compiled terrain block when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        CrocomireArenaArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
