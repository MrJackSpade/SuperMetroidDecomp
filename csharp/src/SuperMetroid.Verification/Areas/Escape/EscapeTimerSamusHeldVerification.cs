using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Checks that only Samus's normal movement mode runs the escape timer (#2504), as only the cartridge's normal Samus
/// handler runs the timer's handler, which counts and draws it. The reserve refill's lock, the death's lock and the Ceres
/// elevator's lock install handlers that never run it: the refill's locked updates hold the countdown and hide the timer
/// until the update the refill completes on, which restores Samus's control before its gameplay; the death's gameplay
/// passes hide it; and the Ceres elevator's hold and blackout, whose game states draw the timer themselves, show it
/// without counting it.
/// </summary>
internal static class EscapeTimerSamusHeldVerification
{
    /// <summary>
    /// An escape whose Samus runs out of health with three reserve units on AUTO: the refill's two locked updates hold the
    /// countdown and hide the timer, even at its last centisecond, and the completion update counts it down to the time-up.
    /// </summary>
    internal static void VerifyEscapeTimerHoldsThroughReserveRefill()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 30, 0));
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 3;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        int beforeEntry = runtime.EscapeTimer.RemainingCentiseconds;

        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "zero health with AUTO reserves enters the refill");
        int atEntry = runtime.EscapeTimer.RemainingCentiseconds;
        AssertTrue(atEntry < beforeEntry, "the update Samus runs out of health on counts down under her normal movement mode");
        AssertTrue(TimerDrawn(runtime), "the update Samus runs out of health on draws the timer");

        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the first refill update leaves reserves to transfer");
        AssertEqual(atEntry, runtime.EscapeTimer.RemainingCentiseconds, "a refill update whose Samus is locked holds the countdown");
        AssertTrue(!TimerDrawn(runtime), "a refill update whose Samus is locked does not draw the timer");

        runtime.EscapeTimer.SetRemaining(1);
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "a countdown at its last centisecond does not run out during the refill");
        AssertEqual(EscapeTimerPhase.InPlace, runtime.EscapeTimer.Phase, "the held countdown keeps counting in its corner");
        AssertEqual(1, runtime.EscapeTimer.RemainingCentiseconds, "the held countdown keeps its last centisecond");
        AssertTrue(!TimerDrawn(runtime), "the held timer stays hidden");

        game.Step(0);
        AssertEqual(0, samus.ReserveEnergy, "the third refill update transfers the last reserve unit");
        AssertEqual(SamusMovementMode.Normal, samus.Control.Mode.Movement, "the completing refill restores Samus's normal movement mode");
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the completion update, run with Samus's control restored, counts the countdown out");
        AssertTrue(TimerDrawn(runtime), "the completion update draws the timer");
        AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the time-up begins after the completion update");

        Console.WriteLine("Escape timer through a reserve refill: the locked refill updates hold and hide it, and the completion update counts it out.");
    }

    /// <summary>
    /// The Ceres elevator's hold and the start of its blackout, which run gameplay while the elevator locks Samus out:
    /// every update shows the timer and none counts it.
    /// </summary>
    internal static void VerifyEscapeTimerShownUncountedOnCeresElevator()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 20, 0));
        samus.Control.LockOutPlayer();
        game.Session.ceresDeparture.Begin();
        game.Session.Enter(SuperMetroidGameState.CeresDeparture);
        int held = runtime.EscapeTimer.RemainingCentiseconds;

        int holdUpdates = game.Session.ceresDeparture.HoldFramesRemaining;
        for (int update = 0; update < holdUpdates + 2; update++)
        {
            game.Step(0);
            AssertEqual(held, runtime.EscapeTimer.RemainingCentiseconds, "the Ceres departure's updates hold the countdown");
            AssertTrue(TimerDrawn(runtime), "the Ceres departure's updates show the timer");
        }
        AssertEqual(CeresDeparturePhase.FadingToBlack, game.Session.ceresDeparture.Phase, "the checked updates reach the blackout");

        Console.WriteLine("Escape timer on the Ceres elevator: the hold and the blackout show it and never count it.");
    }

    /// <summary>The death's two gameplay passes, run with Samus locked out: neither counts nor draws the timer.</summary>
    internal static void VerifyEscapeTimerHiddenThroughDeath()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 20, 0));
        samus.Health = 0;
        samus.ReserveEnergy = 0;
        int beforeDeath = runtime.EscapeTimer.RemainingCentiseconds;

        game.Step(0);
        AssertEqual(DeathSequenceStep.FinalGameplayPass, game.Session.DeathStep, "zero health without reserves starts the death");
        int atDeath = runtime.EscapeTimer.RemainingCentiseconds;
        AssertTrue(atDeath < beforeDeath, "the update Samus dies on counts down under her normal movement mode");
        AssertTrue(TimerDrawn(runtime), "the update Samus dies on draws the timer");

        game.Step(0);
        AssertEqual(DeathSequenceStep.BlackingOutSurroundings, game.Session.DeathStep, "the final gameplay pass begins the surroundings' black-out");
        AssertEqual(atDeath, runtime.EscapeTimer.RemainingCentiseconds, "the final gameplay pass holds the countdown");
        AssertTrue(!TimerDrawn(runtime), "the final gameplay pass does not draw the timer");

        game.Step(0);
        AssertEqual(atDeath, runtime.EscapeTimer.RemainingCentiseconds, "the black-out's gameplay holds the countdown");
        AssertTrue(!TimerDrawn(runtime), "the black-out's gameplay does not draw the timer");

        Console.WriteLine("Escape timer through a death: the death's gameplay passes neither count nor draw it.");
    }

    /// <summary>
    /// A game in main gameplay in a Brinstar room, its Samus standing under her normal movement mode and its escape timer
    /// counting down in its corner from <paramref name="centiseconds"/>.
    /// </summary>
    internal static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) GameplayWithEscapeTimer(int centiseconds)
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = CreateMainGameplayFixture();
        runtime.EscapeTimer.RunIntoPlace();
        runtime.EscapeTimer.SetRemaining(centiseconds);
        AssertEqual(SamusMovementMode.Normal, samus.Control.Mode.Movement, "the fixture's Samus runs under her normal movement mode");
        return (game, runtime, samus);
    }

    /// <summary>Whether the runtime's last update drew the escape timer.</summary>
    private static bool TimerDrawn(SuperMetroidRuntime runtime) =>
        runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
}
