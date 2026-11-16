namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Ridley's body fade has gone (#2228, #2233): the shade the next step shows, from black (zero) to his full
/// colours, and whether the first of that shade's two updates has passed; the fade completes once it has shown the last
/// of <see cref="RidleyShadeDefinitions.BodyFadeShadeCount"/>.
/// </summary>
/// <param name="Shade">The shade the next step shows, zero through the shade count, which no step shows.</param>
public readonly record struct RidleyBodyFadeProgress(int Shade)
{
    /// <summary>Whether the update before the next shade has passed, so the next update shows it.</summary>
    public bool WaitedForShade { get; internal init; }

    /// <summary>Whether every shade has shown.</summary>
    internal bool IsComplete => Shade >= RidleyShadeDefinitions.BodyFadeShadeCount;

    /// <summary>The progress once the update before this shade has passed.</summary>
    internal RidleyBodyFadeProgress Waited() => this with { WaitedForShade = true };

    /// <summary>The progress once this shade has shown: the next shade, not yet waited for.</summary>
    internal RidleyBodyFadeProgress Next() => new(Shade + 1);
}
