namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A phase of a colour effect's animation that a walk of the animations reaches (#1512, #1598, #1600), with the colour
/// group the last event before it named, which its frames show in until one of its own events names another.
/// </summary>
/// <param name="Animation">The segment the animation starts at.</param>
/// <param name="Phase">The phase reached.</param>
/// <param name="Colours">The colour group named on reaching it, or null before any event has named one.</param>
internal readonly record struct PaletteFxReachedAnimation(PaletteFxSegment Animation, int Phase, PaletteFxColours? Colours);
