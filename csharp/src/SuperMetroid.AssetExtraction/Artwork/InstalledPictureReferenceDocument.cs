using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where the installation's picture store holds one frame's picture (#1635): its tag, the page and the top-left pixel of a
/// rectangle the frame's size. Every frame showing the same picture names the same tag and rectangle, so an edit to it
/// changes the picture wherever it shows.
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
}
