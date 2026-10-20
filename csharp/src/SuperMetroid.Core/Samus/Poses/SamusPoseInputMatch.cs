namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A decision result, including the distinction between an empty list and exhausted conditions.</summary>
[IndependentFlag("whether the matched rule has conditions, a fact of the match")]
internal readonly record struct SamusPoseInputMatch(SamusPoseInputRule? Rule, bool HasConditions)
{
    /// <summary>The same decision for the mirrored list: a selected pose faces the other way.</summary>
    internal SamusPoseInputMatch Mirrored() => Rule is { } rule ? this with { Rule = new(SamusPoseMirror.Of(rule.TargetPose)) } : this;
}
