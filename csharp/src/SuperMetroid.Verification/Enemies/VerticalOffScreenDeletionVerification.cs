using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the native camera-relative deletion boundary for n00b-tube shards.</summary>
internal static class VerticalOffScreenDeletionVerification
{
    /// <summary>
    /// Confirms $86:B5B9 retains shards above the camera and through $11F below its top row;
    /// issue #1269 exposed the former $100 cutoff when it removed a shard still advancing in the movie.
    /// </summary>
    internal static void VerifyVerticalOffScreenDeletion()
    {
        const ushort cameraY = 0x100;
        Confirm(0x0ff, cameraY, kept: true, "a projectile above the camera is retained");
        Confirm(0x200, cameraY, kept: true, "$100 below the camera top is still retained");
        Confirm(0x21f, cameraY, kept: true, "$11F below the camera top is still retained");
        Confirm(0x220, cameraY, kept: false, "$120 below the camera top deletes the projectile");
        // #2515: a projectile risen above the room's top keeps a wrapped height; measured the short way it lies above a
        // camera at the room's top and is kept, where a plain difference would put it far below and delete it.
        Confirm(0xfff0, cameraY: 0, kept: true, "a projectile above the room's top is retained by a camera at the top");
        Confirm(0x0120, cameraY: 0, kept: false, "$120 below a camera at the room's top deletes the projectile");
        Console.WriteLine("Vertical off-screen deletion: $86:B5B9 retains above-camera, above-room and sub-$120 projectiles.");

        static void Confirm(ushort y, ushort cameraY, bool kept, string label)
        {
            var projectile = (new EnemyProjectile(0, default, RoomEnemyProjectileKind.NoobTubeShard, SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileDeleteAnimation.Instance)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0, y)),
                Kind = RoomEnemyProjectileKind.NoobTubeShard
            });
            EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
            AssertEqual(kept, projectile.IsActive, label);
        }
    }

    /// <summary>Runs the focused camera-relative shard lifetime check from the verification command line.</summary>
    [VerificationCommand("--vertical-offscreen-deletion")]
    internal static int VerticalOffscreenDeletionCommand()
    {
        VerifyVerticalOffScreenDeletion();
        return 0;
    }
}
