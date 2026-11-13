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

/// <summary>Checks installed scientist and delivered-Baby sprites in intro scenes against native OAM.</summary>
internal static class IntroScientistSpriteArtworkVerification
{
    /// <summary>Compares six frames and the live examination actor with native OAM, and checks missing-frame rejection.</summary>
    internal static void VerifyIntroScientistSpriteArtwork(SuperMetroidAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        const ushort x = 120;
        PictureShade palette = IntroShades.Drawn(IntroBabyActorDefinitions.DeliveredBaby.Colouring);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in
            IntroScientistSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(stock.Sprites.Scientist).Draw(definition.Pose, installed, new(x, y),
                    palette);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro scientist sprite {definition.Name} at Y=${y:X4} preserves native OAM");
            }
        }

        IntroScientistSpriteDocument document =
            RequiredJson.Deserialize<IntroScientistSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), IntroScientistSpriteFormat.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroScientistSprites);
        var examination = IntroScientistCutsceneState.CreateExamination();
        examination.Step(0, introCrossfadeTimer: 0x007f);
        var installedExamination = ScreenSpritesFixtures.Blank();
        installedExamination.BeginFrame();
        examination.Draw(installedExamination, CinematicSheetFixtures.Drawer(stock.Sprites.Scientist));
        installedExamination.FinalizeFrame();
        var nativeExamination = new OamBuffer();
        nativeExamination.BeginFrame();
        DrawImportedCinematicActor(bus, nativeExamination, examination.baby);
        nativeExamination.FinalizeFrame();
        AssertTrue(installedExamination.ImageEquals(nativeExamination),
            "installed examination actor preserves live native OAM");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production scientist scenes never reread six native spritemaps");

        document.Frames.Remove("delivered-baby-1");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroScientistSpritePresentation.Write(malformed, document);
            _ = IntroScientistSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "scientist sprite artwork rejects a missing named frame on read");
    }
}
