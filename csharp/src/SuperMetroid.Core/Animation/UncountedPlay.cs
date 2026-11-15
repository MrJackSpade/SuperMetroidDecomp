namespace SuperMetroid.Core.Animation;

/// <summary>
/// The repeat counter of a play whose animations count no phase: such an owner keeps no loop timer, so a play that reaches
/// a counted phase through it is an authoring error and stops the game at once.
/// </summary>
internal struct UncountedPlay : IAnimationRepeatCounter
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Always: the uncounted play reached a counted phase.</exception>
    public WrappingCountdown Repeats
    {
        readonly get => throw new InvalidOperationException("An uncounted animation play reached a counted phase.");
        set => throw new InvalidOperationException($"An uncounted animation play reached a counted phase, which loaded {value}.");
    }
}
