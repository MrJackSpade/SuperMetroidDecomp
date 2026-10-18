using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Backgrounds.LibraryBackgroundProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Retail and synthetic checks for installed library-background command execution.</summary>
internal static class RoomBackgroundLibraryParityVerification
{
    /// <summary>
    /// Compares the <paramref name="length"/> bank-$7E bytes a decompress-to-WRAM command wrote at its
    /// cartridge-authored <paramref name="destination"/> after native execution with the installed tilemap staging.
    /// </summary>
    [CpuBusBoundary("exact bus test: compares the cartridge-selected WRAM range a library-background decompression fills")]
    private static void AssertSameLibraryBackgroundWorkRam(SuperMetroidAddressSpace nativeBus,
        Bg2TilemapStaging selected, ushort destination, int length, ushort listPointer)
    {
        int start = RoomAssetRomData.LibraryBackground.WorkRamBank | destination;
        int stagingStart = start - LibraryBackgroundStagingDefinitions.NativeAddress;
        for (int offset = 0; offset < length; offset++)
            AssertEqual(nativeBus.ReadWorkRamByte(start + offset),
                selected.LibraryTilemap[stagingStart + offset],
                $"library background $8F:{listPointer:X4} WRAM byte {offset}");
    }

    /// <summary>Runs every authored background list and door-gated transfer, comparing installed VRAM and WRAM against native execution.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyLibraryBackgroundInstalledParity(
        string sourceRom, GameInstallation installed)
    {
        SuperMetroidAddressSpace inventoryBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyCompiledLibraryBackgroundPrograms), () => VerifyCompiledLibraryBackgroundPrograms(inventoryBus));
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
                .Select(source => (source.DoorPointer ?? throw new InvalidOperationException("Verification requires source.DoorPointer."))).Append((ushort)0).Distinct())
            {
                SuperMetroidAddressSpace nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                var nativeVram = new SnesVram();
                LibraryBackgroundExecutionResult native =
                    LibraryBackgroundProgramImporter.ExecuteReference(
                        nativeBus, nativeVram, program.Pointer, door);

                LibraryBackgroundStaging selectedStaging = LibraryBackgroundStagingFixtures.Fresh();
                var selectedVram = new SnesVram();
                LibraryBackgroundExecutionResult selected = LibraryBackgroundLoader.Execute(selectedStaging, selectedVram, program.Pointer, door == 0 ? FixtureRoomHeaders.UnmatchedDoor : DoorDefinitions.Get(door), new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters });
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
                    AssertSameLibraryBackgroundWorkRam(nativeBus, selectedStaging.Tilemap, (source.WorkRamDestination ?? throw new InvalidOperationException("Verification requires source.WorkRamDestination.")),
                        backgrounds.Get(source.SourceAddress).Transfer.Length, program.Pointer);
                }
                cases++;
            }
        }

        AssertTrue(cases > LibraryBackgroundProgramDefinitions.RetailProgramCount,
            "library background parity included door-selected variants");
        try
        {
            LibraryBackgroundLoader.Execute(LibraryBackgroundStagingFixtures.Fresh(), new SnesVram(), 0xf000, FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters });
            throw new InvalidOperationException(
                "Installed library background accepted an uncompiled bank-$8F list.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("$8F:F000", StringComparison.Ordinal),
                "unknown installed background list identifies its pointer");
        }
        Suite(nameof(VerifyInstalledBackgroundTransferFailures), () => VerifyInstalledBackgroundTransferFailures(backgrounds, skies, hud, characters));
        Console.WriteLine($"  Library backgrounds: {cases} ordinary and door-selected cases " +
            "match installed VRAM/WRAM without command-list or visual ROM-source reads.");
    }

    /// <summary>
    /// Synthetic cartridge whose library-background command list at <paramref name="listPointer"/>
    /// is one transfer-to-VRAM ($4800) of <paramref name="byteCount"/> bytes from the long
    /// <paramref name="sourceAddress"/>, then end.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a one-transfer library-background command list")]
    private static TestAddressSpace CreateSingleTransferBackgroundCartridge(
        ushort listPointer, int sourceAddress, ushort byteCount)
    {
        var bus = new TestAddressSpace();
        int commandAddress = RoomAssetRomData.LibraryBackground.CommandBank | listPointer;
        WriteTestWord(bus, commandAddress, (ushort)LibraryBackgroundCommand.TransferToVram);
        WriteTestByte(bus, commandAddress + 2, unchecked((byte)sourceAddress));
        WriteTestByte(bus, commandAddress + 3, SnesLittleEndian.HighByte(unchecked((ushort)sourceAddress)));
        WriteTestByte(bus, commandAddress + 4, SnesLittleEndian.BankByte(sourceAddress));
        WriteTestWord(bus, commandAddress + 5, 0x4800);
        WriteTestWord(bus, commandAddress + 7, byteCount);
        WriteTestWord(bus, commandAddress + 9, (ushort)LibraryBackgroundCommand.End);
        return bus;
    }

    /// <summary>
    /// Synthetic cartridge whose one-transfer command list streams two bytes of the enemy BG2 staging
    /// ($7E:2000) to VRAM.
    /// </summary>
    private static TestAddressSpace CreateStagedWorkRamBackgroundCartridge(ushort listPointer) =>
        CreateSingleTransferBackgroundCartridge(listPointer, EnemyStagingDefinitions.Bg2NativeAddress, 2);

    /// <summary>
    /// A fully bound installation must never quietly fall back to stock ROM pixels
    /// when a direct-transfer source or size is absent from its presentation assets.
    /// The authored retail lists are covered above; these synthetic command lists
    /// test the failure boundary that no retail list presently exercises.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyInstalledBackgroundTransferFailures(
        RoomBackgroundTilemapCatalog backgrounds, RoomSkyTilemapCatalog skies,
        HudTileAtlas hud, RoomCharacterAtlasCatalog characters)
    {
        const ushort listPointer = 0xf000;

        foreach ((int sourceAddress, ushort byteCount) in new[]
        {
            (RoomSkyTilemapFormat.FirstSourceAddress, (ushort)2),
            (0x8a8000, (ushort)2),
        })
        {
            TestAddressSpace bus = CreateSingleTransferBackgroundCartridge(listPointer, sourceAddress, byteCount);
            try
            {
                LibraryBackgroundLoader.ExecuteProgram(LibraryBackgroundStagingFixtures.Fresh(), new SnesVram(), LibraryBackgroundProgramImporter.Read(bus, listPointer), FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters });
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

        // A WRAM-sourced transfer remains a native streaming operation from the owned staging,
        // not a replacement artwork lookup: it must still transfer when all art catalogs are bound.
        TestAddressSpace stagedBus = CreateStagedWorkRamBackgroundCartridge(listPointer);
        var stagedEnemies = new EnemyStagingSurfaces();
        StagingByteView stagedBg2 = stagedEnemies.CorpseTiles;
        stagedBg2[0] = 0x12;
        stagedBg2[1] = 0x34;
        var stagedVram = new SnesVram();
        LibraryBackgroundLoader.ExecuteProgram(new LibraryBackgroundStaging(new Bg2TilemapStaging(), stagedEnemies), stagedVram, LibraryBackgroundProgramImporter.Read(stagedBus, listPointer), FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters });
        AssertEqual((byte)0x12, stagedVram.ReadByte(0x9000),
            "installed-art binding retains the staged enemy BG2 transfer low byte");
        AssertEqual((byte)0x34, stagedVram.ReadByte(0x9001),
            "installed-art binding retains the staged enemy BG2 transfer high byte");
    }
}
