namespace SuperMetroid.Core.Assets;

/// <summary>
/// A picture of a room's tiles as its two installed parts (#1634, <see cref="PictureParts"/>): the pixels drawn behind the
/// actors that pass in front of the room's back tiles, and the pixels drawn in front of them. The parts lie over one
/// another, the same size, and share no pixel; either is none where that part shows nothing, and both are none for a
/// picture that shows nothing at all.
/// </summary>
/// <param name="Behind">The part drawn with the layer's back tiles, or null where nothing of the picture lies behind.</param>
/// <param name="InFront">The part drawn with the layer's front tiles, or null where nothing of the picture lies in front.</param>
public sealed record LayeredPicture(SpriteSheetFrame? Behind, SpriteSheetFrame? InFront)
{
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

    /// <summary>The parts' width and height, or null for a picture that shows nothing.</summary>
    /// <exception cref="InvalidDataException">The two parts differ in size.</exception>
    public Extent16<ImagePixels>? Size => (Behind, InFront) switch
    {
        (null, null) => null,
        ({ } behind, null) => behind.Size,
        (null, { } inFront) => inFront.Size,
        ({ } behind, { } inFront) => behind.Size == inFront.Size ? behind.Size
            : throw new InvalidDataException($"A picture's behind part is {behind.Size.X}x{behind.Size.Y} and its in-front part {inFront.Size.X}x{inFront.Size.Y}."),
    };
}
