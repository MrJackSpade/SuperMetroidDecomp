namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A decision result, including the distinction between an empty list and exhausted conditions.</summary>
internal readonly record struct SamusPoseInputMatch(SamusPoseInputRule? Rule, bool HasConditions);
