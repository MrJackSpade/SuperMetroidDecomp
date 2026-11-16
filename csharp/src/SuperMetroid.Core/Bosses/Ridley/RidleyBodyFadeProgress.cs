namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Ridley's body fade has gone (#2228): the shade the next step shows, from black (zero) to his full colours; the
/// fade completes once it has shown the last of <see cref="RidleyShadeDefinitions.BodyFadeShadeCount"/>.
/// </summary>
/// <param name="Shade">The shade the next step shows, zero through the shade count, which no step shows.</param>
public readonly record struct RidleyBodyFadeProgress(int Shade)
{
    /// <summary>Whether every shade has shown.</summary>
    internal bool IsComplete => Shade >= RidleyShadeDefinitions.BodyFadeShadeCount;

    /// <summary>The progress once this shade has shown.</summary>
    internal RidleyBodyFadeProgress Next() => new(Shade + 1);
}
