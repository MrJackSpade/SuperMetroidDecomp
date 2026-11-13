using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy the enemy pass shakes after Samus's wall-jump checks (#1697).</summary>
internal static class WallJumpLaunchShakeChecks
{
    /// <summary>The draws the cartridge's enemy pass shakes the enemy a wall jump launched from.</summary>
    private const int LaunchShakeDraws = 64;

    /// <summary>
    /// Runs the enemy pass's wall-jump shake for <paramref name="samus"/> over a room whose every enemy place is filled:
    /// only the enemy in <paramref name="expectedSlot"/> shakes, for 64 draws, or none when it is null, and the pass takes
    /// the request once.
    /// </summary>
    internal static void AssertEnemyPassShakes(SamusState samus, int? expectedSlot, string message)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.SlotTable.CoverSpawn(enemies.SlotTable.Slots[enemies.SlotTable.Slots.Count - 1]);
        enemies.ShakeWallJumpLaunchEnemy(samus);
        foreach (RoomEnemySlot slot in enemies.SlotTable.Slots)
            AssertEqual(slot.SlotIndex == expectedSlot ? LaunchShakeDraws : 0, slot.ShakeTimer, $"{message}: enemy {slot.SlotIndex}'s shake");
        AssertEqual<int?>(null, samus.Kinematics.TakeWallJumpLaunchShake(), $"{message}: the enemy pass takes the request once");
    }
}
