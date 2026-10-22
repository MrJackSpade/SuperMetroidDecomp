using System.Collections.Immutable;
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Bounded, pinned bank-$A9 corpse-copy geometry and live-WRAM DMA descriptors.</summary>
/// <remarks>No tile pixels or cartridge byte decoder are stored here.</remarks>
internal static class DeadMonsterRottingDefinitions
{
    /// <summary>Dead Torizo's seven asymmetric touch rectangles at $A9:D77C..D7B5.</summary>
    private static readonly EnemyHitboxRectangle[] TorizoHitboxRecords =
    [
        new(Left: -31, Top: 37, Right: -11, Bottom: 43),
        new(Left: 16, Top: 37, Right: 38, Bottom: 43),
        new(Left: -24, Top: 18, Right: -13, Bottom: 36),
        new(Left: 11, Top: 26, Right: 25, Bottom: 36),
        new(Left: -10, Top: -30, Right: 16, Bottom: 24),
        new(Left: -7, Top: -44, Right: 34, Bottom: -31),
        new(Left: 17, Top: -31, Right: 40, Bottom: -7),
    ];

    /// <summary>Publishes Dead Torizo's seven asymmetric corpse touch rectangles in native order.</summary>
    internal static ReadOnlySpan<EnemyHitboxRectangle> TorizoTouchHitboxes => TorizoHitboxRecords;

    /// <summary>
    /// Five tile rows copied by CorpseRottingInit_Sidehopper_Param1_0/2 at $A9:DEC1/DF08.
    /// The native atlas is sixteen tiles wide; each staged silhouette is five tiles wide.
    /// The second silhouette starts after five staged rows, nine atlas tiles to the right.
    /// </summary>
    internal static CorpseGraphicsCopy SidehopperInitialCopy(int variant, int row)
    {
        if ((uint)row >= 5) throw new ArgumentOutOfRangeException(nameof(row));
        int silhouette = variant == 0 ? 0 : 1;
        int leftClip = row == 0 && silhouette == 0 ? 2 : 0;
        int width = row == 0 ? (silhouette == 0 ? 3 : 2) : 5;
        return new((silhouette * 9 + row * 16 + leftClip) * 32,
            (silhouette * 25 + row * 5 + leftClip) * 32, width * 32);
    }

    /// <summary>Planes-0/1 tile-column words used by the $A9:E468/E564 Sidehopper pixel movers.</summary>
    internal static int SidehopperColumnWordOffset(int variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return ((variant == 0 ? 0 : 25) + column) * 16;
    }

    /// <summary>
    /// Missing first-row tiles in the two sidehopper silhouettes: first two columns in
    /// variant zero, last three in the alternate variant. Native movers $A9:E468/E564
    /// and copiers $A9:E4F5/E5F6 skip these columns until pixel row eight.
    /// </summary>
    internal static int SidehopperColumnMinimumY(int variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return (variant == 0 ? column < 2 : column >= 2) ? 8 : 0;
    }

    /// <summary>The rows of characters a corpse's rotation table reaches and the characters in each.</summary>
    internal static CorpseRotationExtent RotationExtent(CorpseRotationTable table) => table switch
        {
            CorpseRotationTable.Torizo => new CorpseRotationExtent(13, 10),
            CorpseRotationTable.Sidehopper => new CorpseRotationExtent(6, 5),
            CorpseRotationTable.Zoomer or CorpseRotationTable.Ripper => new CorpseRotationExtent(3, 3),
            CorpseRotationTable.Skree => new CorpseRotationExtent(5, 2),
            _ => throw new InvalidOperationException($"Undefined CorpseRotationTable {table}."),
        };

    /// <summary>Calculates live-WRAM DMA rows for supported Sidehopper, Zoomer, Ripper, and Skree variant tables.</summary>
    internal static ImmutableArray<DeadMonsterVramTransferDefinition> ForTransferTable(CorpseTransferTable table)
    {
        int Variant(int count) => (uint)table.Variant < count
            ? table.Variant
            : throw new InvalidDataException($"Corpse transfer table {table} has no compiled definition.");
        return table.Species switch
        {
            CorpseRotationTable.Sidehopper => SidehopperRows(Variant(2)),
            CorpseRotationTable.Zoomer => ZoomerRows(Variant(3)),
            CorpseRotationTable.Ripper => RipperRows(Variant(2)),
            CorpseRotationTable.Skree => SkreeRows(Variant(3)),
            CorpseRotationTable.Torizo => throw new InvalidDataException("The Torizo corpse has no VRAM transfer list."),
            _ => throw new InvalidDataException($"Corpse transfer table {table} has no compiled definition."),
        };
    }

    /// <summary>The rows of Sidehopper corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<DeadMonsterVramTransferDefinition> SidehopperRows(int variant) =>
        Rows(5, 0xa0, (ushort)(0x2040 + variant * 0x2e0), (ushort)(0x7000 + variant * 0x90),
            variant == 0 ? (ushort)0x60 : (ushort)0x40, variant == 0 ? (ushort)0x20 : (ushort)0);

    /// <summary>The rows of Zoomer corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<DeadMonsterVramTransferDefinition> ZoomerRows(int variant) =>
        Rows(2, 0x60, (ushort)(0x2940 + variant * 0xc0), (ushort)(0x7530 + variant * 0x30));

    /// <summary>The rows of Ripper corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<DeadMonsterVramTransferDefinition> RipperRows(int variant) =>
        Rows(2, 0x60, (ushort)(0x2b80 + variant * 0xc0), (ushort)(0x7500 + variant * 0xc0));

    /// <summary>The rows of Skree corpse <paramref name="variant"/>: the first begins on the next VRAM tile row, later ones on row zero.</summary>
    private static ImmutableArray<DeadMonsterVramTransferDefinition> SkreeRows(int variant) =>
        Rows(4, 0x40, (ushort)(0x2640 + variant * 0x100), (ushort)(variant == 0 ? 0x7150 : 0x7000 + variant * 0x70));

    /// <summary>
    /// Consecutive live-WRAM tile rows placed in 32-tile VRAM rows, in native descriptor order; sidehoppers clip their
    /// first row to <paramref name="firstSize"/> bytes and shift it by <paramref name="firstDestinationOffset"/>.
    /// </summary>
    private static ImmutableArray<DeadMonsterVramTransferDefinition> Rows(int count, ushort stride, ushort source, ushort destination,
        ushort firstSize = 0, ushort firstDestinationOffset = 0)
    {
        var rows = ImmutableArray.CreateBuilder<DeadMonsterVramTransferDefinition>(count);
        for (int index = 0; index < count; index++)
        {
            rows.Add(new(index == 0 && firstSize != 0 ? firstSize : stride, 0x7e00,
                (ushort)(source + stride * index - (index == 0 ? 0 : firstDestinationOffset * 2)),
                (ushort)(destination + 0x100 * index + (index == 0 ? firstDestinationOffset : 0))));
        }
        return rows.MoveToImmutable();
    }

    /// <summary>$A9:D67C: eight words per sand strip, second strip displaced by$120 staging bytes.</summary>
    internal static ushort SandDestination(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x120 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
    /// <summary>$A9:D69C: matching eight-word source strips are separated by$200 bytes.</summary>
    internal static ushort SandSource(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x200 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
}
