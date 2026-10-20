using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.GameOver;
using SuperMetroid.Verification.Harness;

/// <summary>Compares the game-over Baby instruction stream and cries with
/// cartridge data, then checks the live menu layout and selection.</summary>
internal static class GameOverRomDataVerification
{
    /// <summary>
    /// Compares the live Baby animation against the original bank-$82 stream,
    /// then reaches the live menu to cover its fixed text and selection layout.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the game-over Baby animation stream")]
    internal static void VerifyGameOverRomData()
    {
        AssertEqual(GameOverRomData.TilemapWidth * sizeof(ushort),
            GameOverRomData.TilemapRowByteCount,
            "game-over tilemap row stride");

        var bus = new TestAddressSpace();
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var audio = new CartridgeAudioState();
        var menu = new GameOverMenuState(audio, RetailPresentationFixture());
        // The Baby stream advances only in indexes one and three onward. A separate menu walks
        // index zero and its NMI continuation, takes index one's first Baby tick, and then
        // enters the fade so every later update ticks the Baby; the music wait and index
        // one's continuation never touch it, so skipping them leaves the stream intact.
        var babyAudio = new CartridgeAudioState();
        var babyMenu = new GameOverMenuState(babyAudio, RetailPresentationFixture());
        babyMenu.Step(0);
        babyMenu.Step(0);
        void StepBaby()
        {
            babyMenu.Step(0);
            if (babyMenu.ResumesAfterNmiWait)
            {
                babyMenu.ResumesAfterNmiWait = false;
                babyMenu.Phase = GameOverMenuPhase.FadeIn;
            }
        }
        var queues = babyAudio.SoundQueues;
        var writes = babyAudio.SoundWritePositions;
        ushort pointer = GameOverRomData.BabyAnimation.FirstInstruction;
        // Record order in the native stream is the program's step identity.
        int step = 0;
        int timer = rom.ReadCartridgeWordFixedBank(0x820000 | pointer), cries = 0, records = 0, ticks = 0;
        bool looped = false;
        while (!looped && ticks < 1000)
        {
            if (--timer == 0)
            {
                records++;
                int next = pointer + 6;
                ushort command = rom.ReadCartridgeWordFixedBank(0x820000 | (ushort)next);
                if (command == ushort.MaxValue)
                {
                    next = GameOverRomData.BabyAnimation.FirstInstruction;
                    step = -1;
                    looped = true;
                }
                else if ((command & 0x8000) != 0)
                {
                    // Native cry callbacks load their effect as an immediate word.
                    AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x820000 | command),
                        "game-over reference cry starts with LDA immediate");
                    StepBaby();
                    // Library-three slot zero holds index zero's CancelAll request.
                    AssertEqual((byte)rom.ReadCartridgeWordFixedBank(0x820000 | (ushort)(command + 1)), queues[2, cries + 1],
                        "game-over cry queues the exact native library-three effect");
                    cries++;
                    next += 2;
                }
                else StepBaby();
                if (looped) StepBaby();
                pointer = (ushort)next;
                step++;
                timer = rom.ReadCartridgeWordFixedBank(0x820000 | pointer);
            }
            else StepBaby();
            ticks++;
            AssertEqual(step, babyMenu.BabyStep,
                "game-over live step matches independent ROM stream record each tick");
            AssertEqual(rom.ReadCartridgeWordFixedBank(0x820000 | (ushort)(pointer + 2)),
                GameOverBabyAnimationDefinitions.NativeSpritemap(babyMenu.BabyFrame),
                "game-over live pose matches independent ROM stream spritemap each tick");
            AssertEqual((byte)(cries + 1), writes[2], "game-over cry queue changes only at native handoffs");
        }
        AssertTrue(looped, "game-over Baby reaches the native end marker and loops");
        AssertEqual(60, records, "game-over loop visits all sixty native frame records");
        AssertEqual(3, cries, "game-over loop dispatches all three cries");
        AssertTrue(babyAudio.HasQueuedSounds, "game-over Baby cries remain in the typed queue");
        int framesToMenu = StepUntil(
            () => menu.Phase == GameOverMenuPhase.Main,
            frame =>
            {
                menu.Step(0);
                _ = audio.AdvanceFrame(default);
            },
            maximumFrames: 160,
            "game-over menu fade-in");
        AssertTrue(framesToMenu > GameOverRomData.MaximumBrightness,
            "game-over menu waits for queued music before fading");

        menu.Step(SnesButton.Down);
        AssertEqual(YesNoChoice.No, menu.SelectedItem, "game-over fixed two-row selection toggles to No");
        AssertEqual(FrontendFrame.Width * FrontendFrame.Height, menu.Render().Length,
            "game-over fixed menu layout renders one frontend frame");

        AssertThrows<InvalidDataException>(
            () => GameOverRomData.BabyAnimation.ResolveCry(0xbeef),
            "unknown game-over Baby opcode fails loudly");

        Console.WriteLine(
            "  Game over: ROM catalog, text layout, Baby sound/frame stream, loop, " +
            "music wait, cursor, and frame geometry agree.");
    }

    /// <summary>Runs the game-over animation and menu fixture from the CLI.</summary>
    [VerificationCommand("--game-over-fixture")]
    internal static int GameOverFixtureCommand()
    {
     VerifyGameOverRomData(); return 0; 
    }
}
