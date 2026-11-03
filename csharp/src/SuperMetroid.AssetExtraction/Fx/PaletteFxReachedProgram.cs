using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// A palette-FX program the import's walk reaches (#1512), with the colour group the last cue before it named, which the
/// program's records write until one of its own cues names another.
/// </summary>
/// <param name="Program">The program reached.</param>
/// <param name="Colours">The colour group named on reaching it, or null before any cue has named one.</param>
internal readonly record struct PaletteFxReachedProgram(PaletteFxProgram Program, PaletteFxColours? Colours);
