using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The steps of one gradual colour change on the shared step count (#1512, <see cref="SharedFadeStepCount"/>): the door fades, Kraid's backdrop fades and
/// the Torizos' body fades take a step each update, reading the shared counter and advancing it, and the fade that
/// completes resets it under any other running in the same updates. What the steps show is the
/// <see cref="GradualColourFade"/> they make; the steps decide only when the fade completes.
/// </summary>
internal sealed class GradualFadeSteps
{
    /// <summary>The shared step counter this fade reads and advances.</summary>
    private readonly SharedFadeStepCount counter;

    /// <summary>Starts a fade of length <paramref name="denominator"/> on <paramref name="counter"/>.</summary>
    /// <param name="denominator">
    /// The fade's length, one step or more (#1627); step denominator plus one reaches the far colour, so that step is
    /// still a count.
    /// </param>
    /// <param name="counter">The shared step counter.</param>
    internal GradualFadeSteps(int denominator, SharedFadeStepCount counter)
    {
        this.counter = Ensure.NotNull(counter);
        Denominator = Ensure.BetweenInclusive(1, GradualColourFade.LongestDenominator, denominator);
    }

    /// <summary>The fade's length.</summary>
    internal int Denominator { get; }

    /// <summary>The counter's value at each step taken, in order.</summary>
    internal ImmutableArray<int> Taken { get; private set; } = [];

    /// <summary>
    /// Takes one step: the first (counter zero) changes nothing, step denominator plus one reaches the far colour, and the
    /// call after it completes the fade and resets the shared counter.
    /// </summary>
    /// <returns>Whether the fade completed on this call, taking no step.</returns>
    internal bool Step()
    {
        int step = counter.Step;
        if (step > Denominator + 1)
        {
            counter.Reset();
            return true;
        }
        Taken = Taken.Add(step);
        counter.Advance();
        return false;
    }

    /// <summary>What the steps taken show of a colour fading toward <paramref name="goal"/>.</summary>
    internal GradualColourFade Toward(RgbColor goal) => new(goal, GradualFadeDirection.Toward, Denominator, Taken);

    /// <summary>What the steps taken show of a colour fading in from <paramref name="start"/>.</summary>
    internal GradualColourFade From(RgbColor start) => new(start, GradualFadeDirection.From, Denominator, Taken);
}
