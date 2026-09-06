using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Runs every natural title card, pan, zoom, hold, and fade, then separately proves
    /// that the native confirm-button shortcut still takes its fade-out/fade-in route.
    /// </summary>
    static void VerifyTitleSequenceRomData()
    {
        byte[] rom = CreateConstructedTitleRom();
        var state = new TitleSequenceState(new SuperMetroidAddressSpace(rom));
        TitleSequencePhase[] naturalOrder =
        [
            TitleSequencePhase.YearText,
            TitleSequencePhase.SceneZeroPan,
            TitleSequencePhase.NintendoText,
            TitleSequencePhase.SceneOnePan,
            TitleSequencePhase.PresentsText,
            TitleSequencePhase.SceneTwoPan,
            TitleSequencePhase.MetroidThreeText,
            TitleSequencePhase.SceneThreeZoom,
            TitleSequencePhase.TitleLogoFade,
            TitleSequencePhase.CopyrightFade,
            TitleSequencePhase.TitleScreen,
        ];
        var observed = new List<TitleSequencePhase> { state.Phase };
        TitleSequencePhase previous = state.Phase;
        StepUntil(
            () => state.Phase == TitleSequencePhase.TitleScreen,
            _ =>
            {
                state.Step(0);
                if (state.Phase != previous)
                {
                    previous = state.Phase;
                    observed.Add(previous);
                    AssertEqual(
                        FrontendFrame.Width * FrontendFrame.Height,
                        state.Render().Length,
                        $"title {previous} frame geometry");
                }
            },
            maximumFrames: 1_500,
            "complete natural title sequence");
        AssertSequenceEqual(naturalOrder, observed, "natural title phase order");
        AssertEqual(TitleSequenceRomData.Scenes.IdentityScale, state.Mode7MatrixScale,
            "natural title reaches identity Mode-7 scale");
        AssertEqual(TitleSequenceRomData.Timing.TitleScreenNtscFrames,
            state.TitleScreenFramesRemaining,
            "natural title arms NTSC demo countdown");

        state.Step((ushort)SnesButton.Start);
        AssertEqual(TitleSequencePhase.TitleScreenFadeOut, state.Phase,
            "title confirmation begins cadence fade");
        StepUntil(
            () => state.FileSelectRequested,
            _ => state.Step(0),
            maximumFrames: 40,
            "title file-select handoff");

        var skipped = new TitleSequenceState(new SuperMetroidAddressSpace(rom));
        skipped.Step((ushort)SnesButton.A);
        AssertEqual(TitleSequencePhase.SkipFadeOut, skipped.Phase,
            "pre-title A press enters native skip fade-out");
        var skipPhases = new HashSet<TitleSequencePhase> { skipped.Phase };
        StepUntil(
            () => skipped.Phase == TitleSequencePhase.TitleScreen,
            _ =>
            {
                skipped.Step(0);
                skipPhases.Add(skipped.Phase);
            },
            maximumFrames: 24,
            "title skip fade route");
        AssertTrue(skipPhases.Contains(TitleSequencePhase.TitleScreenFadeIn),
            "title skip rebuilds objects before fading back in");
        AssertEqual(TitleSequenceRomData.Timing.MaximumBrightness, skipped.Brightness,
            "title skip returns at full brightness");
        for (int idle = 1; idle < TitleSequenceRomData.Timing.TitleScreenNtscFrames; idle++)
            skipped.Step(0);
        skipped.Step((ushort)SnesButton.Start);
        StepUntil(() => skipped.DemoRequested, _ => skipped.Step(0), 40,
            "title timeout takes priority over same-frame confirmation");
        AssertTrue(!skipped.FileSelectRequested, "timeout routes only to the attract dispatcher");

        AssertEqual(SnesAngle.Zero, TitleSequenceRomData.Scenes.Rotation,
            "title Mode-7 scenes use typed zero rotation");
        Console.WriteLine(
            "  Title: ROM catalog, complete natural phase chain, Mode-7 transforms, " +
            "animation, confirmation fade, and skip route agree.");
    }

    private static byte[] CreateConstructedTitleRom()
    {
        var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
        // Phase-chain fixtures supply valid empty palette programs. The separate retail
        // console audit checks the real color words, timing, and rendered blinking.
        foreach (ushort definition in new[] { TitleSequenceRomData.ConsolePaletteFx.SlowLights,
                     TitleSequenceRomData.ConsolePaletteFx.FastLights })
        {
            WriteRomWord(rom, 0x8d0000 | definition, PaletteFxSetupCodes.Null);
            WriteRomWord(rom, (0x8d0000 | definition) + 2, 0xf000);
        }
        WriteRomWord(rom, 0x8df000, PaletteFxInstructionCodes.Delete);
        WriteRepeatedCompressedStream(
            rom,
            TitleSequenceRomData.Assets.Mode7CharactersAddress,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount,
            0);
        WriteRepeatedCompressedStream(
            rom,
            TitleSequenceRomData.Assets.Mode7MapAddress,
            TitleSequenceRomData.Vram.Mode7MapByteCount,
            0);
        WriteRepeatedCompressedStream(
            rom,
            TitleSequenceRomData.Assets.ObjectCharactersAddress,
            TitleSequenceRomData.Vram.ObjectCharacterByteCount,
            0);
        WriteRepeatedCompressedStream(
            rom,
            TitleSequenceRomData.Assets.BabyMetroidCharactersAddress,
            0x0400,
            0);

        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Year,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene0);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Nintendo,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene1);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Presents,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene2);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.MetroidThree,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene3);
        WriteRomWord(
            rom,
            TitleSequenceRomData.Sprites.LogoPointerAddress,
            TitleSequenceRomData.Sprites.SuperMetroidLogo);
        return rom;
    }

    private static void WriteConstructedTitleTextList(
        byte[] rom,
        TitleTextSequenceDefinition definition,
        ushort sceneCommand)
    {
        WriteRomWord(rom, definition.InstructionAddress, 1);
        WriteRomWord(
            rom,
            definition.InstructionAddress + sizeof(ushort),
            TitleSequenceRomData.Sprites.Blank);
        WriteRomWord(
            rom,
            definition.InstructionAddress + TitleSequenceRomData.TextSequences.TimedEntryByteCount,
            sceneCommand);
    }
}
