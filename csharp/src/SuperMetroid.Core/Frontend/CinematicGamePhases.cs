using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>The cinematic phases: the intro, Ceres's destruction, the time-up white-out and the ending.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class CinematicGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.IntroCinematic"/>.</summary>
    internal void DispatchIntroCinematic(ControllerButton controllerInput)
    {
        session.ActiveIntro.Step(controllerInput);
        session.display.PublishIntro(session.ActiveIntro);
        if (session.ActiveIntro.CeresFlightFinished)
        {
            // The intro's end writes the Ceres game state, area six, and load-station zero only
            // after the SPACE COLONY fade has gone fully black. Preserve the
            // intervening dispatcher state instead of constructing a debug room
            // directly from cinematic code.
            session.GameState = SuperMetroidGameState.SetUpNewGame;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.WhitingOutFromTimeUp"/>.</summary>
    internal void DispatchWhitingOutFromTimeUp(ControllerButton controllerInput)
    {
        // The time-up black-out runs no gameplay: it fades the whitened frame to black.
        session.GameplayRuntime.FrameBoundary.BeginAcceptedFrame(controllerInput);
        if (session.StepScreenFadeOut() == 0)
            FinishTimeUpBlackOut();
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoomWithSamus"/>.</summary>
    internal void DispatchCeresGoesBoomWithSamus(ControllerButton controllerInput)
    {
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState fatalDestruction = session.ActiveCeresDestruction;
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // The fatal cinematic's fade-out ends at its game state, already
            // fully black, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = session.Runtime?.Player.Samus ?? throw new InvalidOperationException(
                "Ceres destruction with Samus requires her gameplay state.");
            lostSamus.SelectedHudItem = HudItem.Beam;
            lostSamus.AutoCancelHudItemIndex = HudItem.Beam;
            lostSamus.InvincibilityTimer = WrappingCountdown.Start(0);
            lostSamus.KnockbackTimer = 0;
            session.Close<CeresDestructionCinematicState>();
            BeginBlackDeathBlackOut();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoom"/>.</summary>
    internal void DispatchCeresGoesBoom(ControllerButton controllerInput)
    {
        // Each update begins a frame. A gameplay owner kept from Ceres
        // counts it on the frame clock and latches input; without one the frontend counted it.
        session.Runtime?.FrameBoundary.BeginAcceptedFrame(controllerInput);
        CeresDestructionCinematicState destruction = session.ScreenAs<CeresDestructionCinematicState>() ??
            session.Show(new CeresDestructionCinematicState(
                session.audio, session.Artwork.MapPresentation?.PowerBombGlowColors,
                session.Artwork.IntroCinematicArt, spriteSheets: session.Artwork.SpriteSheets));
        destruction.Step();
        session.display.PublishCeresDestruction(destruction);
        if (destruction.Finished)
        {
            // The cinematic's end forces blank and enters the game-data load. The load's own
            // Zebes-landing branch, not cinematic code, selects area zero/station eighteen.
            session.RouteGameDataLoad(GameDataLoadRoute.ZebesLanding);
            session.GameState = SuperMetroidGameState.LoadingGameData;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.EndingAndCredits"/>.</summary>
    internal void DispatchEndingAndCredits()
    {
        EndingCreditsPhase previousEndingPhase = session.ActiveEndingCredits.Stage.Phase;
        session.ActiveEndingCredits.Step();
        if (previousEndingPhase != EndingCreditsPhase.SeeYouNextMission &&
            session.ActiveEndingCredits.Stage.Phase == EndingCreditsPhase.SeeYouNextMission)
        {
            session.saves.SetGameCompleted(true);
            session.RaiseSavesChanged();
        }
        session.display.PublishEnding(session.ActiveEndingCredits);
    }

    /// <summary>
    /// Once the time-up fade reaches black: stop the escape
    /// timer and Ceres effects, silence all three sound libraries, then lose Samus with Ceres
    /// or, once the Zebes time bomb is set, cut straight to the death black-out.
    /// </summary>
    private void FinishTimeUpBlackOut()
    {
        if (session.Runtime is null)
            throw new InvalidOperationException("Time-up black-out requires a gameplay runtime.");
        session.screenFade.Clear();
        session.Runtime.FadeOut.End();
        session.Runtime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        session.Runtime.EscapeTimer.ClearStatus();
        session.audio.QueueSound(SamusSound.Silence, priority: SoundRequestPriority.Essential);
        session.audio.QueueSound(EnemySound.Silence, priority: SoundRequestPriority.Essential);
        session.audio.QueueSound(RoomSound.Silence, priority: SoundRequestPriority.Essential);
        session.display.PublishBlack();
        if (session.Runtime.System.HasEvent(StoryEvent.ZebesTimebombSet))
        {
            BeginBlackDeathBlackOut();
            return;
        }
        session.Show(new CeresDestructionCinematicState(
            session.audio, session.Artwork.MapPresentation?.PowerBombGlowColors,
            session.Artwork.IntroCinematicArt, outcome: CeresDestructionOutcome.SamusAboard,
            spriteSheets: session.Artwork.SpriteSheets));
        session.GameState = SuperMetroidGameState.CeresGoesBoomWithSamus;
    }

    /// <summary>
    /// Enters the final death black-out already black, as both time-up outcomes do. Its
    /// fade-out finds no brightness left, so that dispatch opens game over.
    /// </summary>
    private void BeginBlackDeathBlackOut()
    {
        session.DeathFade.Start(0, 0);
        session.GameState = SuperMetroidGameState.DeathFinalBlackOut;
        session.display.PublishBlack();
    }
}
