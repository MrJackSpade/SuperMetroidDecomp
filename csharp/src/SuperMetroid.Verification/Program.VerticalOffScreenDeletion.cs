using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: $86:B5B9 keeps n00b-tube shards above the camera and until they are $120
    // pixels below its top row. The port deleted them at $100, removing a 100% movie shard
    // while native still advanced it.
    /// <summary>
    /// Verifies Noob Tube shards remain active above and less than $120 pixels below the
    /// camera's top row, and are deleted at the native $120-pixel boundary.
    /// </summary>
    private static void VerifyVerticalOffScreenDeletion()
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
                YPosition = y,
            };
            RoomEnemySystem.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
            AssertEqual(kept, projectile.IsActive, label);
        }
    }
}
