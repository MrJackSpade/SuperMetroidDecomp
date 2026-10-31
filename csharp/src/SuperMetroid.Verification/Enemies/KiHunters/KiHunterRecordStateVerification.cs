using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the KiHunter values the cartridge keeps in the enemy's common record.</summary>
internal static class KiHunterRecordStateVerification
{
    /// <summary>
    /// Installs a KiHunter state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyKiHunterRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new KiHunterEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<KiHunterEnemyFunction>()[^1];
        state.SwoopTarget = new Position16<RoomPixels>(1, 0);
        state.DetachedSpeedIndex = 1;
        state.SwoopOrigin = new Position16<RoomPixels>(1, 0);
        state.DetachedSpeedAccumulator = 1;
        state.AnimationChangeAngle = 1;
        state.SwoopAngle = 1;
        state.WingArcAngle = RotationAngle.FromTableIndex(1);

        slot.Clear();
        AssertEqual<KiHunterEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.SwoopTarget, "a kill empties SwoopTarget");
        AssertEqual<int>(0, state.DetachedSpeedIndex, "a kill empties DetachedSpeedIndex");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.SwoopOrigin, "a kill empties SwoopOrigin");
        AssertEqual<int>(0, state.DetachedSpeedAccumulator, "a kill empties DetachedSpeedAccumulator");
        AssertEqual<int>(0, state.AnimationChangeAngle, "a kill empties AnimationChangeAngle");
        AssertEqual<int>(0, state.SwoopAngle, "a kill empties SwoopAngle");
        AssertEqual<RotationAngle>(RotationAngle.FromTableIndex(0), state.WingArcAngle, "a kill empties WingArcAngle");
        Console.WriteLine("KiHunter record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused KiHunter record-state check.</summary>
    [VerificationCommand("--ki-hunter-record-state")]
    internal static int KiHunterRecordStateCommand()
    {
        VerifyKiHunterRecordState();
        return 0;
    }
}
