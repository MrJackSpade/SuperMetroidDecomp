using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The death-sequence phases, from the start of the sequence to the final black-out.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class DeathGamePhases(FrontendSession session)
{
    /// <summary>Runs one update of <see cref="SuperMetroidGameState.DeathSequenceStart"/>.</summary>
    internal void RunDeathSequenceStart(ControllerButton controllerInput)
    {
        // The death sequence's first state deliberately completes one final gameplay pass with the game clock held, then
        // begins fading the whole display to black, which Samus's own colours are spared (#1544).
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = GameplayFrameAudio.Gameplay(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        BeginDeathFade();
        SamusState dyingSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
        dyingSamus.SelectedHudItem = HudItem.Beam;
        dyingSamus.AutoCancelledItem = HudItem.Beam;
        dyingSamus.InvincibilityTimer = WrappingCountdown.Start(0);
        dyingSamus.KnockbackTimer = 0;
        dyingSamus.KnockbackActive = false;
        session.GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.DeathBlackOutSurroundings"/>.</summary>
    internal void RunDeathBlackOutSurroundings(ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = GameplayFrameAudio.Gameplay(session.GameplayRuntime) });
        bool fadeComplete = StepDeathFade();
        session.display.PublishGameplay(session.GameplayRuntime);
        if (fadeComplete)
        {
            // Death setup cancels all three sound libraries, then installs the death
            // music data and track through the ordinary delayed music queue.
            session.audio.QueueSound(SamusSound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueSound(EnemySound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueSound(RoomSound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Death));
            session.audio.ScheduleMusic(MusicRequest.SelectTrack(SamusDeathDefinitions.FanfareTrack), MusicQueueDelay.AtLeastShortest(14));
            session.GameState = SuperMetroidGameState.DeathWaitForMusic;
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.DeathWaitForMusic"/>.</summary>
    internal void RunDeathWaitForMusic(ControllerButton controllerInput)
    {
        session.GameplayRuntime.DeathSequence.DrawFatalSamusFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (!session.audio.HasQueuedMusic)
        {
            session.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait();
            session.GameState = SuperMetroidGameState.DeathPreFlashing;
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.DeathPreFlashing"/>, <see cref="SuperMetroidGameState.DeathFlashing"/>, <see cref="SuperMetroidGameState.DeathExplosionWhiteOut"/>.</summary>
    internal void RunDeathFlashing(ControllerButton controllerInput)
    {
        SamusDeathSequenceStepResult deathStep =
            session.GameplayRuntime.DeathSequence.StepDeathSequenceFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.Flashing)
            session.GameState = SuperMetroidGameState.DeathFlashing;
        else if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.SuitExplosion)
            session.GameState = SuperMetroidGameState.DeathExplosionWhiteOut;
        else if (deathStep.Completed)
        {
            session.DeathFade.Start(15, 1);
            session.GameState = SuperMetroidGameState.DeathFinalBlackOut;
        }
    }

    /// <summary>Runs one update of <see cref="SuperMetroidGameState.DeathFinalBlackOut"/>.</summary>
    internal void RunDeathFinalBlackOut(ControllerButton controllerInput)
    {
        session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        session.SetDisplayBrightness(session.DeathFade.Darken());
        if (session.DeathFade.Brightness == 0)
        {
            session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads the save file or
            // soft resets. Releasing the runtime leaves the frame clock with the
            // frontend, which keeps counting it, and the session's random generator, through the menu.
            session.ReleaseRuntime();
            session.GameState = SuperMetroidGameState.GameOverMenu;
        }
    }

    /// <summary>
    /// Begins the death fade: the whole gameplay display but Samus fades to black over six steps of the shared gradual
    /// colour change (#1512), so her suit stays lit as the game keeps it.
    /// </summary>
    private void BeginDeathFade()
    {
        SuperMetroidRuntime runtime = session.Runtime ?? throw new InvalidOperationException("The death fade requires a runtime.");
        runtime.FadeOut.Begin(RgbColor.Black, SamusDeathDefinitions.FadeDenominator, GameplayFadeSpares.Samus);
    }

    /// <summary>
    /// Takes one step of the death fade. Each step moves the display one step of six further toward black, including its
    /// initial step that changes nothing and the separate call that completes the fade once its last step reaches black.
    /// </summary>
    /// <returns>Whether the fade completed on this call.</returns>
    private bool StepDeathFade()
    {
        return (session.Runtime ?? throw new InvalidOperationException("The death fade requires a runtime.")).FadeOut.Step();
    }
}
