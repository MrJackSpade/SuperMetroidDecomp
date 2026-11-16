using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One fade on the shared step count (#1512, #2289, <see cref="SharedFadeStepCount"/>): the door fades, the death fade and
/// the time-up whiteout take a step each update, reading the shared counter and advancing it, and the fade that completes
/// resets it under any other running in the same updates. When the fade completes is what gameplay sees; what its steps
/// show is a typed fade whose progress is the steps taken, each of the counter's values from the fade's first moving one
/// through the one past its length an equal part of the way. The counter's value zero moves nothing, so a fade started on
/// a counter another fade advanced has fewer, larger parts.
/// </summary>
internal sealed class GradualFadeSteps
{
    /// <summary>The shared step counter this fade reads and advances.</summary>
    private readonly SharedFadeStepCount counter;

    /// <summary>The counter's value at the fade's first step, or null before it takes one.</summary>
    private int? first;

    /// <summary>The counter's value at the fade's latest step, or null before it takes one.</summary>
    private int? latest;

    /// <summary>Starts a fade of length <paramref name="denominator"/> on <paramref name="counter"/>.</summary>
    /// <param name="denominator">
    /// The fade's length, one step or more (#1627); step denominator plus one reaches the far colour, so that step is
    /// still a count.
    /// </param>
    /// <param name="counter">The shared step counter.</param>
    internal GradualFadeSteps(int denominator, SharedFadeStepCount counter)
    {
        this.counter = Ensure.NotNull(counter);
        Denominator = Ensure.BetweenInclusive(1, LongestDenominator, denominator);
    }

    /// <summary>
    /// The longest fade (#1627): any length whose step after the last, the one that reaches the far colour, is still a
    /// count of steps.
    /// </summary>
    internal const int LongestDenominator = int.MaxValue - 1;

    /// <summary>The fade's length.</summary>
    internal int Denominator { get; }

    /// <summary>The counter's value whose step reaches the far colour: the one past the fade's length.</summary>
    private int LastMoving => Denominator + 1;

    /// <summary>The counter's first value that moves this fade: its first step's, or one when that step was zero's.</summary>
    private int FirstMoving => Math.Max(first ?? 1, 1);

    /// <summary>The parts of the way the fade has come: none before its first moving step.</summary>
    internal int Progress => latest is { } last ? Math.Max(0, last - FirstMoving + 1) : 0;

    /// <summary>The parts of the whole way: its moving values from its first through the one past its length.</summary>
    internal int Length => LastMoving - FirstMoving + 1;

    /// <summary>
    /// Takes one step: the first (counter zero) changes nothing, step denominator plus one reaches the far colour, and the
    /// call after it completes the fade and resets the shared counter.
    /// </summary>
    /// <returns>Whether the fade completed on this call, taking no step.</returns>
    internal bool Step()
    {
        int step = counter.Step;
        if (step > LastMoving)
        {
            counter.Reset();
            return true;
        }
        first ??= step;
        latest = step;
        counter.Advance();
        return false;
    }

    /// <summary>What the steps taken show of a colour fading toward <paramref name="goal"/>.</summary>
    internal ColourFade Toward(RgbColor goal) => new(goal, GradualFadeDirection.Toward, Progress, Length);

    /// <summary>What the steps taken show of a colour fading in from <paramref name="start"/>.</summary>
    internal ColourFade From(RgbColor start) => new(start, GradualFadeDirection.From, Progress, Length);

    /// <summary>
    /// The fade from <paramref name="from"/> to <paramref name="to"/> a picture shows as far as the steps taken (#2289): the
    /// HUD's and the escape timer's door shades, which keep their lit colours.
    /// </summary>
    internal PictureShadeFade Between(PictureShading from, PictureShading to) => new(from, to, Progress, Length);
}
