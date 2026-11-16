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

    /// <summary>Blends the body colours toward its visible colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        int denominator)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.BodyFade.Complete)
            return;

        state.BodyFade.Denominator = denominator;
        int numerator = state.BodyFade.Numerator;
        if (denominator + 1 < numerator)
        {
            state.BodyFade.Numerator = 0;
            state.BodyFade.Complete = true;
            state.Colours.CompleteFade();
            return;
        }

        // The fade's step is the import's shade of it, toward his health band's colours (#1512).
        state.Colours.Fade(PhantoonBodyLook.Health(HealthBand(body)), denominator, numerator);
        state.BodyFade.Numerator = numerator + 1;
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

    /// <summary>Counts the body phase timer down and reports its expiry: reaching zero or running past it.</summary>
    internal static bool TickPhantoonPhaseTimer(PhantoonEnemyState state) => --state.PhaseTimer <= 0;

    /// <summary>The health band <paramref name="body"/>'s health shows: zero through seven.</summary>
    internal static int HealthBand(RoomEnemyPlace body) =>
        Math.Min(PhantoonColorDefinitions.HealthBandCount - 1, Math.Max(0, (body.Health - 1) / PhantoonColorDefinitions.HealthPerBand));
}
