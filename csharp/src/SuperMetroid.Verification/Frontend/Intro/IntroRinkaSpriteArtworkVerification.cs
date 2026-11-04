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
    /// <summary>Draws a live intro actor from cartridge spritemap data with native origin clipping.</summary>
    internal static void DrawImportedCinematicActor(ISnesAddressSpace bus, OamBuffer oam, IntroDiscoverySprite actor)
    {
        ushort spritemap = CinematicSpritePoseSpritemaps.Of(actor.Pose);
        if (!actor.IsActive || spritemap == 0) return;
        ushort y = actor.WholePosition.Y;
        if (unchecked((ushort)(y + CinematicSpriteDrawDefinitions.OriginYBias)) >= CinematicSpriteDrawDefinitions.BiasedOriginYLimit) return;
        DrawImportedSpritemap(bus, oam,
            ((int)new SnesAddress(            IntroCinematicSourceDefinitions.SpritemapBank, spritemap)),
            new(actor.WholePosition.X, y), SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(actor.Colouring), 0).PaletteBits,
            originIsOnScreen: y < CinematicSpriteDrawDefinitions.OnScreenOriginYLimit);
    }
    /// <summary>Compares installed Rinka OAM and flashback pixels with native art, then verifies a palette override and rebinding.</summary>
    internal static void VerifyIntroRinkaSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtwork stock, GameInstallation installation)
    {
        const ushort x = 120;
        PictureShade palette = IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors);
        ushort paletteBits = SnesObjAttributeWord.Create(0, IntroDrawnShadeChecks.InstalledRow(palette), 0).PaletteBits;
        foreach (CinematicSpriteFrameDefinition definition in IntroRinkaSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(IntroCinematicSourceArtwork.Of(stock).RinkaSprites).Draw(definition.Pose, installed, new(x, y), palette,
                    onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"intro Rinka {definition.Name} at Y=${y:X4} preserves native OAM clipping");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroRinkaSpriteFormat.FileName;
        IntroRinkaSpriteDocument document = RequiredJson.Deserialize<IntroRinkaSpriteDocument>(
            File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // The parts inherit the draw's row, so a palette edit cannot show (#1512); the next frame's character can.
        document.Frames["rinka-0"] = document.Frames["rinka-0"]
            .Select(part => part with { TileColumn = 7 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroRinkaSpritePresentation.Write(output, document);
        IntroCinematicArtwork edited = installation.LoadIntroCinematicArt();

        var guarded = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.IntroRinkas);
        var rinkas = new IntroRinkaSystem();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0100)) };
        for (int frame = 0; frame <= 75; frame++)
            rinkas.Step(samus, motherBrainExploding: false, explosionsAllocated: false);
        AssertEqual(2, rinkas.LiveRinkas.Count,
            "intro Rinka first two-wave actors exist for installed artwork test");
        var stockOam = ScreenSpritesFixtures.Blank();
        stockOam.BeginFrame();
        rinkas.Draw(stockOam, CinematicSheetFixtures.Drawer(IntroCinematicSourceArtwork.Of(stock).RinkaSprites));
        stockOam.FinalizeFrame();
        var nativeLiveOam = new OamBuffer();
        nativeLiveOam.BeginFrame();
        foreach (IntroDiscoverySprite actor in rinkas.LiveRinkas)
            DrawImportedCinematicActor(bus, nativeLiveOam, actor);
        nativeLiveOam.FinalizeFrame();
        AssertTrue(stockOam.ImageEquals(nativeLiveOam),
            "installed intro Rinkas preserve native live-actor OAM placement and attributes");
        var editedOam = ScreenSpritesFixtures.Blank();
        editedOam.BeginFrame();
        rinkas.Draw(editedOam, CinematicSheetFixtures.Drawer(IntroCinematicSourceArtwork.Of(edited).RinkaSprites));
        editedOam.FinalizeFrame();
        AssertTrue(!stockOam.ImageEquals(editedOam),
            "intro Rinka art edit changes live OAM without moving actors");
        for (int part = 0; part < stockOam.LastFinalizedSpriteCount; part++)
            AssertTrue(stockOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(editedOam.LowTable.Slice(part * 4, 2)),
                $"intro Rinka art edit preserves native part {part} X/Y");

        var stockState = CreateRetailIntroFixture(characterArtwork: stock);
        var editedState = CreateRetailIntroFixture(characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            state.SetupFirstIllustratedPage();
            state.SetupMotherBrainFlashback();
        }
        // The flashback fades in from black, and the Rinkas' shading follows the fade (#1512): the scenes step together
        // through the flashback, showing the Rinkas in each, until the edited art shows. The Rinkas are shown only for
        // each look, so the scenes step as they would without them.
        RgbaPixel[] stockPixels, editedPixels;
        int steps = 0;
        while (true)
        {
            stockPixels = WithRinkas(stockState);
            editedPixels = WithRinkas(editedState);
            if (!stockPixels.SequenceEqual(editedPixels) || stockState.CurrentFlashbackMotherBrain is null)
                break;
            stockState.Step(default);
            editedState.Step(default);
            steps++;
        }
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            $"intro Rinka override changes visible production flashback pixels ({steps} flashback steps)");
        stockState.FlashbackRinkas = rinkas;
        stockState.BindFixtureIntroArtwork(edited);
        AssertTrue(stockState.RenderArtwork().SequenceEqual(editedPixels),
            "restored intro Rinka scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production intro Rinkas never reread the three native compositions");

        document.Frames.Remove("rinka-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroRinkaSpritePresentation.Write(malformed, document);
            _ = IntroRinkaSpritePresentation.Load(new MemoryStream(malformed.ToArray(), writable: false));
        }, "intro Rinka artwork rejects a missing named frame on read");
        File.Delete(overridePath);

        RgbaPixel[] WithRinkas(IntroCinematicState state)
        {
            var shown = state.FlashbackRinkas;
            state.FlashbackRinkas = rinkas;
            RgbaPixel[] pixels = state.RenderArtwork();
            state.FlashbackRinkas = shown;
            return pixels;
        }
    }
}
