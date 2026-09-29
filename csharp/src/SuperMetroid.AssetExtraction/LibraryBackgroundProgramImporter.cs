using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Import-only decoding of bank-$8F background lists. Gameplay executes compiled
/// instructions and never interprets cartridge commands or decompresses ROM data.
/// </summary>
public static class LibraryBackgroundProgramImporter
{
    public static LibraryBackgroundProgram Read(ISnesAddressSpace source, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(source);
        if ((short)pointer >= 0) throw new ArgumentOutOfRangeException(nameof(pointer));
        ushort cursor = pointer;
        var instructions = new List<LibraryBackgroundInstruction>();
        for (int guard = 0; guard < RoomAssetRomData.LibraryBackground.MaximumCommandsPerList; guard++)
        {
            ushort commandPointer = cursor;
            LibraryBackgroundCommand command = (LibraryBackgroundCommand)Word(cursor);
            cursor = (ushort)(cursor + 2);
            if (command == LibraryBackgroundCommand.End)
                return new LibraryBackgroundProgram(pointer, instructions.ToArray(), (ushort)(cursor - pointer));
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
                    instructions.Add(new(command, Long(cursor), Word((ushort)(cursor + 3)),
                        Word((ushort)(cursor + 5)), door));
                    cursor = (ushort)(cursor + 7);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    instructions.Add(new(command, Long(cursor), Word((ushort)(cursor + 3)), 0, 0));
                    cursor = (ushort)(cursor + 5);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    instructions.Add(new(command, 0, 0, 0, 0));
                    break;
                default:
                    throw new InvalidDataException($"Unknown import background command ${(ushort)command:X4} at $8F:{commandPointer:X4}.");
            }
        }
        throw new InvalidDataException($"Import background list $8F:{pointer:X4} did not terminate.");

        byte Byte(ushort offset) => ReadImportByte(source, RoomAssetRomData.LibraryBackground.CommandBank | offset);
        ushort Word(ushort offset) => (ushort)(Byte(offset) | Byte((ushort)(offset + 1)) << 8);
        int Long(ushort offset) => Byte(offset) | Byte((ushort)(offset + 1)) << 8 | Byte((ushort)(offset + 2)) << 16;
    }

    /// <summary>
    /// Reference VRAM/WRAM image for an explicitly imported list, independent of the
    /// installed artwork executor. Used by cartridge-versus-asset diagnostics only.
    /// </summary>
    public static LibraryBackgroundExecutionResult ExecuteReference(ISnesAddressSpace source,
        SnesVram vram, ushort pointer, ushort activeDoorPointer)
    {
        LibraryBackgroundProgram program = Read(source, pointer);
        ushort? characterBase = null;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferForDoor when instruction.DoorPointer != activeDoorPointer:
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                case LibraryBackgroundCommand.TransferToVram:
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    if (instruction.ByteCount == 0) throw new InvalidDataException("Import background DMA has a zero size.");
                    var bytes = new byte[instruction.ByteCount];
                    SnesAddress start = SnesAddress.FromBusAddress(instruction.SourceAddress);
                    for (int i = 0; i < bytes.Length; i++) bytes[i] = ReadImportByte(source, (int)start.AddWithinBank(i));
                    vram.ExecuteQueuedAssetWrite(bytes, instruction.Destination);
                    if (instruction.Command == LibraryBackgroundCommand.TransferToVramForKraid)
                        characterBase = RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord;
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    byte[] expanded = RomDataReader.Decompress(CartridgeImportSource.Require(source), instruction.SourceAddress);
                    if (instruction.Destination + expanded.Length > RoomAssetRomData.LibraryBackground.BankByteCount)
                        throw new InvalidDataException("Imported background decompression crosses bank $7E.");
                    for (int i = 0; i < expanded.Length; i++)
                        source.WriteByte(RoomAssetRomData.LibraryBackground.WorkRamBank | (instruction.Destination + i), expanded[i]);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    ClearAndTransfer(RoomAssetRomData.LibraryBackground.ClearFx, false);
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearAndTransfer(RoomAssetRomData.LibraryBackground.ClearBg2,
                        instruction.Command == LibraryBackgroundCommand.ClearBg2ForKraid);
                    break;
                default:
                    throw new InvalidDataException($"Unsupported imported background command {instruction.Command}.");
            }
        }
        return new(program.Instructions.Count + 1, characterBase);

        void ClearAndTransfer(RoomAssetRomData.TilemapTransfer transfer, bool includeKraidPage)
        {
            for (int i = 0; i < transfer.ByteCount; i += 2)
            {
                int address = RoomAssetRomData.LibraryBackground.WorkRamBank | (transfer.WorkRamDestination + i);
                source.WriteByte(address, (byte)transfer.FillValue);
                source.WriteByte(address + 1, (byte)(transfer.FillValue >> 8));
            }
            ISnesMutableMemory memory = source as ISnesMutableMemory ?? throw new InvalidOperationException("Import staging requires WRAM.");
            if (includeKraidPage) vram.ExecuteQueuedMemoryWrite(memory, transfer.WorkRamSourceAddress,
                transfer.ByteCount, RoomAssetRomData.LibraryBackground.KraidBg2VramDestinationWord);
            vram.ExecuteQueuedMemoryWrite(memory, transfer.WorkRamSourceAddress, transfer.ByteCount, transfer.VramDestinationWord);
        }
    }

    private static byte ReadImportByte(ISnesAddressSpace source, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.Cartridge => CartridgeImportSource.Require(source).ReadCartridgeByte(address),
            SnesDmaSourceKind.WorkRam => (source as ISnesMutableMemory ?? throw new InvalidOperationException("Import list requires WRAM.")).ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => (source as ISnesMutableMemory ?? throw new InvalidOperationException("Import list requires SRAM.")).ReadSaveRamByte(address),
            _ => throw new InvalidDataException($"Import background byte ${address:X6} is unmapped."),
        };
}
