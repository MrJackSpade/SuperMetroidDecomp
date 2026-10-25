using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianArtworkVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianDescriptorsVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianProgramVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianSpawnVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World;

/// <summary>Checks migrated Tourian statue eye positions and animated-tile control against the cartridge.</summary>
internal static class TourianStatueUnlockDefinitionsVerification
{
    /// <summary>Matches all four eye coordinates to native tables and checks guarded eye/soul projectile spawns.</summary>
    internal static void VerifyTourianStatueUnlockDefinitions(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        {
            Position16<RoomPixels> position =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            AssertEqual(rom.ReadCartridgeWord((0x86b90e + parameter)), position.X,
                $"Tourian statue eye X parameter {parameter}");
            AssertEqual(rom.ReadCartridgeWord((0x86b916 + parameter)), position.Y,
                $"Tourian statue eye Y parameter {parameter}");
        }

        var guarded = NewTourianStatueEyeReadGuard(rom);
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        for (int soulValue = 0; soulValue < 2; soulValue++)
        {
            bool soul = soulValue != 0;
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
            enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
            if (soul) enemies.TourianUnlockBehavior.SpawnTourianSoul(parameter);
            else enemies.TourianUnlockBehavior.SpawnTourianEyeGlow(parameter);

            Position16<RoomPixels> expected =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            RoomEnemyProjectileSlot projectile =
                enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(expected.X, projectile.Position.X.Whole,
                $"production Tourian unlock X {parameter}/{soul}");
            AssertEqual(expected.Y, projectile.Position.Y.Whole,
                $"production Tourian unlock Y {parameter}/{soul}");
            AssertEqual(soul ? TourianStatueNativeWords.Soul : TourianStatueNativeWords.EyeGlow,
                (ushort)projectile.Kind,
                $"production Tourian unlock kind {parameter}/{soul}");
            AssertEqual(soul ? 0xfc00 : 0,
                projectile.Velocity.Y.RawValue,
                $"production Tourian unlock velocity {parameter}/{soul}");
        }

        foreach (ushort parameter in new ushort[] { 1, 3, 5, 7, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => TourianStatueUnlockDefinitions.EyePosition(parameter),
                $"invalid Tourian statue parameter {parameter}");
        }

        Suite(nameof(VerifyTourianStatueAnimatedTileMechanics), () => VerifyTourianStatueAnimatedTileMechanics(rom));

        Console.WriteLine(
            "Tourian statue eye positions: eight native words and all eight real eye/soul spawns pass with position tables forbidden.");
    }

    /// <summary>Checks the statues' typed programs and pictures against the cartridge, then runs the retail unlock sequence with program and artwork reads denied.</summary>
    private static void VerifyTourianStatueAnimatedTileMechanics(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
        Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
        Suite(nameof(VerifyTourianStatueProgramMappings), () => VerifyTourianStatueProgramMappings(rom));
        Suite(nameof(VerifyTourianStatueSpawnOrder), () => VerifyTourianStatueSpawnOrder(rom));
        int pictureCount = 0;
        RoomFxAnimatedTileAtlas artwork = RoomFxAnimatedTileAtlasPng.Read(
            new MemoryStream(RoomFxAnimatedTileAtlasExtractor.Extract(rom)));
        foreach (TourianStatueAnimatedTileProgramDefinition definition in
                 TourianStatueAnimatedTileMechanicsDefinitions.All)
        foreach (TourianStatueTilePicture picture in TourianStatueTileStripDefinitions.Pictures)
        {
            int source = TourianStatueTileStripDefinitions.SourceAddress(definition.AnimatedObject, picture);
            byte[] native = ReadNativeTourianStatueFrame(rom, source, definition.FrameByteCount);
            AssertTrue(artwork.TryResolve(source, native.Length,
                    out ReadOnlyMemory<byte> installed) &&
                installed.Span.SequenceEqual(native),
                $"statue {definition.AnimatedObject} picture {picture} preserves every cartridge pixel byte");
            pictureCount++;
        }

        AssertEqual(4, TourianStatueAnimatedTileMechanicsDefinitions.All.Count(),
            "Tourian statue animated-tile object count");
        AssertEqual(16, pictureCount, "Tourian statue installed picture count");

        // Every byte of the four native headers and lists, frame operands included, is the oracle's alone.
        var programOffsets = new HashSet<int>();
        foreach (ushort header in OriginalTourianObjects)
        {
            for (int offset = 0; offset < 3 * sizeof(ushort); offset++)
                programOffsets.Add(header + offset);
            foreach (var instruction in OriginalTourianInstructions(rom, header))
                for (int offset = 0; offset < instruction.Width; offset++)
                    programOffsets.Add(instruction.Cursor + offset);
        }

        bool forbidArtworkReads = false;
        var guarded = new MutableCartridgeReadGuard(rom);
        guarded
            .Forbid(address => forbidArtworkReads &&
                    address >= TourianStatueTileStripDefinitions.FirstSource &&
                    address < TourianStatueTileStripDefinitions.SourceEnd,
                address => $"Installed Tourian statue artwork read cartridge byte ${address:X6}.")
            .Forbid(address => IsTourianStatueProgramByte(programOffsets, address),
                address => $"Production read Tourian statue program byte {SnesAddress.FromBusAddress(address)}.");
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Statues);
        AssertTrue(runtime.TourianStatues.Enabled,
            "retail Tourian statue room enables the production sequence");
        forbidArtworkReads = true;

