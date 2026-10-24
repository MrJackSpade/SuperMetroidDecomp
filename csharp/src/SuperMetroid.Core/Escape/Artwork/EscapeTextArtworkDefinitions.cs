namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable OBJ characters copied into VRAM for the Zebes escape typewriter.</summary>
public static class EscapeTextArtworkDefinitions
{
    /// <summary>$B7:DA00, first source page in the five-entry native text-character transfer.</summary>
    public const int SourceAddress = 0xb7da00;

    /// <summary>$0900 contiguous bytes, ending after the half-size fifth source page.</summary>
    public const int ByteCount = 0x0900;

    /// <summary>Indexed four-bit sheet kept separate from the preceding corpse source range.</summary>
    public const string FileName = "mother-brain-escape-text-tiles.png";

    /// <summary>$A6:C4D9-$C4F5 describes five text-character pages.</summary>
    public const int PageCount = 5;
    /// <summary>Sixteen SNES 4bpp tiles occupy each full $200-byte transfer page.</summary>
    public const int PageBytes = 0x200;
    /// <summary>$A6:C4D9 places the first text page at OBJ VRAM word $7820.</summary>
    public const int FirstDestinationWord = 0x7820;
    /// <summary>$A6:C4CB/$C4D2 copies two pages of timer-number characters before the text.</summary>
    public const int TimerPageCount = 2;
    /// <summary>$B0:C000, the first timer-number character source in $A6:C4CB.</summary>
    public const int TimerSourceAddress = 0xb0c000;
    /// <summary>$A6:C4CB/$C4D2 copies $200 then $120 bytes, covering 25 number tiles.</summary>
    public const int TimerByteCount = 0x320;
    /// <summary>$A6:C4CB starts the two timer-number pages at OBJ VRAM word $7E00.</summary>
    public const int TimerFirstDestinationWord = 0x7e00;

    /// <summary>Five source pages from the native $A6:C4CB-$C4FC transfer list.</summary>
    public static uint PageSource(int page) => (uint)page < PageCount
        ? (uint)(SourceAddress + page * PageBytes) : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>Four full $200-byte pages followed by the final $100-byte page.</summary>
    public static ushort PageByteCount(int page) => (uint)page < PageCount
        ? (ushort)Math.Min(PageBytes, ByteCount - page * PageBytes) : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>OBJ VRAM word destinations from the same native transfer list.</summary>
    public static ushort PageDestination(int page) => (uint)page < PageCount
        ? (ushort)(FirstDestinationWord + page * PageBytes / 2) : throw new ArgumentOutOfRangeException(nameof(page));
}
