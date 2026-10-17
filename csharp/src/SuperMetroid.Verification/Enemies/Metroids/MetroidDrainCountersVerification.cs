using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Rooms.SpriteObjects;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Metroid's drain accumulator and the Mochtroid's attachment count, both wrapping word counters the species own.</summary>
internal static class MetroidDrainCountersVerification
{
    /// <summary>
    /// A Metroid's drain borrows (one energy) exactly when the rate exceeds the accumulator, which then wraps back into
    /// its range; a Mochtroid's attachment count wraps after 65535, and its damage check compares the signed word
    /// difference, so a count past the sign bit does not reach the period until it wraps.
    /// </summary>
    internal static void VerifyMetroidDrainCounters()
    {
        var metroid = new MetroidEnemyState(new RoomEnemySystem().SlotTable.Slots[0], new RoomSpriteObjectSlot(0), new RoomSpriteObjectSlot(1))
        {
            DrainAccumulator = 0x3000,
        };
        AssertTrue(!metroid.DrainBorrows(0x3000), "a rate equal to the accumulator does not borrow");
        AssertEqual(0, metroid.DrainAccumulator, "the accumulator reaches zero");
        AssertTrue(metroid.DrainBorrows(0x3000), "a rate above the accumulator borrows");
        AssertEqual(0xd000, metroid.DrainAccumulator, "the borrow wraps back into the word range");

        var mochtroid = new MochtroidEnemyState(new RoomEnemySystem().SlotTable.Slots[0]) { AttachmentDamageTimer = 79 };
        AssertTrue(!mochtroid.HasReachedDamagePeriod(80), "79 attached updates do not reach the period");
        mochtroid.CountAttachedUpdate();
        AssertTrue(mochtroid.HasReachedDamagePeriod(80), "the 80th attached update reaches the period");
        mochtroid.AttachmentDamageTimer = 0x8050;
        AssertTrue(!mochtroid.HasReachedDamagePeriod(80), "a count past the sign bit compares as negative");
        mochtroid.AttachmentDamageTimer = ushort.MaxValue;
        mochtroid.CountAttachedUpdate();
        AssertEqual(0, mochtroid.AttachmentDamageTimer, "the count wraps after 65535");
        Console.WriteLine("Metroid drain counters: the drain borrow, the attachment wrap and its signed check match.");
    }

    /// <summary>Runs the focused Metroid drain-counter check.</summary>
    [VerificationCommand("--metroid-drain-counters")]
    internal static int MetroidDrainCountersCommand()
    {
        VerifyMetroidDrainCounters();
        return 0;
    }
}
