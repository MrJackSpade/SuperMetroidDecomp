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
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks intro egg effect OAM against native spritemaps and the live discovery scene.</summary>
internal static class IntroEggEffectSpriteArtworkVerification
{
    /// <summary>Compares stock frames at visible and wrapped origins, then
    /// checks live particles draw the installed art without rereading cartridge maps.</summary>
    internal static void VerifyIntroEggEffectSpriteArtwork(CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        const ushort x = 120;
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroEggEffectSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(stock.Sprites.EggEffect).Draw(definition.Pose, installed, new(x, y),
                    palette);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro egg effect {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        IntroEggEffectSpriteDocument document =
            RequiredJson.Deserialize<IntroEggEffectSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), IntroEggEffectSpriteFormat.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroEggEffects);
        var fragment = new IntroEggParticle(index: 0, InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve);
        fragment.Step();
        var stockOam = ScreenSpritesFixtures.Blank();
        stockOam.BeginFrame();
        fragment.Draw(stockOam, CinematicSheetFixtures.Drawer(stock.Sprites.EggEffect));
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        DrawImportedCinematicActor(bus, nativeLiveOam, fragment.sprite);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.ImageEquals(nativeLiveOam),
            "installed egg fragment preserves native live-actor OAM");

        var slime = new IntroEggSlimeDrop(new(0x0070, 0x00b8), index: 0);
        for (int frame = 0; frame < 31; frame++)
        {
            slime.Step();
            var slimeOam = ScreenSpritesFixtures.Blank();
            slimeOam.BeginFrame();
            slime.Draw(slimeOam, CinematicSheetFixtures.Drawer(stock.Sprites.EggEffect));
            slimeOam.FinalizeFrame();
        }

        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production egg effects never reread eleven native spritemaps");

        document.Frames.Remove("fragment-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroEggEffectSpritePresentation.Write(malformed, document);
            _ = IntroEggEffectSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "egg effect artwork rejects a missing named frame on read");
    }
}
