namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sprite draw of a finished, pre-coloured image: one frame of a sheet placed at an origin and mirrored about it. Its
/// colours and each pixel's priority against the backgrounds come from the image (#1392).
/// </summary>
public sealed record ImageSpriteDraw : SpriteDraw
{
    /// <summary>Places a sheet frame.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="image">The sheet frame shown.</param>
    /// <param name="origin">The screen point the frame's origin lands on; each coordinate is a signed 16-bit value.</param>
    /// <param name="mirror">Mirrors the frame about its origin.</param>
    /// <param name="paletteOverride">
    /// The palette row ORed into each pixel's part row in place of the one the image was coloured with, as an enemy's
    /// hit or freeze flash supplies, or null for the image's own colours.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The override is not a sprite palette row.</exception>
    public ImageSpriteDraw(SpriteSource source, SpriteSheetFrame image, Position16<ScreenPixels> origin, SpriteMirror mirror,
        int? paletteOverride = null)
        : base(source, origin)
    {
        if (paletteOverride is < 0 or > SpriteImage.MaximumPaletteRow)
            throw new ArgumentOutOfRangeException(nameof(paletteOverride), paletteOverride, "Sprite palette rows are 0..7.");
        Image = Ensure.NotNull(image);
        Mirror = mirror;
        PaletteOverride = paletteOverride;
    }

    /// <summary>
    /// The palette row ORed into each pixel's part row in place of the one the image was coloured with, or null for the
    /// image's own colours.
    /// </summary>
    public int? PaletteOverride { get; }

    /// <summary>The sheet frame shown.</summary>
    public SpriteSheetFrame Image { get; }

    /// <summary>Mirrors the frame about its origin.</summary>
    public SpriteMirror Mirror { get; }


    /// <summary>
    /// The screen position of the frame rectangle's top-left pixel. Mirroring the frame mirrors its rectangle about the
    /// origin, as mirroring a composed frame mirrors each part's rectangle.
    /// </summary>
    public Offset16<ScreenPixels> Corner
    {
        get
        {
            int originX = unchecked((short)Origin.X), originY = unchecked((short)Origin.Y);
            int x = (Mirror & SpriteMirror.Horizontal) != 0 ? originX + Image.Origin.X - Image.Size.X : originX - Image.Origin.X;
            int y = (Mirror & SpriteMirror.Vertical) != 0 ? originY + Image.Origin.Y - Image.Size.Y : originY - Image.Origin.Y;
            return new(checked((short)x), checked((short)y));
        }
    }

    /// <summary>The image pixel shown at <paramref name="cell"/>, a position inside the frame rectangle on screen.</summary>
    public Rgba32 PixelAt(Position16<ScreenPixels> cell)
    {
        int sourceX = (Mirror & SpriteMirror.Horizontal) != 0 ? Image.Size.X - 1 - cell.X : cell.X;
        int sourceY = (Mirror & SpriteMirror.Vertical) != 0 ? Image.Size.Y - 1 - cell.Y : cell.Y;
        return Image.Sheet.Pixels[(Image.Corner.Y + sourceY) * Image.Sheet.Size.X + Image.Corner.X + sourceX];
    }

    /// <summary>The palette row times sixteen plus colour of the image pixel shown at <paramref name="cell"/>.</summary>
    public int PaletteIndexAt(Position16<ScreenPixels> cell) => Image.Sheet.Inks[SheetIndex(cell)].PaletteIndex;

    /// <summary>The part palette row of the image pixel shown at <paramref name="cell"/>, with its fixed-row flag.</summary>
    public int PartRowAt(Position16<ScreenPixels> cell) => Image.Sheet.Inks[SheetIndex(cell)].PartRow;

    /// <summary>The priority against the backgrounds 0..3 of the image pixel shown at <paramref name="cell"/>.</summary>
    public int PriorityAt(Position16<ScreenPixels> cell) => Image.Sheet.Inks[SheetIndex(cell)].Priority;

    /// <summary>Where on the sheet the pixel shown at <paramref name="cell"/> lies.</summary>
    private int SheetIndex(Position16<ScreenPixels> cell)
    {
        int sourceX = (Mirror & SpriteMirror.Horizontal) != 0 ? Image.Size.X - 1 - cell.X : cell.X;
        int sourceY = (Mirror & SpriteMirror.Vertical) != 0 ? Image.Size.Y - 1 - cell.Y : cell.Y;
        return (Image.Corner.Y + sourceY) * Image.Sheet.Size.X + Image.Corner.X + sourceX;
    }

    /// <summary>Names the draw for debugger watches: source, image and signed origin.</summary>
    public override string ToString() =>
        $"{Source.Kind}#{Source.Instance} {Image.Sheet.Name} at ({unchecked((short)Origin.X)}, {unchecked((short)Origin.Y)})";
}
