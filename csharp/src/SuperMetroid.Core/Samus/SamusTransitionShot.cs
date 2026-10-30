namespace SuperMetroid.Core.Samus;

/// <summary>
/// A shot a pose transition hands to the next update's weapon handler (#1470). Its presence admits the turning
/// weapon handler and forces a held charge to release; the shot then leaves in its own direction rather than the
/// direction of the pose Samus is in by then.
/// </summary>
public readonly record struct SamusTransitionShot
{
    /// <summary>Hands over a shot in <paramref name="direction"/>, a pose definition's shot direction, 0 through 255.</summary>
    public SamusTransitionShot(int direction) => Direction = Ensure.BetweenInclusive(0, byte.MaxValue, direction);

    /// <summary>The shot direction, as pose definitions number it, 0 through 255.</summary>
    public int Direction { get; }
}
