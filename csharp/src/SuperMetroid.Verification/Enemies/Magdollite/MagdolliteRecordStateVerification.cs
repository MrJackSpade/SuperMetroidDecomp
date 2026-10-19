using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
namespace SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Magdollite values the cartridge keeps in the enemy's common record.</summary>
internal static class MagdolliteRecordStateVerification
{
    /// <summary>
    /// Installs a Magdollite state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyMagdolliteRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MagdolliteEnemyState(Enum.GetValues<MagdollitePart>()[0]);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<MagdolliteEnemyFunction>()[^1];
        state.BodyPhase = 1;
        state.ThrowsRight = true;
        state.RiseDisplacement = new Offset16<RoomPixels>(1, 0);

        slot.Clear();
        AssertEqual<MagdolliteEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.BodyPhase, "a kill empties BodyPhase");
        AssertEqual<bool>(false, state.ThrowsRight, "a kill empties ThrowsRight");
        AssertEqual<Offset16<RoomPixels>>(Offset16<RoomPixels>.Zero, state.RiseDisplacement, "a kill empties RiseDisplacement");
        Console.WriteLine("Magdollite record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Magdollite record-state check.</summary>
    [VerificationCommand("--magdollite-record-state")]
    internal static int MagdolliteRecordStateCommand()
    {
        VerifyMagdolliteRecordState();
        return 0;
    }
}
