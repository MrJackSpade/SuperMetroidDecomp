using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Frontend;

/// <summary>The cinematic phases: the intro, Ceres's destruction, the time-up white-out and the ending.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class CinematicGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.IntroCinematic"/>.</summary>
    internal void DispatchIntroCinematic(SnesButton controllerInput)
    {
        session.ActiveIntro.Step(controllerInput);
        session.display.PublishIntro(session.ActiveIntro);
        if (session.ActiveIntro.CeresFlightFinished)
        {
            // `$8B:C100` writes state $1F, area six, and load-station zero only
            // after the SPACE COLONY fade has reached forced blank. Preserve the
            // intervening dispatcher state instead of constructing a debug room
            // directly from cinematic code.
            session.GameState = SuperMetroidGameState.SetUpNewGame;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.WhitingOutFromTimeUp"/>.</summary>
    internal void DispatchWhitingOutFromTimeUp(SnesButton controllerInput)
    {
        // `$82:8431` runs no gameplay: it fades the whitened frame to forced blank.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (session.StepScreenFadeOut() == 0)
            FinishTimeUpBlackOut();
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoomWithSamus"/>.</summary>
    internal void DispatchCeresGoesBoomWithSamus(SnesButton controllerInput)
    {
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState fatalDestruction = session.ActiveCeresDestruction;
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // `$8B:C64E`: the fatal cinematic's fade-out ends at state $19, already
            // in forced blank, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = session.Runtime?.Player.Samus ?? throw new InvalidOperationException(
                "Ceres destruction with Samus requires her gameplay state.");
            lostSamus.SelectedHudItem = HudItem.Beam;
            lostSamus.AutoCancelHudItemIndex = HudItem.Beam;
            lostSamus.InvincibilityTimer = NativeCountdown.Start(0);
            lostSamus.KnockbackTimer = NativeCountdown.Start(0);
            session.Close<CeresDestructionCinematicState>();
            BeginForcedBlankDeathBlackOut();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoom"/>.</summary>
    internal void DispatchCeresGoesBoom(SnesButton controllerInput)
    {
        // Each update begins with an accepted NMI. A gameplay owner kept from Ceres
        // counts $05B5/$05B6 and latches input; without one the frontend counted it.
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState destruction = session.ScreenAs<CeresDestructionCinematicState>() ??
            session.Show(new CeresDestructionCinematicState(
                session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
                session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx));
        destruction.Step();
        session.display.PublishCeresDestruction(destruction);
        if (destruction.Finished)
        {
            // CADF forces blank and publishes state six. The loader's special
            // `$22` branch—not cinematic code—selects area zero/station eighteen.
            session.RouteLoading(SaveLoadingGameState.CeresDestruction);
            session.GameState = SuperMetroidGameState.LoadingGameData;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.EndingAndCredits"/>.</summary>
    internal void DispatchEndingAndCredits()
    {
        EndingCreditsPhase previousEndingPhase = session.ActiveEndingCredits.Phase;
        session.ActiveEndingCredits.Step();
        if (previousEndingPhase != EndingCreditsPhase.SeeYouNextMission &&
            session.ActiveEndingCredits.Phase == EndingCreditsPhase.SeeYouNextMission)
        {
            session.saves.SetGameCompleted(true);
            session.RaiseSaveRamChanged();
        }
        session.display.PublishEnding(session.ActiveEndingCredits);
    }

    /// <summary>
    /// <c>$82:8444-$84BC</c>, once the time-up fade reaches forced blank: stop the escape
    /// timer and Ceres effects, silence all three sound libraries, then lose Samus with Ceres
    /// or, once the Zebes time bomb is set, cut straight to the death black-out.
    /// </summary>
    private void FinishTimeUpBlackOut()
    {
        if (session.Runtime is null)
            throw new InvalidOperationException("Time-up black-out requires a gameplay runtime.");
        session.screenFade.Clear();
        session.EndTimeUpPaletteFade();
        session.Runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        session.Runtime.EscapeTimer.ClearStatus();
        session.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
        session.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
        session.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
        session.display.PublishBlack();
        if (session.Runtime.System.HasEvent(EventNumber.ZebesTimebombSet))
        {
            BeginForcedBlankDeathBlackOut();
            return;
        }
        session.Show(new CeresDestructionCinematicState(
            session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
            session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx, outcome: CeresDestructionOutcome.SamusAboard));
        session.GameState = SuperMetroidGameState.CeresGoesBoomWithSamus;
    }

    /// <summary>
    /// Enters state $19 already at forced blank, as both time-up outcomes do. Its
    /// <c>HandleFadingOut</c> finds no brightness left, so that dispatch opens game over.
    /// </summary>
    private void BeginForcedBlankDeathBlackOut()
    {
        session.DeathFade.Start(0, 0);
        session.GameState = SuperMetroidGameState.DeathFinalBlackOut;
        session.display.PublishBlack();
    }
}
