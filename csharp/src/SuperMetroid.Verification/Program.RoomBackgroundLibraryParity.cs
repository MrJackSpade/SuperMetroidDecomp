using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Exercise every cartridge-authored background list, including each door-gated
    /// transfer. Matching one representative list does not prove that an installed
    /// sky page or tilemap is selected by all the other native command paths.
    /// </summary>
    private static void VerifyLibraryBackgroundInstalledParity(
        string sourceRom, GameInstallation installed)
    {
        SuperMetroidAddressSpace inventoryBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        VerifyCompiledLibraryBackgroundPrograms(inventoryBus);
        IReadOnlyList<LibraryBackgroundSource> sources =
            LibraryBackgroundSourceInventory.Scan(inventoryBus);
        RoomBackgroundTilemapCatalog backgrounds = installed.LoadRoomBackgroundTilemaps();
        RoomSkyTilemapCatalog skies = installed.LoadRoomSkyTilemaps();
        RoomCharacterAtlasCatalog characters = installed.LoadRoomCharacters();
        HudTileAtlas hud = installed.LoadMaps().HudTiles;
        int cases = 0;

        foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
        {
            LibraryBackgroundSource[] list = sources
                .Where(source => source.ListPointer == program.Pointer).ToArray();
            // Zero tests the ordinary path. Each authored door pointer additionally
            // selects its conditional DMA; a list can contain more than one.
            foreach (ushort door in list.Where(source => source.DoorPointer.HasValue)
                .Select(source => source.DoorPointer!.Value).Append((ushort)0).Distinct())
            {
                SuperMetroidAddressSpace nativeBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                var nativeVram = new SnesVram();
                LibraryBackgroundExecutionResult native =
                    LibraryBackgroundLoader.ExecuteNativeForVerification(
                        nativeBus, nativeVram, program.Pointer, door);

                SuperMetroidAddressSpace selectedBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                var guarded = new LibraryBackgroundVisualReadGuard(selectedBus, sources);
                var selectedVram = new SnesVram();
                LibraryBackgroundExecutionResult selected = LibraryBackgroundLoader.Execute(
                    guarded, selectedVram, program.Pointer, door,
                    backgrounds, skies, hud, characters);
                AssertEqual(native, selected,
                    $"library background $8F:{program.Pointer:X4}, door ${door:X4} command result");
                for (int index = 0; index < nativeVram.Bytes.Length; index++)
                {
                    if (nativeVram.ReadByte(index) == selectedVram.ReadByte(index)) continue;
                    throw new InvalidDataException(
                        $"Library background $8F:{program.Pointer:X4}, door ${door:X4} differs " +
                        $"at VRAM byte ${index:X4}: native ${nativeVram.ReadByte(index):X2}, " +
                        $"installed ${selectedVram.ReadByte(index):X2}; " +
                        $"sources {string.Join(", ", list.Select(source => $"${source.SourceAddress:X6}"))}.");
                }

                foreach (LibraryBackgroundSource source in list.Where(source =>
                    source.Command == LibraryBackgroundCommand.DecompressToWorkRam))
                {
                    int length = backgrounds.Get(source.SourceAddress).Transfer.Length;
                    int start = RoomAssetRomData.LibraryBackground.WorkRamBank |
                        source.WorkRamDestination!.Value;
                    for (int offset = 0; offset < length; offset++)
                        AssertEqual(nativeBus.ReadByte(start + offset),
                            selectedBus.ReadByte(start + offset),
                            $"library background $8F:{program.Pointer:X4} WRAM byte {offset}");
                }
                cases++;
            }
        }

        AssertTrue(cases > LibraryBackgroundProgramDefinitions.RetailProgramCount,
            "library background parity included door-selected variants");
        var unknownListBus = new LibraryBackgroundUnknownListReadGuard(inventoryBus);
        try
        {
            LibraryBackgroundLoader.Execute(unknownListBus, new SnesVram(), 0xf000, 0,
                backgrounds, skies, hud, characters);
            throw new InvalidOperationException(
                "Installed library background accepted an uncompiled bank-$8F list.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("$8F:F000", StringComparison.Ordinal),
                "unknown installed background list identifies its pointer");
        }
        AssertEqual(0, unknownListBus.ForbiddenReadAttempts,
            "installed background rejects unknown list before any cartridge read");
        VerifyInstalledBackgroundTransferFailures(backgrounds, skies, hud, characters);
        Console.WriteLine($"  Library backgrounds: {cases} ordinary and door-selected cases " +
            "match installed VRAM/WRAM without command-list or visual ROM-source reads.");
    }

    /// <summary>
    /// A fully bound installation must never quietly fall back to stock ROM pixels
    /// when a direct-transfer source or size is absent from its presentation assets.
    /// The authored retail lists are covered above; these synthetic command lists
    /// test the failure boundary that no retail list presently exercises.
    /// </summary>
    private static void VerifyInstalledBackgroundTransferFailures(
        RoomBackgroundTilemapCatalog backgrounds, RoomSkyTilemapCatalog skies,
        HudTileAtlas hud, RoomCharacterAtlasCatalog characters)
    {
        const ushort listPointer = 0xf000;
        int commandAddress = RoomAssetRomData.LibraryBackground.CommandBank | listPointer;

        foreach ((int sourceAddress, ushort byteCount) in new[]
        {
            (RoomSkyTilemapFormat.FirstSourceAddress, (ushort)2),
            (0x8a8000, (ushort)2),
        })
        {
            var bus = new TestAddressSpace();
            WriteWord(bus, commandAddress, (ushort)LibraryBackgroundCommand.TransferToVram);
            bus.WriteByte(commandAddress + 2, (byte)sourceAddress);
            bus.WriteByte(commandAddress + 3, (byte)(sourceAddress >> 8));
            bus.WriteByte(commandAddress + 4, (byte)(sourceAddress >> 16));
            WriteWord(bus, commandAddress + 5, 0x4800);
            WriteWord(bus, commandAddress + 7, byteCount);
            WriteWord(bus, commandAddress + 9, (ushort)LibraryBackgroundCommand.End);
            try
            {
                LibraryBackgroundLoader.ExecuteNativeForVerification(bus, new SnesVram(), listPointer, 0,
                    backgrounds, skies, hud, characters);
                throw new InvalidOperationException(
                    $"Installed art silently accepted ROM transfer ${sourceAddress:X6}.");
            }
            catch (InvalidDataException error)
            {
                AssertTrue(error.Message.Contains($"${sourceAddress:X6}",
                        StringComparison.Ordinal),
                    "invalid installed background transfer identifies its ROM source");
            }
        }

        // WRAM staging remains a native streaming operation, not a replacement
        // artwork lookup: it must still transfer when all art catalogs are bound.
        var stagedBus = new TestAddressSpace();
        const int stagedSource = 0x7e2000;
        stagedBus.WriteByte(stagedSource, 0x12);
        stagedBus.WriteByte(stagedSource + 1, 0x34);
        WriteWord(stagedBus, commandAddress,
            (ushort)LibraryBackgroundCommand.TransferToVram);
        stagedBus.WriteByte(commandAddress + 2, unchecked((byte)stagedSource));
        stagedBus.WriteByte(commandAddress + 3, unchecked((byte)(stagedSource >> 8)));
        stagedBus.WriteByte(commandAddress + 4, (byte)(stagedSource >> 16));
        WriteWord(stagedBus, commandAddress + 5, 0x4800);
        WriteWord(stagedBus, commandAddress + 7, 2);
        WriteWord(stagedBus, commandAddress + 9, (ushort)LibraryBackgroundCommand.End);
        var stagedVram = new SnesVram();
        LibraryBackgroundLoader.ExecuteNativeForVerification(stagedBus, stagedVram, listPointer, 0,
            backgrounds, skies, hud, characters);
        AssertEqual((byte)0x12, stagedVram.ReadByte(0x9000),
            "installed-art binding retains WRAM background transfer low byte");
        AssertEqual((byte)0x34, stagedVram.ReadByte(0x9001),
            "installed-art binding retains WRAM background transfer high byte");
    }

    private sealed class LibraryBackgroundUnknownListReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0x8ff000 and <= 0x8fffff)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Installed background interpreted unknown list at ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class LibraryBackgroundVisualReadGuard(
        ISnesAddressSpace source, IReadOnlyList<LibraryBackgroundSource> entries)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            foreach (LibraryBackgroundProgram program in LibraryBackgroundProgramDefinitions.All)
            {
                int start = RoomAssetRomData.LibraryBackground.CommandBank | program.Pointer;
                if (address >= start && address - start < program.NativeByteCount)
                    throw new InvalidOperationException(
                        $"Installed background reread compiled command list $8F:{program.Pointer:X4}.");
            }
            foreach (LibraryBackgroundSource entry in entries)
            {
                if (entry.Command == LibraryBackgroundCommand.DecompressToWorkRam)
                {
                    // Only the first compressed byte is needed to detect an accidental
                    // fallback into RomDataReader.Decompress.
                    if (address == entry.SourceAddress)
                        throw new InvalidOperationException(
                            $"Installed background reread compressed source ${address:X6}.");
                }
                else if (entry.SourceAddress >=
                    RoomAssetRomData.LibraryBackground.RomSourceAddressFloor &&
                    address >= entry.SourceAddress &&
                    address - entry.SourceAddress < entry.TransferByteCount!.Value)
                    throw new InvalidOperationException(
                        $"Installed background reread direct visual source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
