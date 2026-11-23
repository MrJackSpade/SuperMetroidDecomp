namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One step of the assembled logo's colour crossfade (#2361): the logo shows each step's background and sprite colours in
/// turn, then hands off to the percentage text after the last.
/// </summary>
public readonly partial record struct EndingLogoCrossfadeStep
{
    /// <summary>The step at <paramref name="step"/> of the crossfade.</summary>
    private EndingLogoCrossfadeStep(int step) => Step = step;

    /// <summary>The step's place in the crossfade, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>Every step of the crossfade, in the order the logo shows them.</summary>
    public static IReadOnlyList<EndingLogoCrossfadeStep> All { get; } =
        [.. Enumerable.Range(0, EndingLogoDefinitions.CrossfadeSteps).Select(step => new EndingLogoCrossfadeStep(step))];

    /// <summary>The crossfade's first step.</summary>
    public static EndingLogoCrossfadeStep First => All[0];

    /// <summary>The step after this one, or null after the crossfade's last.</summary>
    public EndingLogoCrossfadeStep? Following => Step + 1 < All.Count ? All[Step + 1] : null;
}
