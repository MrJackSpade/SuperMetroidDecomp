namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sprite draw of a finished, pre-coloured image: one frame of a sheet placed at an origin and mirrored about it. Its
/// colours come from the image, under the shading its owner names (#1392, #1512), and a gameplay draw sits whole at its
/// frame's depth among the room's layers (#1514). Every draw a frame shows is one; a draw shown in Samus's colours is
/// made one when the frame publishes, in the shading her colours show then (#2505).
/// </summary>
public sealed partial record ImageSpriteDraw : SpriteDraw
{
    /// <summary>Places a sheet frame.</summary>
    /// <param name="source">The actor or display element shown.</param>
    /// <param name="image">The sheet frame shown.</param>
    /// <param name="origin">Where on the screen the frame's origin lands; left of or above the screen is negative.</param>
    /// <param name="mirror">Mirrors the frame about its origin.</param>
    public ImageSpriteDraw(SpriteSource source, SpriteSheetFrame image, Offset16<ScreenPixels> origin, SpriteMirror mirror)
        : base(source, origin)
    {
        Image = Ensure.NotNull(image);
        Mirror = mirror;
    }

    /// <summary>
    /// The shades and colour effects the draw's owner shows its picture in (#1512); <see cref="PictureShading.Standard"/>
    /// where the owner shows no effect.
    /// </summary>
    public required PictureShading Shading
    {
        get;
        init => field = Ensure.NotNull(value);
    }

    /// <summary>
    /// How a rotting corpse's frame shows as its own rot has left it (#1539): each pixel row of the image draws, across the
    /// whole image, the image row it names, or nothing where the rot has cleared it. Null for every other draw, which shows
    /// its image as it is.
    /// </summary>
    public CorpseRotPicture? CorpseRot { get; init; }

    /// <summary>
    /// Whether the draw shows at half strength over what lies behind it at its depth (#2642): the steady equivalent of a
    /// picture the game shows only every other update, which a display whose refresh does not match the game's would
    /// otherwise show pulsing on and off.
    /// </summary>
    public bool Translucent { get; init; }

    /// <summary>The sheet frame shown.</summary>
    public SpriteSheetFrame Image { get; }

    /// <summary>Mirrors the frame about its origin.</summary>
    public SpriteMirror Mirror { get; }

    /// <summary>
    /// Where a gameplay draw sits among the room's layers (#1514): its frame's depth, the whole draw at one depth, unless its
    /// owner places it at a depth of its own (#2210), as Kraid's parts sit behind the room's front tiles while he rises. Null
    /// for a frame that is not an actor's or is empty; a frontend screen places its draws by the sprite layer holding them
    /// instead.
    /// </summary>
    public ActorDepth? Depth
    {
        get => field ?? Image.Depth;
        init;
    }

    /// <summary>
    /// The screen position of the frame rectangle's top-left pixel. Mirroring the frame mirrors its rectangle about the
    /// origin, as mirroring a composed frame mirrors each part's rectangle.
    /// </summary>
    public Offset16<ScreenPixels> Corner
    {
        get
        {
            int x = (Mirror & SpriteMirror.Horizontal) != SpriteMirror.None ? Origin.X + Image.Origin.X - Image.Size.X : Origin.X - Image.Origin.X;
            int y = (Mirror & SpriteMirror.Vertical) != SpriteMirror.None ? Origin.Y + Image.Origin.Y - Image.Size.Y : Origin.Y - Image.Origin.Y;
            return new(x, y);
        }
    }

    /// <inheritdoc/>
    internal override ImageSpriteDraw Published(PictureShading samusShading) => this;

    /// <summary>Names the draw by its source, image and screen origin.</summary>
    public override string ToString() =>
        $"{Source.Kind} {Image.Sheet.Name} at ({Origin.X}, {Origin.Y})";
}
