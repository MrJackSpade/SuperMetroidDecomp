using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Editable appearance for the cannon PLM. The compiled draw geometry, full
/// terrain blocks, hit thresholds, damage transitions, and control writes remain fixed.
/// </summary>
public sealed class RoomObjectDraygonCannonVisualCatalog
{
    /// <summary>Owned edited appearances by canonical frame pointer; null retains compiled stock appearance.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' flattened appearances, keyed by draw.</param>
    internal RoomObjectDraygonCannonVisualCatalog(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves a cannon block's selected appearance by projecting run-local indices into the flattened authored frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the frame's draw run.</param>
    /// <param name="blockIndex">Zero-based cell ordinal within that run, not a flattened entry index.</param>
    /// <returns>The selected block and mirroring, or visual bits from the compiled terrain block when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported cannon frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or cell ordinal is outside the compiled draw shape.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        DraygonCannonArtwork.Appearance(customBlocks, draw, runIndex, blockIndex);
}
