using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
namespace SuperMetroid.Verification.Enemies.Yard;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Yard values the cartridge keeps in the enemy's common record.</summary>
internal static class YardRecordStateVerification
{
    /// <summary>
    /// Installs a Yard state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyYardRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new YardEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.CrawlingVelocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(1), default);
        state.AirborneFacingDirection = YardAirborneFacing.Right;
        state.ConsecutiveTurnCounter = 1;
        state.MovementFunction = Enum.GetValues<YardMovementFunction>()[^1];

        slot.Clear();
        AssertEqual<EightEightVector<RoomPixels>>(default(EightEightVector<RoomPixels>), state.CrawlingVelocity, "a kill empties CrawlingVelocity");
        AssertEqual(YardAirborneFacing.Left, state.AirborneFacingDirection, "a kill empties AirborneFacingDirection");
        AssertEqual<int>(0, state.ConsecutiveTurnCounter, "a kill empties ConsecutiveTurnCounter");
        AssertEqual<YardMovementFunction?>(null, state.MovementFunction, "a kill empties MovementFunction");
        Console.WriteLine("Yard record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Yard record-state check.</summary>
    [VerificationCommand("--yard-record-state")]
    internal static int YardRecordStateCommand()
    {
        VerifyYardRecordState();
        return 0;
    }
}
