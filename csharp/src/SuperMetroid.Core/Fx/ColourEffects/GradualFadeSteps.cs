using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// One fade on the shared step count (#1512, #2289, #2421, <see cref="SharedFadeStepCount"/>): the door fades, the death fade
/// and the time-up whiteout take a step each update, reading the shared counter and advancing it, and the fade that
/// completes resets it under any other running in the same updates. When the fade completes is what gameplay sees, and the
/// shared counter alone decides it. What its steps show is a typed fade whose progress is the fade's own steps
/// (<see cref="StepCountedFadeProgress"/>), as the bosses' fades count theirs: its first step shows the colours it starts
/// from, each later step moves one more of its parts, and once it completes it shows its goal. A fade started on a counter
/// another fade advanced completes sooner and shows its goal from the step it completes on.
/// </summary>
internal sealed partial class GradualFadeSteps
{
    /// <summary>The shared step counter this fade reads and advances.</summary>
    private readonly SharedFadeStepCount counter;

    /// <summary>How far the fade has come by its own steps, or null before it takes one.</summary>
    private StepCountedFadeProgress? progress;

    /// <summary>Starts a fade on <paramref name="counter"/> whose colours move in <paramref name="lastStep"/> parts.</summary>
    /// <param name="lastStep">
    /// The counter's last value at which a step moves the fade's colours, one or more (#1627): the step there reaches the
    /// colour faded toward, and the step after completes the fade. The colours move in this many parts, one for each of the
    /// fade's own steps after its first.
    /// </param>
    /// <param name="counter">The shared step counter.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="lastStep"/> is below one.</exception>
    internal GradualFadeSteps(int lastStep, SharedFadeStepCount counter)
    {
        this.counter = Ensure.NotNull(counter);
        ArgumentOutOfRangeException.ThrowIfLessThan(lastStep, 1);
        LastStep = lastStep;
    }

    /// <summary>The counter's last value at which a step moves the fade's colours; the step after completes it.</summary>
    internal int LastStep { get; }

    /// <summary>What the fade shows: its own steps' progress, or the colours it starts from before its first step.</summary>
    private StepCountedFadeProgress Shown => progress ?? StepCountedFadeProgress.Started(LastStep);

    /// <summary>
    /// Takes one step: while the counter stands at zero through <see cref="LastStep"/> the fade takes its next step and
    /// advances the counter, and once the counter has passed them the step completes the fade, which shows its goal from
    /// now on, and resets the counter.
    /// </summary>
    /// <returns>Whether the fade completed on this update, taking no step.</returns>
    internal bool Step()
    {
        if (counter.Step > LastStep)
        {
            counter.Reset();
            progress = StepCountedFadeProgress.Complete(LastStep);
            return true;
        }
        progress = progress is { } running ? running.Then() : StepCountedFadeProgress.Started(LastStep);
        counter.Advance();
        return false;
    }

    /// <summary>
    /// How far the steps taken have come, in the cartridge's gradual colour change (#3036): the fades on the shared counter
    /// all run <c>Advance_GradualColorChange_ofAllPalettes</c>.
    /// </summary>
    private FadeLevel Level => new(Shown.Moved, Shown.Length) { Arithmetic = FadeArithmetic.GradualColourChange };

    /// <summary>What the steps taken show of a colour fading toward <paramref name="goal"/>.</summary>
    internal ColourFade Toward(RgbColor goal) => new(goal, GradualFadeDirection.Toward, Level);

    /// <summary>What the steps taken show of a colour fading in from <paramref name="start"/>.</summary>
    internal ColourFade From(RgbColor start) => new(start, GradualFadeDirection.From, Level);

    /// <summary>
    /// The fade from <paramref name="from"/> to <paramref name="to"/> a picture shows as far as the steps taken (#2289): the
    /// HUD's and the escape timer's door shades, which keep their lit colours.
    /// </summary>
    internal PictureShadeFade Between(PictureShading from, PictureShading to) => new(from, to, Level);
}
