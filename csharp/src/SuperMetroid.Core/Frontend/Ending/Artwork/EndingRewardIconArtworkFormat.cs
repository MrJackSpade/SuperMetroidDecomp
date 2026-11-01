namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>File identities and dimensions of the reward-jump icon artwork.</summary>
public static class EndingRewardIconArtworkFormat
{
    /// <summary>Editable JSON filename for the full post-credits beam/icon tilemap.</summary>
    public const string MapFileName = "post-credits-icon-map.json";
    /// <summary>Indexed PNG filename for the 128x128 post-credits beam/icon character atlas.</summary>
    public const string CharacterFileName = "post-credits-icon-characters.png";
    /// <summary>Width of the full Mode-7 tilemap in tile indices.</summary>
    public const int MapWidth = 128;
    /// <summary>Height of the full Mode-7 tilemap in tile indices; this map is not a repeated 64-row half-map.</summary>
    public const int MapHeight = 128;
    /// <summary>16384 low-lane map bytes, one eight-bit tile index per entry of the 128x128 map.</summary>
    public const int MapBytes = MapWidth * MapHeight;
    /// <summary>32768 bytes after interleaving the map and character lanes, covering sixteen 2048-byte chunks.</summary>
    public const int ByteCount = MapBytes * 2;
}
