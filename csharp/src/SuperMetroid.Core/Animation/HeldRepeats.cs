namespace SuperMetroid.Core.Animation;

/// <summary>A repeat counter held by value (#1600), for an owner that keeps its counter to itself.</summary>
/// <param name="repeats">The counter's starting value.</param>
internal struct HeldRepeats(WrappingCountdown repeats) : IAnimationRepeatCounter
{
    /// <inheritdoc />
    public WrappingCountdown Repeats { get; set; } = repeats;
}
