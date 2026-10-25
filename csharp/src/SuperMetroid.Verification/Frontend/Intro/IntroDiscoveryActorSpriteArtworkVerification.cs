using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks intro discovery actor OAM against native spritemaps and
/// verifies an edited egg composition reaches the rendered scene.</summary>
internal static class IntroDiscoveryActorSpriteArtworkVerification
{
    /// <summary>Compares stock OAM at visible and wrapped origins, then loads an
    /// override through production scene rendering without cartridge artwork reads.</summary>
    internal static void VerifyIntroDiscoveryActorSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        ushort palette = IntroCinematicDefinitions.Objects.DiscoveryPalette.Raw;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroDiscoveryActorSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    y < CinematicSpriteDrawDefinitions.OnScreenOriginYLimit;
                int source = (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    CinematicSpritemapSources.Of(definition.Pose));
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, native, source, new(x, y), palette);
                else
                    DrawImportedSpritemap(bus, native, source, new(x, y), palette, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = ScreenSpritesFixtures.Blank();
                installed.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.DiscoveryActorSprites).Draw(definition.Pose, installed, new(x, y),
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro discovery actor {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroDiscoveryActorSpriteFormat.FileName;
        IntroDiscoveryActorSpriteDocument document =
            RequiredJson.Deserialize<IntroDiscoveryActorSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Frames["egg-intact"] = document.Frames["egg-intact"]
            .Select(part => part with { OffsetX = part.OffsetX + 1 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroDiscoveryActorSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroDiscoveryActors);
        var stockState = CreateRetailIntroFixture(characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
            state.SetupBabyDiscoveryCrossfade();
            for (int frame = 0; frame < 128 &&
                state.Phase == IntroCinematicPhase.BabyDiscoveryCrossfade; frame++)
                state.StepBabyDiscoveryCrossfade();
            AssertEqual(IntroCinematicPhase.BabyDiscovery, state.Phase,
                "discovery actor visual probe reached the lit gameplay palette");
            var discovery = state.CurrentBabyDiscovery;
            (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).Step(0, 1);
        }
        Rgba32[] stockPixels = stockState.RenderArtwork();
        Rgba32[] editedPixels = editedState.RenderArtwork();
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "egg visual override changes visible production discovery pixels");
        stockState.BindFixtureIntroArtwork(edited);
        AssertTrue(stockState.RenderArtwork().SequenceEqual(editedPixels),
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
