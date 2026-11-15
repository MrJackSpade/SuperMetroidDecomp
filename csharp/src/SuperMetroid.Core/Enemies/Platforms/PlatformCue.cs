namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One cue of the Tripper's and the suspensor platform's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct PlatformCue(PlatformCueKind Kind);
