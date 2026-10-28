using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Visual-only block selections for the three crumble frames and final clear.
/// Compiled draw definitions retain their physical words and 2×2 geometry.
/// </summary>
public sealed class RoomPlmSporeSpawnCeilingVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SporeSpawnCeilingArtwork.ContentIdentity(this);

    /// <summary>Owned visual-only edits keyed by draw; null uses all compiled stock words.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates complete coverage of the clear and three crumble frames, capturing edited ceiling appearance without changing compiled geometry or collision.</summary>
    /// <param name="entries">Each of the four compiled visual identities exactly once, with four valid presentation-only words per frame.</param>
    /// <remarks>Edited arrays are copied; stock-identical frames use compiled visual words, so later edits to caller arrays cannot change this catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">A frame is missing, duplicated, null, unknown, incorrectly shaped, or contains collision bits in its visual words.</exception>
    public RoomPlmSporeSpawnCeilingVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customWords = new PlmVisualOverrides(entries, "Spore Spawn ceiling", SporeSpawnCeilingDraws.TryGetByVisualId,
            SporeSpawnCeilingDraws.All.Count());

    /// <summary>Resolves one visual-only cell from an edited or compiled Spore Spawn ceiling frame.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Horizontal row index: zero for the top row, one for the bottom.</param>
    /// <param name="blockIndex">Zero-based left-to-right column within that two-cell row.</param>
    /// <returns>A twelve-bit metatile/parent-flip word without the separately compiled collision nibble.</returns>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four compiled frames.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either cell selector is outside zero through one; the reported parameter is <paramref name="blockIndex"/>.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex) =>
        SporeSpawnCeilingArtwork.Word(customWords, draw, runIndex, blockIndex);
}