        int steps = 0;
        for (; steps < 3000 && !TourianStatueGreyEventsAreSet(runtime); steps++)
            runtime.TourianStatues.StepTiles();

        AssertTrue(TourianStatueGreyEventsAreSet(runtime),
            "production Tourian sequence releases all four defeated-boss statues");
        AssertTrue(steps < 3000, "production Tourian sequence remains bounded");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production Tourian sequence performs no program or artwork ROM reads");
        Console.WriteLine(
            $"Tourian statue animated tiles: typed programs and {pictureCount} pictures match the cartridge, " +
            $"with ROM-free art transfer; release completes in {steps} calls.");
    }

    /// <summary>Identifies a byte of a native bank-$87 statue header or list for the runtime read guard.</summary>
    private static bool IsTourianStatueProgramByte(HashSet<int> programOffsets, int address)
    {
        SnesAddress snesAddress = SnesAddress.FromBusAddress(address);
        return snesAddress.Bank == SnesLittleEndianConstants.BankByte(RoomFxRomData.Banks.AnimatedTiles) &&
            programOffsets.Contains(snesAddress.Offset);
    }

    /// <summary>Checks that all four defeated-boss statue release events have been recorded.</summary>
    private static bool TourianStatueGreyEventsAreSet(SuperMetroidRuntime runtime) =>
        runtime.System.HasEvent(EventNumber.PhantoonStatueGrey) &&
        runtime.System.HasEvent(EventNumber.RidleyStatueGrey) &&
        runtime.System.HasEvent(EventNumber.DraygonStatueGrey) &&
        runtime.System.HasEvent(EventNumber.KraidStatueGrey);

    /// <summary>Forbids production reads of the migrated bank-$86 eye-coordinate tables.</summary>
    private static CartridgeReadGuard NewTourianStatueEyeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86b90e and < 0x86b91e,
            address => $"Tourian statue unlock attempted migrated eye-position read ${address:X6}.");

    /// <summary>Reads one native animated-tile frame for the independent pixel-data comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian statue animated-tile frame pixels")]
    private static byte[] ReadNativeTourianStatueFrame(IImportCartridgeSource rom, int source, int byteCount) =>
        RomDataReader.ReadFixedBank(rom, source, byteCount);

    /// <summary>Runs retail-cartridge statue unlock and animated-tile verification.</summary>
    [VerificationCommand("--tourian-statue-animated-tiles")]
    internal static int TourianStatueAnimatedTilesCommand()
    {
        Suite(nameof(VerifyTourianStatueUnlockDefinitions), () => VerifyTourianStatueUnlockDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
