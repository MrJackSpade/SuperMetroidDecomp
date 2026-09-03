using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Drives a constructed bank-$82 Baby stream through a sound opcode and end-loop,
    /// then reaches the live menu to cover its fixed text and selection layout.
    /// </summary>
    static void VerifyGameOverRomData()
    {
        AssertEqual(5, GameOverRomData.Text.All.Length, "game-over text stream count");
        AssertEqual(4, GameOverRomData.Sprites.MissileFrameIds.Length,
            "game-over missile cursor frame count");
        AssertEqual(GameOverRomData.TilemapWidth * sizeof(ushort),
            GameOverRomData.TilemapRowByteCount,
            "game-over tilemap row stride");

        var bus = new TestAddressSpace();
        SeedGameOverText(bus);
        SeedGameOverBabyAnimation(bus);
        var audio = new CartridgeAudioState();
        var menu = new GameOverMenuState(bus, audio);

        menu.Step(0);
        AssertEqual(GameOverRomData.BabyAnimation.FirstInstruction,
            menu.BabyInstructionPointer,
            "game-over Baby starts at the catalogued instruction");
        AssertEqual(GameOverRomData.BabyAnimation.InitialSpritemap,
            menu.BabySpritemap,
            "game-over Baby starts on the first frame");

        StepFrames(9, _ => menu.Step(0));
        AssertEqual(0xbc2f, menu.BabyInstructionPointer,
            "game-over Baby consumes the eight-byte cry opcode");
        AssertEqual(0x0066, menu.BabySpritemap,
            "game-over Baby advances to the constructed second frame");
        AssertTrue(audio.HasQueuedSounds,
            "game-over Baby cry enters the typed library-three queue");

        StepFrames(2, _ => menu.Step(0));
        AssertEqual(GameOverRomData.BabyAnimation.FirstInstruction,
            menu.BabyInstructionPointer,
            "game-over Baby end marker loops to the first instruction");
        AssertEqual(GameOverRomData.BabyAnimation.InitialSpritemap,
            menu.BabySpritemap,
            "game-over Baby loop restores its first frame");

        int framesToMenu = StepUntil(
            () => menu.Phase == GameOverMenuPhase.Main,
            frame =>
            {
                menu.Step(0);
                _ = audio.AdvanceFrame(bus, default);
            },
            maximumFrames: 160,
            "constructed game-over menu fade-in");
        AssertTrue(framesToMenu > GameOverRomData.MaximumBrightness,
            "game-over menu waits for queued music before fading");

        menu.Step((ushort)SnesButton.Down);
        AssertEqual(1, menu.SelectedItem, "game-over fixed two-row selection toggles to No");
        AssertEqual(FrontendFrame.Width * FrontendFrame.Height, menu.Render().Length,
            "game-over fixed menu layout renders one frontend frame");

        AssertThrows<InvalidDataException>(
            () => GameOverRomData.BabyAnimation.ResolveCry(0xbeef),
            "unknown game-over Baby opcode fails loudly");

        Console.WriteLine(
            "  Game over: ROM catalog, text layout, Baby sound/frame stream, loop, " +
            "music wait, cursor, and frame geometry agree.");
    }

    private static void SeedGameOverText(TestAddressSpace bus)
    {
        ReadOnlySpan<GameOverTextStream> streams = GameOverRomData.Text.All;
        WriteTestWords(
            bus,
            GameOverRomData.TextBank | streams[0].SourcePointer,
            0x1234,
            GameOverRomData.TextNextLine,
            0x5678,
            GameOverRomData.TextEnd);
        for (int index = 1; index < streams.Length; index++)
        {
            WriteTestWord(
                bus,
                GameOverRomData.TextBank | streams[index].SourcePointer,
                GameOverRomData.TextEnd);
        }
    }

    private static void SeedGameOverBabyAnimation(TestAddressSpace bus)
    {
        int first = GameOverRomData.SpriteBank | GameOverRomData.BabyAnimation.FirstInstruction;
        WriteTestWords(
            bus,
            first,
            GameOverRomData.BabyAnimation.InitialFrameDuration,
            GameOverRomData.BabyAnimation.InitialSpritemap,
            0xbd00,
            GameOverRomData.BabyAnimation.CryOpcode23,
            2,
            0x0066,
            0xbd20,
            GameOverRomData.BabyAnimation.End);
    }
}
