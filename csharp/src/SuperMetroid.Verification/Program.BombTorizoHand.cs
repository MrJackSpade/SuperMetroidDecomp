using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static AssertionsVerification;
using static BombTorizoHandArtworkVerification;
using static BombTorizoHandVisualsVerification;
using static LookupBombTorizoHandDrawVerification;
using static LookupBombTorizoHandProgramVerification;
using static SuiteTimingVerification;
using static SyntheticRoomCartridgeFixturesVerification;
using static VerificationHelpersVerification;

/// <summary>Checks Bomb Torizo's hand PLM from inventory gating through debris, VRAM transfer, and deletion.</summary>
internal static class BombTorizoHandVerification
{
    /// <summary>
    /// Locks Bomb Torizo's resident `$D6EA` header, Bombs inventory gate, odd-byte DMA,
    /// eight debris requests, final music command, and cartridge-timed deletion together.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyBombTorizoHandPlm(EnemyTileArtworkCatalog? installedArtwork = null)
    {
        Suite(nameof(VerifyBombTorizoHandProgramDefinitions), () => VerifyBombTorizoHandProgramDefinitions());
        Suite(nameof(VerifyBombTorizoHandVisualInstallation), () => VerifyBombTorizoHandVisualInstallation());
        var bus = new TestAddressSpace();
        const ushort population = 0x9000;
        const int width = 16;
        const int height = 16;

        SeedBombTorizoHandCartridge(bus, population);

        // Both physical draw payloads are compiled. They deliberately remain absent
        // from the synthetic bus so a fallback to the old decoder cannot pass.
        var guarded = NewBombTorizoHandProgramReadGuard(bus);

        var blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState();
        var plms = new RoomPlmSystem();
        var editedVisuals = new RoomPlmBombTorizoHandVisualCatalog(
        [
            new("intact", [0x0053, 0x0066, 0x0064, 0x0045,
                0x0046, 0x0047, 0x0048, 0x0049]),
            new("cleared", Enumerable.Repeat((ushort)0x00ff, 16).ToArray()),
        ]);
        plms.Drawing.BombTorizoHandVisuals = editedVisuals;

        AssertEqual(1, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "undefeated Bomb Torizo hand occupies PLM slot");
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "enemy header scan sees live Bomb Torizo hand");

