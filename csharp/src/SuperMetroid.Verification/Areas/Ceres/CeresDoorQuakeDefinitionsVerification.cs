using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.World.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Ceres Ridley's private door quake offsets and installed overlay art.</summary>
internal static class CeresDoorQuakeDefinitionsVerification
{

    /// <summary>Compares the four authored offset words, timer aliases, real OAM phases, and the native overlay records.</summary>
    internal static void VerifyCeresDoorQuakeDefinitions(CartridgeImportAddressSpace rom)
    {
        const int sourceAddress = 0xa6a321;

        for (ushort phase = 0; phase < 4; phase++)
        {
            short expected = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + phase * 2));
            AssertEqual(expected, CeresDoorQuake.XOffset(phase),
                $"Ceres private door quake phase {phase}");
        }

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            AssertEqual(
                unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + timer % 4 * 2)),
                CeresDoorQuake.XOffset((ushort)timer),
                $"Ceres private door quake timer ${timer:X4}");
        }

        var guarded = NewCeresDoorQuakeReadGuard(rom);
        byte[] stockJson = EnemySpritemapFiles.Extract(rom);
        EnemySpritemapCatalog installed = EnemySpritemapDocuments.Load(
            new MemoryStream(stockJson, writable: false));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(SpawnedObjectFixtures.InstalledTilesWith(installed)),
            RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            RidleyEncounter = { CeresStatus = CeresEscapeStatus.RidleyEscaped },
            BossArtwork = InstalledFixtures.BossArtwork,
        };
        enemies.SlotTable.Slots[0].Bind(EnemyDefinitionId.RidleyCeres, enemies.SlotTable.Slots[0].Definition);
        enemies.RidleyEncounter.Ridley = new RidleyEnemyState { MovementAnimationEnabled = false };
        RoomEnemySlot door = enemies.SlotTable.Slots[1];
        door.Bind(EnemyDefinitionId.CeresDoor, door.Definition);
        var doorState = new CeresDoorEnemyState { HandsDrawingToRidley = true };
        door.SetSpeciesState(doorState);
        door.Position = door.Position.WithWhole(new(100, 80));
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        short[] expectedOffsets = [0, -4, -1, 3];
        int? baseX = null;
        for (ushort phase = 0; phase < expectedOffsets.Length; phase++)
        {
            enemies.Earthquake.Start(NativeEarthquakeTypes.Shake(29), phase);
            var oam = new OamBuffer();
            oam.BeginFrame();
            oam.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, new(0, 0)));
            oam.FinalizeFrame();
            baseX ??= oam.GetEntry(0).Position.X;
            AssertEqual(baseX.Value + expectedOffsets[phase], oam.GetEntry(0).Position.X,
                $"Ceres private door production OAM phase {phase}");
        }

        enemies.Earthquake.Stop();
        var nativeOverlay = new OamBuffer();
        nativeOverlay.BeginFrame();
        DrawImportedEnemySpritemap(rom, nativeOverlay, new NativeEnemySpritemapDraw { Bank = 0xa6, Pointer = 0xa329, Origin = new(100, 80), PaletteBits = NativeSpriteGraphicsWords.PaletteBits(2), BaseTileIndex = 0 });
        nativeOverlay.FinalizeFrame();
        var installedOverlay = new OamBuffer();
        installedOverlay.BeginFrame();
        installedOverlay.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, new(0, 0)));
        installedOverlay.FinalizeFrame();
        // The door shows its colours as its shade (#1512), which keeps no palette row, so the overlay's records match the
        // cartridge's in every field but the row its colours were loaded into.
        AssertEqual(nativeOverlay.LastFinalizedSpriteCount, installedOverlay.LastFinalizedSpriteCount,
            "Ceres private overlay draws the cartridge's record count with ROM frame reads forbidden");
        for (int index = 0; index < nativeOverlay.LastFinalizedSpriteCount; index++)
            AssertEqual(nativeOverlay.GetEntry(index) with { Palette = 0 }, installedOverlay.GetEntry(index) with { Palette = 0 },
                $"Ceres private overlay record {index} matches native OAM with ROM frame reads forbidden");

        Console.WriteLine(
            "Ceres door quake definitions: all four authored offset words, 65,536 timer aliases, four real OAM phases, and the installed private overlay pass with source reads forbidden.");
    }

    /// <summary>Forbids converted quake-table reads and optionally native overlay spritemap reads.</summary>
    internal static CartridgeReadGuard NewCeresDoorQuakeReadGuard(
        ISnesAddressSpace source, bool blockOverlay = false) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa6a321 and < 0xa6a329 ||
                    (blockOverlay && address is >= 0xa6a329 and < 0xa6a353),
                address => $"Ceres private door draw attempted migrated source read ${address:X6}.");

    /// <summary>Runs the Ceres door quake and private-overlay artwork check.</summary>
    [VerificationCommand("--ceres-door-artwork")]
    internal static int CeresDoorArtworkCommand()
    {
        Suite(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
