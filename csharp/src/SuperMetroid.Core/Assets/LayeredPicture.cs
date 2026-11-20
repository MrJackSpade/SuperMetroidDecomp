namespace SuperMetroid.Core.Assets;

/// <summary>
/// A picture of a room's tiles as its two installed parts (#1634, <see cref="PictureParts"/>): the pixels drawn behind the
/// actors that pass in front of the room's back tiles, and the pixels drawn in front of them. The parts lie over one
/// another, the same size, and share no pixel. The picture is a closed choice (#2554): one that shows nothing
/// (<see cref="BlankPicture"/>), or one that shows something (<see cref="ShownLayeredPicture"/>) through its part behind,
/// its part in front or both, so a shown picture always has a size.
/// </summary>
public abstract record LayeredPicture
{
    private protected LayeredPicture()
    {
    }

    /// <summary>The picture that shows nothing.</summary>
    public static LayeredPicture Nothing { get; } = new BlankPicture();

    /// <summary>The part drawn with the layer's back tiles, or null where nothing of the picture lies behind.</summary>
    public abstract SpriteSheetFrame? Behind { get; }

    /// <summary>The part drawn with the layer's front tiles, or null where nothing of the picture lies in front.</summary>
    public abstract SpriteSheetFrame? InFront { get; }

    /// <summary>The installed parts, behind first; none for a picture that shows nothing.</summary>
    public IEnumerable<SpriteSheetFrame> Parts
    {
        get
        {
            if (Behind is { } behind)
                yield return behind;
            if (InFront is { } inFront)
                yield return inFront;
        }
    }

    /// <summary>
    /// The picture whose installed parts are <paramref name="behind"/> and <paramref name="inFront"/>, each none where that
    /// part shows nothing: the blank picture when both are none.
    /// </summary>
    /// <exception cref="InvalidDataException">The two parts differ in size.</exception>
    public static LayeredPicture Of(SpriteSheetFrame? behind, SpriteSheetFrame? inFront) => (behind, inFront) switch
    {
        ({ } back, { } front) => new TwoPartPicture(back, front),
        ({ } back, null) => new BehindPicture(back),
        (null, { } front) => new InFrontPicture(front),
        (null, null) => Nothing,
    };
}

/// <summary>A picture that shows nothing: neither part is installed.</summary>
public sealed record BlankPicture : LayeredPicture
{
    /// <inheritdoc/>
    public override SpriteSheetFrame? Behind => null;

    /// <inheritdoc/>
    public override SpriteSheetFrame? InFront => null;
}

/// <summary>A picture that shows something: at least one of its parts is installed, so it has a size.</summary>
public abstract record ShownLayeredPicture : LayeredPicture
{
    private protected ShownLayeredPicture()
    {
    }

    /// <summary>The parts' width and height.</summary>
    public abstract Extent16<ImagePixels> Size { get; }

    /// <summary>The part behind where it is installed, otherwise the part in front.</summary>
    public abstract SpriteSheetFrame FirstPart { get; }
}

/// <summary>A picture whose pixels all lie behind.</summary>
/// <param name="Part">The part drawn with the layer's back tiles.</param>
public sealed record BehindPicture(SpriteSheetFrame Part) : ShownLayeredPicture
{
    /// <inheritdoc/>
    public override SpriteSheetFrame? Behind => Part;

    /// <inheritdoc/>
    public override SpriteSheetFrame? InFront => null;

    /// <inheritdoc/>
    public override Extent16<ImagePixels> Size => Part.Size;

    /// <inheritdoc/>
    public override SpriteSheetFrame FirstPart => Part;
}

/// <summary>A picture whose pixels all lie in front.</summary>
/// <param name="Part">The part drawn with the layer's front tiles.</param>
public sealed record InFrontPicture(SpriteSheetFrame Part) : ShownLayeredPicture
{
    /// <inheritdoc/>
    public override SpriteSheetFrame? Behind => null;

    /// <inheritdoc/>
    public override SpriteSheetFrame? InFront => Part;

    /// <inheritdoc/>
    public override Extent16<ImagePixels> Size => Part.Size;

    /// <inheritdoc/>
    public override SpriteSheetFrame FirstPart => Part;
}

/// <summary>A picture with pixels both behind and in front.</summary>
public sealed record TwoPartPicture : ShownLayeredPicture
{
    /// <summary>Checks that the two parts are the same size.</summary>
    /// <param name="behindPart">The part drawn with the layer's back tiles.</param>
    /// <param name="inFrontPart">The part drawn with the layer's front tiles.</param>
    /// <exception cref="InvalidDataException">The two parts differ in size.</exception>
    public TwoPartPicture(SpriteSheetFrame behindPart, SpriteSheetFrame inFrontPart)
    {
        BehindPart = Ensure.NotNull(behindPart);
        InFrontPart = Ensure.NotNull(inFrontPart);
        if (behindPart.Size != inFrontPart.Size)
            throw new InvalidDataException(
                $"A picture's behind part is {behindPart.Size.X}x{behindPart.Size.Y} and its in-front part {inFrontPart.Size.X}x{inFrontPart.Size.Y}.");
    }

    /// <summary>The part drawn with the layer's back tiles.</summary>
    public SpriteSheetFrame BehindPart { get; }

    /// <summary>The part drawn with the layer's front tiles.</summary>
    public SpriteSheetFrame InFrontPart { get; }

    /// <inheritdoc/>
    public override SpriteSheetFrame? Behind => BehindPart;

    /// <inheritdoc/>
    public override SpriteSheetFrame? InFront => InFrontPart;

    /// <inheritdoc/>
    public override Extent16<ImagePixels> Size => BehindPart.Size;

    /// <inheritdoc/>
    public override SpriteSheetFrame FirstPart => BehindPart;
}
