using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
[RoomLifetime]
internal sealed class PhantoonPresentation
{
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPresentation(EnemyFrameContext frameContext)
    {
        _frameContext = frameContext;
    }

    /// <summary>Fades his body toward the colours of his health band over <paramref name="steps"/> steps, one every second update.</summary>
    internal void AdvancePhantoonFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        int steps)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.BodyFade.Complete)
            return;

        // The fade's step is a typed fade toward his health band's colours (#1512, #2251).
        if (state.BodyFade.TakeStep(steps) is { } step)
            state.Colours.Fade(PhantoonBodyLook.Health(HealthBand(body)), step, steps);
        else
            state.Colours.CompleteFade();
    }

    /// <summary>Changes the background wave's amplitude toward its current phase target.</summary>
    internal static bool AdvancePhantoonWaveAmplitude(
        PhantoonWaveState wave,
        SignedSixteenSixteen change,
        SignedSixteenSixteen maximum)
    {
        if (wave.AmplitudeShrinking)
        {
            if ((wave.Amplitude - change).IsNegative)
            {
                wave.Amplitude = SignedSixteenSixteen.Zero;
                return true;
            }
            wave.Amplitude -= change;
            return false;
        }

        wave.Amplitude += change;
        if (!(wave.Amplitude - maximum).IsNegative)
            wave.Amplitude = maximum;
        return false;
    }

    /// <summary>
    /// Steps the phase timer one update and reports whether it has run out (#2253): a timer loaded with N runs out on its
    /// Nth step, one at zero on its first, and a timer that has run out stays at zero.
    /// </summary>
    internal static bool TickPhantoonPhaseTimer(PhantoonEnemyState state)
    {
        if (state.PhaseTimer > 0)
            state.PhaseTimer--;
        return state.PhaseTimer == 0;
    }

    /// <summary>The health band <paramref name="body"/>'s health shows (#2259).</summary>
    internal static PhantoonHealthBand HealthBand(RoomEnemyPlace body) => PhantoonHealthBand.Of(body.Health);
}
