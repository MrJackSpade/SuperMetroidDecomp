namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One cue of the Tripper's and Kamer's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct PlatformCue(PlatformCueKind Kind);
