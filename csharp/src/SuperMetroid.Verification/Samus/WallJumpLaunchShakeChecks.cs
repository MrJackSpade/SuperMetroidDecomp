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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.SlotTable.CoverSpawn(enemies.SlotTable.Slots[enemies.SlotTable.Slots.Count - 1]);
        return enemies;
    }

    /// <summary>
    /// Runs <paramref name="room"/>'s enemy pass shake for <paramref name="samus"/>: only <paramref name="expected"/>
    /// shakes, for 64 draws, or no enemy when it is null, and the pass takes the request once.
    /// </summary>
    internal static void AssertEnemyPassShakes(RoomEnemySystem room, SamusState samus, RoomEnemySlot? expected, string message)
    {
        room.ShakeWallJumpLaunchEnemy(samus);
        foreach (RoomEnemySlot slot in room.SlotTable.Slots)
            AssertEqual(ReferenceEquals(slot, expected) ? LaunchShakeDraws : 0, slot.ShakeTimer, $"{message}: enemy {slot.SlotIndex}'s shake");
        AssertEqual<ISolidEnemy?>(null, samus.Kinematics.TakeWallJumpLaunchShake(), $"{message}: the enemy pass takes the request once");
    }
}
