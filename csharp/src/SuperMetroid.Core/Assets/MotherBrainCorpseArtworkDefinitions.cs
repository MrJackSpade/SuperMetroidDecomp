namespace SuperMetroid.Core.Assets;

/// <summary>Editable source characters for Mother Brain's row-by-row corpse decay.</summary>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>$B7:CE00, the tile-aligned bank source containing the native right-hand corpse frame.</summary>
    public const int SourceAddress = 0xb7ce00;

    /// <summary>$0C00 bytes, covering every initial corpse copy through $B7:D99F.</summary>
    public const int ByteCount = 0x0c00;

    /// <summary>Indexed four-bit tile sheet independent of the ordinary ED7F enemy sheet.</summary>
    public const string FileName = "mother-brain-corpse-tiles.png";

    /// <summary>$A9:9003-$902E copies six $1C0-byte pages, skipping each source page's last two tile rows.</summary>
    public const ushort VramPageByteCount = 0x01c0;

    /// <summary>The six bank-$B7 source pages consumed by Mother Brain's corpse VRAM transfer list.</summary>
    public static ReadOnlySpan<uint> VramPageSources =>
        [0xb7ce00, 0xb7d000, 0xb7d200, 0xb7d400, 0xb7d600, 0xb7d800];

    /// <summary>The corresponding OBJ VRAM word destinations from $A9:9003-$902E.</summary>
    public static ReadOnlySpan<ushort> VramPageDestinations =>
        [0x7a00, 0x7b00, 0x7c00, 0x7d00, 0x7e00, 0x7f00];

    /// <summary>Whether a native transfer begins in this installed corpse source sheet.</summary>
    public static bool ContainsSource(uint sourceAddress) =>
        sourceAddress >= SourceAddress && sourceAddress < SourceAddress + ByteCount;
}
