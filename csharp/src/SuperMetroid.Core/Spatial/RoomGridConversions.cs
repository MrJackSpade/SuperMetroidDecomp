namespace SuperMetroid.Core.Spatial;

/// <summary>Named conversions between room pixels and the room's block and screen grids.</summary>
[EncodingOwner("room block and screen grid coordinates")]
public static class RoomGridConversions
{
    /// <summary>Blocks per room screen on each axis.</summary>
    public const int BlocksPerScreen = 16;

    /// <summary>The bits of a pixel's offset within its room screen, below the screen index.</summary>
    private const int ScreenPixelBits = 8;

    /// <summary>Pixels along a block's side.</summary>
    private const int BlockPixels = 16;

    /// <summary>Pixels along a half block's side.</summary>
    private const int HalfBlockPixels = 8;

    /// <summary>
    /// The block containing an unsigned room pixel, exactly as the game's collision
    /// paths derive it from each coordinate: the coordinate divided by 16.
    /// </summary>
    public static Position16<RoomBlocks> ToRoomBlock(this Position16<RoomPixels> position) =>
        new((ushort)(position.X >> 4), (ushort)(position.Y >> 4));

    /// <summary>
    /// The room-pixel top-left corner of a block: each axis multiplied by 16, wrapping at
    /// sixteen bits, as the game's block-to-pixel paths do.
    /// </summary>
    public static Position16<RoomPixels> ToRoomPixels(this Position16<RoomBlocks> block) =>
        new(unchecked((ushort)(block.X << 4)), unchecked((ushort)(block.Y << 4)));

    /// <summary>
    /// The room-pixel origin of a screen cell: each axis shifted left eight bits, wrapping at
    /// sixteen bits.
    /// </summary>
    public static Position16<RoomPixels> ToRoomPixels(this Position16<RoomScreens> screen) =>
        new(unchecked((ushort)(screen.X << 8)), unchecked((ushort)(screen.Y << 8)));

    /// <summary>The room-pixel offset of a screen index along one axis: a room screen is 256 pixels.</summary>
    public static int ScreenToPixels(int screen) => screen << 8;

    /// <summary>
    /// The room screen holding an unsigned room pixel coordinate, 0 through 65535: the index of its 256-pixel screen
    /// along that axis. A coordinate that wrapped above or left of the room reads as a screen past its far edge.
    /// </summary>
    public static int PixelToScreen(int pixel) => RequirePixel(pixel) >> ScreenPixelBits;

    /// <summary>The first pixel of the 256-pixel room screen holding an unsigned room pixel coordinate.</summary>
    public static ushort ScreenStart(int pixel) => (ushort)(PixelToScreen(pixel) << ScreenPixelBits);

    /// <summary>An unsigned room pixel coordinate's offset within its 256-pixel room screen, 0 through 255.</summary>
    public static int OffsetWithinScreen(int pixel) => RequirePixel(pixel) - ScreenStart(pixel);

    /// <summary>
    /// A room pixel coordinate's offset within its 16-pixel block, 0 through 15. The coordinate is read as the
    /// sixteen-bit coordinate the game's arithmetic leaves, so a sum that wrapped below zero or past 65535 keeps the
    /// offset of its wrapped coordinate.
    /// </summary>
    public static int OffsetWithinBlock(int pixel) => unchecked((ushort)pixel) % BlockPixels;

    /// <summary>
    /// How far a room pixel coordinate lies before the last pixel of its block, 0 through 15: its offset measured from
    /// the block's far edge, read from the coordinate's wrapped sixteen-bit value as <see cref="OffsetWithinBlock"/> is.
    /// </summary>
    public static int OffsetFromBlockEnd(int pixel) => BlockPixels - 1 - OffsetWithinBlock(pixel);

    /// <summary>The first pixel of the 16-pixel block holding an unsigned room pixel coordinate.</summary>
    public static int BlockStart(int pixel) => RequirePixel(pixel) - OffsetWithinBlock(pixel);

    /// <summary>The last pixel of the 16-pixel block holding an unsigned room pixel coordinate.</summary>
    public static int BlockEnd(int pixel) => BlockStart(pixel) + BlockPixels - 1;

    /// <summary>The first pixel of the eight-pixel half block holding an unsigned room pixel coordinate.</summary>
    public static int HalfBlockStart(int pixel) => RequirePixel(pixel) - pixel % HalfBlockPixels;

    /// <summary>The block column or row, 0 through 4095, holding an unsigned room pixel coordinate along one axis.</summary>
    public static int PixelToBlock(int pixel) => RequirePixel(pixel) / BlockPixels;

    /// <summary>
    /// The whole 16-pixel blocks in a pixel length of 0 through 65535, 0 through 4095. Measured from a block's first pixel to
    /// a span's last pixel, it is how many blocks the span crosses after its first.
    /// </summary>
    public static int WholeBlocksIn(int pixels) => RequirePixel(pixels) / BlockPixels;

    /// <summary>
    /// The room screens a run of 0 through 65535 blocks along one axis reaches, a part screen counting as a whole one: the
    /// screen count of a room that many blocks wide or high.
    /// </summary>
    public static int ScreensCovering(int blocks) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, blocks) + BlocksPerScreen - 1) / BlocksPerScreen;

    /// <summary>
    /// The block boundary nearest an unsigned room pixel coordinate, 0 through 65536: a coordinate in the first half of its
    /// block rounds back to the block's first pixel, one in the second half on to the next block's. The boundary past the
    /// last block lies one beyond the coordinate range, for the caller to wrap.
    /// </summary>
    public static int NearestBlockBoundary(int pixel) => (RequirePixel(pixel) + HalfBlockPixels) / BlockPixels * BlockPixels;

    /// <summary>
    /// The centre of the 16-pixel step, counted from zero, that a signed pixel displacement of -32768 through 32767 lies
    /// in: the displacement rounded toward lower values to a whole number of blocks, plus half a block, so -32760 through
    /// 32760. A displacement of 20 gives 24, one of -8 gives -8 and one of -20 gives -24.
    /// </summary>
    public static int BlockStepCentre(int displacement) =>
        Ensure.BetweenInclusive(short.MinValue, short.MaxValue, displacement)
        - (displacement % BlockPixels + BlockPixels) % BlockPixels + HalfBlockPixels;

    /// <summary>The block row, 0 through 15, that an unsigned room pixel coordinate lies on within its room screen.</summary>
    public static int BlockRowWithinScreen(int pixel) => OffsetWithinScreen(pixel) / BlockPixels;

    /// <summary>A room pixel coordinate checked to be an unsigned sixteen-bit coordinate.</summary>
    private static int RequirePixel(int pixel) => Ensure.BetweenInclusive(0, ushort.MaxValue, pixel);

    /// <summary>A screen-grid size measured in blocks.</summary>
    public static Extent16<RoomBlocks> ToRoomBlocks(this Extent16<RoomScreens> size) =>
        new(checked((ushort)(size.X * BlocksPerScreen)), checked((ushort)(size.Y * BlocksPerScreen)));
}
