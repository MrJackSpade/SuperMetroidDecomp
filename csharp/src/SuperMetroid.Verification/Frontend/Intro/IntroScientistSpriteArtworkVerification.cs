using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroRinkaSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks installed scientist and delivered-Baby sprites in intro scenes against native OAM.</summary>
internal static class IntroScientistSpriteArtworkVerification
{
    /// <summary>Compares ten frames, an edited delivery-scene override, rebinding, and missing-frame rejection.</summary>
    internal static void VerifyIntroScientistSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        ushort palette = IntroBabyActorDefinitions.DeliveredBaby.PaletteBits;
        foreach (IntroScientistSpriteFrameDefinition definition in
            IntroScientistSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer);
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, native, source, new(x, y), palette);
                else
                    DrawImportedSpritemap(bus, native, source, new(x, y), palette, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = new OamBuffer();
                installed.BeginFrame();
                stock.ScientistSprites.Draw(definition.Pointer, installed, new(x, y),
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro scientist sprite {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroScientistSpriteFormat.FileName;
        IntroScientistSpriteDocument document =
            RequiredJson.Deserialize<IntroScientistSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Frames["delivered-baby-1"] = document.Frames["delivered-baby-1"]
            .Select(part => part with { OffsetX = part.OffsetX + 1 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroScientistSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroScientistSprites);
        var stockState = CreateRetailIntroFixture(characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
            state.SetupBabyDiscoveryCrossfade();
            state.SetupBabyMetroidDelivery();
            for (int frame = 0; frame < 128 &&
                state.Phase == IntroCinematicPhase.BabyMetroidDeliveryCrossfade; frame++)
                state.StepBabyMetroidDeliveryCrossfade();
            AssertEqual(IntroCinematicPhase.BabyMetroidDelivery, state.Phase,
                "scientist sprite probe reached the lit delivery scene");
            var scientist = state.scientistCutscene;
            (scientist ?? throw new InvalidOperationException("Verification requires scientist.")).Step(0, introCrossfadeTimer: 0x007f);
        }
        Rgba32[] stockPixels = stockState.RenderScientistCutscene();
        Rgba32[] editedPixels = editedState.RenderScientistCutscene();
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "scientist sprite override changes visible delivery-scene pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue((stockState.RenderScientistCutscene()).SequenceEqual(editedPixels),
            "restored scientist scene rebinds selected baby artwork");

        var examination = IntroScientistCutsceneState.CreateExamination();
        examination.Step(0, introCrossfadeTimer: 0x007f);
        var installedExamination = new OamBuffer();
        installedExamination.BeginFrame();
        examination.Draw(installedExamination, stock.ScientistSprites);
        installedExamination.FinalizeFrame();
        var nativeExamination = new OamBuffer();
        nativeExamination.BeginFrame();
        DrawImportedCinematicActor(bus, nativeExamination, examination.baby);
        nativeExamination.FinalizeFrame();
        AssertTrue(installedExamination.ImageEquals(nativeExamination),
            "installed examination actor preserves live native OAM");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production scientist scenes never reread ten native spritemaps");

        document.Frames.Remove("delivered-baby-1");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroScientistSpritePresentation.Write(malformed, document);
        }, "scientist sprite artwork rejects a missing named frame");
        File.Delete(overridePath);
    }
}
