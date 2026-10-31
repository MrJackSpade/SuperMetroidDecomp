using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
namespace SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Puyo state the species owns: the airborne index's two sign-bit restarts and the cleared record.</summary>
internal static class PuyoOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step landing on zero keeps rising, one that passes below zero restarts the fall at zero (the
    /// apex), a falling step that reaches $8000 restarts the fall at zero as the cartridge's sign test does, an
    /// ordinary falling step accumulates, and a kill empties the values kept in the common record.
    /// </summary>
    internal static void VerifyPuyoOwnedState()
    {
        var hop = new PuyoEnemyState { YSpeedTableIndex = 0x0200 };
        hop.StepAirborneIndex(0x0200);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, false), new VerifyPuyoOwnedStateCase(hop.YSpeedTableIndex, hop.Falling), "a rising step landing on zero keeps rising");
        hop.StepAirborneIndex(0x0200);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedTableIndex, hop.Falling), "a rising step below zero is the apex");
        hop.StepAirborneIndex(0x01c0);
        AssertEqual(new VerifyPuyoOwnedStateCase(0x01c0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedTableIndex, hop.Falling), "a falling step accumulates");
        hop.YSpeedTableIndex = 0x7f00;
        hop.StepAirborneIndex(0x0100);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedTableIndex, hop.Falling), "a falling step reaching the sign bit restarts the fall");
        hop.YSpeedTableIndex = 0x7e00;
        hop.StepAirborneIndex(0x0100);
        AssertEqual(new VerifyPuyoOwnedStateCase(0x7f00, true), new VerifyPuyoOwnedStateCase(hop.YSpeedTableIndex, hop.Falling), "a falling step below the sign bit keeps falling");

        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puyo, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Puyo, 5);
        PuyoBehavior.InitializePuyo(slot);
        PuyoEnemyState state = enemies.PuyoStates[0]
            ?? throw new InvalidOperationException("The initialized Puyo has no state.");
        AssertEqual(null, state.AirborneFunction, "a fresh Puyo has no airborne behaviour");
        state.HopRecord = 3;
        state.YSpeedTableIndex = 0x1234;
        slot.Clear();
        AssertEqual(new PuyoOwnedStateVerifyPuyoOwnedStateCase(0, 0, 0), new PuyoOwnedStateVerifyPuyoOwnedStateCase(state.HopRecord, state.YSpeedTableIndex, state.HopCooldownTimer.RawValue),
            "a kill empties the hop record, speed index and cooldown");
        AssertEqual(null, state.Function, "a kill empties the function");
        Console.WriteLine("Puyo owned state: the apex, the overlong-fall restart and the cleared record match.");
    }

    /// <summary>Runs the focused Puyo owned-state check.</summary>
    [VerificationCommand("--puyo-owned-state")]
    internal static int PuyoOwnedStateCommand()
    {
        VerifyPuyoOwnedState();
        return 0;
    }
}
