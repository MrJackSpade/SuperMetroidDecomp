using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy a shot on a Zebetite shares its health and flash with (#1542).</summary>
internal static class ZebetiteShotShareVerification
{
    /// <summary>
    /// Confirms <see cref="ZebetiteShotShare"/> gives a linked Zebetite's other half, and a Zebetite with no linked half
    /// the first enemy placed in its room.
    /// </summary>
    internal static void VerifyZebetiteShotShare()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemySlot> slots = enemies.SlotTable.Slots;
        AssertTrue(ReferenceEquals(slots[4], ZebetiteShotShare.Recipient(slots, slots[4])), "a linked Zebetite shares with its other half");
        AssertTrue(ReferenceEquals(slots[0], ZebetiteShotShare.Recipient<RoomEnemySlot>(slots, null)), "a single Zebetite shares with the first enemy placed");
        AssertThrows<InvalidOperationException>(() => ZebetiteShotShare.Recipient<RoomEnemySlot>([], null), "a single Zebetite in a room with no enemy is refused");
        Console.WriteLine("Zebetite shot share: a linked half, or the first enemy placed for a single Zebetite.");
    }

    /// <summary>Runs the focused Zebetite shot-share check.</summary>
    [VerificationCommand("--zebetite-shot-share")]
    internal static int ZebetiteShotShareCommand()
    {
        VerifyZebetiteShotShare();
        return 0;
    }
}
