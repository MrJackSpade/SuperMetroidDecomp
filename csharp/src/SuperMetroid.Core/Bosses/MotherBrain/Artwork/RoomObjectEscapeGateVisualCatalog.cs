using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Presentation-only selection for the three escape-gate frames. The compiled
/// terrain blocks continue to own collision, animation timing, and door handoff.
/// </summary>
public sealed class RoomObjectEscapeGateVisualCatalog
{
    /// <summary>Only escape-gate frames whose appearances differ from compiled stock data.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' four appearances, keyed by draw.</param>
    internal RoomObjectEscapeGateVisualCatalog(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one escape-gate block's selected appearance independently of its compiled solid collision.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="blockIndex">Zero-based row ordinal, zero through three, down the vertical run.</param>
    /// <returns>The authored block and mirroring, or the compiled frame's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported escape-gate frame.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row ordinal is outside the four-block run.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int blockIndex) => EscapeGateArtwork.Appearance(customBlocks, draw, blockIndex);
}
