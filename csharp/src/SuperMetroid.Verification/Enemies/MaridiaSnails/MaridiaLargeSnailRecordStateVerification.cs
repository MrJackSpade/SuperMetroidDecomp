using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the MaridiaLargeSnail values the cartridge keeps in the enemy's common record.</summary>
internal static class MaridiaLargeSnailRecordStateVerification
{
    /// <summary>
    /// Installs a MaridiaLargeSnail state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyMaridiaLargeSnailRecordState()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new MaridiaLargeSnailEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<MaridiaLargeSnailEnemyFunction>()[^1];
        state.YSpeedTableIndex = 1;
        state.AttackCooldown = NativeCountdown.Start(1);
        state.MovingLeft = true;
        state.RemainingBounces = NativeCountdown.Start(1);
        state.BounceFunction = Enum.GetValues<MaridiaLargeSnailBounceFunction>()[^1];

        slot.Clear();
        AssertEqual<MaridiaLargeSnailEnemyFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.YSpeedTableIndex, "a kill empties YSpeedTableIndex");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.AttackCooldown, "a kill empties AttackCooldown");
        AssertEqual<bool>(false, state.MovingLeft, "a kill empties MovingLeft");
        AssertEqual<NativeCountdown>(NativeCountdown.Start(0), state.RemainingBounces, "a kill empties RemainingBounces");
        AssertEqual<MaridiaLargeSnailBounceFunction?>(null, state.BounceFunction, "a kill empties BounceFunction");
        Console.WriteLine("MaridiaLargeSnail record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused MaridiaLargeSnail record-state check.</summary>
    [VerificationCommand("--maridia-large-snail-record-state")]
    internal static int MaridiaLargeSnailRecordStateCommand()
    {
        VerifyMaridiaLargeSnailRecordState();
        return 0;
    }
}
