namespace SuperMetroid.Core.Game;

/// <summary>Bounded, pinned bank-$A9 corpse-copy geometry and live-WRAM DMA descriptors.</summary>
/// <remarks>No tile pixels or cartridge byte decoder are stored here.</remarks>
internal static class DeadMonsterRottingDefinitions
{
    /// <summary>CorpseRottingTileRowOffsets.Torizo at $A9:E226; ten tiles per row.</summary>
    private const ushort TorizoRows = 0xe226;
    /// <summary>CorpseRottingTileRowOffsets.Sidehopper at $A9:E240; five tiles per row.</summary>
    private const ushort SidehopperRows = 0xe240;
    /// <summary>CorpseRottingTileRowOffsets.Zoomer at $A9:E24C; three tiles per row.</summary>
    private const ushort ZoomerRows = 0xe24c;
    /// <summary>CorpseRottingTileRowOffsets.Ripper at $A9:E252; three tiles per row.</summary>
    private const ushort RipperRows = 0xe252;
    /// <summary>CorpseRottingTileRowOffsets.Skree at $A9:E258; two tiles per row.</summary>
    private const ushort SkreeRows = 0xe258;
    /// <summary>CorpseRottingVRAMTransferDefinitions_Sidehopper_Param1_0 at $A9:E0E0; two42-byte variant lists.</summary>
    internal const ushort SidehopperTransfers = 0xe0e0;
    /// <summary>CorpseRottingVRAMTransferDefinitions_Zoomer_Param1_0 at $A9:E134; three18-byte variant lists.</summary>
    internal const ushort ZoomerTransfers = 0xe134;
    /// <summary>CorpseRottingVRAMTransferDefinitions_Ripper_Param1_0 at $A9:E16A; two18-byte variant lists.</summary>
    internal const ushort RipperTransfers = 0xe16a;
    /// <summary>CorpseRottingVRAMTransferDefinitions_Skree_Param1_0 at $A9:E18E; three34-byte variant lists.</summary>
    internal const ushort SkreeTransfers = 0xe18e;
    /// <summary>Dead Torizo's seven asymmetric touch rectangles at $A9:D77C..D7B5.</summary>
    private static readonly DeadCorpseTouchHitbox[] TorizoHitboxRecords =
    [
        new(0xffe1, 0x0025, 0xfff5, 0x002b),
        new(0x0010, 0x0025, 0x0026, 0x002b),
        new(0xffe8, 0x0012, 0xfff3, 0x0024),
        new(0x000b, 0x001a, 0x0019, 0x0024),
        new(0xfff6, 0xffe2, 0x0010, 0x0018),
        new(0xfff9, 0xffd4, 0x0022, 0xffe1),
        new(0x0011, 0xffe1, 0x0028, 0xfff9),
    ];

    /// <summary>Seven asymmetric touch rectangles used by Dead Torizo's native collision callback.</summary>
    internal static ReadOnlySpan<DeadCorpseTouchHitbox> TorizoTouchHitboxes => TorizoHitboxRecords;

    /// <summary>
    /// Five tile rows copied by CorpseRottingInit_Sidehopper_Param1_0/2 at $A9:DEC1/DF08.
    /// The native atlas is sixteen tiles wide; each staged silhouette is five tiles wide.
    /// The second silhouette starts after five staged rows, nine atlas tiles to the right.
    /// </summary>
    internal static (int SourceOffset, int DestinationOffset, int Length) SidehopperInitialCopy(ushort variant, int row)
    {
        if ((uint)row >= 5) throw new ArgumentOutOfRangeException(nameof(row));
        int silhouette = variant == 0 ? 0 : 1;
        int leftClip = row == 0 && silhouette == 0 ? 2 : 0;
        int width = row == 0 ? (silhouette == 0 ? 3 : 2) : 5;
        return ((silhouette * 9 + row * 16 + leftClip) * 32,
            (silhouette * 25 + row * 5 + leftClip) * 32, width * 32);
    }

    /// <summary>Planes-0/1 tile-column words used by the $A9:E468/E564 Sidehopper pixel movers.</summary>
    internal static int SidehopperColumnWordOffset(ushort variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return ((variant == 0 ? 0 : 25) + column) * 16;
    }

    /// <summary>
    /// Missing first-row tiles in the two sidehopper silhouettes: first two columns in
    /// variant zero, last three in the alternate variant. Native movers $A9:E468/E564
    /// and copiers $A9:E4F5/E5F6 skip these columns until pixel row eight.
    /// </summary>
    internal static int SidehopperColumnMinimumY(ushort variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return (variant == 0 ? column < 2 : column >= 2) ? 8 : 0;
    }

    /// <summary>Converts a corpse row's pixel Y offset into its tile-row byte offset in staged graphics.</summary>
    /// <param name="table">Native row-offset table pointer identifying the corpse tile width and height.</param>
    /// <param name="yOffset">Pixel offset whose eight-pixel row is being staged.</param>
    /// <returns>Byte offset of that row in the compact corpse tile buffer.</returns>
    /// <exception cref="InvalidDataException">The table has no compiled corpse geometry.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The Y offset selects a row beyond the corpse data.</exception>
    internal static ushort RotationOffset(ushort table, ushort yOffset)
    {
        (int count, int tiles) = table switch
        {
            TorizoRows => (13, 10),
            SidehopperRows => (6, 5),
            ZoomerRows or RipperRows => (3, 3),
            SkreeRows => (5, 2),
            _ => throw new InvalidDataException($"Corpse rotation table $A9:{table:X4} has no compiled definition."),
        };
        int row = yOffset >> 3;
        if (row >= count) throw new ArgumentOutOfRangeException(nameof(yOffset));
        return (ushort)(row * tiles * 32);
    }

