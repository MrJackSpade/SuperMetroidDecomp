namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One cue of a Tourian boss statue's picture animation: the event it signals.</summary>
/// <param name="Kind">What the cue does or signals.</param>
internal readonly record struct TourianStatueBlockCue(TourianStatueBlockCueKind Kind);
