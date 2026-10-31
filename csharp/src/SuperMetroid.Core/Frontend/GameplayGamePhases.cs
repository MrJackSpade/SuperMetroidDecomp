using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>The gameplay phases: main gameplay, the pause and unpause fades, time-up, reserve tanks and the Ceres and Zebes departures.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class GameplayGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MadeItToCeresElevator"/>.</summary>
    internal void DispatchMadeItToCeresElevator(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        // State $20 is only the departure hold. The arrival descent runs in
        // ordinary state eight while the bank-$86 elevator objects lock Samus.
        if (session.ceresDeparture.Phase != CeresDeparturePhase.HoldingOnElevator)
            throw new InvalidOperationException("State $20 requires the Ceres departure hold.");
        if (session.ceresDeparture.StepHoldAfterGameplay())
            session.GameState = SuperMetroidGameState.BlackoutFromCeres;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MainGameplay"/>.</summary>
    internal void DispatchMainGameplay(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        RunMainGameplayState(controllerInput, gameplayAudio);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.ReserveTanksAuto"/>.</summary>
    internal void DispatchReserveTanksAuto(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        SamusReserveAutoRecoveryStep reserveStep = default;
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime), AfterFrameBoundary = () =>
            {
                SamusState samus = session.GameplayRuntime.Player.Samus
                    ?? throw new InvalidOperationException(
                        "Reserve recovery requires a live Samus state.");
                reserveStep = session.reserveRecovery.StepAfterFrame(
                    samus,
                    session.GameplayRuntime.Clock.Counter);
                if (reserveStep.RefillSoundRequested)
                    session.audio.QueueSound(Library3Sound.IncrementalEnergy, SoundQueueLimit.Max3,
                        soundSuppressed: session.GameplayRuntime.PowerBombExplosionSuppressesSounds);
                // $82:DC18-$DC24 clears the freeze and restores state eight
                // BEFORE calling gameplay. Unfreezing after StepFrame lets
                // Samus resume while projectiles/enemies remain a frame behind.
                if (reserveStep.Completed)
                {
                    session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
                    session.GameState = SuperMetroidGameState.MainGameplay;
                }
            } });
        session.display.PublishGameplay(session.GameplayRuntime);
        // $82:DC2B invokes the external low-health check even while the
        // locked Samus beta handler omits it. Publish prior gameplay sounds
        // first; their native queue occupancy affects this final Max6 call.
        session.CollectTranslatedAudioRequests(gameplayAudio);
        session.GameplayRuntime.Player.IntroducedSamus.HealthWarning.Update(session.GameplayRuntime.Player.IntroducedSamus.Health, session.audio,
            soundSuppressed: session.GameplayRuntime.PowerBombExplosionSuppressesSounds);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausingDarkening"/>.</summary>
    internal void DispatchPausingDarkening(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $0C continues running ordinary gameplay while the screen darkens.
        // This matters for moving enemies/projectiles and is why pause cannot be
        // represented as a desktop-only frozen bitmap.
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        if (session.GameplayRuntime.RoomLoader.HasPendingDoorTransition)
            session.GameState = SuperMetroidGameState.HitDoorBlock;
        if (session.GameplayRuntime.ConsumeTimeUpRequest())
            BeginTimeUp();
        if (session.StepScreenFadeOut() == 0)
        {
            // The finished fade moves on from the state gameplay left: into the pause
            // preparation, or into the next step of a door hit or time-up written on
            // this final fade frame.
            session.screenFade.Clear();
            session.GameState = StateAfterPauseDarkening(session.GameState);
        }
    }

    /// <summary>
    /// The state the finished pause darkening enters from <paramref name="state"/>, the state gameplay left on its final
    /// fade update: the pause preparation, or the step after a door hit or time-up that update wrote.
    /// </summary>
    /// <exception cref="InvalidOperationException">Gameplay left a state the darkening cannot continue from.</exception>
    private static SuperMetroidGameState StateAfterPauseDarkening(SuperMetroidGameState state) => state switch
    {
        SuperMetroidGameState.PausingDarkening => SuperMetroidGameState.Pausing,
        SuperMetroidGameState.HitDoorBlock => SuperMetroidGameState.LoadingNextRoomA,
        SuperMetroidGameState.TimeUp => SuperMetroidGameState.WhitingOutFromTimeUp,
        SuperMetroidGameState.Reset or
        SuperMetroidGameState.OpeningCinematic or
        SuperMetroidGameState.GameOptionsMenu or
        SuperMetroidGameState.FileSelectMenus or
        SuperMetroidGameState.FileSelectMap or
        SuperMetroidGameState.LoadingGameData or
        SuperMetroidGameState.MainGameplayFadeIn or
        SuperMetroidGameState.MainGameplay or
        SuperMetroidGameState.LoadingNextRoomA or
        SuperMetroidGameState.LoadingNextRoomB or
        SuperMetroidGameState.Pausing or
        SuperMetroidGameState.PausedA or
        SuperMetroidGameState.PausedB or
        SuperMetroidGameState.UnpausingA or
        SuperMetroidGameState.UnpausingB or
        SuperMetroidGameState.Unpausing or
        SuperMetroidGameState.DeathSequenceStart or
        SuperMetroidGameState.DeathBlackOutSurroundings or
        SuperMetroidGameState.DeathWaitForMusic or
        SuperMetroidGameState.DeathPreFlashing or
        SuperMetroidGameState.DeathFlashing or
        SuperMetroidGameState.DeathExplosionWhiteOut or
        SuperMetroidGameState.DeathFinalBlackOut or
        SuperMetroidGameState.GameOverMenu or
        SuperMetroidGameState.ReserveTanksAuto or
        SuperMetroidGameState.IntroCinematic or
        SuperMetroidGameState.SetUpNewGame or
        SuperMetroidGameState.MadeItToCeresElevator or
        SuperMetroidGameState.BlackoutFromCeres or
        SuperMetroidGameState.CeresGoesBoom or
        SuperMetroidGameState.WhitingOutFromTimeUp or
        SuperMetroidGameState.CeresGoesBoomWithSamus or
        SuperMetroidGameState.SamusEscapesFromZebes or
        SuperMetroidGameState.EndingAndCredits or
        SuperMetroidGameState.TransitionToDemoA or
        SuperMetroidGameState.TransitionToDemoB or
        SuperMetroidGameState.PlayingDemo or
        SuperMetroidGameState.TransitionFromDemoA or
        SuperMetroidGameState.TransitionFromDemoB =>
            throw new InvalidOperationException($"The pause darkening cannot finish from {state}."),
        _ => throw new InvalidOperationException($"Undefined game state {state}."),
    };

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Unpausing"/>.</summary>
    internal void DispatchUnpausing(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // `$82:93A4` runs the whole state-eight routine behind the screen fade, so
        // its out-of-health, pause, door and other publications all apply here.
        RunMainGameplayState(controllerInput, gameplayAudio);
        if (session.StepScreenFadeIn() == PauseFadeTiming.FullyLit)
        {
            // $82:93BB explicitly restores state eight on the terminal frame.
            session.screenFade.Clear();
            session.GameState = SuperMetroidGameState.MainGameplay;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.BlackoutFromCeres"/>.</summary>
    internal void DispatchBlackoutFromCeres(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        bool ceresFadeFinished = session.ceresDeparture.StepFadeAfterGameplay();
        session.SetDisplayBrightness(checked((byte)session.ceresDeparture.Brightness));
        if (ceresFadeFinished)
        {
            // GameState_33 saves the live area-six/station-zero slot only after
            // the screen is blanked. This is the durable post-Ridley Ceres
            // checkpoint the retail loader later replaces with Zebes arrival.
            AutomaticCheckpointSaver.SaveCeresDeparture(session.saves, session.GameplayRuntime, session.SelectedSaveSlot);
            session.RaiseSaveRamChanged();
            session.GameplayRuntime.EnemySession.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
            session.GameplayRuntime.EscapeTimer.Clear();
            // $82:83F3-$840B: stop music and silence all three sound libraries.
            session.audio.QueueMusicDelayed8(MusicRequest.Stop);
            session.audio.QueueSound(Library1Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            session.audio.QueueSound(Library2Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            session.audio.QueueSound(Library3Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            session.Show(new CeresDestructionCinematicState(
                session.audio, session.Artwork.MapPresentation?.PowerBombFixedColors,
                session.Artwork.IntroCinematicArt, session.Artwork.MapPresentation?.RoomPaletteFx, spriteSheets: session.Artwork.SpriteSheets));
            session.GameState = SuperMetroidGameState.CeresGoesBoom;
            session.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.TimeUp"/>.</summary>
    internal void DispatchTimeUp(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // `$82:8411` runs the whole state-eight routine, then advances every palette
        // toward the white target with denominator eight.
        RunMainGameplayState(controllerInput, gameplayAudio);
        if ((session.TimeUpPaletteFade ?? throw new InvalidOperationException(
                "Time-up white-out started without its target palette.")).Step())
        {
            session.GameState = SuperMetroidGameState.WhitingOutFromTimeUp;
            session.screenFade.Clear();
        }
        session.display.PublishGameplay(session.GameplayRuntime);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.SamusEscapesFromZebes"/>.</summary>
    internal void DispatchSamusEscapesFromZebes(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $26 calls the complete state-eight gameplay coroutine before
        // HandleFadeOut. The fleeing gunship, room animations, and APU publishers
        // therefore continue behind every darkening frame.
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        // HandleFadingOut ($80:8924) reloads the counter from ScreenFadeDelay, which
        // $A2:AD26 zeroed, so the brightness falls one step on every call.
        session.SetDisplayBrightness(session.EscapeFade.Darken());
        if (session.EscapeFade.Brightness == 0)
        {
            // $82:84D3-$8527 resets PPU ownership, stops the escape timer/music,
            // cancels all three SFX libraries, and publishes state $27. The new
            // state performs its own cartridge-backed setup on the following call.
            session.audio.QueueMusicDelayed8(MusicRequest.Stop);
            session.audio.QueueSound(Library1Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            session.audio.QueueSound(Library2Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            session.audio.QueueSound(Library3Sound.Silence, maximumQueued: SoundQueueLimit.Max15);
            SamusState endingSamus = session.GameplayRuntime.Player.Samus ??
                throw new InvalidOperationException(
                    "Ending inventory calculation requires the live Samus state.");
            session.Show(new EndingCreditsState(session.audio, new EndingCreditsSetup { GameTimeHours = session.ConfiguredOptions.EndingTimeOverrideMinutes is { } endingMinutes ? (ushort)(endingMinutes / 60) : session.GameplayRuntime.GameTime.Hours, GameTimeMinutes = session.ConfiguredOptions.EndingTimeOverrideMinutes is { } totalMinutes ? (ushort)(totalMinutes % 60) : session.GameplayRuntime.GameTime.Minutes, Inventory = new EndingInventorySnapshot(
                    endingSamus.MaxHealth,
                    endingSamus.MaxReserveEnergy,
                    endingSamus.MaxMissiles,
                    endingSamus.MaxSuperMissiles,
                    endingSamus.MaxPowerBombs,
                    endingSamus.CollectedItems,
                    endingSamus.CollectedBeams), JapaneseText = session.GameplayRuntime.JapaneseText, CrittersEscaped = session.GameplayRuntime.System.HasEvent(EventNumber.CrittersEscaped) }));
            session.ActiveEndingCredits.InstalledArtwork.BindEndingText(session.Artwork.MapPresentation?.EndingText);
            session.ActiveEndingCredits.Flyaway.BindFlightArtwork(session.Artwork.IntroCinematicArt?.CeresFlight);
            session.ActiveEndingCredits.Stage.BindFloorArtwork(session.Artwork.EndingFloorArt);
            session.ActiveEndingCredits.InstalledArtwork.BindObjectArtwork(session.Artwork.EndingObjectArt);
            session.ActiveEndingCredits.InstalledArtwork.BindPaletteArtwork(session.Artwork.EndingPaletteArt);
            session.ActiveEndingCredits.InstalledArtwork.BindSpriteSheets(session.Artwork.SpriteSheets);
            session.ActiveEndingCredits.InstalledArtwork.BindPaletteFxColors(session.Artwork.MapPresentation?.RoomPaletteFx);
            session.ActiveEndingCredits.Stage.BindEndingFont(session.Artwork.MapPresentation?.EndingFont);
            session.GameState = SuperMetroidGameState.EndingAndCredits;
            session.display.PublishBlack();
        }
    }

    /// <summary>Checks the native state-eight pause gate, including Samus handler locks and new Start input.</summary>
    [IndependentFlag("Message-box ownership and the frame-start input lock are separate runtime states read from the frame.")]
    private bool CanEnterPause(
        bool messageBoxOwnedFrame,
        bool samusInputLockedAtFrameStart)
    {
        if (session.Runtime?.Player.Samus is not SamusState samus || session.Runtime.Room.ActiveRoom is null)
            return false;

        // `$90:EA45` only runs from SamusNewStateHandler_Normal. Station command six
        // replaces that handler with the inert `$90:E8D6`, so its entire insertion,
        // message/data-load, and retraction sequence bypasses pause-check even though game
        // state remains eight. The locked alpha installed with it stands for that handler
        // replacement; the remaining clauses are the predicate inside EA45 itself.
        return session.GameState == SuperMetroidGameState.MainGameplay &&
               !messageBoxOwnedFrame &&
               !session.Runtime.MessageBox.IsActive &&
               !samusInputLockedAtFrameStart &&
               !samus.Control.AlphaLocked &&
               !session.Runtime.PowerBombExplosionActive &&
               !samus.Xray.TimeIsFrozen &&
               !session.Runtime.RoomLoader.HasPendingDoorTransition &&
               session.Runtime.Room.ActiveRoom.AreaIndex != AreaId.Ceres &&
               (session.Runtime.Controller1.NewlyPressed & ControllerButton.Start) != ControllerButton.None;
    }

    /// <summary>
    /// <c>GameState_8_MainGameplay</c>: one gameplay frame and the state publications its
    /// routines make. State $12 (<c>$82:93A1</c>) calls the same routine before fading in.
    /// </summary>
    private void RunMainGameplayState(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // A message box is shown inside ordinary gameplay, which waits for it to close.
        // Its final acknowledgement returns after the point where gameplay checks
        // for pause, so Start may neither enter pause while the box is
        // visible nor on the exact frame it closes. Preserve entry state before
        // StepFrame consumes the acknowledgement; checking IsActive afterward alone
        // misses that final suspended frame.
        bool messageBoxOwnedFrame = session.GameplayRuntime.MessageBox.IsActive;
        bool mapStationMessageOwnedFrame = messageBoxOwnedFrame &&
            session.GameplayRuntime.MessageBox.MessageId == GameplayMessageIds.MapDataAccessCompleted;
        // Station command six installs an inert new-state handler before this
        // frame. Retraction can restore normal input later in StepFrame, after the
        // cartridge's Samus-handler call site has already passed pause-check.
        bool samusInputLockedAtFrameStart = session.GameplayRuntime.Player.Samus?.Control.AlphaLocked == true;
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        bool timeUp = session.GameplayRuntime.ConsumeTimeUpRequest();
        HandleSaveStationPersistence();
        session.display.PublishGameplay(session.GameplayRuntime);
        session.HandleGunshipLandingSave();
        if (RouteOutOfHealth())
        {
            // `$82:DB69` publishes the next outer state at the end of this already-
            // completed gameplay call. Nothing else may replace it on the trigger
            // frame—not pause, an elevator handoff, or a pending ordinary door.
        }
        else if (timeUp)
        {
            // `$90:E0EC` wrote state $23 inside Samus's handler, after her movement's
            // door hits and before pause-check, which then sees a state other than eight.
            BeginTimeUp();
        }
        else if (session.GameplayRuntime.EnemyFrameResults.LastEnemyFrame.Gunship == GunshipFrameEvent.EscapeTakeoffCompleted)
        {
            session.BeginZebesEscapeFade();
        }
        else if (mapStationMessageOwnedFrame && !session.GameplayRuntime.MessageBox.IsActive)
        {
            // Bank $85's message-close return explicitly selects state $0C
            // for map acquisition, independently of Start or station input lock.
            // Keep this separate from admission of a manual pause request.
            session.Close<PauseMenuState>();
            session.GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (CanEnterPause(messageBoxOwnedFrame, samusInputLockedAtFrameStart))
        {
            // Samus_PauseCheck at `$90:EA45` executes during the already-completed
            // state-eight frame. It initializes both fade counters and publishes
            // state $0C; the following dispatcher call consumes the first delay.
            session.screenFade.Reload();
            session.Close<PauseMenuState>();
            session.GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (session.GameplayRuntime.RoomMains.LastCeresElevatorShaftRoomMain.DepartureRequestedThisFrame)
        {
            // `$89:ACC3` has already selected standing pose, locked Samus, and
            // published game state $20. The following dispatcher call owns the
            // 60-frame hold; do not decrement it on this trigger frame.
            session.ceresDeparture.Begin();
            session.GameState = SuperMetroidGameState.MadeItToCeresElevator;
        }
        else if (session.GameplayRuntime.RoomLoader.HasPendingDoorTransition && !session.GameplayRuntime.MessageBox.IsActive)
        {
            // `$94:938B/$93CE` changes WRAM game_state during the gameplay call.
            // The already-produced gameplay image remains this frame's image; the
            // following dispatcher call begins state `$09` from that publication.
            // A message box opened later in the same call (a station's completion
            // notice in the PLM handler) suspends that call in bank $85 until it
            // closes, so the door's state waits for the box's final frame.
            session.GameState = SuperMetroidGameState.HitDoorBlock;
        }
    }

    /// <summary>Freezes gameplay and enters reserve recovery or death after Samus reaches zero health.</summary>
    private bool RouteOutOfHealth()
    {
        if (session.Runtime?.Player.Samus is not SamusState samus || samus.Health > 0)
            return false;

        session.Runtime.Freeze.GameplayTimeFrozen = true;
        if (samus.ReserveTankMode == ReserveTankMode.Auto && samus.ReserveEnergy != 0)
        {
            session.reserveRecovery.Begin(samus);
            session.GameState = SuperMetroidGameState.ReserveTanksAuto;
        }
        else
        {
            // CallSomeSamusCode($11) disables palette effects and installs fatal-input and
            // fatal-draw handlers. Input locking is the shared translated representation;
            // the following named death states own palette and animation side effects.
            samus.Control.LockHandlers();
            session.GameState = SuperMetroidGameState.DeathSequenceStart;
        }
        return true;
    }

    /// <summary>
    /// <c>$90:E0EC-$E106</c>: the escape countdown expired. Every target color becomes white
    /// for state $23's gradual change through the shared <c>$7E:C400</c> numerator.
    /// </summary>
    private void BeginTimeUp()
    {
        if (session.Runtime is null)
            throw new InvalidOperationException("Time-up requires a gameplay runtime.");
        ColourImage white = session.Runtime.Colours.Capture();
        white.Fill(TimeUpDefinitions.WhiteColor);
        session.BeginTimeUpPaletteFade(new GradualPaletteTransition(white, TimeUpDefinitions.WhiteOutDenominator,
            session.Runtime.EnemySession.GradualColorChange));
        session.GameState = SuperMetroidGameState.TimeUp;
    }

    /// <summary>Consumes a confirmed station save and writes the selected SRAM slot.</summary>
    private void HandleSaveStationPersistence()
    {
        if (session.Runtime?.MessageBoxes.ConsumeSaveStationPersistenceRequest() is not { } request)
            return;
        SamusState samus = session.Runtime.Player.Samus
            ?? throw new InvalidOperationException(
                "A confirmed save station has no live Samus state.");
        session.saves.SaveSlot(
            session.SelectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, session.Runtime.System, new SaveSnapshotContext { Area = request.AreaIndex, SaveStation = request.StationIndex, GameTime = session.Runtime.GameTime, ControllerBindings = session.Runtime.Options.ControllerBindings, MoonwalkEnabled = session.Runtime.Options.MoonwalkEnabled, IconCancelEnabled = session.Runtime.Options.IconCancelEnabled, JapaneseText = session.Runtime.JapaneseText }));
        session.RaiseSaveRamChanged();
    }
}
