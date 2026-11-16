using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Import-only decoding of bank-$8F background lists. Gameplay executes compiled
/// instructions and never interprets cartridge commands or decompresses ROM data.
/// </summary>
/// <remarks>
/// The cartridge stages decompressed and cleared tilemaps in work RAM before transferring them. The reference keeps
/// that staging in its own <see cref="ReferenceWorkMemory"/>, fresh for each list, so the address space it is handed
/// only has to supply cartridge bytes (#1688).
/// </remarks>
internal static class LibraryBackgroundProgramImporter
{
    /// <summary>Decodes one bounded bank $8F list into typed transfer, decompression, and clear commands.</summary>
    public static ImportedLibraryBackgroundList Read(ISnesAddressSpace source, ushort pointer) =>
        Decode(new ReferenceWorkMemory(source), pointer, out _);

    /// <summary>The native encoded length of the list at <paramref name="pointer"/>, its two-byte terminator included.</summary>
    public static int ByteSpan(ISnesAddressSpace source, ushort pointer)
    {
        Decode(new ReferenceWorkMemory(source), pointer, out int byteSpan);
        return byteSpan;
    }

    /// <summary>Decodes one list and measures its encoded length.</summary>
    private static ImportedLibraryBackgroundList Decode(ReferenceWorkMemory source, ushort pointer, out int byteSpan)
    {
        Ensure.NotNull(source);
        if ((short)pointer >= 0) throw new ArgumentOutOfRangeException(nameof(pointer));
        ushort cursor = pointer;
        var instructions = new List<LibraryBackgroundInstruction>();
        for (int guard = 0; guard < LibraryBackgroundListConstants.MaximumCommandsPerList; guard++)
        {
            ushort commandPointer = cursor;
            LibraryBackgroundCommand? decoded = LibraryBackgroundOperandDecoding.Command(Word(cursor));
            cursor = (ushort)(cursor + 2);
            if (decoded is not { } command)
            {
                byteSpan = (ushort)(cursor - pointer);
                return new ImportedLibraryBackgroundList($"$8F:{pointer:X4}", instructions.ToArray());
            }
            ushort door = 0;
            if (command == LibraryBackgroundCommand.TransferForDoor)
            {
                door = Word(cursor); cursor = (ushort)(cursor + 2);
            }
            switch (command)
            {
                case LibraryBackgroundCommand.Transfer:
                case LibraryBackgroundCommand.TransferForKraid:
                case LibraryBackgroundCommand.TransferForDoor:
                    instructions.Add(new(command, LibraryBackgroundOperandDecoding.TransferredSource(Long(cursor)),
                        LibraryBackgroundOperandDecoding.TransferDestination(Word((ushort)(cursor + 3))),
                        Word((ushort)(cursor + 5)), door == 0 ? null : DoorHeaderSources.Door(door)));
                    cursor = (ushort)(cursor + 7);
                    break;
                case LibraryBackgroundCommand.Decompress:
                    instructions.Add(new(command, LibraryBackgroundOperandDecoding.DecompressedSource(Long(cursor)),
                        LibraryBackgroundOperandDecoding.DecompressionDestination(Word((ushort)(cursor + 3))), 0, null));
                    cursor = (ushort)(cursor + 5);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    instructions.Add(new(command, LibraryBackgroundSource.None, LibraryBackgroundDestination.None, 0, null));
                    break;
                default:
                    throw new InvalidDataException($"Unknown import background command {command} at $8F:{commandPointer:X4}.");
            }
        }
        throw new InvalidDataException($"Import background list $8F:{pointer:X4} did not terminate.");

        byte Byte(ushort offset) => ReadImportByte(source, LibraryBackgroundOperandDecoding.CommandListBank | offset);
        ushort Word(ushort offset) => LittleEndianBytes.Word(Byte(offset), Byte((ushort)(offset + 1)));
        int Long(ushort offset) => (int)new SnesAddress(Byte((ushort)(offset + 2)), LittleEndianBytes.Word(Byte(offset), Byte((ushort)(offset + 1))));
    }

