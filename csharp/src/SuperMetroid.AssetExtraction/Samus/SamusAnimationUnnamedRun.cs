using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// A run of a Samus animation program that no segment of the animation names (#2395): delays closed by a cue, laid out
/// after a named segment and before the next, which the layout counts but no read reaches.
/// </summary>
/// <param name="Animation">The animation whose program holds the run.</param>
/// <param name="After">The named segment the run is laid out after.</param>
/// <param name="Delays">How many delays the run holds before its cue.</param>
/// <param name="End">The cue that closes the run.</param>
internal sealed record SamusAnimationUnnamedRun(SamusAnimationName Animation, SamusAnimationSegmentName After, int Delays, SamusAnimationCue End);
