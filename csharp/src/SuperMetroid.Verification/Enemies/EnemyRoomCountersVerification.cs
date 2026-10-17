using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the room-wide enemy counters stored as ordinary quantities: each accepts exactly the range its producers can
/// reach and rejects anything wider, and the kill count keeps its wrap through its one named advance.
/// </summary>
internal static class EnemyRoomCountersVerification
{
    /// <summary>Checks the darkness, slot-table and kill-count ranges.</summary>
    internal static void VerifyEnemyRoomCounters()
    {
        var enemies = new RoomEnemySystem();

        enemies.RoomStatus.FirefleaDarknessLevel = EnemyRoomStatus.MaximumFirefleaDarknessLevel;
        AssertEqual(12, enemies.RoomStatus.FirefleaDarknessLevel, "darkness reaches its deepest accepted level");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessLevel = 14,
            "darkness rejects the step the Fireflea advance never takes");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessLevel = -1,
            "darkness rejects a negative level");

        enemies.SlotTable.EnemyCount = RoomEnemySystem.MaximumEnemyCount;
        AssertEqual(32, enemies.SlotTable.EnemyCount, "a full population fills every slot");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.SlotTable.EnemyCount = RoomEnemySystem.MaximumEnemyCount + 1,
            "the population count cannot exceed the slot table");
        enemies.SlotTable.FirstFreeEnemyIndex = RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize;
        AssertEqual(2048, enemies.SlotTable.FirstFreeEnemyIndex, "a full population puts the frontier past the last slot");
        AssertThrows<ArgumentOutOfRangeException>(
            () => enemies.SlotTable.FirstFreeEnemyIndex = RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize + 1,
            "the allocation frontier cannot pass the slot table");

        enemies.RoomStatus.EnemiesKilled = ushort.MaxValue;
        enemies.RoomStatus.RecordKill();
        AssertEqual(0, enemies.RoomStatus.EnemiesKilled, "the kill count wraps from 65535 to zero");

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
