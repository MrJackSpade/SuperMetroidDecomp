namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How far the fade on Phantoon's body, or the Wrecked Ship's power returning after his death, has run (#1933): the steps it
/// takes, the step it is on and whether it has finished. Each phase that fades clears <see cref="Complete"/> before its
/// first step reads it.
/// </summary>
public sealed class PhantoonBodyFadeProgress
{
    /// <summary>The number of steps the fade takes.</summary>
    public int Denominator { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The step the fade is on.</summary>
    public int Numerator { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the fade has finished.</summary>
    public bool Complete { get; internal set; }
}
