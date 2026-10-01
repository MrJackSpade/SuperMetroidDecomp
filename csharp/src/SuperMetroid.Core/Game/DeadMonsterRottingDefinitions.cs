namespace SuperMetroid.Core.Game;

/// <summary>Bounded, pinned bank-$A9 corpse-copy geometry and live-WRAM DMA descriptors.</summary>
/// <remarks>No tile pixels or cartridge byte decoder are stored here.</remarks>
internal static class DeadMonsterRottingDefinitions
{
    /// <summary>CorpseRottingTileRowOffsets at $A9:E226..E261 for Torizo, sidehopper, Zoomer, Ripper, and Skree.</summary>
    private static readonly Dictionary<ushort, ushort[]> RotationRows = new()
    {
        [0xe226] = [0x0000, 0x0140, 0x0280, 0x03c0, 0x0500, 0x0640, 0x0780, 0x08c0, 0x0a00, 0x0b40, 0x0c80, 0x0dc0, 0x0f00],
        [0xe240] = [0x0000, 0x00a0, 0x0140, 0x01e0, 0x0280, 0x0320],
        [0xe24c] = [0x0000, 0x0060, 0x00c0],
        [0xe252] = [0x0000, 0x0060, 0x00c0],
        [0xe258] = [0x0000, 0x0040, 0x0080, 0x00c0, 0x0100],
    };

    /// <summary>Ten zero-terminated corpse VRAM tables at $A9:E0E0..E1F3, with $7E staging sources.</summary>
    private static readonly Dictionary<ushort, DeadMonsterVramTransferDefinition[]> Transfers = new()
    {
        [0xe0e0] = [new(0x0060, 0x7e00, 0x2040, 0x7020), new(0x00a0, 0x7e00, 0x20a0, 0x7100), new(0x00a0, 0x7e00, 0x2140, 0x7200), new(0x00a0, 0x7e00, 0x21e0, 0x7300), new(0x00a0, 0x7e00, 0x2280, 0x7400)],
        [0xe10a] = [new(0x0040, 0x7e00, 0x2320, 0x7090), new(0x00a0, 0x7e00, 0x23c0, 0x7190), new(0x00a0, 0x7e00, 0x2460, 0x7290), new(0x00a0, 0x7e00, 0x2500, 0x7390), new(0x00a0, 0x7e00, 0x25a0, 0x7490)],
        [0xe134] = [new(0x0060, 0x7e00, 0x2940, 0x7530), new(0x0060, 0x7e00, 0x29a0, 0x7630)],
        [0xe146] = [new(0x0060, 0x7e00, 0x2a00, 0x7560), new(0x0060, 0x7e00, 0x2a60, 0x7660)],
        [0xe158] = [new(0x0060, 0x7e00, 0x2ac0, 0x7590), new(0x0060, 0x7e00, 0x2b20, 0x7690)],
        [0xe16a] = [new(0x0060, 0x7e00, 0x2b80, 0x7500), new(0x0060, 0x7e00, 0x2be0, 0x7600)],
        [0xe17c] = [new(0x0060, 0x7e00, 0x2c40, 0x75c0), new(0x0060, 0x7e00, 0x2ca0, 0x76c0)],
        [0xe18e] = [new(0x0040, 0x7e00, 0x2640, 0x7150), new(0x0040, 0x7e00, 0x2680, 0x7250), new(0x0040, 0x7e00, 0x26c0, 0x7350), new(0x0040, 0x7e00, 0x2700, 0x7450)],
        [0xe1b0] = [new(0x0040, 0x7e00, 0x2740, 0x7070), new(0x0040, 0x7e00, 0x2780, 0x7170), new(0x0040, 0x7e00, 0x27c0, 0x7270), new(0x0040, 0x7e00, 0x2800, 0x7370)],
        [0xe1d2] = [new(0x0040, 0x7e00, 0x2840, 0x70e0), new(0x0040, 0x7e00, 0x2880, 0x71e0), new(0x0040, 0x7e00, 0x28c0, 0x72e0), new(0x0040, 0x7e00, 0x2900, 0x73e0)],
    };

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

    internal static ReadOnlySpan<DeadCorpseTouchHitbox> TorizoTouchHitboxes => TorizoHitboxRecords;

    /// <summary>Dead Torizo sand-line destination offsets at $A9:D67C..D69B.</summary>
    private static ReadOnlySpan<ushort> SandDestinations => [0x0000, 0x0002, 0x0004, 0x0006, 0x0008, 0x000a, 0x000c, 0x000e, 0x0120, 0x0122, 0x0124, 0x0126, 0x0128, 0x012a, 0x012c, 0x012e];
    /// <summary>Dead Torizo sand-line source offsets at $A9:D69C..D6BB.</summary>
    private static ReadOnlySpan<ushort> SandSources => [0x0000, 0x0002, 0x0004, 0x0006, 0x0008, 0x000a, 0x000c, 0x000e, 0x0200, 0x0202, 0x0204, 0x0206, 0x0208, 0x020a, 0x020c, 0x020e];

    internal static ushort RotationOffset(ushort table, ushort yOffset)
    {
        if (!RotationRows.TryGetValue(table, out ushort[]? offsets))
            throw new InvalidDataException($"Corpse rotation table $A9:{table:X4} has no compiled definition.");
        int row = yOffset >> 3;
        if ((uint)row >= offsets.Length) throw new ArgumentOutOfRangeException(nameof(yOffset));
        return offsets[row];
    }

    internal static ReadOnlySpan<DeadMonsterVramTransferDefinition> ForTransferTable(ushort table) =>
        Transfers.TryGetValue(table, out DeadMonsterVramTransferDefinition[]? records)
            ? records : throw new InvalidDataException($"Corpse transfer table $A9:{table:X4} has no compiled definition.");

    /// <summary>Immutable descriptor inventory for source audits; never reads the mutable corpse pixels.</summary>
    internal static IEnumerable<DeadMonsterVramTransferDefinition> AllTransfers =>
        Transfers.Values.SelectMany(records => records);

    internal static ushort SandDestination(ushort line) =>
        line < SandDestinations.Length ? SandDestinations[line] : throw new ArgumentOutOfRangeException(nameof(line));
    internal static ushort SandSource(ushort line) =>
        line < SandSources.Length ? SandSources[line] : throw new ArgumentOutOfRangeException(nameof(line));
}

/// <summary>Native signed rectangle words retained as ushort for asymmetric overlap arithmetic.</summary>
internal readonly record struct DeadCorpseTouchHitbox(ushort Left, ushort Top, ushort Right, ushort Bottom);

/// <summary>One immutable DMA descriptor selecting live corpse WRAM, not immutable artwork.</summary>
internal readonly record struct DeadMonsterVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset, ushort EncodedVramDestination)
{
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;
}
