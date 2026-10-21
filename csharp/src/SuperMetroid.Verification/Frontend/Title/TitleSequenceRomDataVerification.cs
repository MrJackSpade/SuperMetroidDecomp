using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionVerification;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using static SuperMetroid.Verification.Frontend.Title.TitleFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Checks the complete title phase chain and its constructed cartridge inputs.</summary>
internal static class TitleSequenceRomDataVerification
{
    /// <summary>
    /// Runs every natural title card, pan, zoom, hold, and fade, then separately proves
    /// that the native confirm-button shortcut still takes its fade-out/fade-in route.
    /// </summary>
    internal static void VerifyTitleSequenceRomData()
    {
        byte[] rom = CreateConstructedTitleRom();
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        TitleGradientPresentation gradient = TitleGradientPresentation.Load(
            new MemoryStream(TitleGradientExtractor.Extract(bus), writable: false));
        var state = CreateTitleFixture(bus, titleGradientPresentation: gradient);
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
            // The fixture now uses complete compiled title cards, not one-frame
            // replacement cards. Match the natural-sequence snapshot's bound.
            maximumFrames: 2_500,
            "complete natural title sequence");
        AssertSequenceEqual(naturalOrder, observed, "natural title phase order");
        AssertEqual(TitleSequenceRomData.Scenes.IdentityScale, state.Mode7MatrixScale,
            "natural title reaches identity Mode-7 scale");
        AssertEqual(TitleSequenceRomData.Timing.TitleScreenNtscFrames,
            state.TitleScreenFramesRemaining,
            "natural title arms NTSC demo countdown");

        state.Step(SnesButton.Start);
        AssertEqual(TitleSequencePhase.TitleScreenFadeOut, state.Phase,
            "title confirmation begins cadence fade");
        StepUntil(
            () => state.FileSelectRequested,
            _ => state.Step(0),
            maximumFrames: 40,
            "title file-select handoff");

        var skipped = CreateTitleFixture(new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom),
            titleGradientPresentation: gradient);
        skipped.Step(SnesButton.A);
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
        skipped.Step(SnesButton.Start);
        StepUntil(() => skipped.DemoRequested, _ => skipped.Step(0), 40,
            "title timeout takes priority over same-frame confirmation");
        AssertTrue(!skipped.FileSelectRequested, "timeout routes only to the attract dispatcher");

        AssertEqual(SnesAngle.Zero, TitleSequenceRomData.Scenes.Rotation,
            "title Mode-7 scenes use typed zero rotation");
        Console.WriteLine(
            "  Title: ROM catalog, complete natural phase chain, Mode-7 transforms, " +
            "animation, confirmation fade, and skip route agree.");
    }

    /// <summary>Builds a ROM image with valid synthetic title art and compiled text/control identities.</summary>
    internal static byte[] CreateConstructedTitleRom()
    {
        var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];
        // Valid constructed fixed-color/control streams; retail tests check the real
        // gradient bands separately. Every zoom index selects the same black run.
        for (int index = 0; index < 16; index++)
            WriteRomWord(rom, FrontendCartridgeTables.FixedColorPointers + index * 2, 0xbc7d);
        WriteRomWord(rom, 0x8cbc7d, 0xe07f);
        WriteRomWord(rom, 0x8cbc7f, 0xe07f);
        WriteRomWord(rom, TitleGradientRomData.ControlTable, 0xa17a);
        WriteRomWord(rom, TitleGradientRomData.ControlTable + 2, 0x317f);
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
            TitleSequenceRomData.Vram.BabyCharacterByteCount,
            0);

        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Year,
            (ushort)TitleSequenceInstruction.TriggerScene0);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Nintendo,
            (ushort)TitleSequenceInstruction.TriggerScene1);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.Presents,
            (ushort)TitleSequenceInstruction.TriggerScene2);
        WriteConstructedTitleTextList(
            rom,
            TitleSequenceRomData.TextSequences.MetroidThree,
            (ushort)TitleSequenceInstruction.TriggerScene3);
        WriteRomWord(
            rom,
            FrontendCartridgeTables.TitleLogoSpritemapOperand,
            TitleSequenceRomDataSpritesConstants.SuperMetroidLogo);
        return rom;
    }

    /// <summary>Writes one compiled timed text list into the constructed ROM up to its scene trigger.</summary>
    private static void WriteConstructedTitleTextList(
        byte[] rom,
        TitleTextSequenceDefinition definition,
        ushort sceneCommand)
    {
        // Timing/selector identities are typed now. Keep synthetic blank
        // artwork, but provide the complete selector inventory to the extractor.
        int pointer = definition.InstructionAddress;
        foreach (TitleCardStep step in TitleCards.Steps(definition.Phase))
        {
            if (step.Command is { } command)
            {
                WriteRomWord(rom, pointer, (ushort)command);
                AssertEqual(sceneCommand, (ushort)command, "constructed title scene command");
                break;
            }
            WriteRomWord(rom, pointer, checked((ushort)step.Duration));
            WriteRomWord(rom, pointer + sizeof(ushort), step.Sprite is { } sprite
                ? ConstructedSpritemap(sprite) : FrontendCartridgeTables.TitleBlankSpritemap);
            pointer += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
        }
    }

    /// <summary>
    /// A distinct synthetic spritemap for one card composition in the constructed ROM: consecutive empty words
    /// after the copyright spritemap, so every composition decodes as blank artwork.
    /// </summary>
    private static ushort ConstructedSpritemap(TitleSprite sprite)
    {
        int ordinal = TitleSpriteDefinitions.All.ToList().IndexOf(sprite);
        if (ordinal < 0)
            throw new ArgumentOutOfRangeException(nameof(sprite), sprite, "Undefined title composition.");
        return checked((ushort)(FrontendCartridgeTables.TitleCopyrightSpritemap + sizeof(ushort) * (1 + ordinal)));
    }
}
