using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Visual-only block selections for the three crumble frames and final clear.
/// Compiled draw definitions retain their terrain blocks and 2×2 geometry.
/// </summary>
public sealed class RoomObjectSporeSpawnCeilingVisualCatalog
{
    /// <summary>Owned visual-only edits keyed by draw; stock draws use their compiled blocks.</summary>
    private readonly RoomObjectVisualOverrides customAppearances;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customAppearances">The changed frames' four appearances, keyed by draw.</param>
    internal RoomObjectSporeSpawnCeilingVisualCatalog(RoomObjectVisualOverrides customAppearances) =>
        this.customAppearances = Ensure.NotNull(customAppearances);

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Horizontal row index: zero for the top row, one for the bottom.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within that two-cell row.</param>
    /// <returns>The block and its mirroring, without the separately compiled collision.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance? GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex) =>
        SporeSpawnCeilingArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
