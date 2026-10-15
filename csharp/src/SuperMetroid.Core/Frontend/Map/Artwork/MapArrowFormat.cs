namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Defines the map-arrow document identity and bounded timing limits.</summary>
public static class MapArrowFormat
{
    /// <summary>Supported map-arrow document schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing directional map-arrow visuals.</summary>
    public const string FileName = "map-arrows.json";
    /// <summary>Largest supported number of cyclic animation phases.</summary>
    public const int MaximumPhases = 255;
    /// <summary>Largest supported duration, in update ticks, for one phase.</summary>
    public const int MaximumDuration = 254;
}
