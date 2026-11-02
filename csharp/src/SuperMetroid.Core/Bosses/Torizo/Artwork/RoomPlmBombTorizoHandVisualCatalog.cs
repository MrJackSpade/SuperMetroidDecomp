using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Editable appearance for the hand PLM. The compiled draw geometry, full
/// terrain blocks, Bombs gate, artwork transfers, debris cadence, and music remain fixed.
/// </summary>
public sealed class RoomPlmBombTorizoHandVisualCatalog
{
    /// <summary>Copied visual-only edits keyed by draw; stock frames use their compiled blocks.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' flattened appearances, keyed by draw.</param>
    internal RoomPlmBombTorizoHandVisualCatalog(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected appearance in a hand draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not the flattened entry index or a room coordinate.</param>
    /// <returns>The metatile reference and parent flips, excluding the compiled physical collision nibble.</returns>
    /// <exception cref="InvalidDataException">The draw identity is not a supported hand frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        BombTorizoHandArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
