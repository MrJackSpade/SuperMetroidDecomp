using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Puyo state the species owns: the airborne index's two sign-bit restarts.</summary>
internal static class PuyoOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step landing on zero keeps rising, one that passes below zero restarts the fall at zero (the
    /// apex), a falling step that reaches $8000 restarts the fall at zero as the cartridge's sign test does, an
    /// ordinary falling step accumulates, and a fresh Puyo has no airborne behaviour.
    /// </summary>
    internal static void VerifyPuyoOwnedState()
    {
        static EnemyQuadraticSpeedIndex Index(int twoHundredFiftySixths) => EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(twoHundredFiftySixths);
        var hop = new PuyoEnemyState { YSpeedCurvePlace = Index(0x0200) };
        hop.StepAirborneIndex(Index(0x0200));
        AssertEqual(new VerifyPuyoOwnedStateCase(0, false), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a rising step landing on zero keeps rising");
        hop.StepAirborneIndex(Index(0x0200));
        AssertEqual(new VerifyPuyoOwnedStateCase(0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a rising step below zero is the apex");
        hop.StepAirborneIndex(Index(0x01c0));
        AssertEqual(new VerifyPuyoOwnedStateCase(0x01c0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a falling step accumulates");
        hop.YSpeedCurvePlace = Index(0x7f00);
        hop.StepAirborneIndex(Index(0x0100));
        AssertEqual(new VerifyPuyoOwnedStateCase(0, true), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a falling step reaching the sign bit restarts the fall");
        hop.YSpeedCurvePlace = Index(0x7e00);
        hop.StepAirborneIndex(Index(0x0100));
        AssertEqual(new VerifyPuyoOwnedStateCase(0x7f00, true), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a falling step below the sign bit keeps falling");
        hop.YSpeedCurvePlace = Index(0x7f00);
        hop.StepAirborneIndex(Index(0x00ff));
        AssertEqual(new VerifyPuyoOwnedStateCase(0x7fff, true), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfRow, hop.Falling), "a falling step to the last place before the sign bit keeps falling");

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Puyo, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Puyo, 5);
        PuyoBehavior.InitializePuyo(slot);
        PuyoEnemyState state = enemies.PuyoStates[0]
            ?? throw new InvalidOperationException("The initialized Puyo has no state.");
        AssertEqual(null, state.AirborneMovement, "a fresh Puyo has no airborne behaviour");
        Console.WriteLine("Puyo owned state: the apex and the overlong-fall restart match.");
    }

    /// <summary>Runs the focused Puyo owned-state check.</summary>
    [VerificationCommand("--puyo-owned-state")]
    internal static int PuyoOwnedStateCommand()
    {
        VerifyPuyoOwnedState();
        return 0;
    }
}
