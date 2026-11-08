using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy the Baby Metroid feeds on (#1542).</summary>
internal static class BabyMetroidVictimVerification
{
    /// <summary>
    /// Confirms <see cref="BabyMetroidVictim"/> gives the enemy placed directly after the Baby Metroid, wherever the Baby
    /// is placed, and refuses a Baby in the final place, after which no enemy is placed.
    /// </summary>
    internal static void VerifyBabyMetroidVictim()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemySlot> slots = enemies.SlotTable.Slots;
        AssertTrue(ReferenceEquals(slots[1], BabyMetroidVictim.Of(slots, 0)), "the retail Baby's victim is the enemy placed after it");
        AssertTrue(ReferenceEquals(slots[6], BabyMetroidVictim.Of(slots, 5)), "a Baby placed later feeds on the enemy placed after it");
        int last = slots.Count - 1;
        AssertTrue(ReferenceEquals(slots[last], BabyMetroidVictim.Of(slots, last - 1)), "the victim may be the final enemy");
        AssertThrows<ArgumentOutOfRangeException>(() => BabyMetroidVictim.Of(slots, last), "a Baby in the final place has no victim");
        AssertThrows<ArgumentOutOfRangeException>(() => BabyMetroidVictim.Of(slots, -1), "a Baby before the first place is refused");
        Console.WriteLine("Baby Metroid victim: the enemy placed directly after the Baby, and none past the last.");
    }

    /// <summary>Runs the focused Baby Metroid victim check.</summary>
    [VerificationCommand("--baby-metroid-victim")]
    internal static int BabyMetroidVictimCommand()
    {
        VerifyBabyMetroidVictim();
        return 0;
    }
}
