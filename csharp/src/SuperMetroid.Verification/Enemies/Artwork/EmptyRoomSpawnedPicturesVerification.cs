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
/// Checks #1560: a room whose population is empty loads no enemy graphics set, so it draws its spawned objects under the
/// session-start set, whatever set its state names. Every projectile the room's own data spawns, such as the smoke The
/// Precious's eye door blows when a missile hits it, is pictured on that set's shared sheet.
/// </summary>
internal static class EmptyRoomSpawnedPicturesVerification
{
    /// <summary>Draws every projectile frame the common spawners of each empty-population room spawn, under the session-start set.</summary>
    internal static void VerifyEmptyRoomSpawnedPictures()
    {
        // A fresh graphics set holds the session-start set: the room load loads none for an empty population.
        var graphics = new EnemyGraphicsSet();
        VerificationAssertions.AssertEqual(RoomEnemyGraphicsSetDefinitions.SessionStart, graphics.LoadedSet,
            "a room that loads no set draws under the session-start set");
        var session = new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint())
        {
            FrameDisplays = InstalledFixtures.EnemyTiles.ExtendedFrames?.Displays,
            Sheets = InstalledFixtures.EnemySheets,
        };
        var presentation = new EnemyPresentation(new EnemyRoomBinding(session, CompiledRoomEnemyDefinitions.Instance, services: null), graphics);
        SpriteGraphics shared = SpriteGraphics.SharedIn(PictureShading.Standard);

        var frames = new HashSet<EnemyProjectileFrameId>();
        int rooms = 0;
        bool precious = false;
        foreach (RoomStateSettings state in RoomStateDefinitions.All.Where(state => state.EnemyPopulation.Records.IsEmpty))
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
            // Fails loudly, naming the set and the sheets, when the session-start set pictures no such frame.
            presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile), EnemySheetDefinitions.ProjectileFrameName(frame),
                new Position16<ScreenPixels>(128, 128), shared);
        }
        sprites.FinalizeFrame();
        Console.WriteLine($"  Empty rooms: {frames.Count} projectile frames that {rooms} empty-population room states spawn resolve under the session-start set.");
    }
}
