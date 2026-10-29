
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Validates the named library background commands against the native bank $8F byte stream (#1414): each command,
/// source and destination is the one the cartridge's word, address and VRAM word decode to.
/// </summary>
internal static class LibraryBackgroundProgramsVerification
{
    /// <summary>Independently decode every pinned native command and compare typed operands.</summary>
    internal static void VerifyCompiledLibraryBackgroundPrograms(ISnesAddressSpace bus)
    {
        int instructionCount = 0;
        foreach (LibraryBackgroundProgram program in LibraryBackgroundSourceDefinitions.All)
        {
            ushort cursor = program.Pointer;
            foreach (LibraryBackgroundInstruction instruction in program.Instructions)
            {
                AssertEqual(LibraryBackgroundOperandDecoding.Word(instruction.Command), ReadWord(cursor),
                    $"library BG $8F:{program.Pointer:X4} opcode at $8F:{cursor:X4}");
                cursor = unchecked((ushort)(cursor + sizeof(ushort)));
                switch (instruction.Command)
                {
                    case LibraryBackgroundCommand.Transfer:
                    case LibraryBackgroundCommand.TransferForKraid:
                        CheckTransfer();
                        break;
                    case LibraryBackgroundCommand.Decompress:
                        AssertTrue(instruction.Source is LibraryBackgroundSource.CompressedTilemap,
                            $"library BG $8F:{program.Pointer:X4} decompresses a named tilemap");
                        AssertEqual(LibraryBackgroundOperandDecoding.Address(instruction.Source), ReadLong(cursor),
                            $"library BG $8F:{program.Pointer:X4} compressed source {instruction.Source}");
                        AssertEqual(LibraryBackgroundDestination.TilemapStaging, instruction.Destination,
                            $"library BG $8F:{program.Pointer:X4} decompresses into the tilemap staging");
                        AssertEqual(LibraryBackgroundOperandDecoding.TilemapStagingOffset,
                            ReadWord(unchecked((ushort)(cursor + 3))),
                            $"library BG $8F:{program.Pointer:X4} WRAM destination");
                        cursor = unchecked((ushort)(cursor + 5));
                        break;
                    case LibraryBackgroundCommand.TransferForDoor:
                        AssertEqual(instruction.Door is { } door ? DoorDefinitions.PointerOf(door) : (ushort)0, ReadWord(cursor),
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
                    AssertEqual(LibraryBackgroundOperandDecoding.Address(instruction.Source), ReadLong(cursor),
                        $"library BG $8F:{program.Pointer:X4} transfer source {instruction.Source}");
                    AssertEqual(LibraryBackgroundOperands.DestinationWord(instruction.Destination),
                        ReadWord(unchecked((ushort)(cursor + 3))),
                        $"library BG $8F:{program.Pointer:X4} VRAM destination");
                    AssertEqual(instruction.ByteCount,
                        ReadWord(unchecked((ushort)(cursor + 5))),
                        $"library BG $8F:{program.Pointer:X4} transfer size");
                    cursor = unchecked((ushort)(cursor + 7));
                }
            }

            AssertEqual(LibraryBackgroundOperandDecoding.EndCommandWord, ReadWord(cursor),
                $"library BG $8F:{program.Pointer:X4} terminator");
        }

        AssertEqual(LibraryBackgroundProgramDefinitionsConstants.RetailProgramCount,
            LibraryBackgroundSourceDefinitions.All.Count,
            "compiled library BG program count");
        AssertEqual(193, instructionCount, "compiled library BG instruction count");
        Console.WriteLine($"  Library BG programs: {instructionCount} commands in " +
            $"{LibraryBackgroundSourceDefinitions.All.Count} lists match every native operand.");

        ushort ReadWord(ushort pointer)
        {
            int address = LibraryBackgroundOperandDecoding.CommandListBank | pointer;
            return unchecked(LittleEndianBytes.Word(CompiledLibraryBackgroundProgramsRomByte(bus, address), CompiledLibraryBackgroundProgramsRomByte(bus, address + 1)));
        }

        int ReadLong(ushort pointer)
        {
            int address = LibraryBackgroundOperandDecoding.CommandListBank | pointer;
            return (int)new SnesAddress(CompiledLibraryBackgroundProgramsRomByte(bus, address + 2),
                LittleEndianBytes.Word(CompiledLibraryBackgroundProgramsRomByte(bus, address), CompiledLibraryBackgroundProgramsRomByte(bus, address + 1)));
        }
    }

    /// <summary>Reads a native command byte while comparing compiled library background lists.</summary>
    private static byte CompiledLibraryBackgroundProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
