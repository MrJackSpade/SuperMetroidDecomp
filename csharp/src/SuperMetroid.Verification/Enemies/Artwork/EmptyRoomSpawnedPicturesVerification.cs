using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks #1560 and #1599: a room whose population is empty loads no enemy artwork, so it draws its spawned objects in
/// their standard variants, the session start's, whatever set its state names. Every projectile the room's own data
/// spawns, such as the smoke The Precious's eye door blows when a missile hits it, is pictured on the standard shared
/// variant's sheets.
/// </summary>
internal static class EmptyRoomSpawnedPicturesVerification
{
    /// <summary>Draws every projectile frame the common spawners of each empty-population room spawn, in the standard variants.</summary>
    internal static void VerifyEmptyRoomSpawnedPictures()
    {
        var session = new EnemySession(InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint(),
            InstalledFixtures.EnemySheets, InstalledFixtures.EnemyFrameDisplays);
        // A fresh presentation draws the standard artwork: the room load draws none of an empty population's.
        var presentation = new EnemyPresentation(new EnemyRoomBinding(session, CompiledRoomEnemyDefinitions.Instance, services: null), new SharedFadeStepCount());
        VerificationAssertions.AssertTrue(presentation.DrawnPopulation is null && ReferenceEquals(presentation.Artwork, InstalledFixtures.EnemySheets.Standard),
            "a room that has placed no enemies draws the standard artwork");
        SpriteGraphics shared = SpriteGraphics.SharedIn(PictureShading.Standard);

        var frames = new HashSet<EnemyProjectileFrameId>();
        int rooms = 0;
        bool precious = false;
        foreach (RoomStateSettings state in RoomStateDefinitions.All.Where(state => state.EnemyPopulation.Placements.IsEmpty))
        {
            rooms++;
            foreach (CommonSpawnedObjects spawner in CommonSpawners.RunningIn([state], []))
            {
                frames.UnionWith(spawner.Objects.Projectiles);
                precious |= state == MaridiaRoomStateDefinitions.ThePreciousDefault && spawner.Spawner == nameof(SuperMetroid.Core.Enemies.EyeDoor.EyeDoorBehavior);
            }
        }
        VerificationAssertions.AssertTrue(precious, "The Precious runs its eye door's smoke spawner");
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        foreach (EnemyProjectileFrameId frame in frames)
        {
            // Fails loudly, naming the sheets, when the standard shared variant pictures no such frame.
            presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile), EnemySheetDefinitions.ProjectileFrameName(frame),
                new Position16<ScreenPixels>(128, 128), shared);
        }
        sprites.FinalizeFrame();
        Console.WriteLine($"  Empty rooms: {frames.Count} projectile frames that {rooms} empty-population room states spawn resolve in the standard variants.");
    }
}
