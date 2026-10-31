namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable sprite characters loaded for the Zebes escape typewriter.</summary>
public static class EscapeTextArtworkDefinitions
{
    /// <summary>2,304 contiguous bytes, ending after the half-size fifth source page.</summary>
    public const int ByteCount = 2304;

    /// <summary>Indexed four-bit sheet kept separate from the preceding corpse source range.</summary>
    public const string FileName = "mother-brain-escape-text-tiles.png";

    /// <summary>The text characters load as five pages.</summary>
    public const int PageCount = 5;
    /// <summary>Sixteen 4bpp tiles occupy each full 512-byte page.</summary>
    public const int PageBytes = 512;
    /// <summary>The first text page starts at sprite character 386.</summary>
    public const int FirstCharacter = 386;
    /// <summary>Two pages of timer-number characters load before the text.</summary>
    public const int TimerPageCount = 2;
    /// <summary>The two timer-number pages start at sprite character 480.</summary>
    public const int TimerFirstCharacter = 480;
    /// <summary>The bytes one four-bit OBJ character holds.</summary>
    internal const int CharacterBytes = 32;
}
