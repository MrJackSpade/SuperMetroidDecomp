using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete terrain blocks, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => CrocomireArenaArtwork.ContentIdentity(this);

    /// <summary>Copied visual-only differences for the five compiled Crocomire arena layouts.</summary>
    private readonly PlmVisualOverrides customAppearances;

    /// <summary>Validates all five Crocomire arena layouts and copies visual differences without changing terrain blocks, run geometry, or PLM programs.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled draw identity with its full flattened block count.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmCrocomireVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customAppearances = new PlmVisualOverrides(entries, "Crocomire", CrocomireArenaDraws.TryGetByVisualId,
            CrocomireArenaDraws.All.Count());

    /// <summary>Resolves one arena-layout block, translating native run-local indices into the flattened visual override.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the compiled draw run.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The selected block and mirroring, taken from the compiled terrain block when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        CrocomireArenaArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
