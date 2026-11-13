using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's eye and body palette fades.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFades
{
    /// <summary>Receives the battle music the finished body fade starts.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFades(EnemyAudioQueue audio)
    {
        _audio = audio;
    }

    /// <summary>Advances the eye-fade schedule one step on each AI call before enabling the body fade.</summary>
    internal static void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The game's increment-and-test advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        if (++state.FunctionTimer == 0)
            return;
        state.FunctionTimer = 0;

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFade.Get(state.FadeStep);
        if (fadeStep.IsComplete)
        {
            state.FadeStep = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = true;
            return;
        }

        state.Colours.ShowEyeFade(fadeStep.Shade);
        state.FadeStep++;
    }

    /// <summary>Reveals the body one shade per step, clears intangible collision and queues the battle track at the final shade.</summary>
    internal void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (++state.FunctionTimer < 2)
            return;
        state.FunctionTimer = 0;

        // Each step of the body fade shows the next shade, from black to the full body colours.
        int shade = state.FadeStep;
        if (shade < CeresRidleyPaletteDefinitions.BodyFadeShadeCount)
            state.Colours.ShowBodyFade(shade);
        else throw new InvalidOperationException(
            $"Ceres Ridley body-fade shade {shade} is outside the installed shades.");
        state.FadeStep++;
        if (state.FadeStep < CeresRidleyPaletteDefinitions.BodyFadeShadeCount)
            return;

        // The final eye-fade step makes Ridley intangible after the final shade, waits
        // five dispatcher calls, then begins the opening-roar list.
        slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
        state.FadeStep = 0;
        state.Function = RidleyAiFunction.WaitBeforeRoar;
        state.FunctionTimer = 4;
        // The fade queues the set's first song immediately after the final body shade.
        // This is the Ceres battle/escape track; it intentionally continues after Ridley
        // retreats, so omitting this single publication silences both scenes.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.FirstSetSong));
    }
}
