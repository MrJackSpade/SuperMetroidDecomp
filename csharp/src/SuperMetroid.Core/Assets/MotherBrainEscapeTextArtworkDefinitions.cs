namespace SuperMetroid.Core.Assets;

/// <summary>Editable OBJ characters copied into VRAM for the Zebes escape typewriter.</summary>
public static class MotherBrainEscapeTextArtworkDefinitions
{
    /// <summary>$B7:DA00, first source page in the five-entry native text-character transfer.</summary>
    public const int SourceAddress = 0xb7da00;

    /// <summary>$0900 contiguous bytes, ending after the half-size fifth source page.</summary>
    public const int ByteCount = 0x0900;

    /// <summary>Indexed four-bit sheet kept separate from the preceding corpse source range.</summary>
    public const string FileName = "mother-brain-escape-text-tiles.png";

    /// <summary>Five source pages from the native $A6:C4CB-$C4FC transfer list.</summary>
    public static ReadOnlySpan<uint> PageSources =>
        [0xb7da00, 0xb7dc00, 0xb7de00, 0xb7e000, 0xb7e200];

    /// <summary>Four full $200-byte pages followed by the final $100-byte page.</summary>
    public static ReadOnlySpan<ushort> PageByteCounts =>
        [0x0200, 0x0200, 0x0200, 0x0200, 0x0100];

    /// <summary>OBJ VRAM word destinations from the same native transfer list.</summary>
    public static ReadOnlySpan<ushort> PageDestinations =>
        [0x7820, 0x7920, 0x7a20, 0x7b20, 0x7c20];

    /// <summary>Whether a native transfer begins in this installed visual source range.</summary>
    public static bool ContainsSource(uint sourceAddress) =>
        sourceAddress >= SourceAddress && sourceAddress < SourceAddress + ByteCount;
}
