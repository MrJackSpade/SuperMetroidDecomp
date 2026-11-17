namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>Defines the map-arrow document identity.</summary>
public static class MapArrowFormat
{
    /// <summary>Supported map-arrow document schema revision: 2 holds each arrow's anchor alone, its cycle gone (#2359).</summary>
    public const int Version = 2;
    /// <summary>JSON filename containing directional map-arrow visuals.</summary>
    public const string FileName = "map-arrows.json";
}
