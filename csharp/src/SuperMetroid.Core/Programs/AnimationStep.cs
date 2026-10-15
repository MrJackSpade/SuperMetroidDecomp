namespace SuperMetroid.Core.Programs;

/// <summary>One step of a typed animation timeline.</summary>
/// <typeparam name="TFrame">The presentation the step shows.</typeparam>
/// <typeparam name="TCue">The gameplay cue the step publishes.</typeparam>
internal readonly record struct AnimationStep<TFrame, TCue>(AnimationStepKind Kind, TFrame Frame, int Duration, TCue? Cue)
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The cue this <see cref="AnimationStepKind.Cue"/> step publishes; every cue step is built with one.</summary>
    internal TCue PublishedCue => Cue ?? throw new InvalidOperationException("A cue step has no cue to publish.");

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates.</summary>
    internal static AnimationStep<TFrame, TCue> Show(TFrame frame, int duration) =>
        new(AnimationStepKind.Show, frame, Ensure.BetweenInclusive(1, ushort.MaxValue, duration), default);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationStep<TFrame, TCue> Publish(TCue cue) => new(AnimationStepKind.Cue, default, 0, cue);

    /// <summary>Holds the current frame for good.</summary>
    internal static AnimationStep<TFrame, TCue> Hold { get; } = new(AnimationStepKind.Hold, default, 0, default);

    /// <summary>Restarts from the first step.</summary>
    internal static AnimationStep<TFrame, TCue> Loop { get; } = new(AnimationStepKind.Loop, default, 0, default);
}
