using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Visual-only Crocomire bridge and invisible-wall blocks. The compiled PLM
/// programs, complete physical level words, and draw geometry remain immutable.
/// </summary>
public sealed class RoomPlmCrocomireVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => CrocomireArenaArtwork.ContentIdentity(this);

    /// <summary>Copied visual-only differences for the five compiled Crocomire arena layouts.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all five Crocomire arena layouts and copies visual differences without changing physical level words, run geometry, or PLM programs.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled draw identity with its full flattened block count.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmCrocomireVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customWords = new PlmVisualOverrides(entries, "Crocomire", CrocomireArenaDraws.TryGetByVisualId,
            CrocomireArenaDraws.All.Count());

    /// <summary>Resolves one arena-layout block, translating native run-local indices into the flattened visual override.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based ordinal of the compiled draw run.</param>
    /// <param name="blockIndex">Zero-based word ordinal within that run, not a flattened entry index or room coordinate.</param>
    /// <returns>The selected metatile/flip word, derived from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported arena layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex) =>
        CrocomireArenaArtwork.Word(customWords, draw, runIndex, blockIndex);
}
