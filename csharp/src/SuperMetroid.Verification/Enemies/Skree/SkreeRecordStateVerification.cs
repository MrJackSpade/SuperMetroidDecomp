using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;
namespace SuperMetroid.Verification.Enemies.Skree;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Skree values the cartridge keeps in the enemy's common record.</summary>
internal static class SkreeRecordStateVerification
{
    /// <summary>
    /// Installs a Skree state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifySkreeRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new SkreeEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.BurrowTimer = NativeCountdown.Start(1);
        state.Function = Enum.GetValues<SkreeEnemyFunction>()[^1];
        state.RequestedInstructionIndex = Enum.GetValues<SkreeMetareeAnimationPhase>()[^1];
        state.InstalledInstructionIndex = Enum.GetValues<SkreeMetareeAnimationPhase>()[^1];
        state.AttackReady = true;

        slot.Clear();
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.BurrowTimer, "a kill empties BurrowTimer");
        AssertEqual<SkreeEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<SkreeMetareeAnimationPhase>(default(SkreeMetareeAnimationPhase), state.RequestedInstructionIndex, "a kill empties RequestedInstructionIndex");
        AssertEqual<SkreeMetareeAnimationPhase>(default(SkreeMetareeAnimationPhase), state.InstalledInstructionIndex, "a kill empties InstalledInstructionIndex");
        AssertEqual<bool>(false, state.AttackReady, "a kill empties AttackReady");
        Console.WriteLine("Skree record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Skree record-state check.</summary>
    [VerificationCommand("--skree-record-state")]
    internal static int SkreeRecordStateCommand()
    {
        VerifySkreeRecordState();
        return 0;
    }
}
