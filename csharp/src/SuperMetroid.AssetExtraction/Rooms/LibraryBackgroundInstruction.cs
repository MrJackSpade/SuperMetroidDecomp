using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One library-background command (#1414): what it reads, where it puts it and how much, named for the room's background
/// rather than the cartridge's operands, which extraction decodes once.
/// </summary>
/// <param name="Command">The operation. A program holds no terminator; its end is the end of its instructions.</param>
/// <param name="Source">What a transfer or decompression reads; <see cref="LibraryBackgroundSource.None"/> for a clear.</param>
/// <param name="Destination">Where a transfer or decompression puts it; <see cref="LibraryBackgroundDestination.None"/> for a clear.</param>
/// <param name="ByteCount">Transfer payload length in bytes, required to be nonzero by the execution consumer; zero for decompression and clear operations whose sizes are determined elsewhere. A count, not a native word: every producer supplies a sixteen-bit operand (at most 4096 in retail), nothing modifies it, and it is only compared and used as a span length.</param>
/// <param name="Door">The door compared with the current door for TransferForDoor, or null for unconditional commands.</param>
public readonly record struct LibraryBackgroundInstruction(
    LibraryBackgroundCommand Command,
    LibraryBackgroundSource Source,
    LibraryBackgroundDestination Destination,
    int ByteCount,
    DoorDefinition? Door)
{
}
