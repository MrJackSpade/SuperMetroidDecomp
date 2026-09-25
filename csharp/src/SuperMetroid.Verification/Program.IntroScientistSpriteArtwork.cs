using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyIntroScientistSpriteArtwork(SuperMetroidAddressSpace bus,
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
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer);
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    native.AddOnScreenSpritemap(bus, source, x, y, palette);
                else
                    native.AddOffScreenSpritemap(bus, source, x, y, palette);
                native.FinalizeFrame();
                var installed = new OamBuffer();
                installed.BeginFrame();
                stock.ScientistSprites.Draw(definition.Pointer, installed, x, y,
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                        installed.HighTable.SequenceEqual(native.HighTable) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro scientist sprite {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroScientistSpriteFormat.FileName;
        IntroScientistSpriteDocument document =
            JsonSerializer.Deserialize<IntroScientistSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["delivered-baby-1"] = document.Frames["delivered-baby-1"]
            .Select(part => part with { OffsetX = part.OffsetX + 1 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroScientistSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = new IntroArtworkSourceReadGuard(bus,
            blockIntroScientistSprites: true);
        var stockState = new IntroCinematicState(guarded, characterArtwork: stock);
        var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupBabyDiscoveryCrossfade", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupBabyMetroidDelivery", flags)!
                .Invoke(state, null);
            var crossfade = typeof(IntroCinematicState).GetMethod(
                "StepBabyMetroidDeliveryCrossfade", flags)!;
            for (int frame = 0; frame < 128 &&
                state.Phase == IntroCinematicPhase.BabyMetroidDeliveryCrossfade; frame++)
                crossfade.Invoke(state, null);
            AssertEqual(IntroCinematicPhase.BabyMetroidDelivery, state.Phase,
                "scientist sprite probe reached the lit delivery scene");
            var scientist = (IntroScientistCutsceneState)typeof(IntroCinematicState)
                .GetField("scientistCutscene", flags)!.GetValue(state)!;
            scientist.Step(guarded, 0, introCrossfadeTimer: 0x007f);
        }
        var render = typeof(IntroCinematicState).GetMethod("RenderScientistCutscene", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "scientist sprite override changes visible delivery-scene pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored scientist scene rebinds selected baby artwork");

        var examination = IntroScientistCutsceneState.CreateExamination();
        examination.Step(guarded, 0, introCrossfadeTimer: 0x007f);
        var installedExamination = new OamBuffer();
        installedExamination.BeginFrame();
        examination.Draw(guarded, installedExamination, stock.ScientistSprites);
        installedExamination.FinalizeFrame();
        var nativeExamination = new OamBuffer();
        nativeExamination.BeginFrame();
        examination.Draw(bus, nativeExamination);
        nativeExamination.FinalizeFrame();
        AssertTrue(installedExamination.LowTable.SequenceEqual(nativeExamination.LowTable) &&
                installedExamination.HighTable.SequenceEqual(nativeExamination.HighTable),
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
