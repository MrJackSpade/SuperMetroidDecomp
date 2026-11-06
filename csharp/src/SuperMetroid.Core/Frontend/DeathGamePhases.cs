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
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathSequenceStart"/>.</summary>
    internal void DispatchDeathSequenceStart(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $13 deliberately completes one final state-eight pass under the
        // global freeze word, then snapshots the visible palette and makes every
        // target colour set black except Samus's sixteen-color suit set.
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        PrepareDeathPaletteFade();
        SamusState dyingSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
        dyingSamus.SelectedHudItem = HudItem.Beam;
        dyingSamus.AutoCancelHudItemIndex = HudItem.Beam;
        dyingSamus.InvincibilityTimer = WrappingCountdown.Start(0);
        dyingSamus.KnockbackTimer = 0;
        dyingSamus.KnockbackActive = false;
        session.GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathBlackOutSurroundings"/>.</summary>
    internal void DispatchDeathBlackOutSurroundings(ControllerButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        bool paletteBlackoutComplete = StepDeathPaletteFade();
        session.display.PublishGameplay(session.GameplayRuntime);
        if (paletteBlackoutComplete)
        {
            // Death setup cancels all three sound libraries, then installs the death
            // music data and track through the ordinary delayed music queue.
            session.audio.QueueSound(Library1Sound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueSound(Library2Sound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueSound(Library3Sound.Silence, priority: SoundRequestPriority.Essential);
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Death));
            session.audio.ScheduleMusic(MusicRequest.SelectTrack(SamusDeathDefinitions.FanfareTrack), MusicQueueDelay.AtLeastShortest(14));
            session.GameState = SuperMetroidGameState.DeathWaitForMusic;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathWaitForMusic"/>.</summary>
    internal void DispatchDeathWaitForMusic(ControllerButton controllerInput)
    {
        session.GameplayRuntime.DeathSequence.DrawFatalSamusFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (!session.audio.HasQueuedMusic)
        {
            session.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait();
            session.GameState = SuperMetroidGameState.DeathPreFlashing;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathPreFlashing"/>, <see cref="SuperMetroidGameState.DeathFlashing"/>, <see cref="SuperMetroidGameState.DeathExplosionWhiteOut"/>.</summary>
    internal void DispatchDeathFlashing(ControllerButton controllerInput)
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

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathFinalBlackOut"/>.</summary>
    internal void DispatchDeathFinalBlackOut(ControllerButton controllerInput)
    {
        session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        session.SetDisplayBrightness(session.DeathFade.Darken());
        if (session.DeathFade.Brightness == 0)
        {
            session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads SRAM or
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
    private void PrepareDeathPaletteFade()
    {
        SuperMetroidRuntime runtime = session.Runtime ?? throw new InvalidOperationException("Death palette setup requires a runtime.");
        runtime.FadeOut.Begin(RgbColor.Black, denominator: 6, runtime.EnemySession.GradualColorChange, GameplayFadeSpares.Samus);
    }

    /// <summary>
    /// Executes <c>AdvancePaletteFadeForAllPalettes</c> with denominator six. The routine
    /// interpolates from the already-updated current color each call, including its initial
    /// no-op step zero and separate completion call after step seven reaches the target.
    /// </summary>
    private bool StepDeathPaletteFade()
    {
        return (session.Runtime ?? throw new InvalidOperationException("Death palette fade requires a runtime.")).FadeOut.Step();
    }
}
