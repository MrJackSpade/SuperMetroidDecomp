using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One fixed transfer/control command. A transfer places its words from <see cref="FirstWord"/>; a decompression
/// stages its stream at <see cref="StagingOffset"/>; unused operands are zero.
/// </summary>
/// <param name="Command">Bank-$82 dispatcher operation. Program arrays contain nonterminating commands; End is represented by the array's end instead.</param>
/// <param name="SourceAddress">Full 24-bit source bus identity for transfers or compressed artwork; transfers may source live WRAM. Zero for operand-free clear commands.</param>
/// <param name="FirstWord">The first word a transfer's words are placed at, as the background artwork bindings place them, 0 through 65535; zero for other commands.</param>
/// <param name="StagingOffset">The byte offset within bank $7E a decompression stages its stream at, 0 through 65535; zero for other commands.</param>
/// <param name="ByteCount">Transfer payload length in bytes, required to be nonzero by the execution consumer; zero for decompression and clear operations whose sizes are determined elsewhere. A count, not a native word: every producer supplies a sixteen-bit operand (at most 4096 in retail), nothing modifies it, and it is only compared and used as a span length.</param>
/// <param name="Door">The door compared with the current door for TransferForDoor, or null for unconditional commands.</param>
public readonly record struct LibraryBackgroundInstruction(
    LibraryBackgroundCommand Command,
    int SourceAddress,
    int FirstWord,
    int StagingOffset,
    int ByteCount,
    CartridgeDoorHeader? Door)
{
    /// <summary>The first word a transfer's words are placed at, 0 through 65535.</summary>
    public int FirstWord { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FirstWord);

    /// <summary>The byte offset within bank $7E a decompression stages its stream at, 0 through 65535.</summary>
    public int StagingOffset { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, StagingOffset);
}
