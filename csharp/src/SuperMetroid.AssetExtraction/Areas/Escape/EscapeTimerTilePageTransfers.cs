using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The two transfer pages the escape timer's characters upload as (#1502): each page's size, the OBJ character it begins
/// at, and the installed characters it carries. Only the import pipeline resolves the pages; the game draws the timer's
/// pre-coloured sheet.
/// </summary>
public static class EscapeTimerTilePageTransfers
{
    /// <summary>The page's upload size in bytes.</summary>
    public static ushort ByteCountOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstByteCount,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>The OBJ character the page begins at.</summary>
    public static int FirstCharacterOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstCharacter,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondCharacter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>Resolves one page of <paramref name="atlas"/> without combining or retiming its original transfer record.</summary>
    /// <param name="atlas">The installed timer characters, laid out from the first page's first character in transfer order.</param>
    /// <param name="page">The page to slice: its characters from its first character, its byte count long.</param>
    public static ReadOnlyMemory<byte> Resolve(EscapeTimerCharacters atlas, EscapeTimerTilePage page)
    {
        ReadOnlySpan<byte> characters = Ensure.NotNull(atlas).characters;
        int firstByte = (FirstCharacterOf(page) - EscapeTimerTileAtlasFormat.FirstCharacter) * EscapeTimerTileAtlasFormat.CharacterByteCount;
        return characters.Slice(firstByte, ByteCountOf(page)).ToArray();
    }
}
