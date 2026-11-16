namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Lower Norfair Ridley's arena reveal has gone (#2228): the step the next reveal update shows, from zero; the step
/// after the last of <see cref="NorfairRidleyRevealDefinitions.RevealStepCount"/> ends the reveal.
/// </summary>
/// <param name="Step">The step the next reveal update shows, zero through the step count, which ends the reveal.</param>
public readonly record struct NorfairArenaRevealProgress(int Step)
{
    /// <summary>Whether the step is one the reveal shows; the step after the last ends it.</summary>
    internal bool ShowsStep => Step < NorfairRidleyRevealDefinitions.RevealStepCount;

    /// <summary>The progress once this step has run.</summary>
    internal NorfairArenaRevealProgress Next() => new(Step + 1);
}
