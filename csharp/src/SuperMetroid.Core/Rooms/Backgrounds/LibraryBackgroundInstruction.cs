using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One fixed transfer/control command. Destination is a VRAM word for transfers
/// or a work-RAM byte offset for decompression; unused operands are zero.
/// </summary>
/// <param name="Command">Bank-$82 dispatcher operation. Program arrays contain nonterminating commands; End is represented by the array's end instead.</param>
/// <param name="SourceAddress">Full 24-bit source bus identity for transfers or compressed artwork; transfers may source live WRAM. Zero for operand-free clear commands.</param>
/// <param name="Destination">The encoded VRAM DMA destination word for transfers, or the byte offset within bank $7E for decompression, 0 through 65535; zero when unused.</param>
/// <param name="ByteCount">Transfer payload length in bytes, required to be nonzero by the execution consumer; zero for decompression and clear operations whose sizes are determined elsewhere. A count, not a native word: every producer supplies a sixteen-bit operand (at most 4096 in retail), nothing modifies it, and it is only compared and used as a span length.</param>
/// <param name="Door">The door compared with the current door for TransferForDoor, or null for unconditional commands.</param>
public readonly record struct LibraryBackgroundInstruction(
    LibraryBackgroundCommand Command,
    int SourceAddress,
    int Destination,
    int ByteCount,
    CartridgeDoorHeader? Door)
{
    /// <summary>The destination word or offset, 0 through 65535.</summary>
    public int Destination { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Destination);
}
