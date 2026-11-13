using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks intro discovery actor OAM against native spritemaps and the rendered scene.</summary>
internal static class IntroDiscoveryActorSpriteArtworkVerification
{
    /// <summary>Compares stock OAM at visible and wrapped origins, then renders the production
    /// discovery scene without cartridge artwork reads.</summary>
    internal static void VerifyIntroDiscoveryActorSpriteArtwork(SuperMetroidAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        const ushort x = 120;
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroDiscoveryActorSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(IntroCinematicSourceDefinitions.SpritemapBank,
                    CinematicSpritemapSources.Of(definition.Pose));
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, native, source, new(x, y), paletteBits);
                else
                    DrawImportedSpritemap(bus, native, source, new(x, y), paletteBits, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = ScreenSpritesFixtures.Blank();
                installed.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.Sprites.DiscoveryActor).Draw(definition.Pose, installed, new(x, y),
                    palette);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro discovery actor {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        IntroDiscoveryActorSpriteDocument document =
            RequiredJson.Deserialize<IntroDiscoveryActorSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), IntroDiscoveryActorSpriteFormat.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroDiscoveryActors);
        var stockState = CreateRetailIntroFixture(characterArtwork: stock);
        stockState.SetupFirstIllustratedPage();
        stockState.SetupMotherBrainFlashback();
        stockState.SetupBabyDiscoveryCrossfade();
        for (int frame = 0; frame < 128 &&
            stockState.Phase == IntroCinematicPhase.BabyDiscoveryCrossfade; frame++)
            stockState.StepBabyDiscoveryCrossfade();
        AssertEqual(IntroCinematicPhase.BabyDiscovery, stockState.Phase,
            "discovery actor visual probe reached the lit gameplay palette");
        (stockState.CurrentBabyDiscovery ?? throw new InvalidOperationException("Verification requires discovery.")).Step(0, 1);
        _ = stockState.RenderArtwork();
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production discovery actors never reread nineteen native spritemaps");

        document.Frames.Remove("egg-intact");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroDiscoveryActorSpritePresentation.Write(malformed, document);
            _ = IntroDiscoveryActorSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "discovery actor artwork rejects a missing named frame on read");
    }
}
