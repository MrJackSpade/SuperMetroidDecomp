using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where the installation's picture store holds one frame's picture (#1635): the page and the top-left pixel of a rectangle
/// the frame's size. Every frame showing the same picture names the same rectangle, so an edit to it changes the picture
/// wherever it shows.
/// </summary>
public sealed record InstalledPictureReferenceDocument
{
    /// <summary>The page holding the picture, counted from zero (<see cref="Installation.PictureStoreFiles.PageFile"/>).</summary>
    public required int Page { get; init; }

    /// <summary>The top-left pixel of the picture's rectangle on its page.</summary>
    public required Position16<ImagePixels> Corner { get; init; }
}
