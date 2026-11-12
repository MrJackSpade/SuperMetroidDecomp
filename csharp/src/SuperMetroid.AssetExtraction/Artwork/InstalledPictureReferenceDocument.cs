using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where the installation's picture store holds one frame's picture (#1635), and the maps the frame shows it with (#1644):
/// its tag, the page and the top-left pixel of a rectangle the frame's size, and the tags of its recolour-region and blend
/// maps where it has them. Every frame showing the same pixels names the same tag and rectangle, so an edit to the picture
/// changes it wherever it shows; frames showing it with different maps name different maps.
/// </summary>
public sealed record InstalledPictureReferenceDocument
{
    /// <summary>
    /// The picture's tag, unique in the installation (#1639): <c>P</c>, its page, a hyphen and its place in the page's write
    /// order, such as <c>P3-0412</c>. The store's provenance lists every tag with the picture's place, and a developer names a
    /// picture by it.
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>The page holding the picture, counted from zero (<see cref="Installation.PictureStoreFiles.PageFile"/>).</summary>
    public required int Page { get; init; }

    /// <summary>The top-left pixel of the picture's rectangle on its page.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>
    /// The tag of the recolour-region map the frame shows the picture with (<see cref="RecolourRegionMapFile"/>), such as
    /// <c>R0042</c>; absent where every pixel lies in region zero.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecolourRegionMap { get; init; }

    /// <summary>
    /// The tag of the blend map the frame shows the picture with (<see cref="BlendMapFile"/>), such as <c>B0007</c>; absent
    /// where no pixel blends by a map.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BlendMap { get; init; }
}
