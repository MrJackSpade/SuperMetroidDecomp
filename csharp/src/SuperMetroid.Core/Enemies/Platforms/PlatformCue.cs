namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One cue of the Tripper's and Kamer's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct PlatformCue(PlatformCueKind Kind);
