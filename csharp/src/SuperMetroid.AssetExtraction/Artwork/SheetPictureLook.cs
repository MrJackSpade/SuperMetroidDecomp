namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What a draw of a painted picture shows in its sheet's colouring (#1544): its rectangle, origin and depth, each pixel's
/// colour and marks, the colours its pixels show, in the order the picture first shows them, each pixel naming its colour
/// by that order, and what each shade shows of those colours. The palette indices, colour groups and palette bits the
/// picture was composed with are left out: the installed image names its own colours, so two pictures drawn alike from
/// different rows or palette bits look the same.
/// </summary>
internal sealed class SheetPictureLook
{
    /// <summary>Keeps a look's key and the inks behind its colours.</summary>
    internal SheetPictureLook(string key, int[] shownInks)
    {
        Key = key;
        ShownInks = shownInks;
    }

    /// <summary>A hash of everything the look holds. Pictures with one key look the same under every shade.</summary>
    internal string Key { get; }

    /// <summary>
    /// The composed palette index of each colour the picture shows, in the order it first shows them, so the colours of two
    /// pictures with one key correspond place by place.
    /// </summary>
    internal IReadOnlyList<int> ShownInks { get; }
}
