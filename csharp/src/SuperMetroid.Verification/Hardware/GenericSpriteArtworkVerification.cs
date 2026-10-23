using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Menus;

/// <summary>Compares installed title and intro sprite compositions and OAM wrapping with retail spritemaps.</summary>
internal static class GenericSpriteArtworkVerification
{
    /// <summary>
    /// Checks the source-identified title/intro generic OAM conversion. This is not a
    /// gameplay search: exact sprite selectors are supplied directly to their owners.
    /// </summary>
    internal static void VerifyGenericSpriteArtwork(string sourceRom)
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        GameInstallation installation = GameAssetInstaller.Install(sourceRom, temporary.Root);
        var reference = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        IntroCinematicArtworkCatalog introArtwork = installation.LoadIntroCinematicArt();
        InstalledProjectilePresentation projectiles = installation.LoadProjectiles();
        SamusBodyArtworkCatalog body = installation.LoadSamusBodyArt();

        // Reference decoding is confined to this diagnostic. The production renderer
        // receives mutable memory only and the installed sprite composition catalog.
        var title = new TitleSequenceState(titleGradientPresentation: maps.TitleGradient,
            titlePalettePresentation: maps.TitlePalette, titleGraphicsPresentation: maps.TitleGraphics, spriteSheets: MenuSheetFixtures.WithTitle(maps.TitleGraphics));
        var document = RequiredJson.Deserialize<TitleFloorMapDocument>(File.ReadAllBytes(
            Path.Combine(installation.MapDirectory, TitleGraphicsFormat.FloorMapFile)),
            ArtworkJson.Options);
        var expected = new OamBuffer();
        int titleCases = 0;
        IReadOnlyDictionary<TitleSprite, ushort> spritemaps =
            SuperMetroid.AssetExtraction.Frontend.Title.TitleSpriteExtractor.MapSprites(reference);
        foreach (TitleSpriteFrame frame in document.Sprites)
        foreach (ushort x in new ushort[] { 0, 255, ushort.MaxValue })
        foreach (ushort y in new ushort[] { 0, 1, 223, 255, ushort.MaxValue })
        foreach (ushort palette in new ushort[] { 0, 0x0e00 })
        {
            title.ActiveSprite = frame.Sprite;
            title.ActiveOrigin = new(x, y);
            title.ActiveObjectPalette = palette;
            title.PrepareRenderSprites();
            expected = new OamBuffer(); expected.BeginFrame();
            DrawImportedSpritemap(reference, expected,
                (int)new SnesAddress(FrontendCartridgeTables.TitleSpritemapBank, spritemaps[frame.Sprite]), new(x, y), palette);
            expected.FinalizeFrame();
            EqualSprites(expected, title.objects, "title composition/coordinate/palette parity");
            titleCases++;
        }
        title.ActiveSprite = null;
        title.Phase = TitleSequencePhase.TitleScreen;
        title.PrepareRenderSprites();
        expected = new OamBuffer(); expected.BeginFrame();
        DrawImportedSpritemap(reference, expected,
            (int)new SnesAddress(FrontendCartridgeTables.TitleSpritemapBank,
                FrontendCartridgeTables.TitleCopyrightSpritemap),
            new(TitleSequenceRomData.Sprites.CopyrightOrigin.X, TitleSequenceRomData.Sprites.CopyrightOrigin.Y),
            TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        expected.FinalizeFrame();
        EqualSprites(expected, title.objects, "title copyright owner parity");

        var intro = new IntroCinematicState(introFont: maps.IntroFont,
            characterArtwork: introArtwork, beamArtwork: projectiles.BeamTiles,
            samusBodyArtwork: body)
        {
            Random = new CartridgeRandom(),
            NarrationPresentation = maps.IntroNarration,
            TrailArtwork = projectiles.Trails,
            ProjectileCompositions = projectiles.Catalog,
            ProjectileFrameBindings = projectiles.FrameBindings,
        };
        intro.BindSpriteSheets(InstalledFixtures.SpriteSheets);
        intro.SetupFirstIllustratedPage();
        var caret = intro.CurrentObjects;
        (caret ?? throw new InvalidOperationException("Verification requires caret.")).Step(0);
        PlacedSpritePart[] actualParts = PlacedSpritePart.Of(intro.PrepareIllustratedPageSprites().Draws);
        expected = new OamBuffer();
        expected = new OamBuffer(); expected.BeginFrame();
        DrawImportedSpritemap(reference, expected,
            (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, SuperMetroid.Verification.Frontend.Intro.CinematicSpritePoseSpritemaps.Of(caret.CaretPose)),
            new(caret.Caret.X, caret.Caret.Y), IntroCinematicRomData.Objects.ScientistPalette.Raw);
        expected.FinalizeFrame();
        EqualParts(PlacedSpritePart.OfOamObjects(expected, MenuRenderDefinitions.ObjectSelection, new SpriteArtworkId(SpriteArtworkKind.IntroObjects)), actualParts, "illustrated-page caret owner parity");

        intro.SetupMotherBrainFlashback();
        var motherBrain = intro.CurrentFlashbackMotherBrain;
        (motherBrain ?? throw new InvalidOperationException("Verification requires motherBrain.")).Step();
        actualParts = PlacedSpritePart.Of(intro.PrepareMotherBrainSprites().Draws);
        var expectedFrame = new SpriteFrameBuffer();
        expectedFrame.BeginFrame();
        SamusState flashbackSamus = (intro.CurrentFlashbackSamus ?? throw new InvalidOperationException("Verification requires intro.flashbackSamus."));
        flashbackSamus.Presentation.Draw(flashbackSamus, expectedFrame, new(0, 0), 0);
        var motherBrainOam = new OamBuffer();
        motherBrainOam.BeginFrame();
        DrawImportedSpritemap(reference, motherBrainOam,
            (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, SuperMetroid.Verification.Frontend.Intro.CinematicSpritePoseSpritemaps.Of(motherBrain.Pose)),
            new(IntroMotherBrainSpriteState.Position.X, IntroMotherBrainSpriteState.Position.Y),
            IntroMotherBrainSpriteState.PaletteBits);
        motherBrainOam.FinalizeFrame();
        expectedFrame.FinalizeFrame();
        EqualParts([.. PlacedSpritePart.Of(expectedFrame.Draws), .. PlacedSpritePart.OfOamObjects(motherBrainOam, MenuRenderDefinitions.ObjectSelection, new SpriteArtworkId(SpriteArtworkKind.IntroObjects))],
            actualParts, "Mother Brain owner preserves actor order and exact sprite parts");

        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        explosions.Step(introCrossfadeTimer: 1);
        var actual = ScreenSpritesFixtures.Blank(); actual.BeginFrame();
        explosions.Draw(actual, CinematicSheetFixtures.Drawer(introArtwork.MotherBrainExplosionSprites));
        actual.FinalizeFrame();
        expected = new OamBuffer();
        expected.BeginFrame();
        foreach (var actor in explosions.actors)
        {
            ushort pointer = SuperMetroid.Verification.Frontend.Intro.CinematicSpritePoseSpritemaps.Of(actor.Pose);
            if (!actor.IsActive || pointer == 0) continue;
            DrawImportedSpritemap(reference, expected,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, pointer),
                new(actor.Position.X, actor.Position.Y),
                IntroCinematicRomData.Objects.ExplosionPalette.Raw);
        }
        expected.FinalizeFrame();
        EqualSprites(expected, actual, "intro explosion owner preserves native placements and ordering");
        AssertTrue(actual.LastFinalizedSpriteCount > 0, "intro explosion comparison contains visible parts");

