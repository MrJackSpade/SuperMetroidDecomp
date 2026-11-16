using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Beetom;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Beetom state the species owns: its arc speed index at the apex and the floor, and its inherited facing.</summary>
internal static class BeetomOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step that lands exactly on zero keeps rising, one that would pass below zero restarts the
    /// index at zero and falls (the cartridge's signed underflow test), falling holds at index 64, and the slot's next
    /// Beetom, set up after a kill took the first with its state, inherits the facing the first left.
    /// </summary>
    internal static void VerifyBeetomOwnedState()
    {
        var arc = new BeetomEnemyState(previous: null, BeetomAction.DecideAction) { YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(8) };
        arc.StepRisingSpeed(4);
        AssertEqual(new VerifyBeetomOwnedStateCase(QuadraticSpeedCurvePlace.FromMarks(4), false), new VerifyBeetomOwnedStateCase(arc.YSpeedCurvePlace, arc.Falling), "a rising step above the apex keeps rising");
        arc.StepRisingSpeed(4);
        AssertEqual(new VerifyBeetomOwnedStateCase(QuadraticSpeedCurvePlace.Start, false), new VerifyBeetomOwnedStateCase(arc.YSpeedCurvePlace, arc.Falling), "a rising step landing on zero keeps rising");
        arc.StepRisingSpeed(5);
        AssertEqual(new VerifyBeetomOwnedStateCase(QuadraticSpeedCurvePlace.Start, true), new VerifyBeetomOwnedStateCase(arc.YSpeedCurvePlace, arc.Falling), "a rising step below zero restarts at zero and falls");
        arc.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(61);
        arc.StepFallingSpeed(3);
        AssertEqual(QuadraticSpeedCurvePlace.FromMarks(64), arc.YSpeedCurvePlace, "falling reaches the maximum");
        arc.StepFallingSpeed(3);
        AssertEqual(QuadraticSpeedCurvePlace.FromMarks(64), arc.YSpeedCurvePlace, "falling holds at the maximum");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Beetom, place.Definition);
        enemies.BeetomBehavior.InitializeBeetom(place, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()), 0);
        BeetomEnemyState state = enemies.BeetomStates[0]
            ?? throw new InvalidOperationException("The initialized Beetom has no state.");
        state.FacingRight = true;
        place.Clear();
        AssertEqual<BeetomEnemyState?>(null, enemies.BeetomStates[0], "a kill takes the Beetom's state with it");

        // The next Beetom set up in the slot inherits the facing its predecessor left; a Beetom in another slot does not.
        place.Bind(EnemyDefinitionId.Beetom, place.Definition);
        enemies.BeetomBehavior.InitializeBeetom(place, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()), 0);
        BeetomEnemyState successor = Ensure.NotNull(enemies.BeetomStates[0]);
        AssertTrue(!ReferenceEquals(state, successor), "re-initialization sets up a new Beetom");
        AssertTrue(successor.FacingRight, "a new Beetom inherits the facing the slot's previous Beetom left");
        RoomEnemyPlace otherPlace = enemies.Places.InUpdateOrder[1];
        otherPlace.Bind(EnemyDefinitionId.Beetom, otherPlace.Definition);
        enemies.BeetomBehavior.InitializeBeetom(otherPlace, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()), 0);
        BeetomEnemyState first = enemies.BeetomStates[1]
            ?? throw new InvalidOperationException("The other slot's Beetom has no state.");
        AssertTrue(!first.FacingRight, "a slot's first Beetom faces left");
        Console.WriteLine("Beetom owned state: the apex underflow, the falling cap and the inherited facing match.");
    }

    /// <summary>Runs the focused Beetom owned-state check.</summary>
    [VerificationCommand("--beetom-owned-state")]
    internal static int BeetomOwnedStateCommand()
    {
        VerifyBeetomOwnedState();
        return 0;
    }
}
