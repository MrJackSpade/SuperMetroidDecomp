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
    /// <param name="colouring">
    /// The colours shown in place of the ones the image was coloured with, as an enemy's hit or freeze flash supplies,
    /// shown over each part group as their placement decides; or null for the image's own colours.
    /// </param>
    public ImageSpriteDraw(SpriteSource source, SpriteSheetFrame image, Position16<ScreenPixels> origin, SpriteMirror mirror,
        DrawnColouring? colouring = null)
        : base(source, origin)
    {
        Image = Ensure.NotNull(image);
        Mirror = mirror;
        Colouring = colouring;
    }

    /// <summary>
    /// The colours shown in place of the ones the image was coloured with, shown over each part group as their placement
    /// decides, or null for the image's own colours (#1464).
    /// </summary>
    public DrawnColouring? Colouring { get; }

    /// <summary>What the draw's pixels show, resolved when its frame is published; null until then.</summary>
    public DrawColours? Colours { get; init; }

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
            int originX = CoordinateAxis.AsSigned(Origin.X), originY = CoordinateAxis.AsSigned(Origin.Y);
            int x = (Mirror & SpriteMirror.Horizontal) != SpriteMirror.None ? originX + Image.Origin.X - Image.Size.X : originX - Image.Origin.X;
            int y = (Mirror & SpriteMirror.Vertical) != SpriteMirror.None ? originY + Image.Origin.Y - Image.Size.Y : originY - Image.Origin.Y;
            return new(x, y);
        }
    }

    /// <summary>Names the draw for debugger watches: source, image and signed origin.</summary>
    public override string ToString() =>
        $"{Source.Kind}#{Source.Instance} {Image.Sheet.Name} at ({CoordinateAxis.AsSigned(Origin.X)}, {CoordinateAxis.AsSigned(Origin.Y)})";
}
