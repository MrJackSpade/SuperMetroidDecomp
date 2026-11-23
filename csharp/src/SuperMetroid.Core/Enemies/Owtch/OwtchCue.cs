namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One cue of Owtch's animations: the way it makes main AI move Owtch (#2151).</summary>
/// <param name="Direction">The way Owtch then moves.</param>
internal readonly partial record struct OwtchCue(HorizontalDirection Direction);
