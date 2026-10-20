using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadInstructionsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid's growth transition when a head command is interrupted by a foot lunge.</summary>
internal static class KraidGrowthCommandCursorVerification
{
    /// <summary>Verifies the offset-two command cursor, retained head image, 64-frame delay, and ROM-free execution.</summary>
    internal static void VerifyKraidGrowthCommandCursor()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(rom));
        var guard = new KraidHeadProgramReadGuard(rom);
        var referenceArt = new EnemyIdentityFixture().Build().KraidBackground;
        var head = KraidHeadTilemapAtlas.Load(new MemoryStream(KraidHeadTilemapAtlas.Encode(
            new byte[KraidBackgroundRomData.HeadTilemapWords * 2])));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), KraidBackground = new KraidBackgroundArtwork((referenceArt ?? throw new InvalidOperationException("Verification requires referenceArt.")).Upper, referenceArt.Lower,
                    new Dictionary<ushort, KraidHeadTilemapAtlas> { [0x9dc8] = head }, referenceArt.RoomBackgroundTiles) }),
        };
        var state = new KraidEnemyState();
        enemies.Presentation.BindVram(BgArtworkFixtures.For(new SnesVram()));
        enemies.KraidBehavior.Kraid = state;
        Func<RoomEnemySlot, KraidEnemyState, ushort> execute = enemies.KraidBehavior.ExecuteKraidHeadInstruction;
        Action<RoomEnemySlot, ushort> footMain = enemies.KraidBehavior.RunKraidFootMain;
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
            ushort word = SnesLittleEndian.Word(KraidGrowthCommandCursorRomByte(rom, address), KraidGrowthCommandCursorRomByte(rom, address + 1));
            (ushort pointer, ushort timer) = KraidContourCaseMappingsVerification.NativeKraidGrowthResume(rom, word);
            KraidHeadGrowthResume resume = KraidHeadAnimations.ReadsAt(record.Cursor).Growth;
            AssertEqual(native.CursorAt(pointer), KraidHeadAnimations.GrowthResumeCursor(resume),
                $"native growth resume cursor from ${record.Pointer:X4}");
            AssertEqual(timer, KraidHeadAnimations.GrowthResumeDelay(resume), $"native growth resume delay from ${record.Pointer:X4}");
        }
        Console.WriteLine("Kraid growth command cursor: reported foot-lunge transition, native 64-frame delay, retained display, and the growth resume of every native cursor pass.");
    }

    /// <summary>Reads one native command byte as an independent growth-selection oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid growth command cursor")]
    private static byte KraidGrowthCommandCursorRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the Kraid growth command-cursor regression.</summary>
    [VerificationCommand("--kraid-growth-command-cursor")]
    internal static int KraidGrowthCommandCursorCommand()
    {
        Suite(nameof(VerifyKraidGrowthCommandCursor), () => VerifyKraidGrowthCommandCursor());
        return 0;
    }
}
