using System.Collections.Frozen;

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
    private static XrayRevealVisualBlocks One(int block) => new(block, null, null, null);

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    private static XrayRevealVisualBlocks Wide(int left, int right) => new(left, right, null, null);

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    public static XrayRevealVisualBlocks Tall(int top, int bottom) => new(top, null, bottom, null);

    /// <summary>The four blocks a square copy shows.</summary>
    private static XrayRevealVisualBlocks Square(int topLeft, int topRight, int bottomLeft, int bottomRight) =>
        new(topLeft, topRight, bottomLeft, bottomRight);

    /// <summary>How many blocks a copy shape shows.</summary>
    public static int CountFor(XrayRevealCopyShape shape) => Counts[shape];

    /// <summary>How many blocks each copy shape shows.</summary>
    private static IdentityTable<XrayRevealCopyShape, int> Counts => field ??= new(new Dictionary<XrayRevealCopyShape, int>
    {
        [XrayRevealCopyShape.CopyOne] = 1,
        [XrayRevealCopyShape.CopyBrinstar] = 1,
        [XrayRevealCopyShape.CopyWide] = 2,
        [XrayRevealCopyShape.CopyTall] = 2,
        [XrayRevealCopyShape.CopySquare] = 4,
    });

    /// <summary>The blocks each copy shape shows, from its blocks in reading order.</summary>
    private static IdentityTable<XrayRevealCopyShape, Func<IReadOnlyList<int>, XrayRevealVisualBlocks>> Copies => field ??=
        new(new Dictionary<XrayRevealCopyShape, Func<IReadOnlyList<int>, XrayRevealVisualBlocks>>
        {
            [XrayRevealCopyShape.CopyWide] = static blocks => Wide(blocks[0], blocks[1]),
            [XrayRevealCopyShape.CopyTall] = static blocks => Tall(blocks[0], blocks[1]),
            [XrayRevealCopyShape.CopySquare] = static blocks => Square(blocks[0], blocks[1], blocks[2], blocks[3]),
            [XrayRevealCopyShape.CopyOne] = static blocks => One(blocks[0]),
            [XrayRevealCopyShape.CopyBrinstar] = static blocks => One(blocks[0]),
        });

    /// <summary>The blocks a copy shape shows, from its blocks in reading order.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="blocks"/> is null.</exception>
    /// <exception cref="InvalidDataException">The count of <paramref name="blocks"/> is not the count <paramref name="shape"/> shows.</exception>
    public static XrayRevealVisualBlocks ForShape(XrayRevealCopyShape shape, IReadOnlyList<int> blocks)
    {
        Ensure.NotNull(blocks);
        if (blocks.Count != CountFor(shape))
            throw new InvalidDataException($"An X-ray {shape} reveal shows {CountFor(shape)} blocks, not {blocks.Count}.");
        return Copies[shape](blocks);
    }
}
