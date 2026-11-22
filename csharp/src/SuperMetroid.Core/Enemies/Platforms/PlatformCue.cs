namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One cue of the Tripper's and the suspensor platform's animations: the way it makes main AI move the platform (#2151).</summary>
/// <param name="Movement">The horizontal movement main AI then applies.</param>
internal readonly partial record struct PlatformCue(HorizontalDirection Movement);
