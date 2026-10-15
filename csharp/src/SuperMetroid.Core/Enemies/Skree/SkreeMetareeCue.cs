namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One cue of the Skree's and Metaree's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SkreeMetareeCue(SkreeMetareeCueKind Kind);
