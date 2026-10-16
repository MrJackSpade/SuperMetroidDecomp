using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Draygon's full BG2 staging and DMA output from a fresh room.</summary>
internal static class DraygonTilemapProductionVerification
{
    /// <summary>#385: reconstruct fresh-room BG2 staging and native DMA, not merely retained current-command destinations.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon tilemap production")]
    internal static void VerifyDraygonTilemapProduction()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, DraygonProductionAuditDefinitions.Room);
        var installation = RepositoryInstallation.Installation;
        var assets = LoadFixtureRoomAssets(room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram(); var cgram = new SnesCgram(); assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState { Health = 999, MaxHealth = 999, Position = FixedPosition<RoomPixels>.FromWhole(new(256, 64)),
            Pose = SamusPoseId.FacingRightNormalPose };
        samus.PoseMetadata.RefreshCollisionRadii(); samus.Presentation.InitializeAnimation();
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.Load(new RoomEnemyLoadRequest(CompiledRoomEnemyDefinitions.Instance, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, vram, cgram, new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber, Bosses = new FixtureAreaBossFlags { IsBossDefeated = () => false } }) { Level = assets.LevelData, Samus = samus });
        var boss = enemies.DraygonBehavior.Draygon ?? throw new InvalidOperationException("Fresh Draygon population did not load.");
        // Observe which native enemy indexes were actually scheduled, without adding
        // a diagnostic callback to production. This does NOT independently verify AI
        // or queue construction; it isolates the downstream tilemap/DMA producer.
        var staging = Enumerable.Repeat((ushort)0x0338, 2048).ToArray();
        var expectedVram = staging.ToArray();
        AssertEqual((ushort)0x0400, boss.Bg2TilemapSize, "A5:86D7 initializes the native DMA prefix size");
        for (int word = 0; word < expectedVram.Length; word++)
            AssertEqual(expectedVram[word], vram.ReadWord(0x4800 + word), "fresh Draygon BG2 starts entirely blank before any draw");
        var destinations = new HashSet<int>(); var streams = new HashSet<int>();
        var functions = new HashSet<DraygonAiFunction>();
        int commands = 0, transfers = 0; long comparisons = 0;
        var oam = new OamBuffer();
        for (int frame = 0; frame < 4096; frame++)
        {
            enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = assets.LevelData, NmiFrameCounter8 = unchecked((byte)frame) });
            functions.Add(boss.Function);
            bool queuedTransfer = false;
            foreach (var queue in enemies.Drawing.QueuesForVerification)
            foreach (ushort nativeIndex in queue)
            {
                var slot = enemies.SlotTable.Slots[nativeIndex / 64];
                if (((ushort)slot.ExtraProperties & 0x8004) != 0x8004) continue;
                int bank = (int)new SnesAddress(slot.Definition.Bank, 0);
                int count = bus.ReadCartridgeByte(bank | slot.SpritemapPointer);
                for (int component = 0; component < count; component++)
                {
                    int stream = bus.ReadCartridgeWordFixedBank(bank | (SnesAddressMath.OffsetInBank(slot.SpritemapPointer + 2 + component * 8 + 4)));
                    if (bus.ReadCartridgeWordFixedBank(bank | SnesAddressMath.OffsetInBank(stream)) != 0xfffe) continue;
                    streams.Add(bank | stream); queuedTransfer = true;
                    int cursor = stream + 2;
                    for (int command = 0; ; command++)
                    {
                        int destination = bus.ReadCartridgeWordFixedBank(bank | (SnesAddressMath.OffsetInBank(cursor))); if (destination == 0xffff) break;
                        int words = bus.ReadCartridgeWordFixedBank(bank | (SnesAddressMath.OffsetInBank(cursor + 2))), first = (destination - 0x2000) / 2;
                        if (command >= 128 || (destination & 1) != 0 || words <= 0 || first < 0 || first + words > staging.Length)
                            throw new InvalidDataException("Invalid native Draygon command in independent producer audit.");
                        for (int i = 0; i < words; i++) { staging[first + i] = (ushort)bus.ReadCartridgeWordFixedBank(bank | (SnesAddressMath.OffsetInBank(cursor + 4 + i * 2))); destinations.Add(first + i); }
                        commands++; cursor += 4 + words * 2;
                    }
                }
            }
            // A0:9726 uploads only enemy_bg2_tilemap_size bytes from the beginning
            // of staging when any extended command ran. Compare untouched cells too.
            if (queuedTransfer)
            {
                staging.AsSpan(0, boss.Bg2TilemapSize / 2).CopyTo(expectedVram);
                transfers++;
            }
            oam.BeginFrame(); enemies.Drawing.DrawLayers(oam, new(0, 0), 0, 7); oam.FinalizeFrame();
            for (int word = 0; word < expectedVram.Length; word++)
            {
                ushort actual = vram.ReadWord(0x4800 + word);
                if (expectedVram[word] != actual)
                    throw new InvalidOperationException($"Fresh Draygon frame {frame}, BG2 word {word:X3}: native staging/DMA {expectedVram[word]:X4}, actual {actual:X4}; published={queuedTransfer}.");
                comparisons++;
            }
        }
        AssertTrue(commands > 0 && transfers > 0 && destinations.Count > 8 && streams.Count > 1,
            "fresh producer audit must cover newly published commands and more than retained eight-word fragments");
        Console.WriteLine($"Fresh Draygon production: {commands} commands, {transfers} transfers, {streams.Count} distinct streams, {destinations.Count} distinct destinations, {functions.Count} AI functions, {comparisons} complete-map word comparisons agree with native staging/DMA reconstruction.");
    }

    /// <summary>Runs the fresh-room BG2 production comparison with the retail cartridge.</summary>
    [VerificationCommand("--draygon-tilemap-production")]
    internal static int DraygonTilemapProductionCommand()
    {
        Suite(nameof(VerifyDraygonTilemapProduction), () => VerifyDraygonTilemapProduction());
        return 0;
    }
}
