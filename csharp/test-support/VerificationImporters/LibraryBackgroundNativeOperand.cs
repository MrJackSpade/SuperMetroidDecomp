using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>One source operand in a native library-background list, not a synthesized image.</summary>
internal sealed record LibraryBackgroundNativeOperand(
    ushort ListPointer,
    ushort CommandPointer,
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort? WorkRamDestination,
    ushort? VramDestination,
    ushort? TransferByteCount,
    ushort? DoorPointer = null);
