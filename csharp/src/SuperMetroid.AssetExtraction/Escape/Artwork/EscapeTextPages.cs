using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Escape.Artwork;
using static SuperMetroid.Core.Escape.Artwork.EscapeTextArtworkDefinitions;
using static SuperMetroid.AssetExtraction.Escape.Artwork.EscapeTextSheetFormat;

namespace SuperMetroid.AssetExtraction.Escape.Artwork;

/// <summary>
/// Locates each of the five escape-text character pages (<see cref="EscapeTextArtworkDefinitions"/>) in the installed sheet
/// and in OBJ character memory.
/// </summary>
public static class EscapeTextPages
{
    /// <summary>Where text page <paramref name="page"/> starts within the installed sheet, from the escape text's page list.</summary>
    public static int PageOffset(int page) => (uint)page < PageCount
        ? page * PageBytes : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>Four full $200-byte pages followed by the final $100-byte page.</summary>
    public static int PageByteCount(int page) => (uint)page < PageCount
        ? WordWrap.Unsigned(Math.Min(PageBytes, ByteCount - page * PageBytes)) : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>The sprite character text page <paramref name="page"/> begins at, from the same page list.</summary>
    public static int PageFirstCharacter(int page) => (uint)page < PageCount
        ? FirstCharacter + page * PageBytes / CharacterBytes : throw new ArgumentOutOfRangeException(nameof(page));
}
