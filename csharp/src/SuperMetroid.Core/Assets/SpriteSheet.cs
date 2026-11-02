namespace SuperMetroid.Core.Assets;

/// <summary>An installed sprite sheet: one pre-coloured image and the named frames cut from it.</summary>
public sealed class SpriteSheet
{
    /// <summary>The frames by name.</summary>
    internal readonly Dictionary<string, SpriteSheetFrame> frames;

    /// <summary>Keeps the frames the import pipeline cut from <paramref name="image"/>.</summary>
    internal SpriteSheet(SpriteImage image, Dictionary<string, SpriteSheetFrame> frames)
    {
        Image = image;
        this.frames = frames;
    }

    /// <summary>The sheet image.</summary>
    public SpriteImage Image { get; }

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public SpriteSheetFrame Frame(string name) =>
        frames.TryGetValue(name, out SpriteSheetFrame? frame) ? frame : throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");
}
