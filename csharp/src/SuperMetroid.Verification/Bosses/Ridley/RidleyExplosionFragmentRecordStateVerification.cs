using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Ridley explosion fragment values the cartridge keeps in the enemy's common record.</summary>
internal static class RidleyExplosionFragmentRecordStateVerification
{
    /// <summary>Installs a Ridley explosion fragment state with its record-clearing operation, sets every record value and confirms a kill empties each.</summary>
    internal static void VerifyRidleyExplosionFragmentRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new RidleyExplosionFragmentState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Velocity = new(new SignedEightEight(0x0130), new SignedEightEight(4));
        state.Lifetime = WrappingCountdown.Start(72);

        slot.Clear();
        AssertEqual(default(EightEightVector<RoomPixels>), state.Velocity, "a kill empties the velocity");
        AssertEqual(WrappingCountdown.Start(0), state.Lifetime, "a kill empties the lifetime");
        Console.WriteLine("Ridley explosion fragment record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Ridley explosion fragment record-state check.</summary>
    [VerificationCommand("--ridley-explosion-fragment-record-state")]
    internal static int RidleyExplosionFragmentRecordStateCommand()
    {
        VerifyRidleyExplosionFragmentRecordState();
        return 0;
    }
}
