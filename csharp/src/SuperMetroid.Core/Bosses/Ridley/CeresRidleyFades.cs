using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's eye and body palette fades.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFades
{
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>Receives the battle music the finished body fade starts.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFades(EnemyPresentation presentation, BossArtworkBinding bossArtwork, EnemyAudioQueue audio)
    {
        _audio = audio;
        _bossArtwork = bossArtwork;
        _presentation = presentation;
    }

    /// <summary>Advances the eye-fade schedule one step on each AI call before enabling the body fade.</summary>
    internal void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The original INC/BNE sequence advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        state.FunctionTimer = state.FunctionTimer.AddFrames(1);
        if (state.FunctionTimer.IsZero)
            return;
        state.FunctionTimer = WrappingCountdown.Start(0);

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFade.Get(state.FadeStep);
        if (fadeStep.IsComplete)
        {
            state.FadeStep = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = true;
            return;
        }

        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley eye fade requires installed colors."))
            .ApplyEyeFade(_presentation.Colours, fadeStep.Shade);
        state.FadeStep++;
    }

    /// <summary>Reveals the body one shade per step, clears intangible collision and queues the battle track at the final shade.</summary>
    internal void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.AddFrames(1);
        if (state.FunctionTimer.RawValue < 2)
            return;
        state.FunctionTimer = WrappingCountdown.Start(0);

        // Each step of the body fade shows the next shade, from black to the full body colours.
        int shade = state.FadeStep;
        if (_bossArtwork.CeresRidleyColors is { } bodyColors &&
            shade < CeresRidleyPaletteDefinitions.BodyFadeShadeCount)
            bodyColors.ApplyBodyFade(_presentation.Colours, shade);
        else throw new InvalidOperationException(
            $"Ceres Ridley body-fade shade {shade} is outside the installed shades.");
        state.FadeStep++;
        if (state.FadeStep < CeresRidleyPaletteDefinitions.BodyFadeShadeCount)
            return;

        // CeresRidley_Func_5 clears tangible bit $0400 after the final shade, waits
        // five dispatcher calls, then begins the opening-roar list.
        slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.FadeStep = 0;
        state.Function = RidleyAiFunction.WaitBeforeRoar;
        state.FunctionTimer = WrappingCountdown.Start(4);
        // `$A6:A449` queues track five immediately after the final body shade.
        // This is the Ceres battle/escape track; it intentionally continues after Ridley
        // retreats, so omitting this single publication silences both scenes.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(5));
    }
}
