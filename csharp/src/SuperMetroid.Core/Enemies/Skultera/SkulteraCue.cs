namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One cue of Skultera's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SkulteraCue(SkulteraCueKind Kind);
