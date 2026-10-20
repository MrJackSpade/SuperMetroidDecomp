using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Enemies.Drawing;
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
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Enemies;

/// <summary>Checks Draygon's full BG2 staging and DMA output from a fresh room.</summary>
internal static class DraygonTilemapProductionVerification
{
    /// <summary>
    /// The enemy BG2 transfer size Draygon's body initializer gives the native NMI upload (A5:86D7): the first
    /// 1,024 bytes of the staging surface. The port writes VRAM directly, so only this oracle models the prefix.
    /// </summary>
    private const int NativeBg2TransferByteCount = 1024;

    /// <summary>#385: reconstruct fresh-room BG2 staging and native DMA, not merely retained current-command destinations.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Draygon tilemap production")]
    internal static void VerifyDraygonTilemapProduction()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, RoomHeaderDefinitions.Draygon.Pointer);
        var installation = RepositoryInstallation.Installation;
        var assets = LoadFixtureRoomAssets(room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var vram = new SnesVram(); var cgram = new SnesCgram(); assets.LoadGraphics(BgArtworkFixtures.For(vram), cgram);
        var random = new Bank80SystemState();
        random.SetRandomNumber(0x1234);
        var samus = new SamusState { Health = 999, MaxHealth = 999, Position = FixedPosition<RoomPixels>.FromWhole(new(256, 64)),
            Pose = SamusPoseId.FacingRightNormalPose };
        SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(random.NextRandom) { ReadRandom = () => random.RandomNumber, SeedRandom = random.SetRandomNumber, Bosses = new FixtureAreaBossFlags { IsBossDefeated = () => false } }) { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.Load(new RoomEnemyLoadRequest(room.State.EnemyPopulation, room.State.EnemyGraphicsSet, BgArtworkFixtures.For(vram), cgram) { Level = assets.LevelData, Samus = samus });
        var boss = enemies.DraygonBehavior.Draygon ?? throw new InvalidOperationException("Fresh Draygon population did not load.");
        // Observe which native enemy indexes were actually scheduled, without adding
        // a diagnostic callback to production. This does NOT independently verify AI
        // or queue construction; it isolates the downstream tilemap/DMA producer.
        var staging = Enumerable.Repeat((ushort)0x0338, 2048).ToArray();
        var expectedVram = staging.ToArray();
        // Draygon's body draws into his published enemy BG2 image (#1354), not VRAM.
        ushort Shown(int word) => enemies.Presentation.RequireBg2Image().Publish().Words[word];
        for (int word = 0; word < expectedVram.Length; word++)
            AssertEqual(expectedVram[word], Shown(word), "fresh Draygon BG2 starts entirely blank before any draw");
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
                if ((slot.ExtraProperties.ToNativeWord() & 0x8004) != 0x8004) continue;
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
                        for (int i = 0; i < words; i++) { staging[first + i] = bus.ReadCartridgeWordFixedBank(bank | (SnesAddressMath.OffsetInBank(cursor + 4 + i * 2))); destinations.Add(first + i); }
                        commands++; cursor += 4 + words * 2;
                    }
                }
            }
            // A0:9726 uploads only enemy_bg2_tilemap_size bytes from the beginning
            // of staging when any extended command ran. Compare untouched cells too.
            if (queuedTransfer)
            {
                staging.AsSpan(0, NativeBg2TransferByteCount / 2).CopyTo(expectedVram);
                transfers++;
            }
            oam.BeginFrame(); enemies.Drawing.DrawLayersAsOam(oam, new(0, 0), 0, 7); oam.FinalizeFrame();
            for (int word = 0; word < expectedVram.Length; word++)
            {
                ushort actual = Shown(word);
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
