using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One picture the picture store holds, under its tag in the store's provenance manifest (#1639): its page, the top-left
/// pixel of its rectangle there, and the rectangle's size, the box of what the frames showing it show (#1646). Every sheet
/// frame showing the picture names it by the same tag.
/// </summary>
public sealed record StoredPictureDocument
{
    /// <summary>The page holding the picture, counted from zero.</summary>
    public required int Page { get; init; }

    /// <summary>The top-left pixel of the picture's rectangle on its page.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>The size of the picture's rectangle.</summary>
    public required Extent16<ImagePixels> Size { get; init; }
}
