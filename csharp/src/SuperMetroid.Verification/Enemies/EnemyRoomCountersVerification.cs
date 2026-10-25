using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks the room-wide enemy counters stored as ordinary quantities: each accepts exactly the range its producers can
/// reach and rejects anything wider, and the kill count keeps its wrap through its one named advance.
/// </summary>
internal static class EnemyRoomCountersVerification
{
    /// <summary>Checks the darkness, slot-table and kill-count ranges.</summary>
    internal static void VerifyEnemyRoomCounters()
    {
        var enemies = new RoomEnemySystem(new EnemySession());

        enemies.RoomStatus.FirefleaDarknessLevel = EnemyRoomStatus.MaximumFirefleaDarknessLevel;
        AssertEqual(12, enemies.RoomStatus.FirefleaDarknessLevel, "darkness reaches its deepest accepted level");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessLevel = 14,
            "darkness rejects the step the Fireflea advance never takes");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.FirefleaDarknessLevel = -1,
            "darkness rejects a negative level");

        enemies.SlotTable.EnemyCount = EnemySlotTable.MaximumEnemyCount;
        AssertEqual(32, enemies.SlotTable.EnemyCount, "a full population fills every slot");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.SlotTable.EnemyCount = EnemySlotTable.MaximumEnemyCount + 1,
            "the population count cannot exceed the slot table");
        enemies.SlotTable.FirstFreeEnemyIndex = EnemySlotTable.MaximumEnemyCount * EnemySlotTable.EnemyIndexStride;
        AssertEqual(2048, enemies.SlotTable.FirstFreeEnemyIndex, "a full population puts the frontier past the last slot");
        AssertThrows<ArgumentOutOfRangeException>(
            () => enemies.SlotTable.FirstFreeEnemyIndex = EnemySlotTable.MaximumEnemyCount * EnemySlotTable.EnemyIndexStride + 1,
            "the allocation frontier cannot pass the slot table");

        enemies.RoomStatus.EnemiesKilled = ushort.MaxValue;
        enemies.RoomStatus.RecordKill();
        AssertEqual(0, enemies.RoomStatus.EnemiesKilled, "the kill count wraps from 65535 to zero");
        AssertThrows<ArgumentOutOfRangeException>(() => enemies.RoomStatus.EnemiesKilled = ushort.MaxValue + 1,
            "the kill count rejects a value past its period");

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
