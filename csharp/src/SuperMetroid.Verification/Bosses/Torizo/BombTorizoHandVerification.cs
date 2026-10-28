using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoHandVisualsVerification;
using static SuperMetroid.Verification.Bosses.Torizo.LookupBombTorizoHandDrawVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.World;

/// <summary>Checks Bomb Torizo's hand PLM from inventory gating through debris, VRAM transfer, and deletion.</summary>
internal static class BombTorizoHandVerification
{
    /// <summary>
    /// Locks Bomb Torizo's resident `$D6EA` header, Bombs inventory gate, crumble delay, eight debris
    /// requests, final music command, and cartridge-timed deletion together.
    /// </summary>
    internal static void VerifyBombTorizoHandPlm()
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
        var samus = new SamusState();
        var plms = PlmFixtures.Unbound();
        var editedVisuals = new RoomPlmBombTorizoHandVisualCatalog(
        [
            new("intact", [0x0053, 0x0066, 0x0064, 0x0045,
                0x0046, 0x0047, 0x0048, 0x0049]),
            new("cleared", Enumerable.Repeat((ushort)0x00ff, 16).ToArray()),
        ]);
        PlmFixtures.VisualsOf(plms).RoomPlmBombTorizoHandVisuals = editedVisuals;

        AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "undefeated Bomb Torizo hand occupies PLM slot");
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "enemy header scan sees live Bomb Torizo hand");

        // First pass draws the closed hand; second installs D33B and reaches sleep. Many
        // additional passes prove that neither a countdown nor host wall clock bypasses it.
        plms.Step(level);
        int handOrigin = level.GetBlockIndex(new(13, 11));
        AssertEqual((ushort)0x8065,
            level.GetCollisionBlockByIndex(handOrigin).PackedWord.Raw,
            "edited Bomb Torizo hand keeps the cartridge's physical level word");
        AssertEqual((ushort)0x0053,
            level.DisplayedBlockTiles(handOrigin).TopLeft.Raw,
            "edited hand survives later background streaming");
        AssertEqual((ushort)0x8064, level.GetCollisionBlock(new(12, 11)).PackedWord.Raw,
            "multi-run hand geometry retains the second native run");
        AssertEqual((ushort)0x8049, level.GetCollisionBlock(new(14, 12)).PackedWord.Raw,
            "multi-run hand geometry retains the fourth native run");
        plms.Step(level);
        for (int frame = 0; frame < 180; frame++)
            plms.Step(level);
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "hand remains resident while Samus lacks Bombs");
        AssertEqual(0, plms.BombTorizoHand.BombTorizoStatueProjectileRequests.Count,
            "sleeping hand drops no premature fragment");

        samus.CollectedItems |= SamusEquipmentFlags.Bombs;
        plms.Step(level); // D33B skips sleep, starts 120-frame wait.
        AssertTrue(plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "Bombs wake starts authored delay without deleting hand");

        for (int frame = 0; frame < 119; frame++)
            plms.Step(level);
        AssertEqual(0, plms.BombTorizoHand.BombTorizoStatueProjectileRequests.Count,
            "the hand holds through its 120-frame post-pickup delay");
        plms.Step(level);
        var fragments = new List<int>();
        int firstFragmentFrame = 0;
        for (int frame = 1; frame <= 400 && plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand); frame++)
        {
            plms.Step(level);
            if (fragments.Count == 0 && plms.BombTorizoHand.BombTorizoStatueProjectileRequests.Count > 0)
                firstFragmentFrame = frame;
            fragments.AddRange(plms.BombTorizoHand.BombTorizoStatueProjectileRequests.Select(
                request => request.Fragment));
        }
        // The crumble starts on the delay's last frame; the first fragment drops its own delay later, with no step
        // between them for the debris characters the cartridge loaded there (#1406).
        AssertEqual(BombTorizoHandPlmProgramDefinitions.FragmentDelay(0), firstFragmentFrame,
            "the first fragment drops its authored delay after the crumble begins");
        AssertSequenceEqual(
            new[] { 0, 1, 2, 3, 4, 5, 6, 7 },
            fragments,
            "hand fragments preserve cartridge order");
        AssertTrue(!plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "final delete removes header seen by Torizo AI");
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(handOrigin).PackedWord.Raw,
            "five-run cleared hand removes its physical origin block");
        AssertEqual(1, plms.BombTorizoHand.MusicRequests.Count,
            "terminal hand pass queues one music command");
        AssertEqual(
            new DelayedMusicRequest(MusicRequest.SelectTrack(6), MusicQueueDelay.Shortest),
            plms.BombTorizoHand.MusicRequests[0],
            "terminal hand music request");

        var defeated = PlmFixtures.Unbound();
        AssertEqual(1, PlmFixtures.LoadPopulation(ref defeated, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => true } }),
            "single loader still parses defeated hand record before setup deletes it");
        AssertTrue(!defeated.BombTorizoHand.HasActiveHeader(PlmHeaderId.BombTorizoHand),
            "defeated setup exposes no transient hand header");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "complete hand lifecycle and defeated reload never reread program or draw bytes");

        Console.WriteLine(
            "  Bomb Torizo hand: inventory gate, crumble delay, debris cadence, music, and deletion agree.");
    }

    /// <summary>Compares hand draw geometry, physical words, instructions, callback, debris, and DMA operands with the cartridge.</summary>
    private static void VerifyBombTorizoHandProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Torizo hand oracle revision");
        Suite(nameof(VerifyBombTorizoHandDrawGeometry), () => VerifyBombTorizoHandDrawGeometry(rom));
        Suite(nameof(VerifyBombTorizoHandDrawCollision), () => VerifyBombTorizoHandDrawCollision(rom));
        Suite(nameof(VerifyBombTorizoHandDrawVisuals), () => VerifyBombTorizoHandDrawVisuals(rom));
        Suite(nameof(BossRoomPlmProgramsVerification.VerifyBombTorizoHandPrograms), () => BossRoomPlmProgramsVerification.VerifyBombTorizoHandPrograms(rom));
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
