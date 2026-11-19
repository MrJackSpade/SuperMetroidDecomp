namespace SuperMetroid.Core.Assets;

/// <summary>
/// Names the two parts a picture of a room's or a screen's tiles is installed as (#1634): what of it is drawn behind the
/// actors that pass in front of the room's back tiles, under the picture's own name, and what is drawn in front of them,
/// under <see cref="InFront"/>'s. Which tiles lie in front is level design; the import decides it once and installs each
/// part as a picture of its own, so a picture is only pixels and a layer's passes choose pictures, not pixels. A part that
/// shows nothing is not installed at all (#1636): a picture with no in-front part has nothing in front, and one with no
/// part behind has nothing behind. A picture that shows nothing at all is named by the sheet as one that shows nothing,
/// so the draw that chooses it draws nothing, and a picture the sheet neither installs nor names is missing
/// (<see cref="SpriteSheet.HoldsPicture"/>, <see cref="SpriteSheet.Picture"/>).
/// </summary>
public static class PictureParts
{
    /// <summary>The mark that ends the name of a picture's in-front part.</summary>
    private const string InFrontMark = "^front";

    /// <summary>The installed name of the in-front part of the picture named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    [InstalledKey]
    public static string InFront([InstalledKey] string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return name + InFrontMark;
    }
}