    /// <summary>Resolves a native transfer-list pointer to the live-WRAM row descriptors it represents.</summary>
    /// <param name="table">Bank-$A9 pointer to a supported corpse VRAM transfer table.</param>
    /// <returns>A bounded sequence of row transfers with the table's clipping and destination offsets.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a compiled transfer table entry.</exception>
    internal static TransferRows ForTransferTable(ushort table)
    {
        int offset = table - SidehopperTransfers;
        if (offset >= 0 && offset < 84 && offset % 42 == 0)
        {
            int variant = offset / 42;
            return new(5, 0xa0, (ushort)(0x2040 + variant * 0x2e0),
                (ushort)(0x7000 + variant * 0x90), variant == 0 ? (ushort)0x60 : (ushort)0x40,
                variant == 0 ? (ushort)0x20 : (ushort)0);
        }
        offset = table - ZoomerTransfers;
        if (offset >= 0 && offset < 54 && offset % 18 == 0)
        {
            int variant = offset / 18;
            return new(2, 0x60, (ushort)(0x2940 + variant * 0xc0), (ushort)(0x7530 + variant * 0x30));
        }
        offset = table - RipperTransfers;
        if (offset >= 0 && offset < 36 && offset % 18 == 0)
        {
            int variant = offset / 18;
            return new(2, 0x60, (ushort)(0x2b80 + variant * 0xc0), (ushort)(0x7500 + variant * 0xc0));
        }
        offset = table - SkreeTransfers;
        if (offset >= 0 && offset < 102 && offset % 34 == 0)
        {
            int variant = offset / 34;
            // The first corpse begins on the next VRAM tile row; later variants occupy row zero.
            int destination = variant == 0 ? 0x7150 : 0x7000 + variant * 0x70;
            return new(4, 0x40, (ushort)(0x2640 + variant * 0x100), (ushort)destination);
        }
        throw new InvalidDataException($"Corpse transfer table $A9:{table:X4} has no compiled definition.");
    }

    /// <summary>Consecutive live-WRAM tile rows placed in 32-tile VRAM rows; sidehoppers clip their first row.</summary>
    /// <param name="count">Number of transfer rows in the bounded sequence.</param>
    /// <param name="stride">Source bytes between corresponding rows in live WRAM.</param>
    /// <param name="source">Starting offset within the declared source bank.</param>
    /// <param name="destination">Encoded VRAM destination for the first row.</param>
    /// <param name="firstSize">Optional byte count for a clipped first row; zero uses the regular stride.</param>
    /// <param name="firstDestinationOffset">Optional first-row tile displacement used by the clipped sidehopper transfer.</param>
    internal readonly struct TransferRows(int count, ushort stride, ushort source, ushort destination,
        ushort firstSize = 0, ushort firstDestinationOffset = 0) : IReadOnlyList<DeadMonsterVramTransferDefinition>
    {
        /// <summary>Number of row transfers represented by this sequence.</summary>
        public int Count => count;

        /// <summary>Gets the transfer descriptor for one row in the bounded sequence.</summary>
        /// <param name="index">Zero-based row index.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the transfer sequence.</exception>
        public DeadMonsterVramTransferDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                return new(index == 0 && firstSize != 0 ? firstSize : stride, 0x7e00,
                    (ushort)(source + stride * index - (index == 0 ? 0 : firstDestinationOffset * 2)),
                    (ushort)(destination + 0x100 * index + (index == 0 ? firstDestinationOffset : 0)));
            }
        }
        /// <summary>Enumerates each row transfer in source and destination order.</summary>
        public IEnumerator<DeadMonsterVramTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>$A9:D67C: eight words per sand strip, second strip displaced by$120 staging bytes.</summary>
    internal static ushort SandDestination(ushort line) => line < 16
        ? (ushort)((line / 8) * 0x120 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
    /// <summary>$A9:D69C: matching eight-word source strips are separated by$200 bytes.</summary>
    internal static ushort SandSource(ushort line) => line < 16
        ? (ushort)((line / 8) * 0x200 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
}

/// <summary>Native signed rectangle words retained as ushort for asymmetric overlap arithmetic.</summary>
/// <param name="Left">Signed horizontal offset of the rectangle's left edge from the corpse origin.</param>
/// <param name="Top">Signed vertical offset of the rectangle's top edge from the corpse origin.</param>
/// <param name="Right">Signed horizontal offset of the rectangle's right edge from the corpse origin.</param>
/// <param name="Bottom">Signed vertical offset of the rectangle's bottom edge from the corpse origin.</param>
internal readonly record struct DeadCorpseTouchHitbox(ushort Left, ushort Top, ushort Right, ushort Bottom);

/// <summary>One immutable DMA descriptor selecting live corpse WRAM, not immutable artwork.</summary>
/// <param name="SizeInBytes">Number of bytes in the DMA transfer.</param>
/// <param name="SourceBankWord">Native bank word used to form the transfer's source address.</param>
/// <param name="SourceOffset">Offset within the source bank for the transfer data.</param>
/// <param name="EncodedVramDestination">Native encoded VRAM destination word for the transfer.</param>
internal readonly record struct DeadMonsterVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset, ushort EncodedVramDestination)
{
    /// <summary>Combines the source bank and offset into the 24-bit bus address used for the DMA read.</summary>
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;
}
