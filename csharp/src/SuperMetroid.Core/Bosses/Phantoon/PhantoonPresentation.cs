using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
[RoomLifetime]
internal sealed class PhantoonPresentation
{
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPresentation(EnemyFrameContext frameContext)
    {
        _frameContext = frameContext;
    }

    /// <summary>Blends the body palette toward its visible colors over the selected interval.</summary>
    internal void AdvancePhantoonFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        int denominator)
    {
        if (!PhantoonCadenceDefinitions.FadeStepEverySecondUpdate.IsDue(_frameContext.FrameCounter) || state.EyeState.FadeComplete)
            return;

        state.EyeState.FadeDenominator = denominator;
        int numerator = state.EyeState.FadeNumerator;
        if (denominator + 1 < numerator)
        {
            state.EyeState.FadeNumerator = 0;
            state.EyeState.FadeComplete = true;
            state.Colours.CompleteFade();
            return;
        }

        // The fade's step is the import's shade of it, toward his health band's colours (#1512).
        state.Colours.Fade(PhantoonBodyLook.Health(HealthBand(body)), denominator, numerator);
        state.EyeState.FadeNumerator = numerator + 1;
    }

    /// <summary>Changes the background wave's amplitude toward its current phase target.</summary>
    internal static bool AdvancePhantoonWaveAmplitude(
        PhantoonMouthState mouth,
        int delta,
        int maximum)
    {
        if (mouth.WaveAmplitudeShrinking)
        {
            if (mouth.WaveAmplitude < delta)
            {
                mouth.WaveAmplitude = 0;
                return true;
            }
            mouth.WaveAmplitude -= delta;
            return false;
        }

        mouth.WaveAmplitude += delta;
        if (mouth.WaveAmplitude >= maximum)
            mouth.WaveAmplitude = maximum;
        return false;
    }

    /// <summary>Counts the body function timer down and reports its expiry: reaching zero or running past it.</summary>
    internal static bool TickPhantoonFunctionTimer(PhantoonBodyState body) => --body.FunctionTimer <= 0;

    /// <summary>The health band <paramref name="body"/>'s health shows: zero through seven.</summary>
    internal static int HealthBand(RoomEnemySlot body) => Math.Min(7, Math.Max(0, (body.Health - 1) / 312));

    /// <summary>Restarts a part's program from its start with a one-tick timer.</summary>
    internal static void InstallPhantoonProgram(
        PhantoonEnemyState state,
        RoomEnemySlot slot,
        PhantoonPartProgram program)
    {
        slot.AnimationClock.StepOnNextUpdate();
        state.PartPlayback(slot) = PhantoonEnemyState.Starting(program);
    }
}
