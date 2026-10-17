namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One cue of a Tourian boss statue's animated-tile program.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a branching cue continues at when its condition holds.</param>
internal readonly record struct TourianStatueTileCue(TourianStatueTileCueKind Kind, TourianStatueTileProgram Target = default);
