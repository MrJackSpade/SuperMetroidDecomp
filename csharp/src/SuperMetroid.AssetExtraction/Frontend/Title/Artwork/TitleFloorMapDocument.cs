using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Serializable title Mode 7 map and native-selected sprite compositions.</summary>
[SpatialComponents("External format: the title Mode 7 map JSON stores its declared grid dimensions as width/height integers.")]
public sealed record TitleFloorMapDocument
{
    /// <summary>Gets the title-graphics document schema version.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the Mode 7 map width in cells.</summary>
    public required int Width { get; init; }
    /// <summary>Gets the Mode 7 map height in cells.</summary>
    public required int Height { get; init; }
    /// <summary>Gets the row-major eight-bit Mode 7 character indexes.</summary>
    public required int[] Tiles { get; init; }
    /// <summary>Gets every native-selected title OBJ frame.</summary>
    public required TitleSpriteFrame[] Sprites { get; init; }
}
