namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Ridley's eye fade has gone (#2228): the step of its schedule (<see cref="RidleyEyeFade"/>) the next update
/// shows, from zero; the schedule's last step completes the fade.
/// </summary>
/// <param name="Step">The step the next update shows, zero through the schedule's completing step.</param>
public readonly record struct RidleyEyeFadeProgress(int Step)
{
    /// <summary>The progress once this step has shown.</summary>
    internal RidleyEyeFadeProgress Next() => new(Step + 1);
}
