namespace SuperMetroid.Core.Assets;

/// <summary>
/// An installed sprite sheet: one pre-coloured image, the named frames cut from it, and the names of the frames its family
/// draws that show nothing (#1513). The import pipeline decides a frame shows nothing when its composition inks no pixel,
/// so the sheet holds no picture for it and the draw that chooses it draws nothing.
/// </summary>
public sealed class SpriteSheet
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
    public SpriteSheetFrame Frame(string name) =>
        frames.TryGetValue(name, out SpriteSheetFrame? frame) ? frame : throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");

    /// <summary>
    /// Whether the frame named <paramref name="name"/> shows nothing (#1513): the import found it inks no pixel and
    /// installed it without a picture, so the draw that chooses it draws nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet neither pictures the frame nor holds it as one that shows nothing.</exception>
    public bool ShowsNothing(string name)
    {
        if (blanks.Contains(name))
            return true;
        if (frames.ContainsKey(name))
            return false;
        throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");
    }
}
