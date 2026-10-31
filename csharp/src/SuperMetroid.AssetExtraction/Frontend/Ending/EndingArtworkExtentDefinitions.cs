namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The byte extents of the installed artwork the ending's floor loads (#1416).</summary>
public static class EndingArtworkExtentDefinitions
{
    /// <summary>The bytes of one complete Mode 7 floor lane, also the full reward character sheet.</summary>
    public const int FloorBytes = 16384;

    /// <summary>The flyaway map bytes kept before the rest of the map is filled with the blank tile.</summary>
    public const int FlyawayMapDataBytes = 768;

    /// <summary>The blank floor tile that fills the flyaway map after its kept bytes.</summary>
    public const int FlyawayBlankTile = 140;
}
