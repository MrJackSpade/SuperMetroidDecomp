using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Tooling;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks projectile and trail artwork binding in the Mother Brain flashback cinematic.</summary>
internal static class IntroProjectileArtworkVerification
{
    /// <summary>Checks stock OAM parity, edited compositions, trail timing, and artwork rebinding after snapshot restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyIntroProjectileArtwork(ISnesAddressSpace bus, ProjectileSpriteCatalog stock, ProjectileSpriteCatalog edited, ushort sprite)
    {
        var intro = CreateRetailIntroFixture();
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback();
        var projectiles = intro.flashbackProjectiles;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.Session.Show(intro);
        game.GameState = SuperMetroidGameState.IntroCinematic;
        byte[] Draw(IntroCinematicState scene) => scene.PrepareMotherBrainSprites().FinalizedAsOam().LowTable.ToArray();
        foreach (ushort family in new ushort[] { 0x10, 0x700 })
        {
            projectiles.Reset();
            var shot = projectiles.Slots[0];
            shot.Type = family; shot.Cursor = ProjectileFixtureCursors.Active; shot.SpritemapPointer = sprite;
            shot.Position = shot.Position.WithWhole(new(100, 100));
            game.BindArtwork(game.Artwork with
            {
                ProjectileCompositions = InstalledFixtures.Projectiles.Catalog,
            });
            byte[] native = Draw(intro);
            game.BindArtwork(game.Artwork with
            {
                ProjectileCompositions = stock,
            });
            AssertTrue(native.SequenceEqual(Draw(intro)), "Intro installed projectile binding matches cartridge-extracted stock OAM");
            game.BindArtwork(game.Artwork with
            {
                ProjectileCompositions = edited,
            });
            AssertTrue(!native.SequenceEqual(Draw(intro)), "Intro projectile/explosion passes consume edited composition");
        }
        projectiles.Reset();
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        Suite(nameof(VerifyIntroTrailPng), () => VerifyIntroTrailPng(bus, intro, game, json, Draw, sprite));
        // Binding a trail catalog recomposes the flashback page from its characters, so the fixtures carry the installed ones.
        ProjectileTrailAtlas installedTrailTiles = InstalledFixtures.Projectiles.Trails.Tiles
            ?? throw new InvalidOperationException("The installation has no trail characters.");
        var trails = ProjectileTrailCatalog.Load(new MemoryStream(json), installedTrailTiles);
        var document = RequiredJson.Parse(json);
        ushort frame = ProjectileTrailVisualDefinitions.Frames[0];
        document.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(frame))["flipX"] = true;
        var editedTrails = ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())), installedTrailTiles);
        var side = projectiles.TrailSlots[0].Left;
        side.InstructionPointer = (ushort)(frame + 4); side.InstructionTimer = NativeCountdown.Start(100);
        side.Position = side.Position.WithX(100).WithY(100); side.TileNumberAttributes = trails.Resolve(frame);
        byte[] nativeTrail = Draw(intro);
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = trails,
        });
        AssertTrue(nativeTrail.SequenceEqual(Draw(intro)), "Intro stock trail binding preserves native OAM");
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = editedTrails,
        });
        AssertTrue(!nativeTrail.SequenceEqual(Draw(intro)), "Intro trail pass consumes selected appearance");
        AssertEqual(97, side.InstructionTimer.RawValue, "Intro preparation advances trail exactly once per call");
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = null,
            ProjectileCompositions = null,
        });         using var without = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = trails,
            ProjectileCompositions = stock,
        });         using var with = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.WrittenBytes().SequenceEqual(with.WrittenBytes()), "Intro snapshot excludes host appearance catalogs");
        with.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(with);
        var restoredIntro = restored.Session.ScreenAs<IntroCinematicState>();
        AssertTrue((restoredIntro ?? throw new InvalidOperationException("Verification requires restoredIntro.")).TrailArtwork is null && restoredIntro.ProjectileCompositions is null, "Restored intro requires host content rebind");
        BindRestoredIntroDrawFixture(restoredIntro);
        restored.BindArtwork(restored.Artwork with
        {
            TrailArtwork = editedTrails,
            ProjectileCompositions = edited,
        });         game.BindArtwork(game.Artwork with
        {
            TrailArtwork = editedTrails,
            ProjectileCompositions = edited,
        });         AssertTrue(Draw(intro).SequenceEqual(Draw(restoredIntro)), "Restored cinematic draws current artwork at saved animation position");
        Console.WriteLine("Intro projectile artwork: live/explosion/trail stock parity, visible edits, native timing and nonserialized state rebind pass.");
    }

    /// <summary>Supplies nonprojectile draw dependencies to a restored intro without reuploading its cinematic sheet.</summary>
    private static void BindRestoredIntroDrawFixture(IntroCinematicState intro)
    {
        // This fixture isolates projectile/trail rebinding. Supply the other draw
        // dependencies through the restore path the game takes.
        intro.BindIntroFont(InstalledFixtures.Maps.IntroFont);
        intro.BindCharacterArtwork(InstalledFixtures.IntroArtwork);
        intro.BindSamusBodyArtwork(InstalledFixtures.SamusBody);
        intro.BindBeamArtwork(InstalledFixtures.Projectiles.BeamTiles);
        intro.ProjectileCompositions = InstalledFixtures.Projectiles.Catalog;
        intro.ProjectileFrameBindings = InstalledFixtures.Projectiles.FrameBindings;
    }
    /// <summary>Checks stock trail atlas VRAM and the exact two byte changes caused by edited PNG pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyIntroTrailPng(ISnesAddressSpace bus, IntroCinematicState intro,
        SuperMetroidGame game, byte[] json, Func<IntroCinematicState, byte[]> draw, ushort sprite)
    {
        byte[] native = intro.Image();
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var stock = ProjectileTrailCatalog.Load(new MemoryStream(json), ProjectileTrailAtlas.Load(new MemoryStream(png)));
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = stock,
        });
        draw(intro);
        AssertTrue(native.SequenceEqual(intro.Image()), "Intro stock trail PNG preserves every shown VRAM byte, including compressed cinematic sprites");
        var image = IndexedPng.Read(new MemoryStream(png), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        image.Pixels[0] ^= 1;
        image.Pixels[64] ^= 1;
        using var changed = new MemoryStream();
        IndexedPng.Write(changed, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        changed.Position = 0;
        var editedTiles = ProjectileTrailAtlas.Load(changed);
        var edited = ProjectileTrailCatalog.Load(new MemoryStream(json), editedTiles);
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, game);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(saved);
        var restoredIntro = restored.Session.ScreenAs<IntroCinematicState>();
        BindRestoredIntroDrawFixture((restoredIntro ?? throw new InvalidOperationException("Verification requires restoredIntro.")));
        foreach (var pair in new[] { new VerifyIntroTrailPngCase(game, intro), new VerifyIntroTrailPngCase(restored, restoredIntro) })
        {
            pair.First.BindArtwork(pair.First.Artwork with
            {
                TrailArtwork = edited,
            });
            AssertTrue(native.SequenceEqual(pair.Second.Image()), "Intro PNG rebind does not change the shown image before preparation");
            SpriteArtwork page = DrawnProjectilePage(pair.Second);
            AssertTrue(native.SequenceEqual(pair.Second.Image()), "Intro trail PNG is a page binding, not a shown-image write");
            AssertTrue(page.Characters.Slice(ProjectileObjectPageDefinitions.IceWaveTrailFirstCharacter * SpriteArtwork.CharacterByteCount,
                    editedTiles.IceAndWave.Length).SequenceEqual(editedTiles.IceAndWave.Span) &&
                page.Characters.Slice(ProjectileObjectPageDefinitions.MissileTrailFirstCharacter * SpriteArtwork.CharacterByteCount,
                    editedTiles.Missile.Length).SequenceEqual(editedTiles.Missile.Span),
                "Intro and restored intro bind the edited trail characters");
            // Binding no replacement retains native/debug behavior without inventing
            // a ROM-backed restore on every draw. Explicitly restore stock for callers.
            pair.First.BindArtwork(pair.First.Artwork with
            {
                TrailArtwork = stock,
            });
            draw(pair.Second);
        }
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = InstalledFixtures.Projectiles.Trails,
        });

        // The page a visible flashback shot draws from, which carries the bound trail characters.
        SpriteArtwork DrawnProjectilePage(IntroCinematicState scene)
        {
            var shot = scene.flashbackProjectiles.Slots[0];
            shot.Type = 0x10; shot.Cursor = ProjectileFixtureCursors.Active; shot.SpritemapPointer = sprite;
            shot.Position = shot.Position.WithWhole(new(100, 100));
            SpriteArtwork page = scene.PrepareMotherBrainSprites().Draws
                .First(entry => entry.Source.Kind == SpriteSourceKind.SamusProjectile).Artwork;
            scene.flashbackProjectiles.Reset();
            return page;
        }
    }
}
