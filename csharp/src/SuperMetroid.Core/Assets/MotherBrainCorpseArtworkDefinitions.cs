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
}
