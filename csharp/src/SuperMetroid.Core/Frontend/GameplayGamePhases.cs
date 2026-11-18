using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The gameplay phases: the first room's fade-in, main gameplay with the transitions its update makes, and the reserve
/// tanks' refill. The pause's brightening and the time-up's white-out run the same main gameplay update.
/// </summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class GameplayGamePhases(FrontendSession session)
{
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.GameplayFadeIn"/>.</summary>
    internal void RunGameplayFadeIn(ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = RuntimeFrameAudio.WithLowHealthCheck });
        session.display.PublishGameplay(session.GameplayRuntime);
        session.SetDisplayBrightness(session.GameplayFadeIn.Level);
        session.HandleGunshipLandingSave();
        if (session.GameplayFadeIn.Brighten())
            session.FinishGameplayFadeIn();
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.MainGameplay"/>.</summary>
    internal void RunMainGameplay(ControllerButton controllerInput)
    {
        RunMainGameplayUpdate(controllerInput);
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.ReserveTankRefill"/>.</summary>
    internal void RunReserveTankRefill(ControllerButton controllerInput)
    {
        // The refill begins from main gameplay, whose updates all run with Samus, and no update replaces her (#2503).
        SamusState samus = session.GameplayRuntime.Player.IntroducedSamus;
        SamusReserveAutoRecoveryStep reserveStep = default;
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = RuntimeFrameAudio.WithLowHealthCheck, AfterFrameBoundary = () =>
            {
                reserveStep = session.reserveRecovery.StepAfterFrame(
                    samus,
                    session.GameplayRuntime.Clock.Counter);
                if (reserveStep.RefillSoundRequested)
                    session.audio.QueueSound(RoomSound.IncrementalEnergy, SoundRequestPriority.Low);
                // The reserve refill's end unfreezes the game and returns to main gameplay before the next gameplay
                // update, so Samus, her projectiles and the enemies resume on the same update.
                if (reserveStep.Completed)
                {
                    session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
                    session.Enter(SuperMetroidGameState.MainGameplay);
                }
            } });
        session.display.PublishGameplay(session.GameplayRuntime);
        // The reserve refill invokes the external low-health check even while the
        // locked Samus movement mode omits it, after the update's gameplay, whose sounds are
        // already queued; the queue occupancy they leave decides whether this final
        // normal-priority request joins.
        samus.HealthWarning.Update(samus.Health, session.GameplayRuntime.AudioRequests);
    }

    /// <summary>Checks the main-gameplay pause gate, including Samus's control locks and new Start input.</summary>
    [IndependentFlag("Message-box ownership and the frame-start input lock are separate runtime states read from the frame.")]
    private bool CanEnterPause(
        SamusState samus,
        bool messageBoxOwnedFrame,
        bool samusInputLockedAtFrameStart)
    {
        if (session.GameplayRuntime.Room.ActiveRoom is null)
            return false;

        // The pause check runs only while Samus takes her normal input. A station's lock holds her input through its
        // whole insertion, message or data load and retraction, so none of it can be paused even though the game
        // remains in main gameplay. The locked input mode the lock sets stands for that hold; the remaining clauses are
        // the pause check's own predicate.
        return session.GameState == SuperMetroidGameState.MainGameplay &&
               !messageBoxOwnedFrame &&
               !session.GameplayRuntime.MessageBox.IsActive &&
               !samusInputLockedAtFrameStart &&
               !samus.Control.InputLocked &&
               !session.GameplayRuntime.PowerBombExplosionActive &&
               !samus.Xray.TimeIsFrozen &&
               !session.GameplayRuntime.RoomLoader.HasPendingDoorTransition &&
               session.GameplayRuntime.Room.ActiveRoom.Area != AreaId.Ceres &&
               session.GameplayRuntime.PlayerActions.NewlyPressed.HasAny(GameplayAction.Start);
    }

    /// <summary>
    /// The main gameplay update: one gameplay frame and the transitions its steps make. The pause's brightening and the
    /// time-up's white-out run the same update behind their fades.
    /// </summary>
    internal void RunMainGameplayUpdate(ControllerButton controllerInput)
    {
        // A message box is shown inside ordinary gameplay, which waits for it to close.
        // Its final acknowledgement returns after the point where gameplay checks
        // for pause, so Start may neither enter pause while the box is
        // visible nor on the exact frame it closes. Preserve entry state before
        // StepFrame consumes the acknowledgement; checking IsActive afterward alone
        // misses that final suspended frame.
        bool messageBoxOwnedFrame = session.GameplayRuntime.MessageBox.IsActive;
        bool mapStationMessageOwnedFrame = messageBoxOwnedFrame &&
            session.GameplayRuntime.MessageBox.MessageId == GameplayMessageId.MapDataAccessCompleted;
        // The station's lock has stopped Samus's input before this
        // frame. Retraction can restore normal input later in StepFrame, after
        // Samus's input phase has already passed pause-check.
        // Every gameplay update runs with Samus, and no update replaces her (#2503).
        SamusState samus = session.GameplayRuntime.Player.IntroducedSamus;
        bool samusInputLockedAtFrameStart = samus.Control.InputLocked;
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = RuntimeFrameAudio.WithLowHealthCheck });
        bool timeUp = session.GameplayRuntime.ConsumeTimeUpRequest();
        HandleSaveStationPersistence(samus);
        session.display.PublishGameplay(session.GameplayRuntime);
        session.HandleGunshipLandingSave();
        if (RouteOutOfHealth(samus))
        {
            // The death trigger enters the next sequence at the end of this update's completed gameplay.
            // Nothing else may replace it on the trigger
            // frame—not pause, an elevator handoff, or a pending ordinary door.
        }
        else if (timeUp)
        {
            // The escape countdown started the time-up during Samus's update, after her movement's
            // door hits and before pause-check, which then sees the game no longer in main gameplay.
            session.BeginTimeUp();
        }
        else if (session.GameplayRuntime.EnemyFrameResults.LastEnemyFrame.Gunship == GunshipFrameEvent.EscapeTakeoffCompleted)
        {
            session.BeginZebesEscapeFade();
        }
        else if (mapStationMessageOwnedFrame && !session.GameplayRuntime.MessageBox.IsActive)
        {
            // The message box's close explicitly starts the pause
            // for map acquisition, independently of Start or station input lock.
            // Keep this separate from admission of a manual pause request.
            session.Close<PauseMenuState>();
            session.Enter(PauseStep.DarkeningGameplay);
        }
        else if (CanEnterPause(samus, messageBoxOwnedFrame, samusInputLockedAtFrameStart))
        {
            // Samus's pause check executes during the already-completed
            // main-gameplay frame. It initializes both fade counters and starts
            // the pause; the following update consumes the first delay.
            session.screenFade.SetCadence(PauseFadeTiming.CounterReload, PauseFadeTiming.CounterReload);
            session.Close<PauseMenuState>();
            session.Enter(PauseStep.DarkeningGameplay);
        }
        else if (session.GameplayRuntime.Room.LoadedScope.CeresElevatorShaft is { DepartureRequestedThisFrame: true })
        {
            // The elevator has already selected standing pose and locked Samus.
            // The following update owns the
            // 60-frame hold; do not decrement it on this trigger frame.
            session.ceresDeparture.Begin();
            session.Enter(SuperMetroidGameState.CeresDeparture);
        }
        else if (session.GameplayRuntime.RoomLoader.HasPendingDoorTransition && !session.GameplayRuntime.MessageBox.IsActive)
        {
            // A door hit enters the door transition during the gameplay update.
            // The already-produced gameplay image remains this frame's image; the
            // following update reaches the door from that publication.
            // A message box opened later in the same update (a station's completion
            // notice in the room objects' step) suspends that update until it
            // closes, so the door waits for the box's final frame.
            session.Enter(DoorTransitionStep.EnteringDoor);
        }
    }

    /// <summary>Freezes gameplay and enters the reserve tanks' refill or death after Samus reaches zero health.</summary>
    private bool RouteOutOfHealth(SamusState samus)
    {
        if (samus.Health > 0)
            return false;

        session.GameplayRuntime.Freeze.GameplayTimeFrozen = true;
        if (samus.ReserveTankMode == ReserveTankMode.Auto && samus.ReserveEnergy != 0)
        {
            session.reserveRecovery.Begin(samus);
            session.Enter(SuperMetroidGameState.ReserveTankRefill);
        }
        else
        {
            // Samus's death disables colour effects and locks her input and drawing.
            // Locking the player out is the shared representation;
            // the death sequence's steps own colour and animation side effects.
            samus.Control.LockOutPlayer();
            session.Enter(DeathSequenceStep.FinalGameplayPass);
        }
        return true;
    }

    /// <summary>Consumes a confirmed station save and writes the selected save slot.</summary>
    private void HandleSaveStationPersistence(SamusState samus)
    {
        if (session.GameplayRuntime.MessageBoxes.ConsumeSaveStationPersistenceRequest() is not { } request)
            return;
        // Saving first stores the live explored map as the current area's.
        session.GameplayRuntime.System.StoredExploredMaps.StoreLiveMap(request.Station.Area);
        session.saves.SaveSlot(
            session.SelectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, session.GameplayRuntime.System, new SaveSnapshotContext { Checkpoint = request.Station, GameTime = session.GameplayRuntime.GameTime, ControllerBindings = session.GameplayRuntime.Options.ControllerBindings, MoonwalkEnabled = session.GameplayRuntime.Options.MoonwalkEnabled, IconCancelEnabled = session.GameplayRuntime.Options.IconCancelEnabled, JapaneseText = session.GameplayRuntime.JapaneseText }));
        session.RaiseSavesChanged();
    }
}
