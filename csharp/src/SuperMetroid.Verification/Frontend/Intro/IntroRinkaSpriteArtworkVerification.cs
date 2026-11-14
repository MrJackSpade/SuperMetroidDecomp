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
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks installed intro Rinka sprites and flashback pixels against native artwork and palette overrides.</summary>
internal static class IntroRinkaSpriteArtworkVerification
{
    /// <summary>
    /// Draws a live intro actor from cartridge spritemap data as the native dispatcher does: it admits only signed origin
    /// rows -128 through 382, and draws an origin at or past row 256 through the off-screen loader.
    /// </summary>
    internal static void DrawImportedCinematicActor(ISnesAddressSpace bus, OamBuffer oam, IntroDiscoverySprite actor)
    {
        ushort spritemap = CinematicSpritePoseSpritemaps.Of(actor.Pose);
        if (!actor.IsActive || spritemap == 0) return;
        ushort y = actor.WholePosition.Y;
        if (unchecked((ushort)(y + 128)) >= 511) return;
        DrawImportedSpritemap(bus, oam,
            ((int)new SnesAddress(            IntroCinematicSourceDefinitions.SpritemapBank, spritemap)),
            new(actor.WholePosition.X, y), SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(actor.Colouring), 0).PaletteBits,
            originIsOnScreen: y < 256);
    }
    /// <summary>Compares installed Rinka OAM with native art, live actors included, without rereading cartridge maps.</summary>
    internal static void VerifyIntroRinkaSpriteArtwork(CartridgeImportAddressSpace bus,
        ImportIntroCinematicArtwork stock)
    {
        const ushort x = 120;
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in IntroRinkaSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(stock.Sprites.Rinka).Draw(definition.Pose, installed, new(x, y), palette);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro Rinka {definition.Name} at Y=${y:X4} preserves native OAM clipping");
            }
        }

        IntroRinkaSpriteDocument document = RequiredJson.Deserialize<IntroRinkaSpriteDocument>(
            File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), IntroRinkaSpriteFormat.FileName)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroRinkas);
        var rinkas = new IntroRinkaSystem();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0100)) };
        for (int frame = 0; frame <= 75; frame++)
            rinkas.Step(samus, motherBrainExploding: false, explosionsAllocated: false);
        AssertEqual(2, rinkas.LiveRinkas.Count,
            "intro Rinka first two-wave actors exist for installed artwork test");
        var stockOam = ScreenSpritesFixtures.Blank();
        stockOam.BeginFrame();
        rinkas.Draw(stockOam, CinematicSheetFixtures.Drawer(stock.Sprites.Rinka));
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        foreach (IntroDiscoverySprite actor in rinkas.LiveRinkas)
            DrawImportedCinematicActor(bus, nativeLiveOam, actor);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.ImageEquals(nativeLiveOam),
            "installed intro Rinkas preserve native live-actor OAM placement and attributes");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production intro Rinkas never reread the three native compositions");

        document.Frames.Remove("rinka-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroRinkaSpritePresentation.Write(malformed, document);
            _ = IntroRinkaSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "intro Rinka artwork rejects a missing named frame on read");
    }
}
