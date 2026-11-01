namespace SuperMetroid.AssetExtraction.Escape.Artwork;

/// <summary>The installed sheet of the Zebes escape typewriter's editable sprite characters and where its pages load.</summary>
public static class EscapeTextSheetFormat
{
    /// <summary>2,304 contiguous bytes, ending after the half-size fifth source page.</summary>
    public const int ByteCount = 2304;

    /// <summary>Indexed four-bit sheet kept separate from the preceding corpse source range.</summary>
    public const string FileName = "mother-brain-escape-text-tiles.png";

    /// <summary>Sixteen 4bpp tiles occupy each full 512-byte page.</summary>
    public const int PageBytes = 512;

    /// <summary>The first text page starts at sprite character 386.</summary>
    public const int FirstCharacter = 386;

    /// <summary>The bytes one four-bit OBJ character holds.</summary>
    internal const int CharacterBytes = 32;
}
