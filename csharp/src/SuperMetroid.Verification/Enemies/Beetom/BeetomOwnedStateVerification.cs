using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Beetom;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Beetom state the species owns: its arc speed index at the apex and the floor, and the cleared record.</summary>
internal static class BeetomOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step that lands exactly on zero keeps rising, one that would pass below zero restarts the
    /// index at zero and falls (the cartridge's signed underflow test), falling holds at index 64, and a kill empties
    /// the values kept in the common record while the slot's facing survives it.
    /// </summary>
    internal static void VerifyBeetomOwnedState()
    {
        var arc = new BeetomEnemyState(new BeetomExtensionWords()) { YSpeedTableIndex = 8 };
        arc.StepRisingSpeed(4);
        AssertEqual((4, false), (arc.YSpeedTableIndex, arc.Falling), "a rising step above the apex keeps rising");
        arc.StepRisingSpeed(4);
        AssertEqual((0, false), (arc.YSpeedTableIndex, arc.Falling), "a rising step landing on zero keeps rising");
        arc.StepRisingSpeed(5);
        AssertEqual((0, true), (arc.YSpeedTableIndex, arc.Falling), "a rising step below zero restarts at zero and falls");
        arc.YSpeedTableIndex = 61;
        arc.StepFallingSpeed(3);
        AssertEqual(64, arc.YSpeedTableIndex, "falling reaches the maximum");
        arc.StepFallingSpeed(3);
        AssertEqual(64, arc.YSpeedTableIndex, "falling holds at the maximum");

        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.VerificationServices().SeedRandom = (Action<ushort>)(_ => { });
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Beetom, slot.Definition);
        enemies.BeetomBehavior.InitializeBeetom(slot, new SamusState(), 0);
        BeetomEnemyState state = enemies.BeetomStates[0]
            ?? throw new InvalidOperationException("The initialized Beetom has no state.");
        state.FacingRight = true;
        state.YSpeedTableIndex = 12;
        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(0, state.YSpeedTableIndex, "a kill empties the speed index");
        AssertEqual((ushort)0, state.ButtonCounter.RawValue, "a kill empties the escape counter");
        AssertTrue(state.FacingRight, "the slot's facing outlives the kill");
        Console.WriteLine("Beetom owned state: the apex underflow, the falling cap and the cleared record match.");
    }

    /// <summary>Runs the focused Beetom owned-state check.</summary>
    [VerificationCommand("--beetom-owned-state")]
    internal static int BeetomOwnedStateCommand()
    {
        VerifyBeetomOwnedState();
        return 0;
    }
}
