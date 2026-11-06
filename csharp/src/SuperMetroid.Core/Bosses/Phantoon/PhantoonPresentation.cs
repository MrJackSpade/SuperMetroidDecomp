using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
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
        if ((_frameContext.FrameCounter & 1) != 0 || state.Parts[state.Eye.SlotIndex].FadeComplete)
            return;

        RoomEnemySlot eye = state.Eye;
        state.Parts[eye.SlotIndex].FadeDenominator = denominator;
        int numerator = state.Parts[eye.SlotIndex].FadeNumerator;
        if (denominator + 1 < numerator)
        {
            state.Parts[eye.SlotIndex].FadeNumerator = 0;
            state.Parts[eye.SlotIndex].FadeComplete = true;
            state.Colours.CompleteFade();
            return;
        }

        // The fade's step is the import's shade of it, toward his health band's colours (#1512).
        state.Colours.Fade(PhantoonBodyLook.Health(HealthBand(body)), denominator, numerator);
        state.Parts[eye.SlotIndex].FadeNumerator = numerator + 1;
    }

    /// <summary>Changes the background wave's amplitude toward its current phase target.</summary>
    internal static bool AdvancePhantoonWaveAmplitude(
        PhantoonPartState mouth,
        int delta,
        int maximum)
    {
        if (mouth.WaveAmplitudeShrinking)
        {
            int previous = mouth.WaveAmplitude;
            mouth.WaveAmplitude = WordWrap.Unsigned(mouth.WaveAmplitude - delta);
            if (previous < delta)
            {
                mouth.WaveAmplitude = 0;
                return true;
            }
            return false;
        }

        mouth.WaveAmplitude = WordWrap.Unsigned(mouth.WaveAmplitude + delta);
        if (mouth.WaveAmplitude >= maximum)
            mouth.WaveAmplitude = maximum;
        return false;
    }

    /// <summary>Counts the body function timer down and reports its expiry.</summary>
    internal static bool TickPhantoonFunctionTimer(PhantoonPartState body)
    {
        CountdownStep step = body.FunctionTimer.Decrement();
        body.FunctionTimer = step.Remaining;
        return step.IsZeroOrNegative;
    }

    /// <summary>The health band <paramref name="body"/>'s health shows: zero through seven.</summary>
    internal static int HealthBand(RoomEnemySlot body) => Math.Min(7, Math.Max(0, (body.Health - 1) / 312));

    /// <summary>Restarts a part's program with a one-tick timer, as a list-pointer store does.</summary>
    internal static void InstallPhantoonProgram(
        PhantoonEnemyState state,
        RoomEnemySlot slot,
        PhantoonPartProgram program)
    {
        slot.FrameHold = WrappingCountdown.Start(1);
        state.PartProgram(slot) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
    }
}