        // The loader oracle's opposite origin-wrap rule is also compared exhaustively against an
        // independent carry/sign oracle, not against a second copy of the loader.
        var loader = new OamBuffer();
        int wrapCases = 0;
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        foreach (bool onScreen in new[] { true, false })
        {
            loader.BeginFrame();
            if (onScreen)
                loader.AddOnScreenSpritePart(SnesSpritemapXWord.Create(5, true), new SnesSpritemapYByte((byte)offset),
                    SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), new Position16<OamPixels>(12, (ushort)origin));
            else
                loader.AddOffScreenSpritePart(SnesSpritemapXWord.Create(5, true), new SnesSpritemapYByte((byte)offset),
                    SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), new Position16<OamPixels>(12, (ushort)origin));
            loader.FinalizeFrame();
            int signedY = origin + unchecked((sbyte)offset);
            bool hide = signedY is < -32 or >= 224;
            if (!onScreen) hide = !hide;
            AssertEqual(hide ? 0x180 : 17, loader.GetEntry(0).Position.X, "generic origin-wrap X parking");
            AssertEqual((byte)(hide ? 224 : origin + offset), loader.GetEntry(0).Position.Y, "generic origin-wrap Y parking");
            AssertTrue(loader.GetEntry(0).IsLarge, "generic origin-wrap retains sprite size");
            wrapCases++;
        }
        Console.WriteLine($"Generic sprite artwork: {titleCases} title cases, copyright/caret/Mother Brain/explosion owners and {wrapCases} origin-wrap cases pass without a runtime cartridge.");

        static void EqualSprites(OamBuffer expected, ScreenSprites actual, string context)
        {
            AssertEqual(expected.LastFinalizedSpriteCount, actual.LastFinalizedSpriteCount, context + " count");
            AssertTrue(expected.ImageEquals(actual), context + " packed bytes");
        }

        static void EqualParts(PlacedSpritePart[] expected, PlacedSpritePart[] actual, string context)
        {
            AssertEqual(expected.Length, actual.Length, context + " part count");
            AssertTrue(expected.SequenceEqual(actual), context + " parts");
        }
    }

    /// <summary>Runs title, intro, and generic sprite origin-wrap comparisons using the repository ROM.</summary>
    [VerificationCommand("--generic-sprite-artwork")]
    internal static int GenericSpriteArtworkCommand()
    {
        string genericSpriteRom = RepositoryRomPath;
        Suite(nameof(VerifyGenericSpriteArtwork), () => VerifyGenericSpriteArtwork(genericSpriteRom));
        return 0;
    }
}
