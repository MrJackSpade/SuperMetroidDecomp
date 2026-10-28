using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Native collision words, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomPlmMotherBrainFakeDeathVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => MotherBrainFakeDeathArtwork.ContentIdentity(this);

    /// <summary>Only changed draw appearances, keyed by draw; unchanged draws use compiled visuals.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all twenty-two fake-death room layouts and copies visual differences while retaining collision words, signed draw offsets, and PLM timing.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, with the exact flattened count of all its native run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmMotherBrainFakeDeathVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries) =>
        customWords = new PlmVisualOverrides(entries, "Mother Brain fake-death", MotherBrainFakeDeathDraws.TryGetByVisualId,
            MotherBrainFakeDeathDraws.All.Count());

    /// <summary>Resolves a fake-death room block's selected appearance, projecting native run-local indices into its flattened authored layout.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; background rows have one thirteen-word horizontal run, while other layouts retain their own run shapes.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex) =>
        MotherBrainFakeDeathArtwork.Word(customWords, draw, runIndex, blockIndex);
}
