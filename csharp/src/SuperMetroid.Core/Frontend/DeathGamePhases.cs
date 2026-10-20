using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Frontend;

/// <summary>The death-sequence phases, from the start of the sequence to the final black-out.</summary>
/// <param name="session">The frontend session these phases run against.</param>
internal sealed class DeathGamePhases(FrontendSession session)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathSequenceStart"/>.</summary>
    internal void DispatchDeathSequenceStart(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $13 deliberately completes one final state-eight pass under the
        // global freeze word, then snapshots the visible palette and makes every
        // target row black except Samus's sixteen-color suit row.
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        session.display.PublishGameplay(session.GameplayRuntime);
        PrepareDeathPaletteFade();
        SamusState dyingSamus = session.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
        dyingSamus.SelectedHudItem = HudItem.Beam;
        dyingSamus.AutoCancelHudItemIndex = HudItem.Beam;
        dyingSamus.InvincibilityTimer = NativeCountdown.Start(0);
        dyingSamus.KnockbackTimer = NativeCountdown.Start(0);
        dyingSamus.KnockbackActive = false;
        session.GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathBlackOutSurroundings"/>.</summary>
    internal void DispatchDeathBlackOutSurroundings(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(session.GameplayRuntime) });
        bool paletteBlackoutComplete = StepDeathPaletteFade();
        session.display.PublishGameplay(session.GameplayRuntime);
        if (paletteBlackoutComplete)
        {
            // `$82:DCE0` cancels all three SFX libraries, then installs the death
            // music data and track through the ordinary delayed music queue.
            session.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
            session.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
            session.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            session.audio.QueueMusicDelayed8(MusicCommand.Stop);
            session.audio.QueueMusicDelayed8(MusicCommand.LoadData(SamusDeathRomData.Music.DataIndex));
            session.audio.QueueMusicDelayed(MusicCommand.SelectTrack(SamusDeathRomData.Music.TrackIndex), MusicCommandDelay.FromDelayedYArgument(14));
            session.GameState = SuperMetroidGameState.DeathWaitForMusic;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathWaitForMusic"/>.</summary>
    internal void DispatchDeathWaitForMusic(SnesButton controllerInput)
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
    internal void DispatchDeathFlashing(SnesButton controllerInput)
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
    internal void DispatchDeathFinalBlackOut(SnesButton controllerInput)
    {
        session.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        session.display.PublishGameplay(session.GameplayRuntime);
        session.ApplyDisplayBrightness(session.DeathFade.Darken());
        if (session.DeathFade.Brightness == 0)
        {
            session.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads SRAM or
            // soft resets. Releasing the runtime hands the accepted-NMI counters to the
            // frontend, which keeps counting them, and the session's RNG word, through the menu.
            session.ReleaseRuntime();
            session.GameState = SuperMetroidGameState.GameOverMenu;
        }
    }

    /// <summary>Builds state $13's target-palettes image and resets <c>$7E:C400</c>.</summary>
    private void PrepareDeathPaletteFade()
    {
        if (session.Runtime is null)
            throw new InvalidOperationException("Death palette setup requires a runtime.");

        var target = new Bgr555[CgramPalettes.ColorCount];
        ReadOnlySpan<Bgr555> current = new CgramPalettes(session.Runtime.Cgram).Image;
        // `$82:DCA3-$DCB3` restores only palette-buffer colors $C0-$CF into the otherwise
        // black target. Suitless row $F0 and all room/OBJ rows therefore fade away.
        current.Slice(192, 16).CopyTo(target.AsSpan(192, 16));
        session.BeginDeathPaletteFade(new CartridgePaletteTransition(target, denominator: 6, session.Runtime.EnemySession.GradualColorChange));
    }

    /// <summary>
    /// Executes <c>AdvancePaletteFadeForAllPalettes</c> with denominator six. The routine
    /// interpolates from the already-updated current color each call, including its initial
    /// no-op step zero and separate completion call after step seven reaches the target.
    /// </summary>
    private bool StepDeathPaletteFade()
    {
        if (session.Runtime is null)
            throw new InvalidOperationException("Death palette fade requires a runtime.");
        return (session.DeathPaletteFade ?? throw new InvalidOperationException(
            "Death palette target was not prepared.")).Step(session.Runtime.Cgram);
    }
}
