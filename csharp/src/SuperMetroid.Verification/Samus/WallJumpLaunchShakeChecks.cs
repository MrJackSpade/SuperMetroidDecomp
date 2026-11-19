using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy the enemy pass shakes after Samus's wall-jump checks (#1697).</summary>
internal static class WallJumpLaunchShakeChecks
{
    /// <summary>The draws the cartridge's enemy pass shakes the enemy a wall jump launched from.</summary>
    private const int LaunchShakeDraws = 64;

    /// <summary>A room whose every enemy place is filled, so its enemy pass takes a waiting shake.</summary>
    internal static RoomEnemySystem CreateFilledRoom()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.Places.CoverSpawn(enemies.Places.InUpdateOrder[enemies.Places.InUpdateOrder.Count - 1]);
        return enemies;
    }

    /// <summary>
    /// Runs <paramref name="room"/>'s enemy pass shake for <paramref name="samus"/>: only <paramref name="expected"/>
    /// shakes, for 64 draws, or no enemy when it is null, and the pass takes the request once.
    /// </summary>
    internal static void AssertEnemyPassShakes(RoomEnemySystem room, SamusState samus, RoomEnemyPlace? expected, string message)
    {
        room.ShakeWallJumpLaunchEnemy(samus);
        foreach (RoomEnemyPlace place in room.Places.InUpdateOrder)
            AssertEqual(ReferenceEquals(place, expected) ? LaunchShakeDraws : 0, place.Shake.UpdatesLeft, $"{message}: enemy {place.Ordinal}'s shake");
        AssertEqual<ISolidEnemy?>(null, samus.Kinematics.WallJumpLaunch.Take(), $"{message}: the enemy pass takes the request once");
    }
}
