namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One cue of Skultera's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SkulteraCue(SkulteraCueKind Kind);
