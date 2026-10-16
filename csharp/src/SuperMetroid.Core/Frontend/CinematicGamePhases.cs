using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;

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
        session.GameplayRuntime.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        session.pauseBrightness = session.screenFade.Advance(session.pauseBrightness, brightening: false);
        session.ApplyDisplayBrightness(session.pauseBrightness);
        if (session.pauseBrightness == 0)
            FinishTimeUpBlackOut();
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoomWithSamus"/>.</summary>
    internal void DispatchCeresGoesBoomWithSamus(SnesButton controllerInput)
    {
        session.runtime?.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        CeresDestructionCinematicState fatalDestruction = session.ceresDestruction ??
            throw new InvalidOperationException("Ceres destruction with Samus has no cinematic.");
        fatalDestruction.Step();
        session.display.PublishCeresDestruction(fatalDestruction);
        if (fatalDestruction.Finished)
        {
            // `$8B:C64E`: the fatal cinematic's fade-out ends at state $19, already
            // in forced blank, after clearing Samus's HUD selection and hurt timers.
            SamusState lostSamus = session.runtime?.Player.Samus ?? throw new InvalidOperationException(
                "Ceres destruction with Samus requires her gameplay state.");
            lostSamus.SelectedHudItem = 0;
            lostSamus.AutoCancelHudItemIndex = 0;
            lostSamus.InvincibilityTimer = NativeCountdown.Start(0);
            lostSamus.KnockbackTimer = NativeCountdown.Start(0);
            session.ceresDestruction = null;
            BeginForcedBlankDeathBlackOut();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.CeresGoesBoom"/>.</summary>
    internal void DispatchCeresGoesBoom(SnesButton controllerInput)
    {
        // Each update begins with an accepted NMI. A gameplay owner kept from Ceres
        // counts $05B5/$05B6 and latches input; without one the frontend counted it.
        session.runtime?.NmiHandler.RunNmi(controllerInput, mainLoopRequestedNmi: true);
        session.ceresDestruction ??= new CeresDestructionCinematicState(
            session.bus, session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
            session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx);
        session.ceresDestruction.Step();
        session.display.PublishCeresDestruction(session.ceresDestruction);
        if (session.ceresDestruction.Finished)
        {
            // CADF forces blank and publishes state six. The loader's special
            // `$22` branch—not cinematic code—selects area zero/station eighteen.
            session.stateSixLoadingRoute = SaveLoadingGameState.CeresDestruction;
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
            session.saveRam.SetGameCompleted(true);
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
        if (session.runtime is null)
            throw new InvalidOperationException("Time-up black-out requires a gameplay runtime.");
        session.screenFade.Clear();
        session.timeUpPaletteFade = null;
        session.runtime.Enemies.RidleyEncounter.CeresStatus = 0;
        session.runtime.EscapeTimer.ClearStatus();
        session.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
        session.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
        session.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
        session.display.PublishBlack();
        if (session.runtime.System.HasEvent(EventNumber.ZebesTimebombSet))
        {
            BeginForcedBlankDeathBlackOut();
            return;
        }
        session.ceresDestruction = new CeresDestructionCinematicState(
            session.bus, session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
            session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx, withSamus: true);
        session.GameState = SuperMetroidGameState.CeresGoesBoomWithSamus;
    }

    /// <summary>
    /// Enters state $19 already at forced blank, as both time-up outcomes do. Its
    /// <c>HandleFadingOut</c> finds no brightness left, so that dispatch opens game over.
    /// </summary>
    private void BeginForcedBlankDeathBlackOut()
    {
        session.deathFadeBrightness = 0;
        session.deathFadeCounter = 0;
        session.GameState = SuperMetroidGameState.DeathFinalBlackOut;
        session.display.PublishBlack();
    }
}
