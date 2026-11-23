using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Replaceable blocks one X-ray reveal shows: pictures of blocks of the room's tileset, shown unmirrored, as every
/// reveal copy lays its blocks out as they are defined. A reveal is one closed choice of its copy shape (#2560), each
/// holding exactly the blocks it copies: one block, a wide pair, a tall pair or a square of four.
/// </summary>
public abstract partial record XrayRevealVisualBlocks
{
    /// <summary>Only the four copy shapes below are reveal blocks.</summary>
    private XrayRevealVisualBlocks()
    {
    }

    /// <summary>The first block, at the reveal's own place, which every drawable reveal shows.</summary>
    public abstract RoomCellPicture TopLeft { get; }

    /// <summary>The blocks the reveal shows, in reading order: top left, top right, bottom left, bottom right, as present.</summary>
    public abstract IReadOnlyList<RoomCellPicture> InReadingOrder { get; }

    /// <summary>The one block a single-block copy shows.</summary>
    internal static OneBlock One(RoomCellPicture block) => new(block);

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    internal static WidePair Wide(RoomCellPicture left, RoomCellPicture right) => new(left, right);

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    public static XrayRevealVisualBlocks Tall(RoomCellPicture top, RoomCellPicture bottom) => new TallPair(top, bottom);

    /// <summary>The four blocks a square copy shows.</summary>
    internal static SquareOfFour Square(RoomCellPicture topLeft, RoomCellPicture topRight, RoomCellPicture bottomLeft, RoomCellPicture bottomRight) =>
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
    private static IdentityTable<XrayRevealCopyShape, Func<IReadOnlyList<RoomCellPicture>, XrayRevealVisualBlocks>> Copies => field ??=
        new(new Dictionary<XrayRevealCopyShape, Func<IReadOnlyList<RoomCellPicture>, XrayRevealVisualBlocks>>
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
    public static XrayRevealVisualBlocks ForShape(XrayRevealCopyShape shape, IReadOnlyList<RoomCellPicture> blocks)
    {
        Ensure.NotNull(blocks);
        if (blocks.Count != CountFor(shape))
            throw new InvalidDataException($"An X-ray {shape} reveal shows {CountFor(shape)} blocks, not {blocks.Count}.");
        return Copies[shape](blocks);
    }

    /// <summary>The one block a single-block copy shows.</summary>
    /// <param name="Block">The block.</param>
    public sealed partial record OneBlock(RoomCellPicture Block) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => Block;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [Block];
    }

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    /// <param name="Left">The block at the reveal's place.</param>
    /// <param name="Right">The block to its right.</param>
    public sealed partial record WidePair(RoomCellPicture Left, RoomCellPicture Right) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => Left;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [Left, Right];
    }

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    /// <param name="Top">The block at the reveal's place.</param>
    /// <param name="Bottom">The block below it.</param>
    public sealed partial record TallPair(RoomCellPicture Top, RoomCellPicture Bottom) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => Top;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [Top, Bottom];
    }

    /// <summary>The four blocks a square copy shows.</summary>
    /// <param name="UpperLeft">The block at the reveal's place.</param>
    /// <param name="UpperRight">The block to its right.</param>
    /// <param name="LowerLeft">The block below it.</param>
    /// <param name="LowerRight">The diagonal block.</param>
    public sealed partial record SquareOfFour(RoomCellPicture UpperLeft, RoomCellPicture UpperRight, RoomCellPicture LowerLeft,
        RoomCellPicture LowerRight) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => UpperLeft;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [UpperLeft, UpperRight, LowerLeft, LowerRight];
    }
}
