namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One cue of the Tripper's and Kamer's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct PlatformCue(PlatformCueKind Kind);
