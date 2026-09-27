namespace SuperMetroid.Core.Game;

/// <summary>One fixed Dead Torizo corpse-transfer descriptor; source bytes remain mutable WRAM.</summary>
internal readonly record struct DeadTorizoVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset,
    ushort EncodedVramDestination)
{
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;
}

/// <summary>
/// The alternating bank-$A9 corpse VRAM queues at $A9:D549 and $A9:D583.
/// These immutable descriptor words select mutable $7E staging bytes; the
/// rotting algorithm, transfer phase, and pixels are not embedded here.
/// </summary>
internal static class DeadTorizoVramTransferDefinitions
{
    /// <summary><c>DeadTorizo_VRAMWriteTable_Even</c> at $A9:D549.</summary>
    internal const ushort EvenTable = 0xd549;
    /// <summary><c>DeadTorizo_VRAMWriteTable_Odd</c> at $A9:D583.</summary>
    internal const ushort OddTable = 0xd583;
    /// <summary>Four 16-bit descriptor words per native record.</summary>
    internal const int RecordByteCount = 8;
    /// <summary>The native interpreter's finite corrupt-table guard.</summary>
    internal const int MaximumNativeRecords = 64;

    private static readonly DeadTorizoVramTransferDefinition[] Even =
    [
        new(0x00c0, 0x7e00, 0x2060, 0x7090),
        new(0x00c0, 0x7e00, 0x21a0, 0x7190),
        new(0x0100, 0x7e00, 0x22c0, 0x7280),
        new(0x0100, 0x7e00, 0x2400, 0x7380),
        new(0x0100, 0x7e00, 0x2540, 0x7480),
        new(0x0100, 0x7e00, 0x2680, 0x7580),
        new(0x0120, 0x7e00, 0x9620, 0x7100),
    ];

    private static readonly DeadTorizoVramTransferDefinition[] Odd =
    [
        new(0x0100, 0x7e00, 0x27c0, 0x7680),
        new(0x0100, 0x7e00, 0x2900, 0x7780),
        new(0x0100, 0x7e00, 0x2a40, 0x7880),
        new(0x0120, 0x7e00, 0x2b60, 0x7970),
        new(0x0140, 0x7e00, 0x2c80, 0x7a60),
        new(0x0140, 0x7e00, 0x2dc0, 0x7b60),
        new(0x0100, 0x7e00, 0x9500, 0x7000),
    ];

    internal static ReadOnlySpan<DeadTorizoVramTransferDefinition> ForPhase(
        ushort phase) => (phase & 1) != 0 ? Odd : Even;
}
