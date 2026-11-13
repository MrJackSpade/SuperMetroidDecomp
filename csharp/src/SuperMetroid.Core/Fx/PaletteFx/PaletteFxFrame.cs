namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// One frame of a colour effect's animation (#1598): the program whose steps show it and its place among the frames that
/// program shows. The import pre-colours the sheets each frame shows from its installed colours.
/// </summary>
/// <param name="Animation">The program that shows the frame.</param>
/// <param name="Frame">The frame's place among the frames <paramref name="Animation"/> shows, from zero.</param>
public readonly record struct PaletteFxFrame(PaletteFxSegment Animation, int Frame);
