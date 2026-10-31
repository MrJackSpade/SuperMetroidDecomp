namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette change numerator: the one step counter every bank-$82
/// gradual colour change advances (<c>$82:DA02</c> and its palette-subset variants). Door
/// fades, Kraid's room-background fades and Torizo's body fades all share it, so a fade
/// that completes resets the counter under any other fade running in the same frames.
/// </summary>
public sealed class GradualColorChangeCounter
{
    /// <summary>
    /// The current transition number: zero when a fade starts, then one more per completed step. Every fade resets it
    /// once it passes the fade's denominator plus one, so it never exceeds the largest denominator plus two.
    /// </summary>
    public int Numerator { get; private set; }

    /// <summary>Moves to the next transition number after a completed step.</summary>
    public void Advance() => Numerator++;

    /// <summary>Restarts the count for the next fade.</summary>
    public void Reset() => Numerator = 0;
}
