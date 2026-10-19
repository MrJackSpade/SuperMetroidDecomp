using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the BabyTurtle values the cartridge keeps in the enemy's common record.</summary>
internal static class BabyTurtleRecordStateVerification
{
    /// <summary>
    /// Installs a BabyTurtle state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyBabyTurtleRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new BabyTurtleEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<BabyTurtleAiFunction>()[^1];
        state.ParentSlotIndex = 1;
        state.SpawnTopCenter = new Position16<RoomPixels>(1, 0);
        state.Velocity = new Offset16<RoomPixels>(1, 0);

        slot.Clear();
        AssertEqual<BabyTurtleAiFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.ParentSlotIndex, "a kill empties ParentSlotIndex");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.SpawnTopCenter, "a kill empties SpawnTopCenter");
        AssertEqual<Offset16<RoomPixels>>(Offset16<RoomPixels>.Zero, state.Velocity, "a kill empties Velocity");
        Console.WriteLine("BabyTurtle record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused BabyTurtle record-state check.</summary>
    [VerificationCommand("--baby-turtle-record-state")]
    internal static int BabyTurtleRecordStateCommand()
    {
        VerifyBabyTurtleRecordState();
        return 0;
    }
}
