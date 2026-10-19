namespace SuperMetroid.Core.Assets;

/// <summary>
/// One whole-frame sprite entry of a published frame: source X showing frame Y at position P, with flip,
/// palette and priority, read from bound artwork. Backends draw it without OAM or VRAM.
/// </summary>
public sealed record SpriteDraw
{
    /// <summary>Checks that the frame reads only characters the artwork has, so a bad binding fails where it is made.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="frame">The whole frame shown.</param>
    /// <param name="artwork">The artwork the frame's characters index.</param>
    /// <param name="origin">The frame origin in screen pixels; each coordinate is a signed 16-bit value.</param>
    /// <param name="flip">Mirrors the whole frame about its origin.</param>
    /// <param name="palette">OBJ palette 0..7 for parts that do not choose their own.</param>
    /// <param name="priority">Priority against the backgrounds 0..3 for parts that do not choose their own.</param>
    /// <exception cref="ArgumentOutOfRangeException">The palette or priority is out of range.</exception>
    /// <exception cref="ArgumentException">The frame reads a character beyond the artwork.</exception>
    public SpriteDraw(SpriteSource source, SpriteFrame frame, SpriteArtwork artwork, Position16<ScreenPixels> origin,
        SnesTileFlipFlags flip, int palette, int priority)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(artwork);
        if (palette is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(palette), palette, "OBJ palettes are 0..7.");
        if (priority is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(priority), priority, "OBJ priorities are 0..3.");
        if (frame.LastCharacter >= artwork.CharacterCount)
            throw new ArgumentException($"Frame {frame.Name} reads character {frame.LastCharacter}; artwork {artwork.Id} has {artwork.CharacterCount}.", nameof(frame));
        Source = source;
        Frame = frame;
        Artwork = artwork;
        Origin = origin;
        Flip = flip;
        Palette = palette;
        Priority = priority;
    }

    /// <summary>The actor or display element shown.</summary>
    public SpriteSource Source { get; }
    /// <summary>The whole frame shown.</summary>
    public SpriteFrame Frame { get; }
    /// <summary>The artwork the frame's characters index.</summary>
    public SpriteArtwork Artwork { get; }
    /// <summary>The frame origin in screen pixels; each coordinate is a signed 16-bit value.</summary>
    public Position16<ScreenPixels> Origin { get; }
    /// <summary>Mirrors the whole frame about its origin.</summary>
    public SnesTileFlipFlags Flip { get; }
    /// <summary>OBJ palette row 0..7 for parts that do not choose their own; validated by the constructor.</summary>
    public int Palette { get; }
    /// <summary>Priority against the backgrounds 0..3 for parts that do not choose their own; validated by the constructor.</summary>
    public int Priority { get; }

    /// <summary>The same draw over other artwork of the same layout, which must hold every character the frame reads.</summary>
    /// <param name="artwork">The artwork the draw now reads.</param>
    public SpriteDraw WithArtwork(SpriteArtwork artwork) => new(Source, Frame, artwork, Origin, Flip, Palette, Priority);

    /// <summary>Names the draw for debugger watches: source, frame and signed origin.</summary>
    public override string ToString() =>
        $"{Source.Kind}#{Source.Instance} {Frame.Name} at ({unchecked((short)Origin.X)}, {unchecked((short)Origin.Y)})";

    /// <summary>Places one part on the screen: applies the whole-frame flip and the draw's palette and priority.</summary>
    /// <param name="index">The part's index in the frame, frontmost first.</param>
    public SpriteScreenPart Place(int index)
    {
        SpriteFramePart part = Frame.Parts[index];
        bool flipX = (Flip & SnesTileFlipFlags.Horizontal) != 0;
        bool flipY = (Flip & SnesTileFlipFlags.Vertical) != 0;
        // Mirroring the frame mirrors each part's rectangle about the origin and reverses its pixels.
        int x = unchecked((short)Origin.X) + (flipX ? -part.Offset.X - part.Size.X : part.Offset.X);
        int y = unchecked((short)Origin.Y) + (flipY ? -part.Offset.Y - part.Size.Y : part.Offset.Y);
        return new(new Offset16<ScreenPixels>(checked((short)x), checked((short)y)), part.Size, part.Character,
            flipX != ((part.Flip & SnesTileFlipFlags.Horizontal) != 0),
            flipY != ((part.Flip & SnesTileFlipFlags.Vertical) != 0),
            part.Palette ?? Palette, part.Priority ?? Priority);
    }
}
