using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Samus.Death.OutOfHealthGameStateVerification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Checks that the Ceres escape's time-up keeps Samus off the elevator (#2534). The Ceres elevator shaft's room main takes
/// Samus onto the elevator only while the game state reads main gameplay (8, $89:ACFF): the run-out countdown writes the
/// time-up's game state (23h, $90:E0EF) in Samus's update, before room main, and the time-up's white-out runs main
/// gameplay's update behind its fade in game state 23h ($82:8414). Neither the update the countdown runs out on nor the
/// white-out locks Samus onto the elevator or leaves Ceres.
/// </summary>
internal static class CeresElevatorTimeUpVerification
{
    /// <summary>
    /// The countdown running out on an update Samus stands on the released elevator: the time-up takes the update, and the
    /// elevator neither locks her nor spends its departure.
    /// </summary>
    internal static void VerifyTimeUpUpdateKeepsSamusOffTheCeresElevator()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayOnTheDormantCeresElevator();
        runtime.EscapeTimer.RunIntoPlace();
        runtime.EscapeTimer.SetRemaining(1);
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;

        game.Step(0);
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the countdown runs out on the update Samus stands on the released elevator");
        AssertTrue(game.IsAt(TimeUpStep.WhiteningOut), $"the time-up takes the update; ended in {game.GameState}");
        AssertTrue(!samus.Control.InputLocked, "the elevator does not lock Samus on the update the time-up began on");

        Console.WriteLine("The update the Ceres countdown runs out on keeps Samus off the elevator she stands on.");
    }

    /// <summary>
    /// Samus standing on the elevator once its room's door is destroyed during the time-up's white-out: none of the
    /// white-out's updates takes her onto it, and the white-out fades on to black.
    /// </summary>
    internal static void VerifyTimeUpWhiteOutKeepsSamusOffTheCeresElevator()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = MainGameplayOnTheDormantCeresElevator();
        runtime.EscapeTimer.RunIntoPlace();
        runtime.EscapeTimer.SetRemaining(1);

        game.Step(0);
        AssertTrue(game.IsAt(TimeUpStep.WhiteningOut), $"the run-out countdown begins the time-up; ended in {game.GameState}");
        runtime.EnemySession.CeresStatus = CeresEscapeStatus.ElevatorDoorDestroyed;

        int updates = 0;
        while (game.IsAt(TimeUpStep.WhiteningOut))
        {
            game.Step(0);
            AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the white-out's update does not take Samus onto the elevator");
            AssertTrue(!samus.Control.InputLocked, "the white-out's update does not lock Samus on the elevator");
            AssertTrue(++updates <= TimeUpDefinitions.WhiteOutLastStep + 2, "the white-out ends");
        }
        AssertTrue(game.IsAt(TimeUpStep.FadingToBlack), $"the white-out fades on to black; ended in {game.GameState}");
        AssertTrue(!samus.Control.InputLocked, "no white-out update takes Samus onto the elevator, which would lock her");

        Console.WriteLine("The time-up's white-out keeps Samus off the Ceres elevator she stands on.");
    }
}
