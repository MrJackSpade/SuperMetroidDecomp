using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Visual-only block selections for the three crumble frames and final clear.
/// Compiled draw definitions retain their terrain blocks and 2×2 geometry.
/// </summary>
public sealed class RoomPlmSporeSpawnCeilingVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SporeSpawnCeilingArtwork.ContentIdentity(this);

    /// <summary>Owned visual-only edits keyed by draw; stock draws use their compiled blocks.</summary>
    private readonly PlmVisualOverrides customAppearances;

    /// <summary>Validates complete coverage of the clear and three crumble frames, capturing edited ceiling appearance without changing compiled geometry or collision.</summary>
    /// <param name="entries">Each of the four compiled visual identities exactly once, with four appearances per frame.</param>
    /// <remarks>Edited arrays are copied; stock-identical frames use compiled appearances, so later edits to caller arrays cannot change this catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">A frame is missing, duplicated, null, unknown, or incorrectly shaped.</exception>
    public RoomPlmSporeSpawnCeilingVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customAppearances = new PlmVisualOverrides(entries, "Spore Spawn ceiling", SporeSpawnCeilingDraws.TryGetByVisualId,
            SporeSpawnCeilingDraws.All.Count());

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Horizontal row index: zero for the top row, one for the bottom.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within that two-cell row.</param>
    /// <returns>The block and its mirroring, without the separately compiled collision.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal RoomBlockAppearance? GetAppearance(PlmDraw draw, int runIndex, int blockIndex) =>
        SporeSpawnCeilingArtwork.Appearance(customAppearances, draw, runIndex, blockIndex);
}
