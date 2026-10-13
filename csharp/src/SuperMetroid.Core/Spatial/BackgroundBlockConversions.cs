namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The bank-$80 pixel-to-block conversions of <c>$80:A4BB</c>: PPU scroll mirrors become
/// circular-tilemap blocks, and layer positions become source level/background-data blocks.
/// </summary>
public static class BackgroundBlockConversions
{
    /// <summary>A BG1 scroll register as tilemap blocks: four logical shifts of each unsigned word.</summary>
    public static Position16<TilemapBlocks> ToTilemapBlocks(this Position16<RoomPixels> scroll) =>
        new((ushort)(scroll.X >> 4), (ushort)(scroll.Y >> 4));

    /// <summary>A BG2 scroll register as tilemap blocks: four logical shifts of each unsigned word.</summary>
    public static Position16<TilemapBlocks> ToTilemapBlocks(this Position16<Bg2Pixels> scroll) =>
        new((ushort)(scroll.X >> 4), (ushort)(scroll.Y >> 4));

    /// <summary>
    /// A layer-one position as source level-data blocks. The native LSR/BIT #$0800/ORA #$F000
    /// sequence sign-extends the twelve-bit result, which an arithmetic shift expresses.
    /// </summary>
    public static Position16<RoomBlocks> ToSourceBlocks(this Position16<RoomPixels> layer) =>
        new(SignExtendedBlock(layer.X), SignExtendedBlock(layer.Y));

    /// <summary>
    /// A layer-two position as source background-data blocks, with the same sign-extended
    /// shift as <see cref="ToSourceBlocks(Position16{RoomPixels})"/>. The room's background
    /// data shares the level data's block grid.
    /// </summary>
    public static Position16<RoomBlocks> ToSourceBlocks(this Position16<Bg2Pixels> layer) =>
        new(SignExtendedBlock(layer.X), SignExtendedBlock(layer.Y));

    /// <summary>
    /// Arithmetic-shifts the signed pixel word by one block width, preserving the native
    /// sign extension when a layer position crosses the twelve-bit source boundary.
    /// </summary>
    private static ushort SignExtendedBlock(ushort position) => unchecked((ushort)((short)position >> 4));
}
