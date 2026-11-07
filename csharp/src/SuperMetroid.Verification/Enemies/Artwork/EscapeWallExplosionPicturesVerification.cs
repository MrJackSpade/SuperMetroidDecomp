using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks #1563: the Climb's escape setup spawns the old Tourian escape shaft's fake wall itself, outside the room's
/// object population, so the wall's explosion is pictured on the shared sheet of the set each such room state draws
/// under.
/// </summary>
internal static class EscapeWallExplosionPicturesVerification
{
    /// <summary>Draws every explosion frame of the fake wall under each escape-wall state's graphics set.</summary>
    internal static void VerifyEscapeWallExplosionPictures()
    {
        EnemyProjectileFrameId[] explosion = [.. SpawnedObjectFrameDefinitions.Common
            .Single(common => common.Spawner == nameof(OldTourianEscapeShaftWallBehavior)).Objects.Projectiles];
        VerificationAssertions.AssertTrue(explosion.Length > 0, "the fake wall's explosion shows frames");
        RoomStateSettings[] states = [.. RoomStateDefinitions.All.Where(state => state.SetupCallback == RoomSetupCallback.AutoDestroyWallDuringEscape)];
        VerificationAssertions.AssertTrue(states.Length > 0, "a room state's setup spawns the fake wall");
        SpriteGraphics shared = SpriteGraphics.SharedIn(PictureShading.Standard);
        foreach (RoomStateSettings state in states)
        {
            // The room load loads the state's set unless its population is empty.
            var graphics = new EnemyGraphicsSet();
            if (!state.EnemyPopulation.Records.IsEmpty)
                graphics.LoadGraphicsSet(state.EnemyGraphicsSet);
            var session = new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())
            {
                TileArtwork = InstalledFixtures.EnemyTiles,
                Sheets = InstalledFixtures.EnemySheets,
            };
            var presentation = new EnemyPresentation(new EnemyRoomBinding(session, CompiledRoomEnemyDefinitions.Instance, services: null), graphics);
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            foreach (EnemyProjectileFrameId frame in explosion)
            {
                // Fails loudly, naming the set and its sheets, when the set pictures no such frame.
                presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.EnemyProjectile), EnemySheetDefinitions.ProjectileFrameName(frame),
                    new Position16<ScreenPixels>(128, 128), shared);
            }
            sprites.FinalizeFrame();
        }
        Console.WriteLine($"  Escape wall: {explosion.Length} explosion frames resolve under the sets of {states.Length} escape-wall room states.");
    }
}
