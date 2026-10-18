using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Verification access to <see cref="LibraryBackgroundProgramDefinitions"/> members production does not use.</summary>
internal static class LibraryBackgroundProgramDefinitionsAccess
{
    /// <summary>Reads a library-background door transfer from its compiled instruction list.</summary>
    extension(LibraryBackgroundProgramDefinitions)
    {
        /// <summary>Finds a command-E transfer for one door in a compiled program.</summary>
        internal static LibraryBackgroundInstruction GetDoorTransfer(ushort listPointer,
            ushort doorPointer)
        {
            foreach (LibraryBackgroundInstruction instruction in LibraryBackgroundProgramDefinitionsTooling.Get(listPointer).Instructions)
            {
                if (instruction.Command == LibraryBackgroundCommand.TransferForDoor &&
                    instruction.Door is { } door && DoorHeaderSources.PointerOf(door) == doorPointer)
                    return instruction;
            }
            throw new InvalidDataException(
                $"Library background $8F:{listPointer:X4} has no command-E record " +
                $"for door $83:{doorPointer:X4}.");
        }
    }
}
