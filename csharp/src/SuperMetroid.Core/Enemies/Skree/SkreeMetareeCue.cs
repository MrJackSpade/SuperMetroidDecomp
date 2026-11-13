namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One cue of the Skree's and Metaree's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SkreeMetareeCue(SkreeMetareeCueKind Kind);
