using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static MapSpritesVerification;

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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        // Reference decoding is confined to this diagnostic. The production renderer
        // receives mutable memory only and the installed sprite composition catalog.
        var title = new TitleSequenceState(memory, titleGradientPresentation: maps.TitleGradient,
            titlePalettePresentation: maps.TitlePalette, titleGraphicsPresentation: maps.TitleGraphics);
        var document = JsonSerializer.Deserialize<TitleMode7MapDocument>(File.ReadAllBytes(
            Path.Combine(installation.MapDirectory, TitleGraphicsFormat.Mode7MapFile)),
            MapPresentationFormat.JsonOptions)!;
        var expected = new OamBuffer();
        int titleCases = 0;
        foreach (TitleSpriteFrame frame in document.Sprites)
        foreach (ushort x in new ushort[] { 0, 255, ushort.MaxValue })
        foreach (ushort y in new ushort[] { 0, 1, 223, 255, ushort.MaxValue })
        foreach (ushort palette in new ushort[] { 0, 0x0e00 })
        {
            Set(title, "activeSpritemap", checked((ushort)frame.Pointer));
            Set(title, "activeOriginX", x);
            Set(title, "activeOriginY", y);
            Set(title, "activeCharacterOffset", palette);
            Invoke(title, "PrepareRenderOam");
            expected.BeginFrame();
            DrawImportedSpritemap(reference, expected,
                (int)new SnesAddress(TitleSequenceRomData.Sprites.Bank, checked((ushort)frame.Pointer)), new(x, y), palette);
            expected.FinalizeFrame();
            EqualOam(expected, Field<OamBuffer>(title, "oam"), "title composition/coordinate/palette parity");
            titleCases++;
        }
        Set(title, "activeSpritemap", TitleSequenceRomData.Sprites.Blank);
        Set(title, "<Phase>k__BackingField", TitleSequencePhase.TitleScreen);
        Invoke(title, "PrepareRenderOam");
        expected.BeginFrame();
        DrawImportedSpritemap(reference, expected,
            (int)new SnesAddress(TitleSequenceRomData.Sprites.Bank,
                TitleSequenceRomData.Sprites.NintendoCopyright),
            new(TitleSequenceRomData.Sprites.CopyrightOrigin.X, TitleSequenceRomData.Sprites.CopyrightOrigin.Y),
            TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        expected.FinalizeFrame();
        EqualOam(expected, Field<OamBuffer>(title, "oam"), "title copyright owner parity");

        var intro = new IntroCinematicState(memory, introFont: maps.IntroFont,
            characterArtwork: introArtwork, beamArtwork: projectiles.BeamTiles,
            samusBodyArtwork: body)
        {
            NarrationPresentation = maps.IntroNarration,
            TrailArtwork = projectiles.Trails,
            ProjectileCompositions = projectiles.Catalog,
            ProjectileFrameBindings = projectiles.FrameBindings,
        };
        Invoke(intro, "SetupFirstIllustratedPage");
        var caret = Field<IntroCinematicObjectSystem>(intro, "objects");
        caret.Step(0);
        var actual = (OamBuffer)Invoke(intro, "PrepareIllustratedPageOam")!;
        expected = new OamBuffer();
        expected.BeginFrame();
        DrawImportedSpritemap(reference, expected,
            (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, caret.SpriteMapPointer),
            new(caret.Caret.X, caret.Caret.Y), IntroCinematicRomData.Objects.ScientistPalette.Raw);
        expected.FinalizeFrame();
        EqualOam(expected, actual, "illustrated-page caret owner parity");

        Invoke(intro, "SetupMotherBrainFlashback");
        var motherBrain = Field<IntroMotherBrainSpriteState>(intro, "flashbackMotherBrain");
        motherBrain.Step(memory);
        actual = (OamBuffer)Invoke(intro, "PrepareMotherBrainOam")!;
        expected = new OamBuffer();
        expected.BeginFrame();
        Field<SamusState>(intro, "flashbackSamus").Presentation.Draw(memory, expected, new(0, 0), 0);
        DrawImportedSpritemap(reference, expected,
            (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, motherBrain.SpriteMapPointer),
            new(IntroMotherBrainSpriteState.Position.X, IntroMotherBrainSpriteState.Position.Y),
            IntroMotherBrainSpriteState.PaletteBits);
        expected.FinalizeFrame();
        EqualOam(expected, actual, "Mother Brain owner preserves actor order and exact OAM");

        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        explosions.Step(memory, introCrossfadeTimer: 1);
        actual = new OamBuffer(); actual.BeginFrame();
        explosions.Draw(actual, introArtwork.MotherBrainExplosionSprites);
        actual.FinalizeFrame();
        expected = new OamBuffer();
        expected.BeginFrame();
        foreach (object actor in Field<IEnumerable>(explosions, "actors"))
        {
            ushort pointer = Property<ushort>(actor, "SpriteMapPointer");
            if (!Property<bool>(actor, "IsActive") || pointer == 0) continue;
            DrawImportedSpritemap(reference, expected,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, pointer),
                new(Property<ushort>(actor, "XPosition"), Property<ushort>(actor, "YPosition")),
                IntroCinematicRomData.Objects.ExplosionPalette.Raw);
        }
        expected.FinalizeFrame();
        EqualOam(expected, actual, "intro explosion owner preserves native placements and ordering");
        AssertTrue(actual.LastFinalizedSpriteCount > 0, "intro explosion comparison contains visible parts");
        AssertThrows<ArgumentNullException>(() => explosions.Draw(new OamBuffer(), null!),
            "intro explosions cannot fall back to cartridge artwork");

        // The opposite origin-wrap rule is also compared exhaustively against an
        // independent carry/sign oracle, not against a second copy of the renderer.
        int wrapCases = 0;
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        foreach (bool onScreen in new[] { true, false })
        {
            actual.BeginFrame();
            if (onScreen)
                actual.AddOnScreenSpritePart(SnesSpritemapXWord.Create(5, true), (byte)offset,
                    SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), new Position16<OamPixels>(12, (ushort)origin));
            else
                actual.AddOffScreenSpritePart(SnesSpritemapXWord.Create(5, true), (byte)offset,
                    SnesObjAttributeWord.Create(3, 2, 1, SnesTileFlipFlags.Horizontal), new Position16<OamPixels>(12, (ushort)origin));
            int signedY = origin + unchecked((sbyte)offset);
            bool hide = signedY is < -32 or >= 224;
            if (!onScreen) hide = !hide;
            AssertEqual(hide ? 0x180 : 17, actual.GetEntry(0).X, "generic origin-wrap X parking");
            AssertEqual((byte)(hide ? 224 : origin + offset), actual.GetEntry(0).Y, "generic origin-wrap Y parking");
            AssertTrue(actual.GetEntry(0).IsLarge, "generic origin-wrap retains sprite size");
            wrapCases++;
        }
        Console.WriteLine($"Generic sprite artwork: {titleCases} title cases, copyright/caret/Mother Brain/explosion owners and {wrapCases} origin-wrap cases pass without a runtime cartridge.");

        static void Set(object owner, string name, object value) =>
            owner.GetType().GetField(name, flags)!.SetValue(owner, value);
        static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, flags)!.GetValue(owner)!;
        static T Property<T>(object owner, string name) => (T)owner.GetType().GetProperty(name)!.GetValue(owner)!;
        static object? Invoke(object owner, string name) => owner.GetType().GetMethod(name, flags)!.Invoke(owner, null);
        static void EqualOam(OamBuffer expected, OamBuffer actual, string context)
        {
            AssertEqual(expected.LastFinalizedSpriteCount, actual.LastFinalizedSpriteCount, context + " count");
            AssertTrue(expected.ImageEquals(actual), context + " packed bytes");
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
