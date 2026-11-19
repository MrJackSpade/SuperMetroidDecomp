using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Serializable title Mode 7 map and native-selected sprite compositions.</summary>
[SpatialComponents("External format: the title Mode 7 map JSON stores its declared grid dimensions as width/height integers.")]
[SuppressMessage("Design", "SME6463", Justification = "Import data: the title floor's packed tile and sprite placements, which only the import reads to compose the title's pictures; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
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
    /// <summary>Gets every native-selected whole title OBJ frame: the logo and the copyright.</summary>
    public required TitleSpriteFrame[] Sprites { get; init; }
    /// <summary>Gets every native-selected title card frame, each its card's text typed so far (#1979).</summary>
    public required TitleCardFrame[] Cards { get; init; }
}
