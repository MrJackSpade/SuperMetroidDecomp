namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The byte extents of the installed artwork the ending's floor loads (#1416).</summary>
public static class EndingArtworkExtentDefinitions
{
    /// <summary>The bytes of one complete Mode 7 floor lane, also the full reward character sheet.</summary>
    public const int FloorBytes = 0x4000;

    /// <summary>The bytes of each of the flyaway's sixteen floor chunks.</summary>
    public const int FlyawayChunkBytes = 0x0800;

    /// <summary>The flyaway map bytes kept before the rest of the map is filled with the blank tile.</summary>
    public const int FlyawayMapDataBytes = 0x0300;

    /// <summary>The blank floor tile that fills the flyaway map after its kept bytes.</summary>
    public const byte FlyawayBlankTile = 0x8c;
}
