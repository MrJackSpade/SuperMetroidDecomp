using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Checks that the escape countdown's expiry is the outcome of the update it happens on (#2499): the runtime frame reports
/// the time-up once, on the expiring update, while the expired timer stays drawn at zero; the time-up's stop hides it and
/// keeps its countdown and place; and the hold-at-one-second assist holds it instead of expiring.
/// </summary>
internal static class EscapeTimeUpOutcomeVerification
{
    /// <summary>Runs a Ceres timer in its corner down from one centisecond through gameplay frames.</summary>
    internal static void VerifyEscapeTimeUpOutcome()
    {
        SuperMetroidRuntime runtime = CeresRuntimeWithTimerAt(EscapeTimerClock.Time(0, 0, 1), preventEscapeTimeout: false);
        FixedVector<ScreenPixels> corner = runtime.EscapeTimer.Position;

        AssertTrue(runtime.StepFrame(0, new RuntimeFrameRequest()).StartsTimeUp,
            "the update whose countdown step reaches zero reports the time-up");
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the countdown that ran out is expired");
        AssertTrue(runtime.EscapeTimer.IsActive, "the expired timer is still running for its drawing");
        AssertEqual(0, runtime.EscapeTimer.RemainingCentiseconds, "the expired countdown shows zero");
        AssertTrue(TimerDrawn(runtime), "the expiring update draws the timer");

        AssertTrue(!runtime.StepFrame(0, new RuntimeFrameRequest()).StartsTimeUp,
            "the update after the expiry reports no second time-up");
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the expired timer stays expired");
        AssertEqual(0, runtime.EscapeTimer.RemainingCentiseconds, "the expired countdown never counts again");
        AssertTrue(TimerDrawn(runtime), "the expired timer is still drawn");

        runtime.EscapeTimer.StopAtTimeUp();
        AssertEqual(EscapeTimerPhase.StoppedAtTimeUp, runtime.EscapeTimer.Phase, "the time-up's black-out stops the timer");
        AssertTrue(!runtime.EscapeTimer.IsActive, "the stopped timer is no longer running");
        AssertEqual(0, runtime.EscapeTimer.RemainingCentiseconds, "the stop keeps the countdown");
        AssertEqual(corner, runtime.EscapeTimer.Position, "the stop keeps the timer's place");
        AssertTrue(!runtime.StepFrame(0, new RuntimeFrameRequest()).StartsTimeUp,
            "a stopped timer reports no time-up");
        AssertTrue(!TimerDrawn(runtime), "the stopped timer is not drawn");

        SuperMetroidRuntime held = CeresRuntimeWithTimerAt(EscapeTimerClock.Time(0, 0, 1), preventEscapeTimeout: true);
        AssertTrue(!held.StepFrame(0, new RuntimeFrameRequest()).StartsTimeUp,
            "the hold-at-one-second assist reports no time-up");
        AssertEqual(EscapeTimerPhase.InPlace, held.EscapeTimer.Phase, "the held countdown keeps counting in its corner");
        AssertEqual(EscapeTimerClock.Time(0, 1, 0), held.EscapeTimer.RemainingCentiseconds, "the assist holds the countdown at one second");

        Console.WriteLine("Escape time-up outcome: the expiring update alone reports the time-up, the expired timer is drawn until the stop, and the assist holds it.");
    }

    /// <summary>A runtime in the Ceres start room whose timer counts down in its corner from <paramref name="centiseconds"/>.</summary>
    private static SuperMetroidRuntime CeresRuntimeWithTimerAt(int centiseconds, bool preventEscapeTimeout)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(preventEscapeTimeout: preventEscapeTimeout);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.EscapeTimer.RunIntoPlace();
        runtime.EscapeTimer.SetRemaining(centiseconds);
        return runtime;
    }

    /// <summary>Whether the runtime's last update drew the escape timer.</summary>
    private static bool TimerDrawn(SuperMetroidRuntime runtime) =>
        runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
}
