using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One kind of map the picture store's pictures are shown with, as the store's provenance manifest lists it (#1644): the map
/// file's SHA-256 and every map in it, by its tag, with the size of the picture it is for. A sheet frame naming a map names
/// it by one of these tags.
/// </summary>
public sealed record StoredMapsDocument
{
    /// <summary>The map file's SHA-256.</summary>
    public required string Sha256 { get; init; }

    /// <summary>The width and height of each map, by its tag, in write order.</summary>
    public required Dictionary<string, Extent16<ImagePixels>> Maps { get; init; }
}
