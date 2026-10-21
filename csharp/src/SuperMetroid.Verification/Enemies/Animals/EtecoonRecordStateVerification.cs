using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Etecoon values the cartridge keeps in the enemy's common record.</summary>
internal static class EtecoonRecordStateVerification
{
    /// <summary>
    /// Installs a Etecoon state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyEtecoonRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new EtecoonEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Velocity = FixedVector<RoomPixels>.FromWords(1, 0, 0, 0);
        state.FunctionTimer = WrappingCountdown.Start(1);
        state.Function = Enum.GetValues<EtecoonAiFunction>()[^1];

        slot.Clear();
        AssertEqual<FixedVector<RoomPixels>>(FixedVector<RoomPixels>.Zero, state.Velocity, "a kill empties Velocity");
        AssertEqual<WrappingCountdown>(WrappingCountdown.Start(0), state.FunctionTimer, "a kill empties FunctionTimer");
        AssertEqual<EtecoonAiFunction?>(null, state.Function, "a kill empties Function");
        Console.WriteLine("Etecoon record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Etecoon record-state check.</summary>
    [VerificationCommand("--etecoon-record-state")]
    internal static int EtecoonRecordStateCommand()
    {
        VerifyEtecoonRecordState();
        return 0;
    }
}
