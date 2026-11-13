namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One cue of the Beetom's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct BeetomCue(BeetomCueKind Kind);
