namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How far one of Mother Brain's colour fades has come (#2251): the steps it has taken since it started. Each fade keeps
/// its own progress: greying at her fake death, the return from that grey, greying as the Baby drains her, her revival
/// from that grey, her final death's body fade and her corpse's greying. Each takes one step for each of its colourings
/// and completes on the step after its last, which shows no new colours.
/// </summary>
public sealed class MotherBrainFadeProgress
{
    /// <summary>The steps the fade has taken since it started, which is the next step's place in the fade.</summary>
    public int Steps { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Starts the fade over, before its first step.</summary>
    internal void Restart() => Steps = 0;

    /// <summary>Takes the fade's next step: its place in the fade, counting from zero.</summary>
    internal int TakeStep() => Steps++;
}
