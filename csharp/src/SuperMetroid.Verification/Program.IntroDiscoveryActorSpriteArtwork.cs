using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyIntroDiscoveryActorSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        ushort palette = IntroCinematicRomData.Objects.DiscoveryPalette.Raw;
        foreach (IntroDiscoveryActorSpriteFrameDefinition definition in
            IntroDiscoveryActorSpriteDefinitions.Frames)
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
                stock.DiscoveryActorSprites.Draw(definition.Pointer, installed, x, y,
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                        installed.HighTable.SequenceEqual(native.HighTable) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro discovery actor {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroDiscoveryActorSpriteFormat.FileName;
        IntroDiscoveryActorSpriteDocument document =
            JsonSerializer.Deserialize<IntroDiscoveryActorSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["egg-intact"] = document.Frames["egg-intact"]
            .Select(part => part with { OffsetX = part.OffsetX + 1 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroDiscoveryActorSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = new IntroArtworkSourceReadGuard(bus,
            blockIntroDiscoveryActors: true);
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
            var crossfade = typeof(IntroCinematicState).GetMethod(
                "StepBabyDiscoveryCrossfade", flags)!;
            for (int frame = 0; frame < 128 &&
                state.Phase == IntroCinematicPhase.BabyDiscoveryCrossfade; frame++)
                crossfade.Invoke(state, null);
            AssertEqual(IntroCinematicPhase.BabyDiscovery, state.Phase,
                "discovery actor visual probe reached the lit gameplay palette");
            var discovery = (IntroBabyDiscoveryState)typeof(IntroCinematicState)
                .GetField("babyDiscovery", flags)!.GetValue(state)!;
            discovery.Step(0, 1);
        }
        var render = typeof(IntroCinematicState).GetMethod("RenderBabyDiscovery", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "egg visual override changes visible production discovery pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored discovery scene rebinds selected egg and baby visual compositions");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production discovery actors never reread twenty native spritemaps");

        document.Frames.Remove("egg-intact");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroDiscoveryActorSpritePresentation.Write(malformed, document);
        }, "discovery actor artwork rejects a missing named frame");
        File.Delete(overridePath);
    }
}
