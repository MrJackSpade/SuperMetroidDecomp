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
}
