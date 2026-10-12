using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;

internal static class VerticalOffScreenDeletionVerification
{
    // #1269: $86:B5B9 keeps n00b-tube shards above the camera and until they are $120
    // pixels below its top row. The port deleted them at $100, removing a 100% movie shard
    // while native still advanced it.
    internal static void VerifyVerticalOffScreenDeletion()
    {
        const ushort cameraY = 0x100;
        Confirm(0x0ff, kept: true, "a projectile above the camera is retained");
        Confirm(0x200, kept: true, "$100 below the camera top is still retained");
        Confirm(0x21f, kept: true, "$11F below the camera top is still retained");
        Confirm(0x220, kept: false, "$120 below the camera top deletes the projectile");
        Console.WriteLine("Vertical off-screen deletion: $86:B5B9 retains above-camera and sub-$120 projectiles.");

        static void Confirm(ushort y, bool kept, string label)
        {
            var projectile = new RoomEnemyProjectileSlot(0)
            {
                Kind = RoomEnemyProjectileKind.NoobTubeShard,
                Position = FixedPosition<RoomPixels>.FromWhole(new(0, y)),
            };
            EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
            AssertEqual(kept, projectile.IsActive, label);
        }
    }

    [VerificationCommand("--vertical-offscreen-deletion")]
    internal static int VerticalOffscreenDeletionCommand()
    {
        VerifyVerticalOffScreenDeletion();
        return 0;
    }
}
