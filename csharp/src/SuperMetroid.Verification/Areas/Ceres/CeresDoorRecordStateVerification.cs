using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Areas.Ceres;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Ceres door values the cartridge keeps in the enemy's common record.</summary>
internal static class CeresDoorRecordStateVerification
{
    /// <summary>Installs a Ceres door state with its record-clearing operation, sets every record value and confirms a kill empties each.</summary>
    internal static void VerifyCeresDoorRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new CeresDoorEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = CeresDoorFunction.RotatingElevatorRumble;
        state.HandsDrawingToRidley = true;
        state.RumbleCallsLeft = 48;
        state.ExplosionIntervalLeft = 4;
        state.ExplosionPositionIndex = 3;

        slot.Clear();
        AssertEqual<CeresDoorFunction?>(null, state.Function, "a kill empties the function");
        AssertEqual(false, state.HandsDrawingToRidley, "a kill takes the drawing back");
        AssertEqual(0, state.RumbleCallsLeft, "a kill empties the rumble count");
        AssertEqual(0, state.ExplosionIntervalLeft, "a kill empties the explosion interval");
        AssertEqual(0, state.ExplosionPositionIndex, "a kill empties the explosion position");
        Console.WriteLine("Ceres door record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Ceres door record-state check.</summary>
    [VerificationCommand("--ceres-door-record-state")]
    internal static int CeresDoorRecordStateCommand()
    {
        VerifyCeresDoorRecordState();
        return 0;
    }
}
