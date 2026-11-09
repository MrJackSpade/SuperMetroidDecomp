namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sprite draw of a finished, pre-coloured image: one frame of a sheet placed at an origin and mirrored about it. Its
/// colours come from the image, under the shading its owner names (#1392, #1512), and a gameplay draw sits whole at its
/// frame's depth among the room's layers (#1514).
/// </summary>
public sealed record ImageSpriteDraw : SpriteDraw
{
    /// <summary>Places a sheet frame.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="image">The sheet frame shown.</param>
    /// <param name="origin">The screen point the frame's origin lands on; each coordinate lies from -32768 through 32767.</param>
    /// <param name="mirror">Mirrors the frame about its origin.</param>
    public ImageSpriteDraw(SpriteSource source, SpriteSheetFrame image, Position16<ScreenPixels> origin, SpriteMirror mirror)
        : base(source, origin)
    {
        Image = Ensure.NotNull(image);
        Mirror = mirror;
    }

    /// <summary>
    /// The shades and colour effects the draw's owner shows its picture in (#1512); <see cref="PictureShading.Standard"/>
    /// where the owner shows no effect. Null only until the owner names it: Samus's draws are made before
    /// Samus's colours name her shading at publication, and a frame refuses a draw
    /// that still names none.
    /// </summary>
    public PictureShading? Shading { get; init; }

    /// <summary>
    /// How a rotting corpse's frame shows as its rot has left it (#1531): each pixel row of each character column of the
    /// image draws the image row it names, or nothing where the rot has cleared it. Null for every other draw, which shows
    /// its image as it is.
    /// </summary>
    public CorpseRotPicture? CorpseRot { get; init; }

    /// <summary>The sheet frame shown.</summary>
    public SpriteSheetFrame Image { get; }

    /// <summary>Mirrors the frame about its origin.</summary>
    public SpriteMirror Mirror { get; }

    /// <summary>
    /// Where a gameplay draw sits among the room's layers (#1514): its frame's depth, the whole draw at one depth. Null for a
    /// frame that is not an actor's or is empty; a frontend screen places its draws by the sprite layer holding them instead.
    /// </summary>
    public ActorDepth? Depth => Image.Depth;

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

    /// <summary>Names the draw by its source, image and signed origin.</summary>
    public override string ToString() =>
        $"{Source.Kind}#{Source.Instance} {Image.Sheet.Name} at ({CoordinateAxis.AsSigned(Origin.X)}, {CoordinateAxis.AsSigned(Origin.Y)})";
}
