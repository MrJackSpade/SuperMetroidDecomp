namespace SuperMetroid.Core.Assets;

/// <summary>
/// Builds the 512-character OBJ page a nine-bit OBJ character number indexes. Characters past the supplied sheet are
/// blank, and a further row plus one is blank overhang, so any character a large part names stays inside the page.
/// </summary>
public static class ObjCharacterPage
{
    /// <summary>Characters a nine-bit OBJ name selects.</summary>
    public const int Characters = 512;

    /// <summary>Copies <paramref name="sheet"/> into a page of every nine-bit OBJ character plus large-part overhang.</summary>
    /// <param name="id">The page's identity.</param>
    /// <param name="sheet">Planar characters placed from character zero.</param>
    /// <exception cref="ArgumentException">The sheet is larger than the page.</exception>
    public static SpriteArtwork Create(SpriteArtworkId id, ReadOnlySpan<byte> sheet)
    {
        var characters = new byte[(Characters + SpriteArtwork.CharactersPerRow + 1) * SpriteArtwork.CharacterByteCount];
        if (sheet.Length > Characters * SpriteArtwork.CharacterByteCount)
            throw new ArgumentException($"An OBJ page holds {Characters} characters; the sheet has {sheet.Length} bytes.", nameof(sheet));
        sheet.CopyTo(characters);
        return new SpriteArtwork(id, characters);
    }

    /// <summary>Copies <paramref name="page"/> with <paramref name="characters"/> written from <paramref name="firstCharacter"/>.</summary>
    /// <param name="id">The composed page's identity.</param>
    /// <param name="page">The page the characters replace part of.</param>
    /// <param name="characters">Planar characters, written in order.</param>
    /// <param name="firstCharacter">Page character the first written character replaces.</param>
    /// <exception cref="ArgumentOutOfRangeException">The characters do not fit in the page there.</exception>
    public static SpriteArtwork With(SpriteArtworkId id, SpriteArtwork page, ReadOnlySpan<byte> characters, int firstCharacter)
    {
        Ensure.NotNull(page);
        if (firstCharacter < 0 || firstCharacter * SpriteArtwork.CharacterByteCount + characters.Length > page.Characters.Length)
            throw new ArgumentOutOfRangeException(nameof(firstCharacter), firstCharacter, $"The characters do not fit in {page.Id} there.");
        byte[] combined = page.Characters.ToArray();
        characters.CopyTo(combined.AsSpan(firstCharacter * SpriteArtwork.CharacterByteCount));
        return new SpriteArtwork(id, combined);
    }
}
