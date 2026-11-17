namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// How far a boss's colour fade on the shared step count has come by its own steps (#2302): Kraid's backdrop fades and the
/// Torizos' body fades. A fade's first step shows the colours it starts from and each later step moves one more of its
/// parts; one that has come the whole way holds its goal. The shared counter (<see cref="SharedFadeStepCount"/>) decides
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

    /// <summary>This fade after its next step: one more part of the way, or its goal still once it has come the whole way.</summary>
    internal StepCountedFadeProgress Then() => new(Math.Min(Moved + 1, Length), Length);

    /// <summary>
    /// The part of this fade run since it had moved <paramref name="moved"/> parts: what it has moved of colours written
    /// then, such as a Torizo's belly glow's next step, over the parts it had left. Colours written once the fade had come
    /// the whole way show its goal.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="moved"/> is not zero through the parts this fade has moved.</exception>
    internal StepCountedFadeProgress Since(int moved)
    {
        Ensure.BetweenInclusive(0, Moved, moved);
        int left = Length - moved;
        return left == 0 ? new(1, 1) : new(Moved - moved, left);
    }
}
