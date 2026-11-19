using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Samus.Death.OutOfHealthGameStateVerification;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Samus.Death;

/// <summary>
/// Checks that the update the AUTO reserves' refill completes on is main gameplay's own (#2526). The cartridge's refill game
/// state (1Bh, $82:DC10) writes main gameplay's game state (8) and restores Samus's normal handlers (command 10h, $82:DC24)
/// before that update's main gameplay runs, so a pause its pause check admits ($90:EA45, game state 8 only), a door its
/// movement touches ($94:93B8), the Ceres elevator its room main takes her onto ($89:AD17, game state 8 only) and the
/// gunship's takeoff its enemies complete ($A2:AD23) each take the game where it leads on the next update. The refill's own
/// updates, still in game state 1Bh with Samus locked and time frozen, admit no pause and take her onto no elevator.
/// </summary>
internal static class RefillCompletionTransitionVerification
{
    /// <summary>
    /// Start newly pressed on a refill update that leaves reserves keeps the refill; newly pressed on the update the refill
    /// completes on, it pauses.
    /// </summary>
    internal static void VerifyRefillCompletionUpdatePauses()
    {
        (SuperMetroidGame game, _, SamusState samus) = MainGameplayInTheScopeRoom();
        BeginRefill(game, samus, reserveUnits: 3);

        game.Step(ControllerButton.Start);
        AssertEqual(2, samus.ReserveEnergy, "the refill's first update moves one reserve unit");
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "Start on a refill update that leaves reserves keeps the refill");
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the refill's second update leaves a reserve unit");

        game.Step(ControllerButton.Start);
        AssertEqual(0, samus.ReserveEnergy, "the refill's third update moves the last reserve unit");
        AssertTrue(game.IsAt(PauseStep.DarkeningGameplay), $"Start on the refill's completion update, back in main gameplay, pauses; ended in {game.GameState}");

        Console.WriteLine("Start on the refill's completion update pauses; on its earlier updates it does not.");
    }

    /// <summary>
    /// A spark the refill held reaching a door on the update the refill completes on: the door transition takes the game on
    /// the next update.
    /// </summary>
    internal static void VerifyRefillCompletionUpdateEntersDoor()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplaySparkingTowardADoor();
        // The spark's second update stays short of the door, and the periodic damage Samus's movement applies after it
        // empties her into the refill, which holds her mid-spark.
        samus.MaxHealth = 999;
        samus.ReserveEnergy = 1;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        samus.Position = samus.Position.WithWholeX(64);
        samus.PeriodicDamage.Accumulate(new UnsignedSixteenSixteen(999, 0));
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "Samus's emptied energy enters the refill");
        AssertTrue(runtime.EnteredDoor is null, "the spark is still short of the door as the refill begins");

        // The refill's completion update gives Samus her control back before its gameplay, whose spark reaches the door.
        samus.Health = 200;
        PlaceJustShortOfTheDoor(samus);
        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the refill's update moves the last reserve unit");
        AssertTrue(runtime.EnteredDoor is not null, "the completion update's spark reaches the door");
        AssertTrue(game.IsAt(DoorTransitionStep.EnteringDoor), $"the door Samus touches on the refill's completion update takes the game; ended in {game.GameState}");

        Console.WriteLine("A door touched on the refill's completion update begins the door transition.");
    }

    /// <summary>
    /// Samus held by the refill on the Ceres elevator once its room's door is destroyed: the refill's own update does not
    /// take her onto it, and the update the refill completes on begins the Ceres departure.
    /// </summary>
    internal static void VerifyRefillCompletionUpdateTakesCeresElevator()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayOnTheDormantCeresElevator();
        BeginRefill(game, samus, reserveUnits: 2);
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;

        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the refill's update that leaves a reserve unit keeps the refill");

        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the refill's update moves the last reserve unit");
        AssertEqual(SuperMetroidGameState.CeresDeparture, game.GameState, "the elevator Samus stands on takes the refill's completion update to the Ceres departure");

        Console.WriteLine("The Ceres elevator takes the refill's completion update, and not the refill's own updates.");
    }

    /// <summary>
    /// The escaping gunship's top hull rising past the takeoff height on the update the refill completes on: the game leaves
    /// Zebes on the next update.
    /// </summary>
    internal static void VerifyRefillCompletionUpdateLeavesZebes()
    {
        // The camera shows the landed ship, whose pieces the enemies' update then runs.
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayAtTheLandingSite(new Position16<RoomPixels>(0x0400, 0x0400));
        BeginRefill(game, samus, reserveUnits: 1);

        // The ship carries Samus, the top hull above her: past the takeoff height, the completion update's unfrozen ascent
        // completes the takeoff.
        RoomEnemyPlace top = runtime.Enemies.Places.InUpdateOrder.Single(place => place.SpeciesState<GunshipState>() is not null);
        GunshipState.Of(top).Phase = GunshipPhase.AcceleratingLiftoff;
        samus.Position = samus.Position.WithWholeY(GunshipTakeoffDefinitions.TakeoffCompleteY + GunshipMotionDefinitions.TopHullAboveSamus - 8);
        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the refill's update moves the last reserve unit");
        AssertTrue(runtime.EnemyFrameResults.LastEnemyFrame.Reports(GunshipFrameEvent.EscapeTakeoffCompleted),
            $"the ship's ascent completes the takeoff; the top hull is at {top.Position.Y.Whole} in {GunshipState.Of(top).Phase}");
        AssertTrue(game.IsAt(EndingStep.LeavingZebes), $"the gunship's takeoff on the refill's completion update leaves Zebes; ended in {game.GameState}");

        Console.WriteLine("The gunship's takeoff on the refill's completion update leaves Zebes.");
    }

    /// <summary>
    /// Empties Samus's energy with <paramref name="reserveUnits"/> in her AUTO reserves, as damage would, and runs the
    /// update whose out-of-health check enters the refill; each later refill update moves one unit.
    /// </summary>
    private static void BeginRefill(SuperMetroidGame game, SamusState samus, int reserveUnits)
    {
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = reserveUnits;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "zero health with AUTO reserves enters the refill");
    }
}
