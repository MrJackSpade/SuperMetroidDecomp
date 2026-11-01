namespace SuperMetroid.Core.Assets;

/// <summary>An installed sprite sheet: one pre-coloured image and the named frames cut from it.</summary>
public sealed class SpriteSheet
{
    /// <summary>The frames by name.</summary>
    private readonly Dictionary<string, SpriteSheetFrame> frames;

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

    /// <summary>The names of the sheet's frames.</summary>
    internal IEnumerable<string> FrameNames => frames.Keys;

    /// <summary>Appends the sheet's pixels and frames to a content identity, frames in name order.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.AppendName("sheet", Image.Name);
        content.AppendName("palette", Image.Colours.ToString());
        foreach (BackgroundColourSet set in Image.BackgroundSets)
            content.AppendName("background set", set.ToString());
        content.Append("width", Image.Size.X);
        content.Append("height", Image.Size.Y);
        content.Append("pixels", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.Pixels));
        // Each ink field is identified as its own plane, as the image was installed.
        ReadOnlySpan<SpriteImageInk> inks = Image.Inks;
        content.AppendBytes("palette indices", inks.SelectToArray(ink => ink.PaletteIndex));
        content.Append("standard colours", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.StandardColours));
        content.AppendBytes("priorities", inks.SelectToArray(ink => ink.Priority));
        content.AppendBytes("part groups", inks.SelectToArray(ink => ink.PartGroup.Code));
        foreach ((string name, SpriteSheetFrame frame) in frames.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            content.AppendName("frame", name);
            content.Append("corner x", frame.Corner.X);
            content.Append("corner y", frame.Corner.Y);
            content.Append("width", frame.Size.X);
            content.Append("height", frame.Size.Y);
            content.Append("origin x", frame.Origin.X);
            content.Append("origin y", frame.Origin.Y);
            foreach (ImageColourGroup group in ImageColourGroup.Every)
                content.Append("colour group", frame.ColourGroups.Contains(group) ? 1 : 0);
            foreach (SpriteSheetFramePart part in frame.Parts)
            {
                content.Append("part corner x", part.Image.Corner.X);
                content.Append("part corner y", part.Image.Corner.Y);
                content.Append("part width", part.Image.Size.X);
                content.Append("part height", part.Image.Size.Y);
                content.Append("part offset x", part.Offset.X);
                content.Append("part offset y", part.Offset.Y);
            }
        }
    }
}
