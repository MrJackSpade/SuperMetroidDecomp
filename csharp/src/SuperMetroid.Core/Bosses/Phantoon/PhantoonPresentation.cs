using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
[RoomLifetime]
internal sealed class PhantoonPresentation
{
    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonPresentation(EnemyPresentation presentation, EnemyFrameContext frameContext, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _presentation = presentation;
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
            return;
        }

        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        for (int color = 0; color < 16; color++)
        {
            RgbColor current = _presentation.BossColours().PhantoonBody[color];
            RgbColor target = ReadPhantoonHealthColor(healthBand, color);
            _presentation.BossColours().PhantoonBody.Write(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        state.Parts[eye.SlotIndex].FadeNumerator = numerator + 1;
    }

    /// <summary>Interpolates one RGB5 color during Phantoon's fade using native channel rounding.</summary>
    /// <param name="numerator">The fade's step, 0 through 65535.</param>
    /// <param name="denominator">The fade's step count, 0 through 65535.</param>
    /// <param name="current">The colour now shown.</param>
    /// <param name="target">The colour the fade reaches.</param>
    internal static RgbColor CalculatePhantoonTransitionColor(
        int numerator,
        int denominator,
        RgbColor current,
        RgbColor target)
    {
        return current.Zip(target, (_, from, to) =>
            CalculatePhantoonTransitionComponent(numerator, denominator, from, to));
    }

    /// <summary>Interpolates one RGB5 channel during a palette transition.</summary>
    private static int CalculatePhantoonTransitionComponent(
        int numerator,
        int denominator,
        int current,
        int target)
    {
        if (numerator == 0)
            return current;
        if (numerator - 1 == denominator)
            return target;
        // $A7:DCF1 divides the absolute component delta in 8.8 fixed point,
        // then applies its sign before selecting the high byte. Integer division
        // of the signed delta both adds an extra fade step and rounds darkening
        // in the wrong direction. The native divisor is an eight-bit register; the steps left are 1 through the
        // denominator (at most 12), because the first and last steps return above, so it never wraps or reaches zero.
        int remainingSteps = denominator - numerator + 1;
        int quotient = EightEight.FromWhole(Math.Abs(target - current)) / remainingSteps;
        int signedStep = target < current ? -quotient : quotient;
        return UnsignedEightEight.Whole(CoordinateAxis.Wrap(EightEight.FromWhole(current) + signedStep));
    }

    /// <summary>Changes BG2 wave amplitude toward its current phase target.</summary>
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

    /// <summary>Resolves the selected visual color; health-band selection stays in AI.</summary>
    internal RgbColor ReadPhantoonHealthColor(int healthBand, int color) =>
        (_bossArtwork.Artwork?.PhantoonColors ?? throw new InvalidDataException(
            "Phantoon health palette requires installed artwork.")).ResolveHealth(healthBand, color);

    /// <summary>Restarts a part's program with a one-tick timer, as a native list-pointer store does.</summary>
    internal static void InstallPhantoonProgram(
        PhantoonEnemyState state,
        RoomEnemySlot slot,
        PhantoonPartProgram program)
    {
        slot.InstructionTimer = WrappingCountdown.Start(1);
        state.PartProgram(slot) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
    }
}
