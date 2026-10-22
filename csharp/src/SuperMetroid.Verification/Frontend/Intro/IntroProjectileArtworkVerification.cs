using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Mother Brain flashback's projectiles and trails draw from the intro's projectile sheet.</summary>
internal static class IntroProjectileArtworkVerification
{
    /// <summary>
    /// Checks flashback shots, explosions and trails show their frames of the intro's projectile sheet, that saved states
    /// exclude the sheets, and that a restored intro draws alike once rebound.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyIntroProjectileArtwork(ushort sprite)
    {
        var intro = CreateRetailIntroFixture();
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback();
        var projectiles = intro.flashbackProjectiles;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.Session.Show(intro);
        game.GameState = SuperMetroidGameState.IntroCinematic;
        game.BindArtwork(game.Artwork with { SpriteSheets = InstalledFixtures.SpriteSheets });
        byte[] Draw(IntroCinematicState scene) => scene.PrepareMotherBrainSprites().FinalizedAsOam().LowTable.ToArray();
        ImageSpriteDraw Shown(IntroCinematicState scene, SpriteSourceKind kind) =>
            scene.PrepareMotherBrainSprites().Draws.OfType<ImageSpriteDraw>().Single(draw => draw.Source.Kind == kind);
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroProjectiles);
        foreach (ushort family in new ushort[] { 0x10, 0x700 })
        {
            projectiles.Reset();
            var shot = projectiles.Slots[0];
            shot.Type = family; shot.Cursor = ProjectileFixtureCursors.Active; shot.SpritemapPointer = sprite;
            shot.Position = shot.Position.WithWhole(new(100, 100));
            AssertEqual(sheet.Frame(ProjectileSheetDefinitions.ProjectileFrame(ProjectileSpriteNative.Projectile(sprite))),
                Shown(intro, SpriteSourceKind.SamusProjectile).Image, $"Intro family ${family:X} shows its frame of the intro's projectile sheet");
        }
        projectiles.Reset();
        ProjectileTrailFrame frame = ProjectileTrailVisualDefinitions.Frames[0];
        var side = projectiles.TrailSlots[0].Left;
        side.InstructionPointer = (ushort)(ProjectileTrailNative.Address(frame) + 4); side.InstructionTimer = WrappingCountdown.Start(100);
        side.Position = side.Position.WithX(100).WithY(100); side.ShownPose = frame;
        AssertEqual(sheet.Frame(ProjectileSheetDefinitions.TrailFrame(frame)), Shown(intro, SpriteSourceKind.SamusProjectileTrail).Image,
            "Intro trail shows its pose's frame of the intro's projectile sheet");
        AssertEqual(99, side.InstructionTimer.RawValue, "Intro preparation advances trail exactly once per call");
        game.BindArtwork(game.Artwork with { SpriteSheets = null });
        using var without = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindArtwork(game.Artwork with { SpriteSheets = InstalledFixtures.SpriteSheets });
        using var with = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.WrittenBytes().SequenceEqual(with.WrittenBytes()), "Intro snapshot excludes the sprite sheets");
        with.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(with);
        var restoredIntro = restored.Session.ScreenAs<IntroCinematicState>()
            ?? throw new InvalidOperationException("Verification requires restoredIntro.");
        BindRestoredIntroDrawFixture(restoredIntro);
        restored.BindArtwork(restored.Artwork with { SpriteSheets = InstalledFixtures.SpriteSheets });
        AssertTrue(Draw(intro).SequenceEqual(Draw(restoredIntro)), "Restored cinematic draws the bound sheet at its saved animation position");
        Console.WriteLine("Intro projectile artwork: shot, explosion and trail sheet frames, native timing and nonserialized sheet rebind pass.");
    }

    /// <summary>Supplies nonprojectile draw dependencies to a restored intro without reuploading its cinematic sheet.</summary>
    private static void BindRestoredIntroDrawFixture(IntroCinematicState intro)
    {
        // This fixture isolates projectile/trail rebinding. Supply the other draw
        // dependencies through the restore path the game takes.
        intro.BindIntroFont(InstalledFixtures.Maps.IntroFont);
        intro.BindFixtureIntroArtwork(InstalledFixtures.IntroArtwork);
        intro.BindSamusBodyArtwork(InstalledFixtures.SamusBody);
        intro.ProjectileFrameBindings = InstalledFixtures.Projectiles.FrameBindings;
    }
}
