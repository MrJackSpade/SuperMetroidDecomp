using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Compares the live Baby animation against the original bank-$82 stream,
    /// then reaches the live menu to cover its fixed text and selection layout.
    /// </summary>
    static void VerifyGameOverRomData()
    {
        AssertEqual(GameOverRomData.TilemapWidth * sizeof(ushort),
            GameOverRomData.TilemapRowByteCount,
            "game-over tilemap row stride");

        var bus = new TestAddressSpace();
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int offset) => (ushort)(rom.ReadCartridgeByte(0x820000 | (ushort)offset) |
            rom.ReadCartridgeByte(0x820000 | (ushort)(offset + 1)) << 8);
        var audio = new CartridgeAudioState();
        var menu = new GameOverMenuState(bus, audio, RetailPresentationFixture());
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", flags)!.GetValue(audio)!;
        var writes = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", flags)!.GetValue(audio)!;
        ushort pointer = GameOverRomData.BabyAnimation.FirstInstruction;
        int timer = Word(pointer), cries = 0, records = 0, ticks = 0;
        bool looped = false;
        while (!looped && ticks < 1000)
        {
            if (--timer == 0)
            {
                records++;
                int next = pointer + 6;
                ushort command = Word(next);
                if (command == ushort.MaxValue)
                {
                    next = GameOverRomData.BabyAnimation.FirstInstruction;
                    looped = true;
                }
                else if ((command & 0x8000) != 0)
                {
                    // Native cry callbacks load their effect as an immediate word.
                    AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x820000 | command),
                        "game-over reference cry starts with LDA immediate");
                    menu.Step(0);
                    AssertEqual((byte)Word(command + 1), queues[2, cries],
                        "game-over cry queues the exact native library-three effect");
                    cries++;
                    next += 2;
                }
                else menu.Step(0);
                if (looped) menu.Step(0);
                pointer = (ushort)next;
                timer = Word(pointer);
            }
            else menu.Step(0);
            ticks++;
            AssertEqual(pointer, menu.BabyInstructionPointer,
                "game-over live pointer matches independent ROM stream each tick");
            AssertEqual(Word(pointer + 2), menu.BabySpritemap,
                "game-over live spritemap matches independent ROM stream each tick");
            AssertEqual((byte)cries, writes[2], "game-over cry queue changes only at native handoffs");
        }
        AssertTrue(looped, "game-over Baby reaches the native end marker and loops");
        AssertEqual(60, records, "game-over loop visits all sixty native frame records");
        AssertEqual(3, cries, "game-over loop dispatches all three cries");
        AssertTrue(audio.HasQueuedSounds, "game-over Baby cries remain in the typed queue");
        int framesToMenu = StepUntil(
            () => menu.Phase == GameOverMenuPhase.Main,
            frame =>
            {
                menu.Step(0);
                _ = audio.AdvanceFrame(bus, default);
            },
            maximumFrames: 160,
            "game-over menu fade-in");
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

}
