using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Independently decode every pinned native command and compare typed operands.</summary>
    private static void VerifyCompiledLibraryBackgroundPrograms(ISnesAddressSpace bus)
    {
        int instructionCount = 0;
        foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
        {
            ushort cursor = program.Pointer;
            foreach (LibraryBackgroundInstruction instruction in program.Instructions)
            {
                AssertEqual((ushort)instruction.Command, ReadWord(cursor),
                    $"library BG $8F:{program.Pointer:X4} opcode at $8F:{cursor:X4}");
                cursor = unchecked((ushort)(cursor + sizeof(ushort)));
                switch (instruction.Command)
                {
                    case LibraryBackgroundCommand.TransferToVram:
                    case LibraryBackgroundCommand.TransferToVramForKraid:
                        CheckTransfer();
                        break;
                    case LibraryBackgroundCommand.DecompressToWorkRam:
                        AssertEqual(instruction.SourceAddress, ReadLong(cursor),
                            $"library BG $8F:{program.Pointer:X4} compressed source");
                        AssertEqual(instruction.Destination,
                            ReadWord(unchecked((ushort)(cursor + 3))),
                            $"library BG $8F:{program.Pointer:X4} WRAM destination");
                        cursor = unchecked((ushort)(cursor + 5));
                        break;
                    case LibraryBackgroundCommand.TransferForDoor:
                        AssertEqual(instruction.DoorPointer, ReadWord(cursor),
                            $"library BG $8F:{program.Pointer:X4} door selector");
                        cursor = unchecked((ushort)(cursor + sizeof(ushort)));
                        CheckTransfer();
                        break;
                    case LibraryBackgroundCommand.ClearFxTilemap:
                    case LibraryBackgroundCommand.ClearBg2:
                    case LibraryBackgroundCommand.ClearBg2ForKraid:
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Unexpected compiled library BG command {instruction.Command}.");
                }
                instructionCount++;

                void CheckTransfer()
                {
                    AssertEqual(instruction.SourceAddress, ReadLong(cursor),
                        $"library BG $8F:{program.Pointer:X4} transfer source");
                    AssertEqual(instruction.Destination,
                        ReadWord(unchecked((ushort)(cursor + 3))),
                        $"library BG $8F:{program.Pointer:X4} VRAM destination");
                    AssertEqual(instruction.ByteCount,
                        ReadWord(unchecked((ushort)(cursor + 5))),
                        $"library BG $8F:{program.Pointer:X4} transfer size");
                    cursor = unchecked((ushort)(cursor + 7));
                }
            }

            AssertEqual((ushort)LibraryBackgroundCommand.End, ReadWord(cursor),
                $"library BG $8F:{program.Pointer:X4} terminator");
            cursor = unchecked((ushort)(cursor + sizeof(ushort)));
            AssertEqual(program.NativeByteCount, unchecked((ushort)(cursor - program.Pointer)),
                $"library BG $8F:{program.Pointer:X4} native byte span");
        }

        AssertEqual(LibraryBackgroundProgramDefinitions.RetailProgramCount,
            LibraryBackgroundProgramDefinitions.All.Count,
            "compiled library BG program count");
        AssertEqual(193, instructionCount, "compiled library BG instruction count");
        Console.WriteLine($"  Library BG programs: {instructionCount} commands in " +
            $"{LibraryBackgroundProgramDefinitions.All.Count} lists match every native operand.");

        ushort ReadWord(ushort pointer)
        {
            int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
            return unchecked((ushort)(bus.ReadByte(address) |
                (bus.ReadByte(address + 1) << 8)));
        }

        int ReadLong(ushort pointer)
        {
            int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
            return bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8) |
                (bus.ReadByte(address + 2) << 16);
        }
    }
}
