using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Editable appearance for the hand PLM. The compiled draw geometry, full
/// terrain blocks, Bombs gate, DMA, debris cadence, and music remain fixed.
/// </summary>
public sealed class RoomPlmBombTorizoHandVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => BombTorizoHandArtwork.ContentIdentity(this);

    /// <summary>Copied visual-only edits keyed by draw; stock frames use their compiled blocks.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates complete hand-frame coverage and copies only authored visual differences, leaving Bombs gating, collision, draw geometry and debris/music choreography fixed.</summary>
    /// <param name="entries">One entry for every compiled hand frame, with exactly the flattened native block count and no physical collision bits.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats a frame, changes a compiled identity/shape, or leaves coverage incomplete.</exception>
    public RoomPlmBombTorizoHandVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customBlocks = new PlmVisualOverrides(entries, "Bomb Torizo hand", BombTorizoHandDraws.TryGetByVisualId,
            BombTorizoHandDraws.All.Count());

    /// <summary>Resolves one selected appearance in a hand draw run, using compiled stock art when no independent edit was supplied.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not the flattened entry index or a room coordinate.</param>
    /// <returns>The metatile reference and parent flips, excluding the compiled physical collision nibble.</returns>
    /// <exception cref="InvalidDataException">The draw identity is not a supported hand frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled shape.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        BombTorizoHandArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
