namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A colour fade counted in its own steps at its own cadence (#2488), as a <see cref="ScreenFade"/> steps the brightness
/// at its own: the ending's explosion crossfade and the post-credits reward's colours count how far they have come with
/// it, and the scene that runs the fade owns it. The fade takes its first step on the update its start names and one
/// every so many updates after, until it has taken them all.
/// </summary>
internal sealed partial class PacedFadeSteps
{
    /// <summary>A fade of <paramref name="steps"/> steps that has taken none yet, paced as <see cref="PacedFadeSteps"/> says.</summary>
    /// <param name="steps">The fade's steps from its start to its goal, one or more.</param>
    /// <param name="updatesPerStep">The updates the fade takes from one step to the next, one or more.</param>
    /// <param name="firstStepUpdate">The update, counted from one, on which the fade takes its first step.</param>
    /// <exception cref="ArgumentOutOfRangeException">A count is below one.</exception>
    internal PacedFadeSteps(int steps, int updatesPerStep, int firstStepUpdate)
    {
        Steps = Ensure.BetweenInclusive(1, int.MaxValue, steps);
        this.updatesPerStep = Ensure.BetweenInclusive(1, int.MaxValue, updatesPerStep);
        stepWait = UpdateCountdown.Lasting(firstStepUpdate);
    }

    /// <summary>The updates the fade takes from one step to the next.</summary>
    private readonly int updatesPerStep;

    /// <summary>The updates the fade takes to its next step, the update that takes it included.</summary>
    private readonly UpdateCountdown stepWait;

    /// <summary>The fade's steps from its start to its goal.</summary>
    internal int Steps { get; }

    /// <summary>The steps the fade has taken, from none at its start to <see cref="Steps"/> at its goal.</summary>
    internal int Taken { get; private set; }

    /// <summary>The steps the fade has still to take before its goal.</summary>
    internal int Left => Steps - Taken;

    /// <summary>Whether the fade has taken every step and shows its goal.</summary>
    internal bool IsComplete => Taken == Steps;

    /// <summary>
    /// One update of the fade: on the update its cadence is due it takes its next step, unless it has taken them all.
    /// </summary>
    internal void Advance()
    {
        if (!stepWait.Step())
            return;
        stepWait.Restart(updatesPerStep);
        if (!IsComplete)
            Taken++;
    }

    /// <summary>One update of the fade, as <see cref="Advance"/> runs it, for a scene that shows each step it takes.</summary>
    /// <returns>Whether the update took a step.</returns>
    internal bool AdvanceTakingStep()
    {
        int taken = Taken;
        Advance();
        return Taken != taken;
    }
}
