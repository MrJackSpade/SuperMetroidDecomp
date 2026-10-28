using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroRinkaSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks intro egg effect OAM against native spritemaps and verifies
/// an edited fragment palette reaches the rendered discovery scene.</summary>
internal static class IntroEggEffectSpriteArtworkVerification
{
    /// <summary>Compares stock frames at visible and wrapped origins, then
    /// checks live particles use editable art without rereading cartridge maps.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyIntroEggEffectSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        const ushort x = 120;
        int palette = IntroCinematicDefinitions.Objects.DiscoveryPalette;
        ushort paletteBits = SnesObjAttributeWord.Create(0, palette, 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroEggEffectSpriteDefinitions.Frames)
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
                    DrawImportedSpritemap(bus, native, source, new(x, y), paletteBits);
                else
                    DrawImportedSpritemap(bus, native, source, new(x, y), paletteBits, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = ScreenSpritesFixtures.Blank();
                installed.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.EggEffectSprites).Draw(definition.Pose, installed, new(x, y),
                    palette, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro egg effect {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroEggEffectSpriteFormat.FileName;
        IntroEggEffectSpriteDocument document =
            RequiredJson.Deserialize<IntroEggEffectSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        document.Frames["fragment-0"] = document.Frames["fragment-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroEggEffectSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroEggEffects);
        var fragment = new IntroEggParticle(index: 0);
        fragment.Step();
        var stockOam = ScreenSpritesFixtures.Blank();
        stockOam.BeginFrame();
        fragment.Draw(stockOam, CinematicSheetFixtures.Drawer(stock.EggEffectSprites));
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        DrawImportedCinematicActor(bus, nativeLiveOam, fragment.sprite);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.ImageEquals(nativeLiveOam),
            "installed egg fragment preserves native live-actor OAM");
        var editedOam = ScreenSpritesFixtures.Blank();
        editedOam.BeginFrame();
        fragment.Draw(editedOam, CinematicSheetFixtures.Drawer(edited.EggEffectSprites));
        editedOam.FinalizeFrame();
        AssertTrue(!stockOam.ImageEquals(editedOam) &&
                stockOam.LowTable.Slice(0, 2).SequenceEqual(editedOam.LowTable.Slice(0, 2)),
            "egg-fragment art edit changes OAM palette without moving its particle");

        var slime = new IntroEggSlimeDrop(new(0x0070, 0x00b8), index: 0);
        for (int frame = 0; frame < 31; frame++)
        {
            slime.Step();
            var slimeOam = ScreenSpritesFixtures.Blank();
            slimeOam.BeginFrame();
            slime.Draw(slimeOam, CinematicSheetFixtures.Drawer(stock.EggEffectSprites));
            slimeOam.FinalizeFrame();
        }

        var stockState = CreateRetailIntroFixture(characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
            state.SetupBabyDiscoveryCrossfade();
            var discovery = state.CurrentBabyDiscovery;
            var particles = (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).eggParticles;
            particles.Add(fragment);
        }
        RgbaPixel[] stockPixels = stockState.RenderArtwork();
        RgbaPixel[] editedPixels = editedState.RenderArtwork();
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "egg-fragment override changes visible production discovery pixels");
        stockState.BindFixtureIntroArtwork(edited);
        AssertTrue(stockState.RenderArtwork().SequenceEqual(editedPixels),
            "restored egg-discovery scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production egg effects never reread eleven native spritemaps");

        document.Frames.Remove("fragment-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroEggEffectSpritePresentation.Write(malformed, document);
        }, "egg effect artwork rejects a missing named frame");
        File.Delete(overridePath);
    }
}
