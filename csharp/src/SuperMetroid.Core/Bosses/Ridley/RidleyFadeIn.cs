using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's fade-in, which both Ridleys share: Ceres Ridley as he appears in the space station and Norfair Ridley as he
/// appears in his lair. His eyes fade in from the dark first, then his body one shade at a time; the final body shade
/// ends his ignoring Samus's touch, starts the wait before his opening roar and starts the Ridley and Draygon battle
/// music. Ceres Ridley's behaviour owns the one instance, and Norfair Ridley's behaviour runs the same fade through it.
/// </summary>
[RoomLifetime]
internal sealed class RidleyFadeIn
{
    /// <summary>Receives the battle music the finished body fade starts.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal RidleyFadeIn(EnemyAudioQueue audio)
    {
        _audio = audio;
    }

    /// <summary>
    /// Advances either Ridley's eye fade one step on each AI call, showing the step's eye shade, and starts his body fade
    /// once the eyes have finished.
    /// </summary>
    internal static void TickEyeFadeIn(RidleyEnemyState state)
    {
        // The game's increment-and-test advances once per AI call for every reachable
        // counter value, resetting that timer to zero immediately afterward.
        if (++state.PhaseTimer == 0)
            return;
        state.PhaseTimer = 0;

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFade.Get(state.FadeStep);
        if (fadeStep.IsComplete)
        {
            state.FadeStep = 0;
            state.Phase = RidleyPhase.FadeInBody;
            state.MovementAnimationEnabled = true;
            return;
        }

        state.Colours.ShowEyeFade(fadeStep.Shade);
        state.FadeStep++;
    }

    /// <summary>
    /// Reveals either Ridley's body one shade every second update; after the final shade he stops ignoring Samus, waits
    /// before his opening roar, and the battle music starts.
    /// </summary>
    internal void TickBodyFadeIn(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (++state.PhaseTimer < 2)
            return;
        state.PhaseTimer = 0;

        // Each step of the body fade shows the next shade, from black to the full body colours.
        int shade = state.FadeStep;
        if (shade < CeresRidleyShadeDefinitions.BodyFadeShadeCount)
            state.Colours.ShowBodyFade(shade);
        else throw new InvalidOperationException(
            $"Ridley body-fade shade {shade} is outside the installed shades.");
        state.FadeStep++;
        if (state.FadeStep < CeresRidleyShadeDefinitions.BodyFadeShadeCount)
            return;

        // After the final shade Ridley stops ignoring Samus, so the ordinary touch check meets his body again; he waits
        // five updates, then begins the opening-roar animation.
        slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
        state.FadeStep = 0;
        state.Phase = RidleyPhase.WaitBeforeRoar;
        state.PhaseTimer = 4;
        // The fade queues the battle music immediately after the final body shade. It
        // intentionally continues after Ridley retreats, so omitting this single publication
        // silences both scenes.
        _audio.RequestRidleyMusic(MusicRequest.SelectTrack(MusicTrack.RidleyAndDraygonBattle));
    }
}
