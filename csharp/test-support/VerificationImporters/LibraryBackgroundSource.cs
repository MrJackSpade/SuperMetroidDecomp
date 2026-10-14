using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>One source operand in a native library-background list, not a synthesized image.</summary>
internal sealed record LibraryBackgroundSource(
    ushort ListPointer,
    ushort CommandPointer,
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort? WorkRamDestination,
    ushort? VramDestination,
    ushort? TransferByteCount,
    ushort? DoorPointer = null);
