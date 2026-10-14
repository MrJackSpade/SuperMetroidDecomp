using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;

/// <summary>Checks compiled Mama and Baby Turtle headers in their retail room population.</summary>
internal static class MamaTurtleEnemyDefinitionsVerification
{
    /// <summary>
    /// Compares both compiled family headers field-for-field with the pinned cartridge,
    /// then loads the real Mama Turtle room while every source-header byte is forbidden.
    /// </summary>
    internal static void VerifyMamaTurtleEnemyDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guard = NewMamaTurtleDefinitionGuard(rom);
        foreach (EnemyDefinitionId pointer in new EnemyDefinitionId[]
                 {
                     EnemyDefinitionId.MamaTurtle,
                     EnemyDefinitionId.BabyTurtle,
                 })
        {
            RoomEnemyDefinition expected = EnemyDefinitionsVerification.ReadNativeEnemyDefinition(rom, pointer);
            AssertTrue(MamaTurtleEnemyDefinitionCatalogAccess.TryGet(pointer, out RoomEnemyDefinition actual),
                $"Mama Turtle header $A0:{(int)pointer:X4} is compiled");
            AssertEqual(expected, actual, $"compiled enemy header $A0:{(int)pointer:X4}");
        }

        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(rom, 0xd055);
        var installation = RepositoryInstallation.Installation;
        CartridgeRoomAssets assets = LoadFixtureRoomAssets(rom, room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.Load(new RoomEnemyLoadRequest(guard, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, vram, cgram, new StandaloneRoomEnemyServices(() => 0)) { Level = assets.LevelData, Samus = new SamusState() });

        AssertEqual(5, enemies.SlotTable.EnemyCount, "Mama Turtle retail population count");
        AssertEqual(EnemyDefinitionId.MamaTurtle,
            enemies.SlotTable.Slots[0].EnemyDefinitionPointer, "Mama Turtle parent header identity");
        AssertEqual(EnemyDefinitionId.BabyTurtle,
            enemies.SlotTable.Slots[1].EnemyDefinitionPointer, "Baby Turtle header identity");
        AssertEqual(EnemyDefinitionsVerification.ReadNativeEnemyDefinition(rom, EnemyDefinitionId.MamaTurtle),
            enemies.SlotTable.Slots[0].Definition, "Mama Turtle loaded definition");
        for (int slot = 1; slot < 5; slot++)
        {
            AssertEqual(EnemyDefinitionsVerification.ReadNativeEnemyDefinition(rom, EnemyDefinitionId.BabyTurtle),
                enemies.SlotTable.Slots[slot].Definition, $"Baby Turtle loaded definition slot {slot}");
        }

        Console.WriteLine(
            "  Mama Turtle enemy definitions: two native headers and the five-actor " +
            "retail population load pass without reading 128 source bytes.");
    }

    /// <summary>Reads all fields of a native enemy header for independent definition comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for native enemy definition headers")]
    private static RoomEnemyDefinition ReadNativeEnemyDefinition(
        SuperMetroidAddressSpace bus,
        ushort pointer)
    {
        int address = RoomEnemyRomLayout.DefinitionBank | pointer;
        int Long(int offset) =>
            (CartridgeImportSource.Require(bus).ReadCartridgeLong(address + offset));

        return new RoomEnemyDefinition(CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 0), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 2), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 4), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 6), new(CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 8), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 10)), bus.ReadCartridgeByte(address + 12), bus.ReadCartridgeByte(address + 13), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 14), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 16), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 18), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 20), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 22), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 24), ClosedNativeWords.Decode<GrappleAiRoutine>(CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 26), "grapple AI routine"), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 28), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 30), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 32), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 34), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 36), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 38), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 40), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 42), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 44), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 46), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 48), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 50), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 52), Long(54), bus.ReadCartridgeByte(address + 57), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 58), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 60), CartridgeImportSource.Require(bus).ReadCartridgeWord(address + 62));
    }

    /// <summary>Rejects runtime reads of both compiled turtle header records.</summary>
    private static CartridgeReadGuard NewMamaTurtleDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(MamaTurtleEnemyDefinitionCatalogConstants.SourceAddress,
            MamaTurtleEnemyDefinitionCatalogConstants.SourceByteLength,
            address => $"Mama Turtle family still reads compiled header byte ${address:X6}.");

    /// <summary>Runs the focused turtle definition and population check.</summary>
    [VerificationCommand("--mama-turtle-enemy-definitions")]
    internal static int MamaTurtleEnemyDefinitionsCommand()
    {
        Suite(nameof(VerifyMamaTurtleEnemyDefinitions), () => VerifyMamaTurtleEnemyDefinitions());
        return 0;
    }
}
