using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Beetom;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Beetom state the species owns: its arc speed index at the apex and the floor, and the cleared record.</summary>
internal static class BeetomOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step that lands exactly on zero keeps rising, one that would pass below zero restarts the
    /// index at zero and falls (the cartridge's signed underflow test), falling holds at index 64, and a kill empties
    /// the values kept in the common record while the facing survives it, and the slot's next Beetom inherits that facing.
    /// </summary>
    internal static void VerifyBeetomOwnedState()
    {
        var arc = new BeetomEnemyState(previous: null) { YSpeedTableIndex = 8 };
        arc.StepRisingSpeed(4);
        AssertEqual(new VerifyBeetomOwnedStateCase(4, false), new VerifyBeetomOwnedStateCase(arc.YSpeedTableIndex, arc.Falling), "a rising step above the apex keeps rising");
        arc.StepRisingSpeed(4);
        AssertEqual(new VerifyBeetomOwnedStateCase(0, false), new VerifyBeetomOwnedStateCase(arc.YSpeedTableIndex, arc.Falling), "a rising step landing on zero keeps rising");
        arc.StepRisingSpeed(5);
        AssertEqual(new VerifyBeetomOwnedStateCase(0, true), new VerifyBeetomOwnedStateCase(arc.YSpeedTableIndex, arc.Falling), "a rising step below zero restarts at zero and falls");
        arc.YSpeedTableIndex = 61;
        arc.StepFallingSpeed(3);
        AssertEqual(64, arc.YSpeedTableIndex, "falling reaches the maximum");
        arc.StepFallingSpeed(3);
        AssertEqual(64, arc.YSpeedTableIndex, "falling holds at the maximum");

        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Beetom, slot.Definition);
        enemies.BeetomBehavior.InitializeBeetom(slot, new SamusState(InstalledFixtures.AnimationContinuation), 0);
        BeetomEnemyState state = enemies.BeetomStates[0]
            ?? throw new InvalidOperationException("The initialized Beetom has no state.");
        state.FacingRight = true;
        state.YSpeedTableIndex = 12;
        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(0, state.YSpeedTableIndex, "a kill empties the speed index");
        AssertEqual((ushort)0, state.ButtonCounter.RawValue, "a kill empties the escape counter");
        AssertTrue(state.FacingRight, "the Beetom's facing outlives the kill");

        // The next Beetom set up in the slot inherits the facing its predecessor left; a Beetom in another slot does not.
        slot.Bind(EnemyDefinitionId.Beetom, slot.Definition);
        enemies.BeetomBehavior.InitializeBeetom(slot, new SamusState(InstalledFixtures.AnimationContinuation), 0);
        BeetomEnemyState successor = Ensure.NotNull(enemies.BeetomStates[0]);
        AssertTrue(!ReferenceEquals(state, successor), "re-initialization sets up a new Beetom");
        AssertTrue(successor.FacingRight, "a new Beetom inherits the facing the slot's previous Beetom left");
        RoomEnemySlot otherSlot = enemies.SlotTable.Slots[1];
        otherSlot.Bind(EnemyDefinitionId.Beetom, otherSlot.Definition);
        enemies.BeetomBehavior.InitializeBeetom(otherSlot, new SamusState(InstalledFixtures.AnimationContinuation), 0);
        BeetomEnemyState first = enemies.BeetomStates[1]
            ?? throw new InvalidOperationException("The other slot's Beetom has no state.");
        AssertTrue(!first.FacingRight, "a slot's first Beetom faces left");
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
