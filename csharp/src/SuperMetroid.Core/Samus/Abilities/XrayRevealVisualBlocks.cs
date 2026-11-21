using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Replaceable blocks one X-ray reveal shows: pictures of blocks of the room's tileset, shown unmirrored, as every
/// reveal copy lays its blocks out as they are defined. A reveal is one closed choice of its copy shape (#2560), each
/// holding exactly the blocks it copies: one block, a wide pair, a tall pair or a square of four.
/// </summary>
public abstract record XrayRevealVisualBlocks
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
    private static OneBlock One(RoomCellPicture block) => new(block);

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    private static WidePair Wide(RoomCellPicture left, RoomCellPicture right) => new(left, right);

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    public static XrayRevealVisualBlocks Tall(RoomCellPicture top, RoomCellPicture bottom) => new TallPair(top, bottom);

    /// <summary>The four blocks a square copy shows.</summary>
    private static SquareOfFour Square(RoomCellPicture topLeft, RoomCellPicture topRight, RoomCellPicture bottomLeft, RoomCellPicture bottomRight) =>
        new(topLeft, topRight, bottomLeft, bottomRight);

    /// <summary>How many blocks a drawable copy shape shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> copies no blocks.</exception>
    public static int CountFor(XrayRevealShape shape) => XrayRevealShapes.CopiesBlocks(shape)
        ? XrayRevealShapeDefinitions.Shapes[shape].CopiedBlocks
        : throw new ArgumentOutOfRangeException(nameof(shape), shape, "An extension shape copies no blocks.");

    /// <summary>The blocks a drawable copy shape shows, from its blocks in reading order.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="blocks"/> is null.</exception>
    /// <exception cref="InvalidDataException">The count of <paramref name="blocks"/> is not the count <paramref name="shape"/> shows.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> copies no blocks.</exception>
    public static XrayRevealVisualBlocks ForShape(XrayRevealShape shape, IReadOnlyList<RoomCellPicture> blocks)
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

    /// <summary>The one block a single-block copy shows.</summary>
    /// <param name="Block">The block.</param>
    public sealed record OneBlock(RoomCellPicture Block) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => Block;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [Block];
    }

    /// <summary>The two blocks side by side a wide copy shows.</summary>
    /// <param name="Left">The block at the reveal's place.</param>
    /// <param name="Right">The block to its right.</param>
    public sealed record WidePair(RoomCellPicture Left, RoomCellPicture Right) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => Left;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [Left, Right];
    }

    /// <summary>The two blocks one above the other a tall copy shows.</summary>
    /// <param name="Top">The block at the reveal's place.</param>
    /// <param name="Bottom">The block below it.</param>
    public sealed record TallPair(RoomCellPicture Top, RoomCellPicture Bottom) : XrayRevealVisualBlocks
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
    public sealed record SquareOfFour(RoomCellPicture UpperLeft, RoomCellPicture UpperRight, RoomCellPicture LowerLeft,
        RoomCellPicture LowerRight) : XrayRevealVisualBlocks
    {
        /// <inheritdoc/>
        public override RoomCellPicture TopLeft => UpperLeft;

        /// <inheritdoc/>
        public override IReadOnlyList<RoomCellPicture> InReadingOrder => [UpperLeft, UpperRight, LowerLeft, LowerRight];
    }
}
