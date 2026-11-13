namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One cue of a Tourian boss statue's picture animation.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The segment a branching cue continues at when its condition holds.</param>
internal readonly record struct TourianStatueTileCue(TourianStatueTileCueKind Kind, TourianStatueTileSegment Target = default);
