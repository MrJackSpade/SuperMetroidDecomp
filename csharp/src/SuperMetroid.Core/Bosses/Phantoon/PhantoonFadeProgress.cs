namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How far one of Phantoon's fades has run (#1933, #2251): the fade on his body, or, as its own progress, the Wrecked
/// Ship's power coming on after his death. A fade of a given length takes one step for each of its steps and one past
/// them, the first moving nothing and the last reaching its goal, and finishes on the step after, which starts the next
/// fade over. A finished fade takes no further step until a phase that fades resumes it (<see cref="Resume"/>) before its
/// first step; the phases that wait on a fade read <see cref="IsFinished"/>.
/// </summary>
/// <remarks>
/// The count and the finish stay as they are (#2471, #2562), unlike Mother Brain's fades, which each start over on the
/// shared animation player. Phantoon's fades share one count that only a finished fade starts over: a hit, a pattern or his
/// death that moves him on mid-fade resumes the fade but leaves the count where the fade stopped, so the next fade begins
/// from that step, and a fade toward his health band reads the band afresh at every step. Each step's goal and starting step are
/// therefore state the fight carries across fades, which a fade of installed images started by its phase would not keep.
/// </remarks>
public sealed class PhantoonFadeProgress
{
    /// <summary>The steps the fade has taken since it last completed, which is the next step's place in the fade.</summary>
    private int Steps { get; set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the fade has finished: it takes no step until a phase that fades resumes it.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>A phase that fades lets the fade run again, from the step its count stands at.</summary>
    internal void Resume() => IsFinished = false;

    /// <summary>
    /// Takes the fade's next step over <paramref name="length"/> steps: the place of the step taken, zero (moving nothing)
    /// through one past <paramref name="length"/> (reaching the goal), or null when the step finishes the fade, which marks
    /// it finished and starts the next over.
    /// </summary>
    /// <param name="length">The fade's length in steps, one or more.</param>
    internal int? TakeStep(int length)
    {
        _ = Ensure.BetweenInclusive(1, int.MaxValue, length);
        if (Steps > length + 1)
        {
            Steps = 0;
            IsFinished = true;
            return null;
        }
        return Steps++;
    }
}
