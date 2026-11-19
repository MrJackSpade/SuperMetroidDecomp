using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Areas.Escape.EscapeTimerSamusHeldVerification;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Checks the order of the escape countdown's expiry and Samus running out of health on the same update (#2509). The
/// cartridge's countdown writes the time-up's game state from Samus's update; the out-of-health check later in that update
/// enters the AUTO reserves' refill whatever the game state, but begins Samus's death only while the game is still in
/// main gameplay, and otherwise returns before counting the play time. Without reserves the time-up therefore wins: its
/// white-out runs with Samus out of health, neither killing her nor counting the play time, and fades on to black.
/// </summary>
internal static class EscapeTimeUpOutOfHealthVerification
{
    /// <summary>
    /// An escape whose countdown runs out on the update Samus's energy reaches zero without reserves: the time-up begins,
    /// not the death, and neither the expiring update nor the white-out's updates count the play time or kill her.
    /// </summary>
    internal static void VerifyTimeUpWinsOverDeathWithoutReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 0, 1));
        samus.Health = 0;
        samus.ReserveEnergy = 0;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        GameClockReading beforeExpiry = runtime.GameTime.Reading;

        game.Step(0);
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the update Samus runs out of health on counts the countdown out");
        AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the countdown's time-up takes the update, not Samus's death");
        AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the time-up begins with its white-out");
        AssertTrue(!runtime.Freeze.GameplayTimeFrozen, "the withheld death does not freeze gameplay");
        AssertTrue(!samus.Control.InputLocked, "the withheld death does not lock Samus out");
        AssertEqual(beforeExpiry, runtime.GameTime.Reading, "the out-of-health check returns before counting the play time");

        // From a shared fade step count at zero the white-out steps once at each count through its last step, and
        // completes on the update after.
        AssertTrue(!runtime.FadeStepCount.IsPastStart, "no other fade has advanced the shared fade step count");
        for (int update = 0; update < TimeUpDefinitions.WhiteOutParts + 2; update++)
        {
            AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the white-out runs until its fade completes");
            game.Step(0);
            AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the white-out's gameplay does not kill Samus out of health");
            AssertEqual(0, samus.Health, "Samus stays out of health behind the white-out");
            AssertEqual(beforeExpiry, runtime.GameTime.Reading, "the white-out's gameplay with Samus out of health does not count the play time");
        }
        AssertEqual(TimeUpStep.FadingToBlack, game.Session.TimeUpStep, "the white-out fades on to black");

        Console.WriteLine("Escape time-up out of health without reserves: the time-up wins, and its white-out neither kills Samus nor counts the play time.");
    }

    /// <summary>
    /// An escape whose countdown runs out on the update Samus's energy reaches zero with AUTO reserves: the refill begins
    /// over the time-up, and the update counts the play time.
    /// </summary>
    internal static void VerifyReserveRefillWinsOverTimeUp()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 0, 1));
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 3;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        GameClockReading beforeExpiry = runtime.GameTime.Reading;

        game.Step(0);
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the update Samus runs out of health on counts the countdown out");
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the AUTO reserves' refill takes the update over the time-up");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the refill freezes gameplay");
        AssertTrue(!runtime.GameTime.Reading.Equals(beforeExpiry), "the refill's update counts the play time");

        Console.WriteLine("Escape time-up out of health with AUTO reserves: the refill wins and the update counts the play time.");
    }
}
