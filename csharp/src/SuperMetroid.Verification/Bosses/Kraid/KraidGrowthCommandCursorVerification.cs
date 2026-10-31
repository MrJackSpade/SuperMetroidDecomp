using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadInstructionsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid's growth transition when a head command is interrupted by a foot lunge.</summary>
internal static class KraidGrowthCommandCursorVerification
{
    /// <summary>Verifies the offset-two command cursor, retained head image, 64-frame delay, and ROM-free execution.</summary>
    internal static void VerifyKraidGrowthCommandCursor()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(rom));
        var guard = new KraidHeadProgramReadGuard(rom);
        var referenceArt = new EnemyIdentityFixture().Build().Bosses.KraidBackground;
        var head = KraidHeadTilemapAtlas.Load(new MemoryStream(KraidHeadTilemapFiles.Encode(
            new byte[KraidWorkingMapDefinitions.HeadImageCells * 2])));
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), BossArtwork = new BossSourceArtwork { KraidBackground = new KraidBackgroundArtwork((referenceArt ?? throw new InvalidOperationException("Verification requires referenceArt.")).Upper, referenceArt.Lower,
                    new Dictionary<int, KraidHeadTilemapAtlas> { [2] = head }, referenceArt.RoomBackgroundTiles) },
        };
        var state = new KraidEnemyState();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        enemies.KraidBehavior.Access.Kraid = state;
        Func<RoomEnemySlot, KraidEnemyState, ushort> execute = enemies.KraidBehavior.Head.ExecuteKraidHeadInstruction;
        Action<RoomEnemySlot, ushort> footMain = (foot, cameraY) => enemies.KraidBehavior.Limbs.RunKraidFootMain(foot, cameraY);
        var body = enemies.SlotTable.Slots[0];
        var foot = enemies.SlotTable.Slots[5];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        state.InitialHealth = 1000;
        body.Health = 874;
        state.HeadProgram = native.CursorAt(0x96e2);
        execute(body, state);
        AssertEqual(native.CursorAt(0x96ea), state.HeadProgram, "head interpreter leaves the next cursor on the roar command");
        KraidHeadFrame? displayed = state.DisplayedHeadFrame;
        state.Parts[5].Function = KraidAiFunction.FootPrepareFirstPhaseLunge;
        footMain(foot, 0);
        AssertEqual(native.CursorAt(0x96f4), state.HeadProgram, "native command-cursor growth resumes at the closing-mouth frame");
        AssertEqual(64, state.Parts[0].HeadTimer, "native quick-kill growth delay is preserved");
        AssertEqual(displayed, state.DisplayedHeadFrame, "growth selection does not advance the displayed head");
        AssertEqual(KraidAiFunction.GrowReleaseCamera, state.Parts[0].NextFunction, "growth schedules camera release");
        AssertEqual(180, state.Parts[0].FunctionTimer, "growth function timer");
        AssertEqual(KraidAiFunction.NoOperation, state.Parts[5].Function, "growth stops the foot lunge");
        AssertEqual(0, guard.ForbiddenReadAttempts, "growth selection does not read compiled cartridge programs");

        // Confirm the growth resume for every cursor form, including cries and list ends adjacent to the next list:
        // the cartridge selects it from the word two bytes past the cursor.
        foreach (KraidHeadNativeRecord record in native.Records)
        {
            int address = 0xa70000 | (record.Pointer + 2);
            ushort word = LittleEndianBytes.Word(KraidGrowthCommandCursorRomByte(rom, address), KraidGrowthCommandCursorRomByte(rom, address + 1));
            (ushort pointer, ushort timer) = KraidContourCaseMappingsVerification.NativeKraidGrowthResume(rom, word);
            KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(record.Cursor).Growth;
            AssertEqual(native.CursorAt(pointer), KraidHeadAnimations.GrowthResumeCursor(resume),
                $"native growth resume cursor from ${record.Pointer:X4}");
            AssertEqual(timer, KraidHeadAnimations.GrowthResumeDelay(resume), $"native growth resume delay from ${record.Pointer:X4}");
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
