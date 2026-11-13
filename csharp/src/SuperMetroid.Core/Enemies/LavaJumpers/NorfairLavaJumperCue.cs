namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One cue of the Norfair lava jumper's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct NorfairLavaJumperCue(NorfairLavaJumperCueKind Kind);
