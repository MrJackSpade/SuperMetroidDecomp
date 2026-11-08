using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemies the Dragon's and the Evir's follow-up to a hit reaches (#1500).</summary>
internal static class FollowUpAfterStruckPartVerification
{
    /// <summary>
    /// Confirms <see cref="FollowUpAfterStruckPart"/> reaches the places the original follow-ups write, one and two
    /// records after the struck part, whichever part was struck: a Dragon body's follow-up reaches its wing, a power
    /// bomb on the wing in Main Hall's place 18 reaches place 19; an Evir body's follows its arms and spit, a power bomb
    /// on the West Sand Hall arms in place 4 reaches their spit and the next Evir's body. A part placed where its
    /// follow-up would pass the final place, which no retail population does, is refused.
    /// </summary>
    internal static void VerifyFollowUpAfterStruckPart()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        EnemySlotTable table = enemies.SlotTable;
        IReadOnlyList<RoomEnemySlot> slots = table.Slots;
        AssertEqual(EnemySlotOrder.Capacity, slots.Count, "the room has every enemy place");

        IReadOnlyList<RoomEnemySlot> dragonBody = FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[17], distance), reach: 1);
        AssertEqual(1, dragonBody.Count, "a Dragon's follow-up reaches one enemy");
        AssertTrue(ReferenceEquals(slots[18], dragonBody[0]), "a Dragon body's follow-up reaches its wing");
        AssertTrue(ReferenceEquals(slots[19], FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[18], distance), reach: 1)[0]),
            "a power bomb on a Dragon wing reaches the enemy after it");

        IReadOnlyList<RoomEnemySlot> evirArms = FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[4], distance), reach: 2);
        AssertEqual(2, evirArms.Count, "an Evir's follow-up reaches two enemies");
        AssertTrue(ReferenceEquals(slots[5], evirArms[0]) && ReferenceEquals(slots[6], evirArms[1]),
            "a power bomb on Evir arms reaches their spit and the next Evir's body, nearest first");

        int last = slots.Count - 1;
        AssertTrue(ReferenceEquals(slots[last], FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[last - 2], distance), reach: 2)[1]),
            "a follow-up may reach the final place");
        AssertThrows<InvalidDataException>(() => FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[last], distance), reach: 1),
            "a follow-up past the final place is refused");
        AssertThrows<InvalidDataException>(() => FollowUpAfterStruckPart.Recipients(distance => table.ListedAfter(slots[last - 1], distance), reach: 2),
            "a two-place follow-up past the final place is refused");
        Console.WriteLine("Follow-up after the struck part: Dragon and Evir follow-ups reach the places after the part hit, and none past the last.");
    }

    /// <summary>Runs the focused follow-up-after-struck-part check.</summary>
    [VerificationCommand("--follow-up-after-struck-part")]
    internal static int FollowUpAfterStruckPartCommand()
    {
        VerifyFollowUpAfterStruckPart();
        return 0;
    }
}