    /// <summary>
    /// Reference VRAM/WRAM image for an explicitly imported list, independent of the
    /// installed artwork executor. Used by cartridge-versus-asset diagnostics only.
    /// </summary>
    public static void ExecuteReference(ISnesAddressSpace source,
        SnesVram vram, ushort pointer, ushort activeDoorPointer)
    {
        var staging = new ReferenceWorkMemory(source);
        ImportedLibraryBackgroundList list = Decode(staging, pointer, out _);
        foreach (LibraryBackgroundInstruction instruction in list.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferForDoor when instruction.Door is not { } door || DoorHeaderSourcesConstants.PointerOf(door) != activeDoorPointer:
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                case LibraryBackgroundCommand.Transfer:
                case LibraryBackgroundCommand.TransferForKraid:
                    if (instruction.ByteCount == 0) throw new InvalidDataException("Import background DMA has a zero size.");
                    var bytes = new byte[instruction.ByteCount];
                    SnesAddress start = SnesAddress.FromBusAddress(LibraryBackgroundOperandDecoding.Address(instruction.Source));
                    for (int i = 0; i < bytes.Length; i++) bytes[i] = ReadImportByte(staging, (int)start.AddWithinBank(i));
                    vram.ExecuteQueuedAssetWrite(bytes, LibraryBackgroundOperands.DestinationWord(instruction.Destination));
                    break;
                case LibraryBackgroundCommand.Decompress:
                    byte[] expanded = RomDataReader.Decompress(CartridgeImportSource.Require(source), LibraryBackgroundOperandDecoding.Address(instruction.Source));
                    if (LibraryBackgroundOperandDecoding.TilemapStagingOffset + expanded.Length > LibraryBackgroundListConstants.BankByteCount)
                        throw new InvalidDataException("Imported background decompression crosses bank $7E.");
                    for (int i = 0; i < expanded.Length; i++)
                        staging.WriteByte(LibraryBackgroundOperandDecoding.TilemapStaging + i, expanded[i]);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    ClearAndTransfer(LibraryBackgroundStagingDefinitions.ClearFx, false);
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearAndTransfer(LibraryBackgroundStagingDefinitions.ClearBg2,
                        instruction.Command == LibraryBackgroundCommand.ClearBg2ForKraid);
                    break;
                default:
                    throw new InvalidDataException($"Unsupported imported background command {instruction.Command}.");
            }
        }

        void ClearAndTransfer(LibraryBackgroundClear transfer, bool includeKraidPage)
        {
            for (int i = 0; i < transfer.ByteCount; i += 2)
            {
                int address = LibraryBackgroundListConstants.WorkRamBank | (transfer.WorkRamDestination + i);
                staging.WriteByte(address, LittleEndianBytes.LowByte(transfer.FillValue.Raw));
                staging.WriteByte(address + 1, LittleEndianBytes.HighByte(transfer.FillValue.Raw));
            }
            if (includeKraidPage) vram.ExecuteQueuedMemoryWrite(staging, transfer.WorkRamSourceAddress,
                transfer.ByteCount, LibraryBackgroundStagingDefinitions.KraidBg2VramDestinationWord);
            vram.ExecuteQueuedMemoryWrite(staging, transfer.WorkRamSourceAddress, transfer.ByteCount, transfer.VramDestination.Encoded);
        }
    }

    /// <summary>Routes an import source byte through the cartridge or the reference's staging according to DMA mapping.</summary>
    private static byte ReadImportByte(ReferenceWorkMemory source, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.Cartridge => source.ReadCartridgeByte(address),
            SnesDmaSourceKind.WorkRam => source.ReadWorkRamByte(address),
            SnesDmaSourceKind.Unmapped => throw new InvalidDataException($"Import background byte ${address:X6} is unmapped."),
            _ => throw new InvalidDataException($"Import background byte ${address:X6} is unmapped."),
        };

    /// <summary>
    /// The work RAM one reference list execution stages its tilemaps in, read through the cartridge's bank $7E/$7F
    /// addresses and the system banks' low-window mirrors, beside the cartridge it decodes from. It starts cleared.
    /// </summary>
    /// <param name="cartridge">The address space supplying the list's cartridge bytes.</param>
    private sealed class ReferenceWorkMemory(ISnesAddressSpace cartridge) : ISnesMemoryBus, IImportCartridgeSource
    {
        /// <summary>Both work-RAM banks, bank $7E first.</summary>
        private readonly byte[] _bytes = new byte[2 * SnesBusLayout.BankBytes];

        /// <summary>The cartridge source this staging sits beside.</summary>
        private readonly IImportCartridgeSource _cartridge = CartridgeImportSource.Require(cartridge);

        /// <inheritdoc />
        public byte ReadCartridgeByte(int cpuAddress) => _cartridge.ReadCartridgeByte(cpuAddress);

        /// <inheritdoc />
        public byte ReadWorkRamByte(int cpuAddress) => _bytes[Offset(cpuAddress)];

        /// <inheritdoc />
        public void WriteByte(int address, byte value) => _bytes[Offset(address)] = value;

        /// <summary>The staging offset of a work-RAM address or one of its low-window mirrors.</summary>
        private static int Offset(int cpuAddress)
        {
            int bank = SnesAddressMath.BankNumber(cpuAddress);
            int offset = SnesAddressMath.OffsetInBank(cpuAddress);
            if ((uint)cpuAddress <= SnesBusLayout.MaximumAddress && SnesBusLayout.IsWorkRamBank(bank))
                return (bank - SnesBusLayout.FirstWorkRamBank) * SnesBusLayout.BankBytes + offset;
            if ((uint)cpuAddress <= SnesBusLayout.MaximumAddress && SnesBusLayout.IsSystemBank(bank) && offset < SnesBusLayout.WorkRamMirrorEnd)
                return offset;
            throw new ArgumentOutOfRangeException(nameof(cpuAddress), cpuAddress,
                "Library-background staging is work RAM: bank $7E/$7F or a low-window system-bank mirror.");
        }
    }
}
