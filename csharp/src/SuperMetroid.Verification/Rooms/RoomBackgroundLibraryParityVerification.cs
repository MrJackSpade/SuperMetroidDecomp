
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Backgrounds.LibraryBackgroundProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Retail and synthetic checks for installed library-background command execution.</summary>
internal static class RoomBackgroundLibraryParityVerification
{
    /// <summary>Runs every authored background list and door-gated transfer, comparing installed VRAM and WRAM against native execution.</summary>
    internal static void VerifyLibraryBackgroundInstalledParity(
        string sourceRom, GameInstallation installed)
    {
        SuperMetroidAddressSpace inventoryBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyCompiledLibraryBackgroundPrograms), () => VerifyCompiledLibraryBackgroundPrograms(inventoryBus));
        IReadOnlyList<LibraryBackgroundNativeOperand> sources =
            LibraryBackgroundSourceInventory.Scan(inventoryBus);
        RoomBackgroundTilemaps backgrounds = installed.LoadRoomBackgroundTilemaps();
        RoomSkyTilemaps skies = installed.LoadRoomSkyTilemaps();
        RoomCharacterAtlases characters = installed.LoadRoomCharacters();
        HudTileAtlas hud = installed.LoadMaps().HudTiles;
        int cases = 0;

        foreach (LibraryBackgroundProgram program in LibraryBackgroundSourceDefinitions.All)
        {
            LibraryBackgroundNativeOperand[] list = sources
                .Where(source => source.ListPointer == program.Pointer).ToArray();
            // Zero tests the ordinary path. Each authored door pointer additionally
            // selects its conditional DMA; a list can contain more than one.
            foreach (ushort door in list.Where(source => source.DoorPointer.HasValue)
                .Select(source => (source.DoorPointer ?? throw new InvalidOperationException("Verification requires source.DoorPointer."))).Append((ushort)0).Distinct())
            {
                SuperMetroidAddressSpace nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                var nativeVram = new SnesVram();
                LibraryBackgroundProgramImporter.ExecuteReference(
                    nativeBus, nativeVram, program.Pointer, door);

                // Installed transfers write the installed artwork where the native transfer writes the cartridge's;
                // the statue ghost's characters alone are written nowhere.
                var selectedVram = new SnesVram();
                LibraryBackgroundReplay.Execute(selectedVram, LibraryBackgroundProgramDefinitions.Get(program.Pointer), door == 0 ? FixtureRoomHeaders.UnmatchedDoor : DoorDefinitions.Get(door), new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters });
                byte[] selectedImage = selectedVram.Bytes.ToArray();
                Range ghost = VerifyStatueGhostCharactersBindNowhere(program, nativeVram, selectedImage, characters);
                for (int index = 0; index < nativeVram.Bytes.Length; index++)
                {
                    if (index >= ghost.Start.Value && index < ghost.End.Value) continue;
                    if (nativeVram.ReadByte(index) == selectedImage[index]) continue;
                    throw new InvalidDataException(
                        $"Library background $8F:{program.Pointer:X4}, door ${door:X4} differs " +
                        $"at VRAM byte ${index:X4}: native ${nativeVram.ReadByte(index):X2}, " +
                        $"installed ${selectedImage[index]:X2}; " +
                        $"sources {string.Join(", ", list.Select(source => $"${source.SourceAddress:X6}"))}.");
                }
                cases++;
            }
        }

        AssertTrue(cases > LibraryBackgroundProgramDefinitionsConstants.RetailProgramCount,
            "library background parity included door-selected variants");
        Suite(nameof(VerifyInstalledBackgroundTransferFailures), () => VerifyInstalledBackgroundTransferFailures(backgrounds, skies, hud, characters));
        Console.WriteLine($"  Library backgrounds: {cases} ordinary and door-selected cases " +
            "match installed VRAM without command-list or visual ROM-source reads.");
    }

    /// <summary>
    /// The VRAM bytes a list's Tourian statue-ghost transfer covers, and the checks that stand for comparing them (#1454):
    /// the native transfer writes the installed statue-ghost characters there, and the installed list binds them nowhere
    /// (#1406), since only the statue ghost would draw them and it draws nothing, so the composed image leaves them unwritten.
    /// An empty range when the list has no such transfer.
    /// </summary>
    /// <exception cref="InvalidDataException">The native bytes differ from the installed characters, or the installed image writes the range.</exception>
    private static Range VerifyStatueGhostCharactersBindNowhere(LibraryBackgroundProgram program, SnesVram nativeVram, byte[] selectedImage,
        RoomCharacterAtlases characters)
    {
        LibraryBackgroundInstruction[] transfers = [.. program.Instructions
            .Where(instruction => instruction.Transfers && instruction.Source == LibraryBackgroundSource.TourianStatueGhostCharacters)];
        if (transfers.Length == 0) return 0..0;
        AssertEqual(1, transfers.Length, $"{program.Name} transfers the statue ghost's characters once");
        int start = LibraryBackgroundOperands.DestinationWord(transfers[0].Destination) * sizeof(ushort);
        ReadOnlySpan<byte> installed = characters.Get(RoomCharacterSet.TourianStatueGhost).Characters.Span;
        AssertEqual(installed.Length, transfers[0].ByteCount, $"{program.Name} transfers every statue-ghost character");
        if (!nativeVram.Bytes.Slice(start, installed.Length).SequenceEqual(installed))
            throw new InvalidDataException($"{program.Name}'s native statue-ghost transfer differs from the installed statue-ghost characters.");
        if (selectedImage.AsSpan(start, installed.Length).ContainsAnyExcept((byte)0))
            throw new InvalidDataException($"{program.Name}'s installed list writes the statue-ghost characters it binds nowhere.");
        return start..(start + installed.Length);
    }

    /// <summary>
    /// Synthetic cartridge whose library-background command list at <paramref name="listPointer"/>
    /// is one transfer-to-VRAM ($4800) of <paramref name="byteCount"/> bytes from the long
    /// <paramref name="sourceAddress"/>, then end.
    /// </summary>
    private static TestAddressSpace CreateSingleTransferBackgroundCartridge(
        ushort listPointer, int sourceAddress, ushort byteCount)
    {
        var bus = new TestAddressSpace();
        int commandAddress = LibraryBackgroundOperandDecoding.CommandListBank | listPointer;
        WriteTestWord(bus, commandAddress, LibraryBackgroundOperandDecoding.Word(LibraryBackgroundCommand.Transfer));
        WriteTestByte(bus, commandAddress + 2, unchecked((byte)sourceAddress));
        WriteTestByte(bus, commandAddress + 3, LittleEndianBytes.HighByte(unchecked((ushort)sourceAddress)));
        WriteTestByte(bus, commandAddress + 4, SnesLittleEndianConstants.BankByte(sourceAddress));
        WriteTestWord(bus, commandAddress + 5, 0x4800);
        WriteTestWord(bus, commandAddress + 7, byteCount);
        WriteTestWord(bus, commandAddress + 9, LibraryBackgroundOperandDecoding.EndCommandWord);
        return bus;
    }

    /// <summary>
    /// Synthetic cartridge whose one-transfer command list streams two bytes of the enemy BG2 staging
    /// ($7E:2000) to VRAM.
    /// </summary>
    private static TestAddressSpace CreateStagedWorkRamBackgroundCartridge(ushort listPointer) =>
        CreateSingleTransferBackgroundCartridge(listPointer, EnemySourceDefinitions.EnemyBg2Staging, 2);

    /// <summary>
    /// A fully bound installation must never quietly fall back to stock ROM pixels
    /// when a direct-transfer source or size is absent from its presentation assets.
    /// The authored retail lists are covered above; these synthetic command lists
    /// test the failure boundary that no retail list presently exercises. A transfer from
    /// ROM that names no library-background source cannot be compiled at all (#1414): its
    /// list fails to decode, naming the source.
    /// </summary>
    private static void VerifyInstalledBackgroundTransferFailures(
        RoomBackgroundTilemaps backgrounds, RoomSkyTilemaps skies,
        HudTileAtlas hud, RoomCharacterAtlases characters)
    {
        const ushort listPointer = 0xf000;

        var partialSky = new LibraryBackgroundProgram(new LibraryBackground("partial sky page", []),
            [new(LibraryBackgroundCommand.Transfer, LibraryBackgroundSource.Sky(0), LibraryBackgroundDestination.BackgroundFirstPage, 2, null)]);
        AssertThrows<InvalidDataException>(
            () => LibraryBackgroundReplay.Execute(new SnesVram(), partialSky, FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters }),
            "installed art rejects a partial sky page");
        const int unownedRomSource = 0x8a8000;
        TestAddressSpace bus = CreateSingleTransferBackgroundCartridge(listPointer, unownedRomSource, 2);
        InvalidDataException unowned = AssertThrows<InvalidDataException>(() => LibraryBackgroundProgramImporter.Read(bus, listPointer),
            "a transfer from ROM no library-background source names does not compile");
        AssertTrue(unowned.Message.Contains($"${unownedRomSource:X6}", StringComparison.Ordinal),
            "the uncompiled background transfer identifies its ROM source");

        // A WRAM-sourced transfer remains a native streaming operation from the owned staging,
        // not a replacement artwork lookup: it must still transfer when all art catalogs are bound.
        TestAddressSpace stagedBus = CreateStagedWorkRamBackgroundCartridge(listPointer);
        byte[] stagedBg2 = [0x12, 0x34];
        var stagedVram = new SnesVram();
        LibraryBackgroundReplay.Execute(stagedVram, LibraryBackgroundProgramImporter.Read(stagedBus, listPointer), FixtureRoomHeaders.UnmatchedDoor, new LibraryBackgroundArtwork { TilemapArt = backgrounds, SkyArt = skies, HudArt = hud, CharacterArt = characters }, stagedBg2);
        AssertEqual((byte)0x12, stagedVram.ReadByte(0x9000),
            "installed-art binding retains the staged enemy BG2 transfer low byte");
        AssertEqual((byte)0x34, stagedVram.ReadByte(0x9001),
            "installed-art binding retains the staged enemy BG2 transfer high byte");
    }
}
