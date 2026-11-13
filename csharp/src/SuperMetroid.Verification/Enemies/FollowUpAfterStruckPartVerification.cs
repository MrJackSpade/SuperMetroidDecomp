using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
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
        VerifyRetailMagdolliteFollowUps(table);
        Console.WriteLine("Follow-up after the struck part: Dragon, Evir and Magdollite follow-ups reach the places after the part hit, and none past the last.");
    }

    /// <summary>
    /// Every Magdollite part of every retail population, placed at its listing's place, reaches the two places after it, as
    /// the original's follow-up writes the records one and two after the struck part (#1620). A Magdollite's rising body
    /// and tracking overlay, which a power bomb kills, reach the enemies the room lists after their Magdollite: the Acid
    /// Statue room's Holtz pair, the next Magdollite in the Magdollite Tunnel, and the Multiviolas after its last.
    /// </summary>
    private static void VerifyRetailMagdolliteFollowUps(EnemySlotTable table)
    {
        IReadOnlyList<RoomEnemySlot> slots = table.Slots;
        var reachedPastTheirMagdollite = new SortedSet<EnemyDefinitionId>();
        int parts = 0;
        foreach (RoomEnemyPopulationDefinition population in RoomEnemyPopulationDefinitions.Definitions)
        {
            RoomEnemyPopulationRecord[] records = population.Records.ToArray();
            int head = -1;
            for (int place = 0; place < records.Length; place++)
            {
                if (records[place].DefinitionId != EnemyDefinitionId.Magdollite)
                    continue;
                parts++;
                if (records[place].SpawnSettings is MagdolliteSpawnSettings { Part: MagdollitePart.Head })
                    head = place;
                int part = place;
                IReadOnlyList<RoomEnemySlot?> recipients = FollowUpAfterStruckPart.Recipients(distance => table.ListedAfterOrNone(slots[part], distance), reach: 2);
                AssertTrue(ReferenceEquals(slots[part + 1], recipients[0]) && ReferenceEquals(slots[part + 2], recipients[1]),
                    $"{population.Name}'s Magdollite part at place {part} reaches the two places after it");
                for (int distance = 1; distance <= 2; distance++)
                {
                    if (part + distance > head + 2 && part + distance < records.Length)
                        reachedPastTheirMagdollite.Add(records[part + distance].DefinitionId);
                }
            }
        }
        AssertEqual(12, parts, "the retail populations list four Magdollites of three parts each");
        AssertTrue(reachedPastTheirMagdollite.SetEquals([EnemyDefinitionId.Holtz, EnemyDefinitionId.Magdollite, EnemyDefinitionId.Multiviola]),
            $"the Magdollites' trailing parts reach {string.Join(", ", reachedPastTheirMagdollite)}");
    }

    /// <summary>Runs the focused follow-up-after-struck-part check.</summary>
    [VerificationCommand("--follow-up-after-struck-part")]
    internal static int FollowUpAfterStruckPartCommand()
    {
        VerifyFollowUpAfterStruckPart();
        return 0;
    }
}
