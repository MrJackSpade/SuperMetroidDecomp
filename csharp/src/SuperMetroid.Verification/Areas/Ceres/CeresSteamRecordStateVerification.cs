using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Ceres steam values the cartridge keeps in the enemy's common record.</summary>
internal static class CeresSteamRecordStateVerification
{
    /// <summary>Installs a Ceres steam state with its record-clearing operation, sets every record value and confirms a kill empties each.</summary>
    internal static void VerifyCeresSteamRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new CeresSteamEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = CeresSteamFunction.ApplyRotatingElevatorOffset;
        state.ActivationDelay = NativeCountdown.Start(20);

        slot.Clear();
        AssertEqual<CeresSteamFunction?>(null, state.Function, "a kill empties the function");
        AssertEqual(NativeCountdown.Start(0), state.ActivationDelay, "a kill empties the activation delay");
        Console.WriteLine("Ceres steam record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Ceres steam record-state check.</summary>
    [VerificationCommand("--ceres-steam-record-state")]
    internal static int CeresSteamRecordStateCommand()
    {
        VerifyCeresSteamRecordState();
        return 0;
    }
}
