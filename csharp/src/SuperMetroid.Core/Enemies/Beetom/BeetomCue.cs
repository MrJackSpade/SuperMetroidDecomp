namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One cue of the Beetom's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct BeetomCue(BeetomCueKind Kind);
