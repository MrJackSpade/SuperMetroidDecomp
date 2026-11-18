using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Samus's death, from the final gameplay pass to the final black-out before the game-over menu.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class DeathGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.DeathSequence;

    /// <inheritdoc/>
    /// <remarks>
    /// The steps that draw Samus advance the generator through gameplay's own update or not at all; the final black-out
    /// only fades, and the generator advances before it.
    /// </remarks>
    public PhaseRandomStep RandomStep => session.DeathStep switch
    {
        DeathSequenceStep.FinalGameplayPass or DeathSequenceStep.BlackingOutSurroundings or DeathSequenceStep.WaitingForMusic or
            DeathSequenceStep.SamusDying => PhaseRandomStep.RuntimeOwned,
        DeathSequenceStep.FinalBlackOut => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}."),
    };

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.DeathStep switch
    {
        DeathSequenceStep.FinalGameplayPass => "Death: final gameplay pass",
        DeathSequenceStep.BlackingOutSurroundings => "Death: surroundings black out",
        DeathSequenceStep.WaitingForMusic => "Death: waiting for music",
        DeathSequenceStep.SamusDying => $"Death: {session.Runtime?.Player.Samus?.DeathSequence.Phase}",
        DeathSequenceStep.FinalBlackOut => "Death: final black-out",
        _ => throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}."),
    };

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.DeathStep)
        {
            case DeathSequenceStep.FinalGameplayPass:
                RunFinalGameplayPass(controllerInput);
                break;
            case DeathSequenceStep.BlackingOutSurroundings:
                BlackOutSurroundings(controllerInput);
                break;
            case DeathSequenceStep.WaitingForMusic:
                WaitForMusic(controllerInput);
                break;
            case DeathSequenceStep.SamusDying:
                StepSamusDying(controllerInput);
                break;
            case DeathSequenceStep.FinalBlackOut:
                FinalBlackOut(controllerInput);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DeathSequenceStep)} {session.DeathStep}.");
        }
    }

    /// <summary>The final frozen gameplay pass, which prepares Samus and begins the surroundings' fade.</summary>
    private void RunFinalGameplayPass(ControllerButton controllerInput)
    {
        // The death sequence deliberately completes one final gameplay pass with the game clock held, then
        // begins fading the whole display to black, which Samus's own colours are spared (#1544).
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false, RunsLowHealthCheck = true } });
        session.display.PublishGameplay(session.GameplayRuntime);
        BeginDeathFade();
        SamusState dyingSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
        dyingSamus.SelectedHudItem = HudItem.Beam;
        dyingSamus.AutoCancelledItem = HudItem.Beam;
        dyingSamus.InvincibilityTimer = WrappingCountdown.Start(0);
        dyingSamus.KnockbackTimer = 0;
        dyingSamus.KnockbackActive = false;
        session.Enter(DeathSequenceStep.BlackingOutSurroundings);
    }

    /// <summary>One update of the surroundings fading to black, which queues the death music once they are black.</summary>
    private void BlackOutSurroundings(ControllerButton controllerInput)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false, RunsLowHealthCheck = true } });
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
            session.audio.ScheduleMusic(MusicRequest.SelectTrack(SamusDeathDefinitions.FanfareTrack), SamusDeathDefinitions.FanfareDelay);
            session.Enter(DeathSequenceStep.WaitingForMusic);
        }
    }

    /// <summary>One update of the fatal pose, which begins Samus's death animation once the death music is processed.</summary>
    private void WaitForMusic(ControllerButton controllerInput)
    {
        session.GameplayRuntime.DeathSequence.DrawFatalSamusFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (!session.audio.HasQueuedMusic)
        {
            session.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait(session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
            session.Enter(DeathSequenceStep.SamusDying);
        }
    }

    /// <summary>One update of Samus's death animation, which begins the final black-out once her death sequence completes.</summary>
    private void StepSamusDying(ControllerButton controllerInput)
    {
        SamusDeathSequenceStepResult deathStep =
            session.GameplayRuntime.DeathSequence.StepDeathSequenceFrame(controllerInput, session.GameplayRuntime.Room.LoadedScope, session.GameplayRuntime.Player.IntroducedSamus);
        session.display.PublishGameplay(session.GameplayRuntime);
        if (deathStep.Completed)
        {
            session.DeathFade.Restart(DisplayBrightness.Full, 1);
            session.Enter(DeathSequenceStep.FinalBlackOut);
        }
    }

    /// <summary>One update of the final fade to black, which releases the game and opens the game-over menu once black.</summary>
    private void FinalBlackOut(ControllerButton controllerInput)
    {
        session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        bool black = session.DeathFade.Darken();
        session.SetDisplayBrightness(session.DeathFade.Level);
        if (black)
        {
            session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads the save file or
            // soft resets. Releasing the runtime leaves the frame clock with the
            // frontend, which keeps counting it, and the session's random generator, through the menu.
            session.ReleaseRuntime();
            session.Enter(SuperMetroidGameState.GameOverMenu);
        }
    }

    /// <summary>
    /// Begins the death fade: the whole gameplay display but Samus fades to black on the shared gradual colour change
    /// (#1512, <see cref="SamusDeathDefinitions.FadeLastStep"/>), so her suit stays lit as the game keeps it.
    /// </summary>
    private void BeginDeathFade()
    {
        SuperMetroidRuntime runtime = session.Runtime ?? throw new InvalidOperationException("The death fade requires a runtime.");
        runtime.FadeOut.Begin(RgbColor.Black, SamusDeathDefinitions.FadeLastStep, GameplayFadeSpares.Samus);
    }

    /// <summary>
    /// Takes one step of the death fade. Each step after its first, which changes nothing, moves the display one part
    /// further toward black, and the update after the shared counter passes the fade's last step completes the fade.
    /// </summary>
    /// <returns>Whether the fade completed on this update.</returns>
    private bool StepDeathFade()
    {
        return (session.Runtime ?? throw new InvalidOperationException("The death fade requires a runtime.")).FadeOut.Step();
    }
}
