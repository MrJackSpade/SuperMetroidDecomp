using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadInstructionsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks Kraid's growth transition when a head command is interrupted by a foot lunge.</summary>
internal static class KraidGrowthCommandCursorVerification
{
    /// <summary>Verifies the offset-two command cursor, retained head image, 64-frame delay, and ROM-free execution.</summary>
    internal static void VerifyKraidGrowthCommandCursor()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var native = new KraidHeadNativeCursors(rom);
        var guard = new KraidHeadProgramReadGuard(rom);
        var referenceArt = new EnemyIdentityFixture().Build().Bosses.KraidBackground;
        var head = KraidHeadTilemapAtlas.Load(new MemoryStream(KraidHeadTilemapFiles.Encode(
            new byte[KraidWorkingMapDefinitions.HeadImageCells * 2])));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EnemyArtworkParts with { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BossArtwork with { KraidBackground = new KraidBackgroundArtwork((referenceArt ?? throw new InvalidOperationException("Verification requires referenceArt.")).Upper, referenceArt.Lower,
                    new Dictionary<int, KraidHeadTilemapAtlas> { [2] = head }, referenceArt.RoomBackgroundTiles) });
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        KraidEnemyState state = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        Func<KraidEnemyState, KraidHeadAnimationResult> execute = enemies.KraidBehavior.Head.RunKraidHeadAnimationStep;
        Action<RoomEnemyPlace, ushort> footMain = (foot, cameraY) => enemies.KraidBehavior.Limbs.RunKraidFootMain(foot, state.FootState, cameraY);
        var body = enemies.Places.InUpdateOrder[0];
        var foot = enemies.Places.InUpdateOrder[5];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        state.InitialHealth = 1000;
        body.Health = 874;
        state.HeadPlayback = native.PlaybackAt(0x96e2);
        execute(state);
        AssertEqual(native.PlaybackAt(0x96ea), state.HeadPlayback, "head interpreter leaves the next cursor on the roar command");
        KraidHeadFrame? displayed = state.DisplayedHeadFrame;
        state.FootState.Phase = KraidFootPhase.PrepareFirstPhaseLunge;
        footMain(foot, 0);
        AssertEqual(native.PlaybackAt(0x96f4), state.HeadPlayback, "native command-cursor growth resumes at the closing-mouth frame");
        AssertEqual(64, state.HeadHold.UpdatesLeft, "native quick-kill growth delay is preserved");
        AssertEqual(displayed, state.DisplayedHeadFrame, "growth selection does not advance the displayed head");
        // The growth's wait names its own continuation, the camera's release (#2208).
        AssertEqual(KraidFightPhase.HeadAnimationBeforeGrowth, state.BodyState.FightPhase, "growth runs the head animation before releasing the camera");
        AssertEqual(180, state.BodyState.PhaseWait.UpdatesLeft, "growth's head animation lasts 180 updates");
        AssertEqual(KraidFootPhase.Resting, state.FootState.Phase, "growth rests the foot from its lunge");
        AssertEqual(0, guard.ForbiddenReadAttempts, "growth selection does not read compiled cartridge programs");

        // Confirm the growth resume for every cursor form, including cries and list ends adjacent to the next list:
        // the cartridge selects it from the word two bytes past the cursor.
        foreach (KraidHeadNativeRecord record in native.Records)
        {
            int address = 0xa70000 | (record.Pointer + 2);
            ushort word = LittleEndianBytes.Word(KraidGrowthCommandCursorRomByte(rom, address), KraidGrowthCommandCursorRomByte(rom, address + 1));
            (ushort pointer, ushort timer) = KraidContourCaseMappingsVerification.NativeKraidGrowthResume(rom, word);
            KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(record.Playback).Growth;
            KraidHeadEntry resumed = KraidHeadAnimations.GrowthResume(resume);
            AssertEqual(pointer, native.PointerAt(resumed.Playback), $"native growth resume cursor from ${record.Pointer:X4}");
            AssertEqual(timer, resumed.Delay, $"native growth resume delay from ${record.Pointer:X4}");
        }
        Console.WriteLine("Kraid growth command cursor: reported foot-lunge transition, native 64-frame delay, retained display, and the growth resume of every native cursor pass.");
    }

    /// <summary>Reads one native command byte as an independent growth-selection oracle.</summary>
    private static byte KraidGrowthCommandCursorRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Kraid growth command-cursor regression.</summary>
    [VerificationCommand("--kraid-growth-command-cursor")]
    internal static int KraidGrowthCommandCursorCommand()
    {
        Suite(nameof(VerifyKraidGrowthCommandCursor), () => VerifyKraidGrowthCommandCursor());
        return 0;
    }
}