        // First pass draws the closed hand; second installs D33B and reaches sleep. Many
        // additional passes prove that neither a countdown nor host wall clock bypasses it.
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        int handOrigin = level.GetBlockIndex(new(13, 11));
        AssertEqual((ushort)0x8065,
            level.GetCollisionBlockByIndex(handOrigin).LevelWord,
            "edited Bomb Torizo hand keeps the cartridge's physical level word");
        AssertEqual((ushort)0x0053,
            plms.TilemapUpdates[0].TopRow[0],
            "edited hand reaches the immediate tilemap update");
        AssertEqual((ushort)0x0053,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                handOrigin, 0).TopRow[0],
            "edited hand survives later background streaming");
        AssertEqual((ushort)0x8064, level.GetCollisionBlock(new(12, 11)).LevelWord,
            "multi-run hand geometry retains the second native run");
        AssertEqual((ushort)0x8049, level.GetCollisionBlock(new(14, 12)).LevelWord,
            "multi-run hand geometry retains the fourth native run");
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        for (int frame = 0; frame < 180; frame++)
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "hand remains resident while Samus lacks Bombs");
        AssertEqual(0, plms.BombTorizoHand.VramWriteRequests.Count,
            "sleeping hand performs no premature DMA");

        samus.CollectedItems |= (ushort)SamusEquipmentFlags.Bombs;
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }); // D33B skips sleep, starts 120-frame wait.
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "Bombs wake starts authored delay without deleting hand");

        for (int frame = 0; frame < 119; frame++)
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, plms.BombTorizoHand.VramWriteRequests.Count,
            "DMA waits for complete 120-frame post-pickup delay");
        plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(1, plms.BombTorizoHand.VramWriteRequests.Count,
            "120th frame emits one PLM VRAM write");
        AssertEqual(0x0400, plms.BombTorizoHand.VramWriteRequests[0].SizeInBytes,
            "hand DMA byte count");
        AssertEqual(0xadb200, plms.BombTorizoHand.VramWriteRequests[0].SourceAddress,
            "hand DMA source address");
        AssertEqual(0x6e00, plms.BombTorizoHand.VramWriteRequests[0].EncodedVramDestination,
            "hand DMA VRAM destination");

        if (installedArtwork is not null)
        {
            // #1162: checking only the emitted descriptor missed the actual failure.
            // Drain that exact production request through the runtime's installed-art
            // provider and RAM-only memory boundary, as the accepted NMI does.
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(memory, new SuperMetroid.Core.Runtime.HostGameplayOverrides(), initialPaletteArt: BombTorizoHandFixturePalette());
            runtime.Enemies.TileArtwork = installedArtwork;
            PlmVramWriteRequest request = plms.BombTorizoHand.VramWriteRequests[0];
            runtime.VramWrites.Enqueue(request.SizeInBytes, request.SourceAddress,
                VramDmaDestination.FromNative(request.EncodedVramDestination));
            runtime.VramWrites.DrainTo(runtime.Vram, memory, runtime.Artwork);
            AssertTrue(installedArtwork.TryResolve(request.SourceAddress,
                request.SizeInBytes, out ReadOnlyMemory<byte> expected),
                "Bomb Torizo PLM transfer resolves installed artwork");
            for (int offset = 0; offset < expected.Length; offset++)
                AssertEqual(expected.Span[offset],
                    runtime.Vram.ReadByte(request.EncodedVramDestination * 2 + offset),
                    $"Bomb Torizo debris DMA installs exact tile byte {offset:X4}");
            AssertEqual(0, runtime.VramWrites.TailInBytes,
                "successful debris DMA clears the pending queue");
        }

        var parameters = new List<ushort>();
        for (int frame = 0; frame < 400 && plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand); frame++)
        {
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            parameters.AddRange(plms.BombTorizoHand.BombTorizoStatueProjectileRequests.Select(
                request => request.Parameter));
        }
        AssertSequenceEqual(
            new ushort[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            parameters,
            "hand fragment parameters preserve cartridge order");
        AssertTrue(!plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "final delete removes header seen by Torizo AI");
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(handOrigin).LevelWord,
            "five-run cleared hand removes its physical origin block");
        AssertEqual(1, plms.BombTorizoHand.MusicRequests.Count,
            "terminal hand pass queues one music command");
        AssertEqual(
            new PlmMusicRequest(MusicCommand.SelectTrack(6), MusicCommandDelay.EightFrames),
            plms.BombTorizoHand.MusicRequests[0],
            "terminal hand music request");

        var defeated = new RoomPlmSystem();
        AssertEqual(1, defeated.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, streamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => true } }),
            "single loader still parses defeated hand record before setup deletes it");
        AssertTrue(!defeated.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "defeated setup exposes no transient hand header");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "complete hand lifecycle and defeated reload never reread program or draw bytes");

        Console.WriteLine(
            "  Bomb Torizo hand: inventory gate, DMA, debris cadence, music, and deletion agree.");
    }

    /// <summary>Compares hand draw geometry, physical words, instructions, callback, debris, and DMA operands with the cartridge.</summary>
    private static void VerifyBombTorizoHandProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Torizo hand oracle revision");
        Suite(nameof(VerifyBombTorizoHandDrawGeometry), () => VerifyBombTorizoHandDrawGeometry(rom));
        Suite(nameof(VerifyBombTorizoHandDrawCollision), () => VerifyBombTorizoHandDrawCollision(rom));
        Suite(nameof(VerifyBombTorizoHandDrawVisuals), () => VerifyBombTorizoHandDrawVisuals(rom));
        Suite(nameof(VerifyHandProgramControls), () => VerifyHandProgramControls(rom));
        Suite(nameof(VerifyHandProgramDraws), () => VerifyHandProgramDraws(rom));
        Suite(nameof(VerifyHandProgramCallback), () => VerifyHandProgramCallback(rom));
        Suite(nameof(VerifyHandProgramDebrisArguments), () => VerifyHandProgramDebrisArguments(rom));
        Suite(nameof(VerifyHandProgramTransferSize), () => VerifyHandProgramTransferSize(rom));
        Suite(nameof(VerifyHandProgramTransferSource), () => VerifyHandProgramTransferSource(rom));
        Suite(nameof(VerifyHandProgramTransferDestination), () => VerifyHandProgramTransferDestination(rom));
    }

    /// <summary>Forbids live reads from migrated hand instruction and draw-list bytes.</summary>
    private static CartridgeReadGuard NewBombTorizoHandProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address is >= 0x84d368 and <= 0x84d3c6 or >= 0x849877 and <= 0x8498d0,
            address => $"Bomb Torizo hand reread compiled program byte ${address:X6}.");

    /// <summary>Seeds the Bomb Torizo hand placement and its complete `$84:D368` instruction stream.</summary>
    static void SeedBombTorizoHandCartridge(TestAddressSpace bus, ushort population)
    {
        // One six-byte room-population record at the retail Bomb Torizo hand coordinate.
        SeedRoomPlmPopulation(bus, population,
            [0xea, 0xd6, 0x0d, 0x0b, 0x00, 0x00, 0x00, 0x00]);

        // This is the complete `$84:D368` stream. Keeping the seven unaligned operands of
        // `$87E5` byte-for-byte is important: parsing this as words changes the following
        // 96-frame duration into `$7700` and never reaches the fragment sequence.
        SeedPlmProgramBytes(bus, 0xd368,
        [
            0x01, 0x00, 0x77, 0x98,
            0xc1, 0x86, 0x3b, 0xd3,
            0xb4, 0x86,
            0x78, 0x00, 0x77, 0x98,
            0xe5, 0x87, 0x00, 0x04, 0x00, 0xb2, 0xad, 0x00, 0x6e,
            0x60, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x00, 0x00,
            0x30, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x02, 0x00,
            0x0f, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x04, 0x00,
            0x0e, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x06, 0x00,
            0x0d, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x08, 0x00,
            0x0c, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0a, 0x00,
            0x0b, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0c, 0x00,
            0x0a, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0e, 0x00,
            0x01, 0x00, 0x9d, 0x98,
            0xc7, 0xd3,
            0xbc, 0x86,
        ]);
    }

    /// <summary>Runs the complete Bomb Torizo hand PLM lifecycle verification.</summary>
    [VerificationCommand("--bomb-torizo-hand-plm-program")]
    internal static int BombTorizoHandPlmProgramCommand()
    {
        Suite(nameof(VerifyBombTorizoHandPlm), () => VerifyBombTorizoHandPlm());
        return 0;
    }
}
