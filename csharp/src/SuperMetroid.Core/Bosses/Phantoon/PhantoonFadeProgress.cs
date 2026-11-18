namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How far one of Phantoon's fades has run (#1933, #2251): the fade on his body, or, as its own progress, the Wrecked
/// Ship's power coming on after his death. A fade of a given length takes one step for each of its steps and one past
/// them, the first moving nothing and the last reaching its goal, and completes on the step after, which starts the next
/// fade over; the phases that wait on a fade read <see cref="Complete"/>, which each phase that fades clears before its
/// first step reads it.
/// </summary>
/// <remarks>
/// The count and the latch stay as they are (#2471), unlike Mother Brain's fades, which each start over on the shared
/// animation player. Phantoon's fades share one count that only a completed fade starts over: a hit, a pattern or his death
/// that moves him on mid-fade clears the latch but leaves the count where the fade stopped, so the next fade begins from
/// that step, and a fade toward his health band reads the band afresh at every step. Each step's goal and starting step are
/// therefore state the fight carries across fades, which a fade of installed images started by its phase would not keep.
/// </remarks>
public sealed class PhantoonFadeProgress
{
    /// <summary>The steps the fade has taken since it last completed, which is the next step's place in the fade.</summary>
    public int Steps { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the fade has finished.</summary>
    public bool Complete { get; internal set; }

    /// <summary>
    /// Takes the fade's next step over <paramref name="length"/> steps: the place of the step taken, zero (moving nothing)
    /// through one past <paramref name="length"/> (reaching the goal), or null when the step completes the fade, which marks
    /// it complete and starts the next over.
    /// </summary>
    /// <param name="length">The fade's length in steps, one or more.</param>
    internal int? TakeStep(int length)
    {
        _ = Ensure.BetweenInclusive(1, int.MaxValue, length);
        if (Steps > length + 1)
        {
            Steps = 0;
            Complete = true;
            return null;
        }
        return Steps++;
    }
}
