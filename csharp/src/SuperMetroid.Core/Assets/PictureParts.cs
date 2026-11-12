namespace SuperMetroid.Core.Assets;

/// <summary>
/// Names the two parts a picture of a room's or a screen's tiles is installed as (#1634): what of it is drawn behind the
/// actors that pass in front of the room's back tiles, under the picture's own name, and what is drawn in front of them,
/// under <see cref="InFront"/>'s. Which tiles lie in front is level design; the import decides it once and installs each
/// part as a picture of its own, so a picture is only pixels and a layer's passes choose pictures, not pixels. A part that
/// shows nothing is not installed at all (#1636): a picture with no in-front part has nothing in front, and one with no
/// part behind has nothing behind. A picture that shows nothing at all is named by the sheet as one that shows nothing,
/// so the draw that chooses it draws nothing, and a picture the sheet neither installs nor names is missing.
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
    /// Whether <paramref name="sheet"/> holds the picture named <paramref name="name"/>: it installs a part of it, or names
    /// it as a picture that shows nothing.
    /// </summary>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    public static bool Holds(SpriteSheet sheet, string name)
    {
        Ensure.NotNull(sheet);
        return sheet.frames.ContainsKey(name) || sheet.frames.ContainsKey(InFront(name)) || sheet.blanks.Contains(name);
    }

    /// <summary>
    /// The two parts of the picture named <paramref name="name"/> that <paramref name="sheet"/> installs: each its own
    /// picture, or none where the import found that part shows nothing and installed nothing for it.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The sheet installs no part of such a picture and does not name it as one that shows nothing, or names it as showing
    /// nothing yet installs its in-front part.
    /// </exception>
    public static LayeredPicture Of(SpriteSheet sheet, string name)
    {
        Ensure.NotNull(sheet);
        string inFront = InFront(name);
        SpriteSheetFrame? behind = sheet.frames.GetValueOrDefault(name);
        SpriteSheetFrame? before = sheet.frames.GetValueOrDefault(inFront);
        bool showsNothing = sheet.blanks.Contains(name);
        if (showsNothing && before is not null)
            throw new InvalidDataException($"Sprite sheet {sheet.Image.Name} names picture {name} as showing nothing but installs its in-front part {inFront}.");
        if (behind is null && before is null && !showsNothing)
            throw new InvalidDataException(
                $"Sprite sheet {sheet.Image.Name} has no picture {name}: it installs neither {name} nor {inFront} and does not name it as showing nothing.");
        return new LayeredPicture(behind, before);
    }
}
