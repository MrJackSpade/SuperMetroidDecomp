namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One colour program of the installed room palette-effect document (#2557): the key its colours are stored under, how a
/// failure names it, and the record colours it holds, each record run validated against the document's frames. The
/// Brinstar blue spores' ordinary and Spore Spawn programs share one entry.
/// </summary>
/// <param name="Key">The key the program's frames are stored under in the document.</param>
/// <param name="Runs">The record runs the stored frames give colours to, each named as a failure names it; extraction reads the first.</param>
internal sealed record RoomPaletteFxProgram(string Key, IReadOnlyList<RoomPaletteFxRun> Runs);
