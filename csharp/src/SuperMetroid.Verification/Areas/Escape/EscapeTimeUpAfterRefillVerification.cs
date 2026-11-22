using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Areas.Escape.EscapeTimerSamusHeldVerification;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Checks that the time-up an AUTO reserve refill takes an update from comes back (#2518). Once the Zebes time bomb is set,
/// the cartridge's Samus draw ($90:F576, run by every main-gameplay update) writes the time-up's game state again whenever
/// the countdown has run out and the game is not in the time-up; the out-of-health check after it then finds the game out
/// of main gameplay. The refill's first update therefore returns to the time-up, leaving the refill unfinished, Samus locked
/// and gameplay frozen, and a white-out the refill interrupted goes on from where it stood. Before the time bomb is set (the
/// Ceres escape) only Samus's Ceres handler writes it again, and the refill's end replaces that handler with the normal one,
/// so the game stays in gameplay with the expired timer at zero.
/// </summary>
internal static class EscapeTimeUpAfterRefillVerification
{
    /// <summary>
    /// A Zebes escape whose countdown runs out on the update Samus's energy reaches zero with AUTO reserves: the refill takes
    /// that update, its first update returns to the time-up, and the white-out then runs to black with the refill left
    /// unfinished.
    /// </summary>
    internal static void VerifyZebesTimeUpReturnsAfterRefillTookExpiringUpdate()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = ZebesEscapeOutOfHealthWithReserves();

        game.Step(0);
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the update Samus runs out of health on counts the countdown out");
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the AUTO reserves' refill takes the expiring update over the time-up");
        SamusMovementMode refillMode = samus.Control.Mode.Movement;
        GameClockReading beforeReturn = runtime.GameTime.Reading;

        game.Step(0);
        AssertEqual(1, samus.Health, "the refill's update transfers one reserve unit before its gameplay");
        AssertEqual(2, samus.ReserveEnergy, "the refill's update leaves two reserve units");
        AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the refill's first update returns to the time-up the expired countdown started");
        AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the returned time-up begins with its white-out");
        AssertTrue(runtime.Freeze.GameplayTimeFrozen, "the unfinished refill leaves gameplay frozen");
        AssertEqual(refillMode, samus.Control.Mode.Movement, "the unfinished refill leaves Samus locked");
        AssertTrue(!TimerDrawn(runtime), "the locked Samus's update does not draw the timer");
        AssertTrue(!runtime.GameTime.Reading.Equals(beforeReturn), "the update Samus has health on counts the play time");

        AssertTrue(!runtime.FadeStepCount.IsPastStart, "no other fade has advanced the shared fade step count");
        for (int update = 0; update < TimeUpDefinitions.WhiteOutParts + 2; update++)
        {
            AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the white-out runs until its fade completes");
            game.Step(0);
            AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the white-out keeps the time-up");
            AssertEqual(2, samus.ReserveEnergy, "the refill the time-up took back transfers no more reserves");
            AssertTrue(!TimerDrawn(runtime), "the white-out with Samus locked does not draw the timer");
        }
        AssertEqual(TimeUpStep.FadingToBlack, game.Session.TimeUpStep, "the white-out fades on to black");

        Console.WriteLine("Zebes time-up after a refill took the expiring update: the refill's first update returns to the time-up.");
    }

    /// <summary>
    /// A Zebes white-out whose gameplay sends Samus out of health to the AUTO reserves' refill: the refill's first update
    /// returns to the time-up, and its white-out goes on from the colours the refill left it showing.
    /// </summary>
    internal static void VerifyZebesTimeUpResumesWhiteOutAfterRefill()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = ZebesEscapeOutOfHealthWithReserves();
        samus.Health = 50;

        game.Step(0);
        AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the countdown running out with Samus's health left starts the time-up");
        for (int update = 0; update < 3; update++)
            game.Step(0);
        AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the white-out is under way");

        samus.Health = 0;
        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the white-out's out-of-health check enters the AUTO reserves' refill");
        GameplayDisplayFade? interrupted = WhiteOutShown(runtime);
        AssertTrue(interrupted is not null, "the interrupted white-out still shows its fade");

        game.Step(0);
        AssertEqual(SuperMetroidGameState.TimeUp, game.GameState, "the refill's first update returns to the time-up");
        AssertEqual(TimeUpStep.WhiteningOut, game.Session.TimeUpStep, "the returned time-up goes on whitening out");
        AssertEqual(interrupted, WhiteOutShown(runtime), "the returned white-out shows the colours the refill left it at");

        game.Step(0);
        AssertTrue(!Equals(interrupted, WhiteOutShown(runtime)), "the returned white-out's next update moves the fade on");

        Console.WriteLine("Zebes time-up after a refill interrupted the white-out: the white-out goes on where it stood.");
    }

    /// <summary>
    /// The Ceres escape, before the Zebes time bomb is set: after the refill takes the expiring update, its completion
    /// returns to main gameplay, which stays there with the expired timer drawn at zero.
    /// </summary>
    internal static void VerifyCeresStaysInGameplayAfterRefillTookExpiringUpdate()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 0, 1));
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 3;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;

        game.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTankRefill, game.GameState, "the AUTO reserves' refill takes the expiring update over the time-up");
        for (int update = 0; update < 3; update++)
        {
            game.Step(0);
            AssertTrue(game.GameState != SuperMetroidGameState.TimeUp, "the refill's updates do not return to the time-up before the time bomb is set");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the refill's completion returns to main gameplay");

        game.Step(0);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "main gameplay stays after the refill with the countdown run out");
        AssertEqual(EscapeTimerPhase.Expired, runtime.EscapeTimer.Phase, "the countdown stays run out");
        AssertTrue(TimerDrawn(runtime), "Samus's normal movement mode draws the expired timer");

        Console.WriteLine("Ceres time-up after a refill took the expiring update: gameplay goes on with the timer at zero.");
    }

    /// <summary>
    /// A Zebes escape, its time bomb set, whose countdown runs out on the next update, its Samus out of health with three
    /// reserve units on AUTO.
    /// </summary>
    private static (SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus) ZebesEscapeOutOfHealthWithReserves()
    {
        (SuperMetroidGame game, SuperMetroidRuntime runtime, SamusState samus) = GameplayWithEscapeTimer(EscapeTimerClock.Time(0, 0, 1));
        runtime.System.SetEvent(StoryEvent.ZebesTimebombSet);
        samus.Health = 0;
        samus.MaxHealth = 99;
        samus.ReserveEnergy = 3;
        samus.MaxReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        return (game, runtime, samus);
    }

    /// <summary>The fade of the whole display the time-up's white-out shows now, or null while none runs.</summary>
    private static GameplayDisplayFade? WhiteOutShown(SuperMetroidRuntime runtime)
    {
        var frame = new GameplayShadingBuilder();
        runtime.FadeOut.ShadeGameplay(frame);
        return frame.Build(runtime.Room.LoadedScope.Assets.BlockSheet.Sheet.Image, runtime.Room.LoadedScope.Assets.BlockSheet.Backdrop).Fade;
    }

    /// <summary>Whether the runtime's last update drew the escape timer.</summary>
    private static bool TimerDrawn(SuperMetroidRuntime runtime) =>
        runtime.Sprites.Draws.Any(draw => draw.Source.Kind == SpriteSourceKind.EscapeTimer);
}
