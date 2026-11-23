namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="LibraryBackgroundInstruction"/> queries only verification asks (#1466).</summary>
internal static class LibraryBackgroundInstructionAccess
{
    /// <summary>A library background instruction's command and operands as the cartridge's list records them (#2558).</summary>
    extension(LibraryBackgroundInstruction self)
    {
        /// <summary>Whether the instruction copies bytes to video memory.</summary>
        internal bool Transfers => self is LibraryBackgroundInstruction.Copy;

        /// <summary>The cartridge command the instruction is.</summary>
        internal LibraryBackgroundCommand Command => self switch
        {
            LibraryBackgroundInstruction.Transfer => LibraryBackgroundCommand.Transfer,
            LibraryBackgroundInstruction.TransferForKraid => LibraryBackgroundCommand.TransferForKraid,
            LibraryBackgroundInstruction.TransferForDoor => LibraryBackgroundCommand.TransferForDoor,
            LibraryBackgroundInstruction.Decompress => LibraryBackgroundCommand.Decompress,
            LibraryBackgroundInstruction.ClearFxTilemap => LibraryBackgroundCommand.ClearFxTilemap,
            LibraryBackgroundInstruction.ClearBg2 => LibraryBackgroundCommand.ClearBg2,
            LibraryBackgroundInstruction.ClearBg2ForKraid => LibraryBackgroundCommand.ClearBg2ForKraid,
            _ => throw new InvalidOperationException($"Undefined library background instruction {self}."),
        };

        /// <summary>What a copy or decompression reads.</summary>
        internal LibraryBackgroundSource Source => self switch
        {
            LibraryBackgroundInstruction.Copy copy => copy.Source,
            LibraryBackgroundInstruction.Decompress decompress => decompress.Source,
            _ => throw new InvalidOperationException($"{self} reads no source."),
        };

        /// <summary>Where a copy or decompression puts what it reads.</summary>
        internal LibraryBackgroundDestination Destination => self switch
        {
            LibraryBackgroundInstruction.Copy copy => copy.Destination,
            _ => throw new InvalidOperationException($"{self} writes to no destination of its own."),
        };

        /// <summary>A copy's payload length in bytes.</summary>
        internal int ByteCount => self is LibraryBackgroundInstruction.Copy copy ? copy.ByteCount
            : throw new InvalidOperationException($"{self} copies no byte count of its own.");

        /// <summary>The door a door transfer belongs to, or none for every other instruction.</summary>
        internal SuperMetroid.Core.Rooms.Doors.DoorDefinition? Door => self is LibraryBackgroundInstruction.TransferForDoor forDoor ? forDoor.Door : null;
    }
}
