namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct KiHunterCue(KiHunterCueKind Kind);
