using SuperMetroid.Core.Samus.Palettes;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.MamaTurtles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Checks compiled Mama and Baby Turtle headers in their retail room population.</summary>
internal static class MamaTurtleEnemyDefinitionsVerification
{
    /// <summary>
    /// Compares both compiled family headers field-for-field with the pinned cartridge,
    /// then loads the real Mama Turtle room while every source-header byte is forbidden.
    /// </summary>
    internal static void VerifyMamaTurtleEnemyDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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

        CartridgeRoomHeader room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(rom, 0xd055);
        var installation = RepositoryInstallation.Installation;
        CartridgeRoomAssets assets = LoadFixtureRoomAssets(room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram();
        var cgram = new SamusColourRig();
        assets.LoadGraphics(BgArtworkFixtures.For(vram), cgram);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 0)) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(vram), cgram.Gameplay) { Level = assets.LevelData, Samus = new SamusState() });

        AssertEqual(5, enemies.SlotTable.EnemyCount, "Mama Turtle retail population count");
        AssertEqual(EnemyDefinitionId.MamaTurtle,
            enemies.SlotTable.Slots[0].DefinitionId, "Mama Turtle parent header identity");
        AssertEqual(EnemyDefinitionId.BabyTurtle,
            enemies.SlotTable.Slots[1].DefinitionId, "Baby Turtle header identity");
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
