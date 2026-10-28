namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable OBJ characters copied into VRAM for the Zebes escape typewriter.</summary>
public static class EscapeTextArtworkDefinitions
{
    /// <summary>$0900 contiguous bytes, ending after the half-size fifth source page.</summary>
    public const int ByteCount = 0x0900;

    /// <summary>Indexed four-bit sheet kept separate from the preceding corpse source range.</summary>
    public const string FileName = "mother-brain-escape-text-tiles.png";

    /// <summary>$A6:C4D9-$C4F5 describes five text-character pages.</summary>
    public const int PageCount = 5;
    /// <summary>Sixteen SNES 4bpp tiles occupy each full $200-byte transfer page.</summary>
    public const int PageBytes = 0x200;
    /// <summary>$A6:C4D9 places the first text page at OBJ character $182.</summary>
    public const int FirstCharacter = 0x182;
    /// <summary>$A6:C4CB/$C4D2 copies two pages of timer-number characters before the text.</summary>
    public const int TimerPageCount = 2;
    /// <summary>$A6:C4CB starts the two timer-number pages at OBJ character $1E0.</summary>
    public const int TimerFirstCharacter = 0x1e0;
    /// <summary>The bytes one four-bit OBJ character holds.</summary>
    internal const int CharacterBytes = 32;
}
