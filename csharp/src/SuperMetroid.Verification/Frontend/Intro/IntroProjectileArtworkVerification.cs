using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Tooling;
using System.Reflection;
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
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks projectile and trail artwork binding in the Mother Brain flashback cinematic.</summary>
internal static class IntroProjectileArtworkVerification
{
    /// <summary>Checks stock OAM parity, edited compositions, trail timing, and artwork rebinding after snapshot restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyIntroProjectileArtwork(ISnesAddressSpace bus, ProjectileSpriteCatalog stock, ProjectileSpriteCatalog edited, ushort sprite)
    {
        var intro = CreateRetailIntroFixture(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback();
        var projectiles = intro.flashbackProjectiles;
        var game = CreateRetailGameFixture(bus);
        game.intro = intro;
        game.GameState = SuperMetroidGameState.IntroCinematic;
        byte[] Draw(IntroCinematicState scene) => (scene.PrepareMotherBrainOam()).LowTable.ToArray();
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
        Suite(nameof(VerifyIntroTrailPng), () => VerifyIntroTrailPng(bus, intro, game, json, Draw));
        var trails = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var document = RequiredJson.Parse(json);
        ushort frame = ProjectileTrailVisualDefinitions.Frames[0];
        document.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(frame))["flipX"] = true;
        var editedTrails = ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
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
        var restoredIntro = restored.intro;
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
        // dependencies without invoking the separate cinematic-sheet reupload path,
        // which deliberately replaces VRAM and is covered by the cinematic art fixture.
        intro.characterArtwork = InstalledFixtures.IntroArtwork;
        intro.BindSamusBodyArtwork(InstalledFixtures.SamusBody);
        intro.ProjectileCompositions = InstalledFixtures.Projectiles.Catalog;
        intro.ProjectileFrameBindings = InstalledFixtures.Projectiles.FrameBindings;
    }
    /// <summary>Checks stock trail atlas VRAM and the exact two byte changes caused by edited PNG pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyIntroTrailPng(ISnesAddressSpace bus, IntroCinematicState intro,
        SuperMetroidGame game, byte[] json, Func<IntroCinematicState, byte[]> draw)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var vram = intro.vram;
        byte[] native = vram.Bytes.ToArray();
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var stock = ProjectileTrailCatalog.Load(new MemoryStream(json), ProjectileTrailAtlas.Load(new MemoryStream(png)));
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = stock,
        });
        draw(intro);
        AssertTrue(native.SequenceEqual(vram.Bytes), "Intro stock trail PNG preserves every VRAM byte, including compressed cinematic sprites");
        var image = IndexedPng.Read(new MemoryStream(png), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        image.Pixels[0] ^= 1;
        image.Pixels[64] ^= 1;
        using var changed = new MemoryStream();
        IndexedPng.Write(changed, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        changed.Position = 0;
        var edited = ProjectileTrailCatalog.Load(new MemoryStream(json), ProjectileTrailAtlas.Load(changed));
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, game);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(saved);
        var restoredIntro = restored.intro;
        BindRestoredIntroDrawFixture((restoredIntro ?? throw new InvalidOperationException("Verification requires restoredIntro.")));
        foreach (var pair in new[] { (game, intro), (restored, restoredIntro) })
        {
            pair.Item1.BindArtwork(pair.Item1.Artwork with
            {
                TrailArtwork = edited,
            });
            var actual = pair.Item2.vram;
            AssertTrue(native.SequenceEqual(actual.Bytes), "Intro PNG rebind does not mutate the retained display before preparation");
            draw(pair.Item2);
            for (int i = 0; i < native.Length; i++)
            {
                bool changedByte = i is (ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2) or
                    (ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2);
                AssertEqual((byte)(native[i] ^ (changedByte ? 0x80 : 0)), actual.Bytes[i], "Intro and restored intro replace exactly the two edited trail pixels");
            }
            // Binding no replacement retains native/debug behavior without inventing
            // a ROM-backed restore on every draw. Explicitly restore stock for callers.
            pair.Item1.BindArtwork(pair.Item1.Artwork with
            {
                TrailArtwork = stock,
            });
            draw(pair.Item2);
        }
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = InstalledFixtures.Projectiles.Trails,
        });
    }
}
