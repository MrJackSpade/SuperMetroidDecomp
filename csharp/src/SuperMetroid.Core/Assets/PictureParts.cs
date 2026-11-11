namespace SuperMetroid.Core.Assets;

/// <summary>
/// Names the two parts a picture of a room's or a screen's tiles is installed as (#1634): what of it is drawn behind the
/// actors that pass in front of the room's back tiles, under the picture's own name, and what is drawn in front of them,
/// under <see cref="InFront"/>'s. Which tiles lie in front is level design; the import decides it once and installs each
/// part as a picture of its own, so a picture is only pixels and a layer's passes choose pictures, not pixels. A part that
/// shows nothing is installed as a frame that shows nothing.
/// </summary>
public static class PictureParts
{
    /// <summary>The mark that ends the name of a picture's in-front part.</summary>
    private const string InFrontMark = "^front";

    /// <summary>The installed name of the in-front part of the picture named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    public static string InFront(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return name + InFrontMark;
    }

    /// <summary>
    /// The two parts of the picture named <paramref name="name"/> that <paramref name="sheet"/> installs: each its own
    /// picture, or none where the import found that part shows nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet neither pictures a part nor holds it as one that shows nothing.</exception>
    public static LayeredPicture Of(SpriteSheet sheet, string name)
    {
        Ensure.NotNull(sheet);
        string inFront = InFront(name);
        return new LayeredPicture(sheet.ShowsNothing(name) ? null : sheet.Frame(name), sheet.ShowsNothing(inFront) ? null : sheet.Frame(inFront));
    }
}
