namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct KiHunterCue(KiHunterCueKind Kind);
