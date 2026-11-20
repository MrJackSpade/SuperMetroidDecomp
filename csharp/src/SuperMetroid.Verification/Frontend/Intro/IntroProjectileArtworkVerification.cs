using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the Mother Brain flashback's projectiles and trails draw from the intro's projectile sheet.</summary>
internal static class IntroProjectileArtworkVerification
{
    /// <summary>
    /// Checks flashback shots, explosions and trails show their frames of the intro's projectile sheet, that saved states
    /// exclude the sheets, and that a restored intro draws alike once rebound.
    /// </summary>
    internal static void VerifyIntroProjectileArtwork(ushort sprite)
    {
        var intro = CreateRetailIntroFixture();
        intro.SetupFirstIllustratedPage();
        intro.SetupMotherBrainFlashback((intro.CurrentNarration ?? throw new InvalidOperationException("The check runs the intro once its pages have begun.")));
        var projectiles = intro.flashbackProjectiles;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        game.Session.Enter(new IntroCinematicGamePhase(game.Session, intro));
        game.BindArtwork(game.Artwork with { SpriteSheets = InstalledFixtures.SpriteSheets });
        SamusColouredDraw Shown(IntroCinematicState scene, SpriteSourceKind kind) =>
            scene.PrepareMotherBrainSprites().Draws.OfType<SamusColouredDraw>().Single(draw => draw.Source.Kind == kind);
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroProjectiles);
        foreach (ushort family in new ushort[] { 0x10, 0x700 })
        {
            projectiles.Reset();
            var shot = projectiles.Shots[0];
            shot.Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(family); shot.Animation = ProjectileFixturePlaybacks.Active(); shot.SpritemapPointer = sprite;
            shot.Position = shot.Position.WithWhole(new(100, 100));
            AssertEqual(sheet.Frame(ProjectileSheetFrames.ProjectileFrame(ProjectileSpriteNative.Projectile(sprite))),
                Shown(intro, SpriteSourceKind.SamusProjectile).Image, $"Intro family ${family:X} shows its frame of the intro's projectile sheet");
        }
        projectiles.Reset();
        ProjectileTrailFrame frame = ProjectileTrailFrame.IceSparkleClustered;
        var side = projectiles.AddTrail(Position16<RoomPixels>.Of(100, 100)).Left;
        side.RestoreNative((ushort)(ProjectileTrailNative.Address(frame) + 4), 100, frame);
        AssertEqual(sheet.Frame(ProjectileSheetFrames.TrailFrame(frame)), Shown(intro, SpriteSourceKind.SamusProjectileTrail).Image,
            "Intro trail shows its pose's frame of the intro's projectile sheet");
        AssertEqual(99, side.FrameHold, "Intro preparation advances trail exactly once per call");
        Console.WriteLine("Intro projectile artwork: shot, explosion and trail sheet frames and native timing pass.");
    }
}
