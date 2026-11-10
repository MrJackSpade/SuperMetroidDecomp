namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A colour effect's program a walk of the programs reaches (#1512, #1598), with the colour group the last cue before it
/// named, which the program's frames show in until one of its own cues names another.
/// </summary>
/// <param name="Program">The program reached.</param>
/// <param name="Colours">The colour group named on reaching it, or null before any cue has named one.</param>
internal readonly record struct PaletteFxReachedProgram(PaletteFxProgram Program, PaletteFxColours? Colours);
