namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Replaceable blocks one X-ray reveal shows: block numbers in the room's tileset block set, shown unmirrored, as every
/// reveal copy lays its blocks out as they are defined. A reveal holds exactly the corners its copy shape copies: one block,
/// a wide pair, a tall pair or a square of four.
/// </summary>
public readonly record struct XrayRevealVisualBlocks
{
    /// <summary>Holds the corners a shape's factory supplies.</summary>
    private XrayRevealVisualBlocks(int topLeft, int? topRight, int? bottomLeft, int? bottomRight)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    /// <summary>The first block, shown by every drawable reveal.</summary>
    public int TopLeft { get; }

    /// <summary>The block to its right, which wide and square copies show; none for the other shapes.</summary>
    public int? TopRight { get; }

    /// <summary>The block below it, which tall and square copies show; none for the other shapes.</summary>
    public int? BottomLeft { get; }

    /// <summary>The diagonal block, which square copies show; none for the other shapes.</summary>
    public int? BottomRight { get; }

    /// <summary>The blocks the reveal shows, in reading order: top left, top right, bottom left, bottom right, as present.</summary>
    public IReadOnlyList<int> InReadingOrder
    {
        get
        {
            var blocks = new List<int>(4) { TopLeft };
            if (TopRight is { } topRight) blocks.Add(topRight);
            if (BottomLeft is { } bottomLeft) blocks.Add(bottomLeft);
            if (BottomRight is { } bottomRight) blocks.Add(bottomRight);
            return blocks;
        }
    }

    /// <summary>The one block a single-block copy shows.</summary>
    public static XrayRevealVisualBlocks One(int block) => new(block, null, null, null);

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    public static XrayRevealVisualBlocks Wide(int left, int right) => new(left, right, null, null);

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    public static XrayRevealVisualBlocks Tall(int top, int bottom) => new(top, null, bottom, null);

    /// <summary>The four blocks a square copy shows.</summary>
    public static XrayRevealVisualBlocks Square(int topLeft, int topRight, int bottomLeft, int bottomRight) =>
        new(topLeft, topRight, bottomLeft, bottomRight);

    /// <summary>How many blocks a drawable copy shape shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> copies no blocks.</exception>
    public static int CountFor(XrayRevealShape shape) => shape switch
    {
        XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar => 1,
        XrayRevealShape.CopyWide or XrayRevealShape.CopyTall => 2,
        XrayRevealShape.CopySquare => 4,
        XrayRevealShape.HorizontalExtension or XrayRevealShape.VerticalExtension =>
            throw new ArgumentOutOfRangeException(nameof(shape), shape, "An extension shape copies no blocks."),
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, $"Undefined {nameof(XrayRevealShape)}."),
    };

    /// <summary>The blocks a drawable copy shape shows, from its blocks in reading order.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="blocks"/> is null.</exception>
    /// <exception cref="InvalidDataException">The count of <paramref name="blocks"/> is not the count <paramref name="shape"/> shows.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> copies no blocks.</exception>
    public static XrayRevealVisualBlocks ForShape(XrayRevealShape shape, IReadOnlyList<int> blocks)
    {
        Ensure.NotNull(blocks);
        if (blocks.Count != CountFor(shape))
            throw new InvalidDataException($"An X-ray {shape} reveal shows {CountFor(shape)} blocks, not {blocks.Count}.");
        return shape switch
        {
            XrayRevealShape.CopyWide => Wide(blocks[0], blocks[1]),
            XrayRevealShape.CopyTall => Tall(blocks[0], blocks[1]),
            XrayRevealShape.CopySquare => Square(blocks[0], blocks[1], blocks[2], blocks[3]),
            XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar => One(blocks[0]),
            XrayRevealShape.HorizontalExtension or XrayRevealShape.VerticalExtension =>
                throw new ArgumentOutOfRangeException(nameof(shape), shape, "An extension shape copies no blocks."),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, $"Undefined {nameof(XrayRevealShape)}."),
        };
    }
}
