using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="LibraryBackgroundProgramDefinitions"/> members production does not use.</summary>
internal static class LibraryBackgroundProgramDefinitionsAccess
{
    /// <summary>Reads a library-background door transfer from its compiled instruction list.</summary>
    extension(LibraryBackgroundProgramDefinitions)
    {
        /// <inheritdoc cref="LibraryBackgroundSources.Program"/>
        internal static LibraryBackgroundProgram Get(ushort pointer) => LibraryBackgroundSources.Program(pointer);

        /// <summary>Finds a command-E transfer for one door in a compiled program.</summary>
        internal static LibraryBackgroundInstruction GetDoorTransfer(ushort listPointer,
            ushort doorPointer)
        {
            foreach (LibraryBackgroundInstruction instruction in LibraryBackgroundSources.Program(listPointer).Instructions)
            {
                if (instruction.Command == LibraryBackgroundCommand.TransferForDoor &&
                    instruction.Door is { } door && DoorHeaderSourcesConstants.PointerOf(door) == doorPointer)
                    return instruction;
            }
            throw new InvalidDataException(
                $"Library background $8F:{listPointer:X4} has no command-E record " +
                $"for door $83:{doorPointer:X4}.");
        }
    }

    /// <summary>The command-list pointer of a compiled program.</summary>
    extension(LibraryBackgroundProgram self)
    {
        /// <summary>The bank-$8F command-list pointer of this program.</summary>
        internal ushort Pointer => LibraryBackgroundSourcesConstants.PointerOf(self);
    }

    /// <summary>The background-data word of a room state.</summary>
    extension(RoomStateSettings self)
    {
        /// <summary>The word this state stores for its library background: its program's pointer, or zero for none.</summary>
        internal ushort BackgroundDataPointer => self.Background is { } background
            ? LibraryBackgroundSourcesConstants.PointerOf(LibraryBackgroundPrograms.Of(background)) : (ushort)0;
    }
}
