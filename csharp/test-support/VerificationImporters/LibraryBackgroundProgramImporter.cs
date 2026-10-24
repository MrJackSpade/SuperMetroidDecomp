using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Import-only decoding of bank-$8F background lists. Gameplay executes compiled
/// instructions and never interprets cartridge commands or decompresses ROM data.
/// </summary>
[CpuBusBoundary("codec: ROM bank-$8F library background command lists and their staged sources → LibraryBackgroundProgram")]
internal static class LibraryBackgroundProgramImporter
{
    /// <summary>Decodes one bounded bank $8F list into typed transfer, decompression, and clear commands.</summary>
    public static LibraryBackgroundProgram Read(ISnesAddressSpace source, ushort pointer) => Decode(source, pointer, out _);

    /// <summary>The native encoded length of the list at <paramref name="pointer"/>, its two-byte terminator included.</summary>
    public static int ByteSpan(ISnesAddressSpace source, ushort pointer)
    {
        Decode(source, pointer, out int byteSpan);
        return byteSpan;
    }

    /// <summary>Decodes one list and measures its encoded length.</summary>
    private static LibraryBackgroundProgram Decode(ISnesAddressSpace source, ushort pointer, out int byteSpan)
    {
        Ensure.NotNull(source);
        if ((short)pointer >= 0) throw new ArgumentOutOfRangeException(nameof(pointer));
        ushort cursor = pointer;
        var instructions = new List<LibraryBackgroundInstruction>();
        for (int guard = 0; guard < RoomAssetRomDataLibraryBackgroundConstants.MaximumCommandsPerList; guard++)
        {
            ushort commandPointer = cursor;
            LibraryBackgroundCommand command = (LibraryBackgroundCommand)Word(cursor);
            cursor = (ushort)(cursor + 2);
            if (command == LibraryBackgroundCommand.End)
            {
                byteSpan = (ushort)(cursor - pointer);
                return new LibraryBackgroundProgram($"$8F:{pointer:X4}", instructions.ToArray());
            }
            ushort door = 0;
            if (command == LibraryBackgroundCommand.TransferForDoor)
            {
                door = Word(cursor); cursor = (ushort)(cursor + 2);
            }
            switch (command)
            {
                case LibraryBackgroundCommand.TransferToVram:
                case LibraryBackgroundCommand.TransferToVramForKraid:
                case LibraryBackgroundCommand.TransferForDoor:
                    instructions.Add(new(command, Long(cursor), Word((ushort)(cursor + 3)), 0,
                        Word((ushort)(cursor + 5)), door == 0 ? null : DoorHeaderSources.Door(door)));
                    cursor = (ushort)(cursor + 7);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    instructions.Add(new(command, Long(cursor), 0, Word((ushort)(cursor + 3)), 0, null));
                    cursor = (ushort)(cursor + 5);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    instructions.Add(new(command, 0, 0, 0, 0, null));
                    break;
                case LibraryBackgroundCommand.End:
                default:
                    throw new InvalidDataException($"Unknown import background command ${(ushort)command:X4} at $8F:{commandPointer:X4}.");
            }
        }
        throw new InvalidDataException($"Import background list $8F:{pointer:X4} did not terminate.");

        byte Byte(ushort offset) => ReadImportByte(source, RoomAssetRomData.LibraryBackground.CommandBank | offset);
        ushort Word(ushort offset) => SnesLittleEndian.Word(Byte(offset), Byte((ushort)(offset + 1)));
        int Long(ushort offset) => (int)new SnesAddress(Byte((ushort)(offset + 2)), SnesLittleEndian.Word(Byte(offset), Byte((ushort)(offset + 1))));
    }

    /// <summary>
    /// Reference VRAM/WRAM image for an explicitly imported list, independent of the
    /// installed artwork executor. Used by cartridge-versus-asset diagnostics only.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    public static LibraryBackgroundExecutionResult ExecuteReference(ISnesAddressSpace source,
        SnesVram vram, ushort pointer, ushort activeDoorPointer)
    {
        LibraryBackgroundProgram program = Read(source, pointer);
        bool selectsKraidHudCharacters = false;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferForDoor when instruction.Door is not { } door || DoorHeaderSourcesConstants.PointerOf(door) != activeDoorPointer:
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                case LibraryBackgroundCommand.TransferToVram:
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    if (instruction.ByteCount == 0) throw new InvalidDataException("Import background DMA has a zero size.");
                    var bytes = new byte[instruction.ByteCount];
                    SnesAddress start = SnesAddress.FromBusAddress(instruction.SourceAddress);
                    for (int i = 0; i < bytes.Length; i++) bytes[i] = ReadImportByte(source, (int)start.AddWithinBank(i));
                    vram.ExecuteQueuedAssetWrite(bytes, checked((ushort)instruction.FirstWord));
                    if (instruction.Command == LibraryBackgroundCommand.TransferToVramForKraid)
                        selectsKraidHudCharacters = true;
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    byte[] expanded = RomDataReader.Decompress(CartridgeImportSource.Require(source), instruction.SourceAddress);
                    if (instruction.StagingOffset + expanded.Length > RoomAssetRomData.LibraryBackground.BankByteCount)
                        throw new InvalidDataException("Imported background decompression crosses bank $7E.");
                    for (int i = 0; i < expanded.Length; i++)
                        source.WriteByte(RoomAssetRomData.LibraryBackground.WorkRamBank | (instruction.StagingOffset + i), expanded[i]);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    ClearAndTransfer(LibraryBackgroundStagingDefinitions.ClearFx, false);
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearAndTransfer(LibraryBackgroundStagingDefinitions.ClearBg2,
                        instruction.Command == LibraryBackgroundCommand.ClearBg2ForKraid);
                    break;
                case LibraryBackgroundCommand.End:
                default:
                    throw new InvalidDataException($"Unsupported imported background command {instruction.Command}.");
            }
        }
        return new(selectsKraidHudCharacters);

        void ClearAndTransfer(LibraryBackgroundClear transfer, bool includeKraidPage)
        {
            for (int i = 0; i < transfer.ByteCount; i += 2)
            {
                int address = RoomAssetRomData.LibraryBackground.WorkRamBank | (transfer.WorkRamDestination + i);
                source.WriteByte(address, SnesLittleEndian.LowByte(transfer.FillValue.Raw));
                source.WriteByte(address + 1, SnesLittleEndian.HighByte(transfer.FillValue.Raw));
            }
            ISnesMutableMemory memory = source as ISnesMutableMemory ?? throw new InvalidOperationException("Import staging requires WRAM.");
            if (includeKraidPage) vram.ExecuteQueuedMemoryWrite(memory, transfer.WorkRamSourceAddress,
                transfer.ByteCount, LibraryBackgroundStagingDefinitions.KraidBg2VramDestinationWord);
            vram.ExecuteQueuedMemoryWrite(memory, transfer.WorkRamSourceAddress, transfer.ByteCount, transfer.VramDestination.Encoded);
        }
    }

    /// <summary>Routes an import source byte through cartridge, work RAM, or save RAM according to DMA mapping.</summary>
    private static byte ReadImportByte(ISnesAddressSpace source, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.Cartridge => CartridgeImportSource.Require(source).ReadCartridgeByte(address),
            SnesDmaSourceKind.WorkRam => (source as ISnesMutableMemory ?? throw new InvalidOperationException("Import list requires WRAM.")).ReadWorkRamByte(address),
            SnesDmaSourceKind.Unmapped => throw new InvalidDataException($"Import background byte ${address:X6} is unmapped."),
            _ => throw new InvalidDataException($"Import background byte ${address:X6} is unmapped."),
        };
}
