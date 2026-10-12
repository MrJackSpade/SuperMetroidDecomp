namespace SuperMetroid.Core.Game;

/// <summary>What one step of an <see cref="AnimationTimeline{TFrame, TCue}"/> does.</summary>
internal enum AnimationStepKind
{
    /// <summary>Shows a frame for a number of processing updates; the timeline yields until it expires.</summary>
    Show,
    /// <summary>Publishes a gameplay cue (sound, collision shape, attack timing) and continues at once.</summary>
    Cue,
    /// <summary>Keeps the last frame indefinitely: the timeline stays here and publishes nothing more.</summary>
    Hold,
    /// <summary>Continues from the first step: a looping animation.</summary>
    Loop,
}

/// <summary>One step of a typed animation timeline.</summary>
/// <typeparam name="TFrame">The presentation the step shows.</typeparam>
/// <typeparam name="TCue">The gameplay cue the step publishes.</typeparam>
internal readonly record struct AnimationStep<TFrame, TCue>(AnimationStepKind Kind, TFrame Frame, int Duration, TCue Cue)
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates.</summary>
    internal static AnimationStep<TFrame, TCue> Show(TFrame frame, int duration) =>
        new(AnimationStepKind.Show, frame, Ensure.BetweenInclusive(1, ushort.MaxValue, duration), default!);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationStep<TFrame, TCue> Publish(TCue cue) => new(AnimationStepKind.Cue, default!, 0, cue);

    /// <summary>Holds the current frame for good.</summary>
    internal static AnimationStep<TFrame, TCue> Hold { get; } = new(AnimationStepKind.Hold, default!, 0, default!);

    /// <summary>Restarts from the first step.</summary>
    internal static AnimationStep<TFrame, TCue> Loop { get; } = new(AnimationStepKind.Loop, default!, 0, default!);
}

/// <summary>The result of advancing a timeline: a newly shown frame, or a hold.</summary>
internal readonly record struct AnimationAdvance<TFrame>(bool Held, TFrame Frame, int Duration);

/// <summary>
/// A typed animation: frames with durations, gameplay cues, and a terminal hold or loop (#641).
/// Owners replace a native instruction list with one of these and keep the cartridge's timer
/// gate themselves, so a frame shown here lasts exactly as long as the native frame did.
/// </summary>
internal sealed class AnimationTimeline<TFrame, TCue>
{
    /// <summary>Stores steps.</summary>
    private readonly AnimationStep<TFrame, TCue>[] steps;

    /// <summary>Creates a timeline; it must end in a hold or loop and a loop must show at least one frame.</summary>
    internal AnimationTimeline(params AnimationStep<TFrame, TCue>[] steps)
    {
        Ensure.NotNull(steps);
        if (steps.Length == 0 || steps[^1].Kind is not (AnimationStepKind.Hold or AnimationStepKind.Loop))
            throw new ArgumentException("An animation timeline must end with a hold or a loop.", nameof(steps));
        if (steps.Take(steps.Length - 1).Any(step => step.Kind is AnimationStepKind.Hold or AnimationStepKind.Loop))
            throw new ArgumentException("Only the last step of an animation timeline may hold or loop.", nameof(steps));
        if (steps[^1].Kind == AnimationStepKind.Loop && !steps.Any(step => step.Kind == AnimationStepKind.Show))
            throw new ArgumentException("A looping animation timeline must show a frame.", nameof(steps));
        this.steps = steps;
    }

    /// <summary>
    /// Runs from <paramref name="position"/> until a frame is shown or the timeline holds, publishing
    /// each cue on the way. On return <paramref name="position"/> is the step to resume from.
    /// </summary>
    internal AnimationAdvance<TFrame> Advance(ref int position, Action<TCue> publish)
    {
        Ensure.NotNull(publish);
        if ((uint)position >= (uint)steps.Length)
            throw new InvalidOperationException($"Animation position {position} is outside its {steps.Length}-step timeline.");
        // A loop back to the start visits each step at most once more before it must show a frame.
        for (int visited = 0; visited <= steps.Length; visited++)
        {
            AnimationStep<TFrame, TCue> step = steps[position];
            switch (step.Kind)
            {
                case AnimationStepKind.Show:
                    position++;
                    return new(false, step.Frame, step.Duration);
                case AnimationStepKind.Cue:
                    publish(step.Cue);
                    position++;
                    break;
                case AnimationStepKind.Hold:
                    return new(true, default!, 0);
                case AnimationStepKind.Loop:
                    position = 0;
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(AnimationStepKind)} {step.Kind}.");
            }
        }
        throw new InvalidOperationException("An animation timeline looped without showing a frame.");
    }
}

/// <summary>The cue type of a timeline that publishes no gameplay cues: frames and flow only.</summary>
internal enum NoAnimationCue
{
}
