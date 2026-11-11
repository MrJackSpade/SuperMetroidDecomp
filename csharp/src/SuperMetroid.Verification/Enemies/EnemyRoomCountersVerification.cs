using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the room-wide enemy counters stored as ordinary quantities: each accepts exactly the range its producers can
/// reach and rejects anything wider, and the kill count counts every kill, past where the game's word would start again.
/// </summary>
internal static class EnemyRoomCountersVerification
{
    /// <summary>Checks the darkness, slot-table and kill-count ranges.</summary>
    internal static void VerifyEnemyRoomCounters()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());

        enemies.RoomStatus.FirefleaDarknessStep = EnemyRoomStatus.MaximumFirefleaDarknessStep;
        AssertEqual(6, enemies.RoomStatus.FirefleaDarknessStep, "darkness reaches its last step");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessStep = 7,
            "darkness rejects the step the Fireflea advance never takes");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessStep = -1,
            "darkness rejects a negative step");

        enemies.SlotTable.EnemyCount = EnemySlotOrder.Capacity;
        AssertEqual(32, enemies.SlotTable.EnemyCount, "a full population fills every slot");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.SlotTable.EnemyCount = EnemySlotOrder.Capacity + 1,
            "the population count cannot exceed the slot table");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.SlotTable.EnemyCount = -1,
            "the population count cannot be negative");

        enemies.RoomStatus.EnemiesKilled = 65535;
        enemies.RoomStatus.RecordKill();
        AssertEqual(65536, enemies.RoomStatus.EnemiesKilled, "the kill count keeps counting past 65535");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.EnemiesKilled = -1,
            "the kill count rejects a negative count");

        Console.WriteLine("Enemy room counters: darkness, slot-table and kill-count ranges hold at their bounds.");
    }

    /// <summary>Runs the focused enemy room counter verification command.</summary>
    [VerificationCommand("--enemy-room-counters")]
    internal static int EnemyRoomCountersCommand()
    {
        Suite(nameof(VerifyEnemyRoomCounters), () => VerifyEnemyRoomCounters());
        return 0;
    }
}
