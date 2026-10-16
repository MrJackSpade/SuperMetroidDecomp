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

/// <summary>The gameplay phases: main gameplay, the pause and unpause fades, time-up, reserve tanks and the Ceres and Zebes departures.</summary>
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class GameplayGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MadeItToCeresElevator"/>.</summary>
    internal void DispatchMadeItToCeresElevator(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        game.display.PublishGameplay(game.GameplayRuntime);
        // State $20 is only the departure hold. The arrival descent runs in
        // ordinary state eight while the bank-$86 elevator objects lock Samus.
        if (game.ceresDeparture.Phase != CeresDeparturePhase.HoldingOnElevator)
            throw new InvalidOperationException("State $20 requires the Ceres departure hold.");
        if (game.ceresDeparture.StepHoldAfterGameplay())
            game.GameState = SuperMetroidGameState.BlackoutFromCeres;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.MainGameplay"/>.</summary>
    internal void DispatchMainGameplay(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        RunMainGameplayState(controllerInput, gameplayAudio);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.ReserveTanksAuto"/>.</summary>
    internal void DispatchReserveTanksAuto(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        SamusReserveAutoRecoveryStep reserveStep = default;
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime), AfterAcceptedNmi = () =>
            {
                SamusState samus = game.GameplayRuntime.Player.Samus
                    ?? throw new InvalidOperationException(
                        "Reserve recovery requires a live Samus state.");
                reserveStep = game.reserveRecovery.StepAfterNmi(
                    samus,
                    game.GameplayRuntime.Nmi.NmiFrameCounter);
                if (reserveStep.RefillSoundRequested)
                    game.audio.QueueSoundAndGetAccumulator(SoundEffectLibrary3Sounds.ReserveRefill, 3,
                        soundSuppressed: game.GameplayRuntime.PowerBombExplosionSuppressesSounds);
                // $82:DC18-$DC24 clears the freeze and restores state eight
                // BEFORE calling gameplay. Unfreezing after StepFrame lets
                // Samus resume while projectiles/enemies remain a frame behind.
                if (reserveStep.Completed)
                {
                    game.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
                    game.GameState = SuperMetroidGameState.MainGameplay;
                }
            } });
        game.display.PublishGameplay(game.GameplayRuntime);
        // $82:DC2B invokes the external low-health check even while the
        // locked Samus beta handler omits it. Publish prior gameplay sounds
        // first; their native queue occupancy affects this final Max6 call.
        game.CollectTranslatedAudioRequests(gameplayAudio);
        game.GameplayRuntime.Player.IntroducedSamus.HealthWarning.Update(game.GameplayRuntime.Player.IntroducedSamus.Health, game.audio,
            soundSuppressed: game.GameplayRuntime.PowerBombExplosionSuppressesSounds);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.PausingDarkening"/>.</summary>
    internal void DispatchPausingDarkening(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $0C continues running ordinary gameplay while INIDISP darkens.
        // This matters for moving enemies/projectiles and is why pause cannot be
        // represented as a desktop-only frozen bitmap.
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        game.display.PublishGameplay(game.GameplayRuntime);
        if (game.GameplayRuntime.RoomLoader.HasPendingDoorTransition)
            game.GameState = SuperMetroidGameState.HitDoorBlock;
        if (game.GameplayRuntime.ConsumeTimeUpRequest())
            BeginTimeUp();
        game.pauseBrightness = game.screenFade.Advance(game.pauseBrightness, brightening: false);
        game.ApplyDisplayBrightness(game.pauseBrightness);
        if (game.pauseBrightness == 0)
        {
            // $82:8CEA increments the state written by gameplay, including a
            // door hit on this final fade frame ($09 becomes $0A).
            game.screenFade.Clear();
            game.GameState = game.GameState.Next();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.Unpausing"/>.</summary>
    internal void DispatchUnpausing(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // `$82:93A4` runs the whole state-eight routine behind the INIDISP fade, so
        // its out-of-health, pause, door and other publications all apply here.
        RunMainGameplayState(controllerInput, gameplayAudio);
        game.pauseBrightness = game.screenFade.Advance(game.pauseBrightness, brightening: true);
        game.ApplyDisplayBrightness(game.pauseBrightness);
        if (game.pauseBrightness == PauseFadeTiming.FullyLit)
        {
            // $82:93BB explicitly restores state eight on the terminal frame.
            game.screenFade.Clear();
            game.GameState = SuperMetroidGameState.MainGameplay;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.BlackoutFromCeres"/>.</summary>
    internal void DispatchBlackoutFromCeres(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false }, Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        game.display.PublishGameplay(game.GameplayRuntime);
        bool ceresReachedForcedBlank = game.ceresDeparture.StepFadeAfterGameplay();
        game.ApplyDisplayBrightness(game.ceresDeparture.Brightness);
        if (ceresReachedForcedBlank)
        {
            // GameState_33 saves the live area-six/station-zero slot only after
            // INIDISP reaches forced blank. This is the durable post-Ridley Ceres
            // checkpoint the retail loader later replaces with Zebes arrival.
            AutomaticCheckpointSaver.SaveCeresDeparture(game.bus, game.GameplayRuntime, game.selectedSaveSlot);
            game.RaiseSaveRamChanged();
            game.GameplayRuntime.Enemies.RidleyEncounter.CeresStatus = 0;
            game.GameplayRuntime.EscapeTimer.Clear();
            // $82:83F3-$840B: stop music and silence all three sound libraries.
            game.audio.QueueMusicDelayed8(MusicCommand.Stop);
            game.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            game.ceresDestruction = new CeresDestructionCinematicState(
                game.bus, game.audio, game.Artwork.MapPresentation?.PowerBombFixedColors,
                game.Artwork.IntroCinematicArt, game.Artwork.MapPresentation?.RoomPaletteFx);
            game.GameState = SuperMetroidGameState.CeresGoesBoom;
            game.display.PublishBlack();
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.TimeUp"/>.</summary>
    internal void DispatchTimeUp(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // `$82:8411` runs the whole state-eight routine, then advances every palette
        // toward the white target with denominator eight.
        RunMainGameplayState(controllerInput, gameplayAudio);
        if ((game.timeUpPaletteFade ?? throw new InvalidOperationException(
                "Time-up white-out started without its target palette.")).Step(game.GameplayRuntime.Cgram))
        {
            game.GameState = SuperMetroidGameState.WhitingOutFromTimeUp;
            game.screenFade.Clear();
        }
        game.display.PublishGameplay(game.GameplayRuntime);
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.SamusEscapesFromZebes"/>.</summary>
    internal void DispatchSamusEscapesFromZebes(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $26 calls the complete state-eight gameplay coroutine before
        // HandleFadeOut. The fleeing gunship, room animations, and APU publishers
        // therefore continue behind every darkening frame.
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        game.display.PublishGameplay(game.GameplayRuntime);
        // HandleFadingOut ($80:8924) reloads the counter from ScreenFadeDelay, which
        // $A2:AD26 zeroed, so the brightness falls one step on every call.
        if (game.endingFadeCounter-- <= 0)
        {
            game.endingFadeCounter = 0;
            game.endingFadeBrightness = (byte)Math.Max(0, game.endingFadeBrightness - 1);
        }
        game.ApplyDisplayBrightness(game.endingFadeBrightness);
        if (game.endingFadeBrightness == 0)
        {
            // $82:84D3-$8527 resets PPU ownership, stops the escape timer/music,
            // cancels all three SFX libraries, and publishes state $27. The new
            // state performs its own cartridge-backed setup on the following call.
            game.audio.QueueMusicDelayed8(MusicCommand.Stop);
            game.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            SamusState endingSamus = game.GameplayRuntime.Player.Samus ??
                throw new InvalidOperationException(
                    "Ending inventory calculation requires the live Samus state.");
            game.endingCredits = new EndingCreditsState(game.bus, game.audio, new EndingCreditsSetup { GameTimeHours = game.ConfiguredOptions.EndingTimeOverrideMinutes is { } endingMinutes ? (ushort)(endingMinutes / 60) : game.GameplayRuntime.GameTime.Hours, GameTimeMinutes = game.ConfiguredOptions.EndingTimeOverrideMinutes is { } totalMinutes ? (ushort)(totalMinutes % 60) : game.GameplayRuntime.GameTime.Minutes, Inventory = new EndingInventorySnapshot(
                    endingSamus.MaxHealth,
                    endingSamus.MaxReserveEnergy,
                    endingSamus.MaxMissiles,
                    endingSamus.MaxSuperMissiles,
                    endingSamus.MaxPowerBombs,
                    endingSamus.CollectedItems,
                    endingSamus.CollectedBeams), JapaneseText = game.GameplayRuntime.JapaneseText, CrittersEscaped = game.GameplayRuntime.System.HasEvent(EventNumber.CrittersEscaped) });
            game.endingCredits.BindEndingText(game.Artwork.MapPresentation?.EndingText);
            game.endingCredits.BindFlightArtwork(game.Artwork.IntroCinematicArt?.CeresFlight);
            game.endingCredits.BindMode7Artwork(game.Artwork.EndingMode7Art);
            game.endingCredits.BindObjectArtwork(game.Artwork.EndingObjectArt);
            game.endingCredits.BindPaletteArtwork(game.Artwork.EndingPaletteArt);
            game.endingCredits.BindPaletteFxColors(game.Artwork.MapPresentation?.RoomPaletteFx);
            game.endingCredits.BindEndingFont(game.Artwork.MapPresentation?.EndingFont);
            game.endingCredits.BindStaffCredits(game.Artwork.MapPresentation?.StaffCredits);
            game.GameState = SuperMetroidGameState.EndingAndCredits;
            game.display.PublishBlack();
        }
    }

    /// <summary>Checks the native state-eight pause gate, including Samus handler locks and new Start input.</summary>
    private bool CanEnterPause(
        bool messageBoxOwnedFrame,
        bool samusInputLockedAtFrameStart)
    {
        if (game.runtime?.Player.Samus is not SamusState samus || game.runtime.Room.ActiveRoom is null)
            return false;

        // `$90:EA45` only runs from SamusNewStateHandler_Normal. Station command six
        // replaces that handler with the inert `$90:E8D6`, so its entire insertion,
        // message/data-load, and retraction sequence bypasses pause-check even though game
        // state remains eight. InputLocked is the semantic representation of that handler
        // replacement; the remaining clauses are the predicate inside EA45 itself.
        return game.GameState == SuperMetroidGameState.MainGameplay &&
               !messageBoxOwnedFrame &&
               !game.runtime.MessageBox.IsActive &&
               !samusInputLockedAtFrameStart &&
               !samus.InputLocked &&
               game.runtime.PowerBombExplosionStatus == 0 &&
               !samus.Xray.TimeIsFrozen &&
               !game.runtime.RoomLoader.HasPendingDoorTransition &&
               game.runtime.Room.ActiveRoom.AreaIndex != AreaId.Ceres &&
               (game.runtime.Controller1.NewlyPressed & SnesButton.Start) != 0;
    }

    /// <summary>
    /// <c>GameState_8_MainGameplay</c>: one gameplay frame and the state publications its
    /// routines make. State $12 (<c>$82:93A1</c>) calls the same routine before fading in.
    /// </summary>
    private void RunMainGameplayState(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // DisplayMessageBox is a nested NMI-waiting coroutine inside gameplay
        // state eight. Its final acknowledgement returns below the normal bank-$90
        // pause-check call site, so Start may neither enter pause while the box is
        // visible nor on the exact frame it closes. Preserve entry state before
        // StepFrame consumes the acknowledgement; checking IsActive afterward alone
        // misses that final suspended frame.
        bool messageBoxOwnedFrame = game.GameplayRuntime.MessageBox.IsActive;
        bool mapStationMessageOwnedFrame = messageBoxOwnedFrame &&
            game.GameplayRuntime.MessageBox.MessageId == GameplayMessageIds.MapDataAccessCompleted;
        // Station command six installs an inert new-state handler before this
        // frame. Retraction can restore normal input later in StepFrame, after the
        // cartridge's Samus-handler call site has already passed pause-check.
        bool samusInputLockedAtFrameStart = game.GameplayRuntime.Player.Samus?.InputLocked == true;
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        bool timeUp = game.GameplayRuntime.ConsumeTimeUpRequest();
        HandleSaveStationPersistence();
        game.display.PublishGameplay(game.GameplayRuntime);
        game.HandleGunshipLandingSave();
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
        else if (game.GameplayRuntime.Enemies.GunshipBehavior.LastGunshipEvent == GunshipFrameEvent.EscapeTakeoffCompleted)
        {
            game.BeginZebesEscapeFade();
        }
        else if (mapStationMessageOwnedFrame && !game.GameplayRuntime.MessageBox.IsActive)
        {
            // Bank $85's message-close return explicitly selects state $0C
            // for map acquisition, independently of Start or station input lock.
            // Keep this separate from admission of a manual pause request.
            game.pauseMenu = null;
            game.GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (CanEnterPause(messageBoxOwnedFrame, samusInputLockedAtFrameStart))
        {
            // Samus_PauseCheck at `$90:EA45` executes during the already-completed
            // state-eight frame. It initializes both fade counters and publishes
            // state $0C; the following dispatcher call consumes the first delay.
            game.screenFade.Reload();
            game.pauseMenu = null;
            game.GameState = SuperMetroidGameState.PausingDarkening;
        }
        else if (game.GameplayRuntime.RoomMains.LastCeresElevatorShaftRoomMain.DepartureRequestedThisFrame)
        {
            // `$89:ACC3` has already selected standing pose, locked Samus, and
            // published game state $20. The following dispatcher call owns the
            // 60-frame hold; do not decrement it on this trigger frame.
            game.ceresDeparture.Begin();
            game.GameState = SuperMetroidGameState.MadeItToCeresElevator;
        }
        else if (game.GameplayRuntime.RoomLoader.HasPendingDoorTransition && !game.GameplayRuntime.MessageBox.IsActive)
        {
            // `$94:938B/$93CE` changes WRAM game_state during the gameplay call.
            // The already-produced gameplay image remains this frame's image; the
            // following dispatcher call begins state `$09` from that publication.
            // A message box opened later in the same call (a station's completion
            // notice in the PLM handler) suspends that call in bank $85 until it
            // closes, so the door's state waits for the box's final frame.
            game.GameState = SuperMetroidGameState.HitDoorBlock;
        }
    }

    /// <summary>Freezes gameplay and enters reserve recovery or death after Samus reaches zero health.</summary>
    private bool RouteOutOfHealth()
    {
        if (game.runtime?.Player.Samus is not SamusState samus || unchecked((short)samus.Health) > 0)
            return false;

        game.runtime.Freeze.GameplayTimeFrozen = true;
        if ((samus.ReserveTankMode & 1) != 0 && samus.ReserveEnergy != 0)
        {
            game.reserveRecovery.Begin(samus);
            game.GameState = SuperMetroidGameState.ReserveTanksAuto;
        }
        else
        {
            // CallSomeSamusCode($11) disables palette effects and installs fatal-input and
            // fatal-draw handlers. Input locking is the shared translated representation;
            // the following named death states own palette and animation side effects.
            samus.InputLocked = true;
            game.GameState = SuperMetroidGameState.DeathSequenceStart;
        }
        return true;
    }

    /// <summary>
    /// <c>$90:E0EC-$E106</c>: the escape countdown expired. Every target color becomes white
    /// for state $23's gradual change through the shared <c>$7E:C400</c> numerator.
    /// </summary>
    private void BeginTimeUp()
    {
        if (game.runtime is null)
            throw new InvalidOperationException("Time-up requires a gameplay runtime.");
        var white = new Bgr555[CgramPalettes.ColorCount];
        Array.Fill(white, TimeUpRomData.WhiteColor);
        game.timeUpPaletteFade = new CartridgePaletteTransition(white, TimeUpRomData.WhiteOutDenominator,
            game.runtime.Enemies.Presentation.GradualColorChange);
        game.GameState = SuperMetroidGameState.TimeUp;
    }

    /// <summary>Consumes a confirmed station save and writes the selected SRAM slot.</summary>
    private void HandleSaveStationPersistence()
    {
        if (game.runtime?.MessageBoxes.ConsumeSaveStationPersistenceRequest() is not { } request)
            return;
        SamusState samus = game.runtime.Player.Samus
            ?? throw new InvalidOperationException(
                "A confirmed save station has no live Samus state.");
        game.saveRam.SaveSlot(
            game.selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, game.runtime.System, new SaveSnapshotContext { Area = (ushort)request.AreaIndex, SaveStation = request.StationIndex, GameTime = game.runtime.GameTime, ControllerBindings = game.runtime.Options.ControllerBindings, MoonwalkEnabled = game.runtime.Options.MoonwalkEnabled, IconCancelEnabled = game.runtime.Options.IconCancelEnabled, JapaneseText = game.runtime.JapaneseText }));
        game.RaiseSaveRamChanged();
    }
}
