namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// How far a colour fade on the shared step count has come by its own steps (#2302, #2421): Kraid's backdrop fades, the
/// Torizos' body fades, a door's fades, the death fade and the time-up whiteout (<see cref="GradualFadeSteps"/>). A fade's
/// first step shows the colours it starts from and each later step moves one more of its parts; one that has come the whole way holds its goal. The shared counter (<see cref="SharedFadeStepCount"/>) decides
/// only when the fade completes, which gameplay sees, so a fade that starts on a counter another fade advanced, or runs
/// beside a door's fade, completes sooner and shows its goal from that step, without the counter's values changing how
/// far its colours have come.
/// </summary>
internal readonly record struct StepCountedFadeProgress
{
    /// <summary>Holds a fade that has moved <paramref name="moved"/> of its <paramref name="length"/> parts.</summary>
    private StepCountedFadeProgress(int moved, int length)
    {
        Length = Ensure.BetweenInclusive(1, int.MaxValue, length);
        Moved = Ensure.BetweenInclusive(0, Length, moved);
    }

    /// <summary>The parts of the way the fade has come.</summary>
    internal int Moved { get; }

    /// <summary>The parts of the whole way.</summary>
    internal int Length { get; }

    /// <summary>Whether the fade has come the whole way.</summary>
    internal bool IsComplete => Moved == Length;

    /// <summary>A fade of <paramref name="length"/> parts after its first step, which shows the colours it starts from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The length is not one or more.</exception>
    internal static StepCountedFadeProgress Started(int length) => new(0, length);

    /// <summary>A fade of <paramref name="length"/> parts that has come the whole way: it shows its goal.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The length is not one or more.</exception>
    internal static StepCountedFadeProgress Complete(int length) => new(length, length);

    /// <summary>This fade after its next step: one more part of the way, or its goal still once it has come the whole way.</summary>
    internal StepCountedFadeProgress Then() => new(Math.Min(Moved + 1, Length), Length);

    /// <summary>
    /// The part of this fade a look shows when it started once the fade had moved <paramref name="moved"/> parts, such
    /// as a Torizo's belly glow's next step: what the fade has moved since, over the parts it had left. A look started
    /// once the fade had come the whole way shows its goal.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="moved"/> is not zero through the parts this fade has moved.</exception>
    internal StepCountedFadeProgress Since(int moved)
    {
        Ensure.BetweenInclusive(0, Moved, moved);
        int left = Length - moved;
        return left == 0 ? new(1, 1) : new(Moved - moved, left);
    }
}
