namespace SuperMetroid.Core.Assets;

/// <summary>
/// An installed sprite sheet: one pre-coloured image, the named frames cut from it, and the names of the frames its family
/// draws that show nothing (#1513). The import pipeline decides a frame shows nothing when its composition inks no pixel,
/// so the sheet holds no picture for it and the draw that chooses it draws nothing.
/// </summary>
public sealed partial class SpriteSheet
{
    /// <summary>The frames by name.</summary>
    internal readonly Dictionary<string, SpriteSheetFrame> frames;

    /// <summary>The names of the frames that show nothing, none of them a frame of <see cref="frames"/>.</summary>
    internal readonly HashSet<string> blanks;

    /// <summary>
    /// Keeps the frames the import pipeline cut from <paramref name="image"/>, and the names of the frames it found show
    /// nothing.
    /// </summary>
    internal SpriteSheet(SpriteImage image, Dictionary<string, SpriteSheetFrame> frames, HashSet<string> blanks)
    {
        Image = image;
        this.frames = frames;
        this.blanks = blanks;
    }

    /// <summary>The sheet image.</summary>
    public SpriteImage Image { get; }

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public SpriteSheetFrame Frame([InstalledKey] string name) =>
        frames.TryGetValue(name, out SpriteSheetFrame? frame) ? frame : throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");

    /// <summary>
    /// Whether the frame named <paramref name="name"/> shows nothing (#1513): the import found it inks no pixel and
    /// installed it without a picture, so the draw that chooses it draws nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet neither pictures the frame nor holds it as one that shows nothing.</exception>
    public bool ShowsNothing([InstalledKey] string name)
    {
        if (blanks.Contains(name))
            return true;
        if (frames.ContainsKey(name))
            return false;
        throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");
    }

    /// <summary>
    /// Whether the sheet holds the picture named <paramref name="name"/>: it installs a part of it
    /// (<see cref="PictureParts"/>), or names it as a picture that shows nothing.
    /// </summary>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    public bool HoldsPicture([InstalledKey] string name)
    {
        string inFront = PictureParts.InFront(name);
        return frames.ContainsKey(name) || frames.ContainsKey(inFront) || blanks.Contains(name);
    }

    /// <summary>
    /// The two parts of the picture named <paramref name="name"/> the sheet installs (<see cref="PictureParts"/>): each its
    /// own picture, or none where the import found that part shows nothing and installed nothing for it.
    /// </summary>
    /// <exception cref="ArgumentException">The name is empty.</exception>
    /// <exception cref="InvalidDataException">
    /// The sheet installs no part of such a picture and does not name it as one that shows nothing, or names it as showing
    /// nothing yet installs its in-front part.
    /// </exception>
    public LayeredPicture Picture([InstalledKey] string name)
    {
        string inFront = PictureParts.InFront(name);
        SpriteSheetFrame? behind = frames.GetValueOrDefault(name);
        SpriteSheetFrame? before = frames.GetValueOrDefault(inFront);
        bool showsNothing = blanks.Contains(name);
        if (showsNothing && before is not null)
            throw new InvalidDataException($"Sprite sheet {Image.Name} names picture {name} as showing nothing but installs its in-front part {inFront}.");
        if (behind is null && before is null && !showsNothing)
            throw new InvalidDataException(
                $"Sprite sheet {Image.Name} has no picture {name}: it installs neither {name} nor {inFront} and does not name it as showing nothing.");
        return new LayeredPicture(behind, before);
    }
}
