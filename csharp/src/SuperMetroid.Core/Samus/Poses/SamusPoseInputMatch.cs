namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A decision result, including the distinction between an empty list and exhausted conditions.</summary>
/// <param name="Transition">The input transition the first satisfied condition selects, or none.</param>
/// <param name="HasConditions">Whether the list has conditions, a fact of the match.</param>
[IndependentFlag("whether the matched rule has conditions, a fact of the match")]
internal readonly record struct SamusPoseInputMatch(SamusPoseTransition? Transition, bool HasConditions)
{
    /// <summary>The same decision for the mirrored list: a selected pose faces the other way.</summary>
    internal SamusPoseInputMatch Mirrored() =>
        Transition is { } transition ? this with { Transition = new(SamusPoseMirror.Of(transition.ProspectivePose)) } : this;
}
