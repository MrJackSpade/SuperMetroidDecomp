using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// Checks where a Work Robot must stand for its laser to request the laser sound (#2186) against the original lasers'
/// common setup: each edge test is the sign of a sixteen-bit difference, and the last compares the robot's top edge less
/// a screen's height with nothing, never with the camera's top.
/// </summary>
internal static class WorkRobotLaserSoundRangeVerification
{
    /// <summary>Whether the sixteen-bit difference <paramref name="value"/> less <paramref name="subtracted"/> is negative.</summary>
    private static bool DifferenceIsNegative(int value, int subtracted) => ((value - subtracted) & 0x8000) != 0;

    /// <summary>
    /// The rule matches the original's word arithmetic for robots across both edges of every test, with cameras on, above,
    /// below and beside them, and the production laser requests its sound from a robot in the room's top screen's height
    /// and none from the same robot on screen one screen lower.
    /// </summary>
    internal static void VerifyWorkRobotLaserSoundRange()
    {
        var radius = new Extent16<RoomPixels>(12, 16);
        int compared = 0;
        int[] cameraXs = [0, 0x100, 0x200, 0x7f00, 0xff00];
        int[] cameraYs = [0, 0x20, 0x100, 0x200, 0x7f00, 0xff00];
        foreach (int cameraX in cameraXs)
        foreach (int cameraY in cameraYs)
        for (int offsetX = -0x120; offsetX <= 0x220; offsetX += 4)
        foreach (int y in new[] { 0x0000, 0x000f, 0x0010, 0x0011, 0x0020, 0x00ef, 0x00f0, 0x00f1, 0x00ff, 0x0100, 0x0110, 0x0200, 0x7fff, 0x8000, 0xffef, 0xfff0, 0xffff })
        {
            int x = (cameraX + offsetX) & 0xffff;
            var robot = new Position16<RoomPixels>(x, y);
            var camera = new Position16<RoomPixels>(cameraX, cameraY);
            bool expected =
                !DifferenceIsNegative(x + radius.X, cameraX) &&
                DifferenceIsNegative(x - radius.X - 0x101, cameraX) &&
                !DifferenceIsNegative(y + radius.Y, cameraY) &&
                DifferenceIsNegative(y - radius.Y - 0xe0, 0);
            AssertEqual(expected, WorkRobotLaserSoundRange.RequestsSound(robot, radius, camera),
                $"a laser from a robot at {x:x4},{y:x4} with the camera at {cameraX:x4},{cameraY:x4}");
            compared++;
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var place = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            Radius = radius,
        };
        var state = new WorkRobotEnemyState { Facing = HorizontalDirection.Left };
        enemies.WorkRobotBehavior.SpawnWorkRobotLaser(place, state, WorkRobotLaser.Horizontal, new Position16<RoomPixels>(0, 0));
        AssertEqual(1, enemies.Audio.SoundRequests.Count(request => request == new SoundRequest(EnemySoundEffectDefinitions.WorkRobotLaserSound, SoundRequestPriority.Normal)),
            "a robot in the room's top screen's height requests the laser sound");

        place.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112 + ScreenGeometry.ScreenHeightPixels));
        int before = enemies.Audio.SoundRequests.Count;
        enemies.WorkRobotBehavior.SpawnWorkRobotLaser(place, state, WorkRobotLaser.Horizontal, new Position16<RoomPixels>(0, ScreenGeometry.ScreenHeightPixels));
        AssertEqual(2, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Animation is WorkRobotLaserAnimation),
            "the lower robot still fires its laser");
        AssertEqual(before, enemies.Audio.SoundRequests.Count, "the same robot on screen one screen lower fires silently");
        Console.WriteLine($"  Work Robot laser sound range: {compared} robot and camera placements match the original's tests; the production laser is heard from the top screen only.");
    }
}
