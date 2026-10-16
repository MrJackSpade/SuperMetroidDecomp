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

/// <summary>The death-sequence phases, from the start of the sequence to the final black-out.</summary>
/// <param name="game">The SuperMetroidGame whose shared state these members use.</param>
internal sealed class DeathGamePhases(SuperMetroidGame game)
{
    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathSequenceStart"/>.</summary>
    internal void DispatchDeathSequenceStart(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        // State $13 deliberately completes one final state-eight pass under the
        // global freeze word, then snapshots the visible palette and makes every
        // target row black except Samus's sixteen-color suit row.
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        game.display.PublishGameplay(game.GameplayRuntime);
        PrepareDeathPaletteFade();
        SamusState dyingSamus = game.GameplayRuntime.Player.Samus
            ?? throw new InvalidOperationException("Death sequence lost its Samus owner.");
        dyingSamus.SelectedHudItem = 0;
        dyingSamus.AutoCancelHudItemIndex = 0;
        dyingSamus.InvincibilityTimer = NativeCountdown.Start(0);
        dyingSamus.KnockbackTimer = NativeCountdown.Start(0);
        dyingSamus.KnockbackActive = false;
        game.GameState = SuperMetroidGameState.DeathBlackOutSurroundings;
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathBlackOutSurroundings"/>.</summary>
    internal void DispatchDeathBlackOutSurroundings(SnesButton controllerInput, GameplayAudioFramePublication gameplayAudio)
    {
        game.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AdvanceGameTime = false }, Audio = gameplayAudio.GameplayFrameAudio(game.GameplayRuntime) });
        bool paletteBlackoutComplete = StepDeathPaletteFade();
        game.display.PublishGameplay(game.GameplayRuntime);
        if (paletteBlackoutComplete)
        {
            // `$82:DCE0` cancels all three SFX libraries, then installs the death
            // music data and track through the ordinary delayed music queue.
            game.audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
            game.audio.QueueMusicDelayed8(MusicCommand.Stop);
            game.audio.QueueMusicDelayed8(MusicCommand.LoadData(SamusDeathRomData.Music.DataIndex));
            game.audio.QueueMusicDelayed(MusicCommand.SelectTrack(SamusDeathRomData.Music.TrackIndex), MusicCommandDelay.FromDelayedYArgument(14));
            game.GameState = SuperMetroidGameState.DeathWaitForMusic;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathWaitForMusic"/>.</summary>
    internal void DispatchDeathWaitForMusic(SnesButton controllerInput)
    {
        game.GameplayRuntime.DeathSequence.DrawFatalSamusFrame(controllerInput);
        game.display.PublishGameplay(game.GameplayRuntime);
        if (!game.audio.HasQueuedMusic)
        {
            game.GameplayRuntime.DeathSequence.BeginDeathSequenceAfterMusicWait();
            game.GameState = SuperMetroidGameState.DeathPreFlashing;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathPreFlashing"/>, <see cref="SuperMetroidGameState.DeathFlashing"/>, <see cref="SuperMetroidGameState.DeathExplosionWhiteOut"/>.</summary>
    internal void DispatchDeathFlashing(SnesButton controllerInput)
    {
        SamusDeathSequenceStepResult deathStep =
            game.GameplayRuntime.DeathSequence.StepDeathSequenceFrame(controllerInput);
        game.display.PublishGameplay(game.GameplayRuntime);
        if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.Flashing)
            game.GameState = SuperMetroidGameState.DeathFlashing;
        else if (deathStep.PhaseAfterStep == SamusDeathSequencePhase.SuitExplosion)
            game.GameState = SuperMetroidGameState.DeathExplosionWhiteOut;
        else if (deathStep.Completed)
        {
            game.deathFadeBrightness = 15;
            game.deathFadeCounter = 1;
            game.GameState = SuperMetroidGameState.DeathFinalBlackOut;
        }
    }

    /// <summary>Runs one dispatch of <see cref="SuperMetroidGameState.DeathFinalBlackOut"/>.</summary>
    internal void DispatchDeathFinalBlackOut(SnesButton controllerInput)
    {
        game.GameplayRuntime.RunBlankGameplayFrame(controllerInput);
        game.display.PublishGameplay(game.GameplayRuntime);
        if (game.deathFadeCounter == 0)
        {
            game.deathFadeCounter = 1;
            if (game.deathFadeBrightness == 1)
                game.deathFadeBrightness = 0;
            else if (game.deathFadeBrightness != 0)
                game.deathFadeBrightness = NativeWordCounter.DecrementByte(game.deathFadeBrightness);
        }
        else
        {
            game.deathFadeCounter--;
        }
        game.ApplyDisplayBrightness(game.deathFadeBrightness);
        if (game.deathFadeBrightness == 0)
        {
            game.GameplayRuntime.Freeze.GameplayTimeFrozen = false;
            // The game-over menu runs no gameplay owner: every exit reloads SRAM or
            // soft resets. Releasing the runtime hands the live RNG and accepted-NMI
            // counters to the frontend, which keeps counting them through the menu.
            game.ReleaseRuntimePreservingRandom();
            game.GameState = SuperMetroidGameState.GameOverMenu;
        }
    }

    /// <summary>Builds state $13's target-palettes image and resets <c>$7E:C400</c>.</summary>
    private void PrepareDeathPaletteFade()
    {
        if (game.runtime is null)
            throw new InvalidOperationException("Death palette setup requires a runtime.");

        var target = new Bgr555[CgramPalettes.ColorCount];
        ReadOnlySpan<Bgr555> current = new CgramPalettes(game.runtime.Cgram).Image;
        // `$82:DCA3-$DCB3` restores only palette-buffer colors $C0-$CF into the otherwise
        // black target. Suitless row $F0 and all room/OBJ rows therefore fade away.
        current.Slice(192, 16).CopyTo(target.AsSpan(192, 16));
        game.deathPaletteFade = new CartridgePaletteTransition(target, denominator: 6, game.runtime.Enemies.Presentation.GradualColorChange);
    }

    /// <summary>
    /// Executes <c>AdvancePaletteFadeForAllPalettes</c> with denominator six. The routine
    /// interpolates from the already-updated current color each call, including its initial
    /// no-op step zero and separate completion call after step seven reaches the target.
    /// </summary>
    private bool StepDeathPaletteFade()
    {
        if (game.runtime is null)
            throw new InvalidOperationException("Death palette fade requires a runtime.");
        return (game.deathPaletteFade ?? throw new InvalidOperationException(
            "Death palette target was not prepared.")).Step(game.runtime.Cgram);
    }
}
