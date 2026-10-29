using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification's reading of the cartridge's bank-$92 spritemap pointer table as installed compositions.</summary>
internal static class SamusSpritemapNative
{
    /// <summary>The records <paramref name="pointers"/> reference, in record order: the installed composition list.</summary>
    internal static ushort[] Compositions(ushort[] pointers) => [.. pointers.Distinct().Where(pointer => pointer != 0).Order()];

    /// <summary>Each table entry's composition: a nonzero pointer's place among the referenced records, or none for zero.</summary>
    internal static SamusSpritemapId?[] Entries(ushort[] pointers)
    {
        ushort[] compositions = Compositions(pointers);
        return [.. pointers.Select(pointer => pointer == 0 ? (SamusSpritemapId?)null : new SamusSpritemapId(Array.BinarySearch(compositions, pointer)))];
    }

    /// <summary>The ordinal of the composition <paramref name="entry"/> selects.</summary>
    /// <exception cref="InvalidOperationException">The entry selects no composition.</exception>
    internal static int Ordinal(SamusSpritemapId? entry) =>
        entry?.Ordinal ?? throw new InvalidOperationException("The table entry selects no composition.");

    /// <summary>
    /// The cartridge's spritemap index for a drawn body picture: its pose half's first table entry plus its animation
    /// frame, or zero for no picture, the word the cartridge's bottom selector clears when a frame draws no bottom half.
    /// </summary>
    internal static int Index(InstalledSamusSpritemaps spritemaps, SamusBodyHalfFrame? frame) =>
        frame is { } drawn ? spritemaps.Source.EntryIndex(drawn) : 0;

    /// <summary>
    /// The body picture a recorded cartridge spritemap index names: the frame of <paramref name="pose"/>'s half that reads
    /// it, or, for an index before that half's first entry (drawn under an earlier pose), the nearest pose half whose
    /// frames read it, which shows the same composition. A zero bottom index is no picture.
    /// </summary>
    /// <exception cref="InvalidDataException">No pose half's covered frames read the index.</exception>
    internal static SamusBodyHalfFrame? Frame(InstalledSamusSpritemaps spritemaps, SamusPoseId pose, SamusBodyHalf half, int index)
    {
        if (half == SamusBodyHalf.Bottom && index == 0)
            return null;
        SamusSpritemapSources source = spritemaps.Source;
        IEnumerable<SamusPoseId> candidates = [pose, .. Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).Select(SamusPoseNumbering.FromNumber)
            .OrderByDescending(other => source.EntryIndex(new SamusBodyHalfFrame(other, half, 0)))];
        foreach (SamusPoseId candidate in candidates)
        {
            var frame = new SamusBodyHalfFrame(candidate, half, index - source.EntryIndex(new SamusBodyHalfFrame(candidate, half, 0)));
            if (frame.AnimationFrame >= 0 && spritemaps.Covers(frame))
                return frame;
        }
        throw new InvalidDataException($"No Samus pose half's frames read spritemap index {index}.");
    }
}
