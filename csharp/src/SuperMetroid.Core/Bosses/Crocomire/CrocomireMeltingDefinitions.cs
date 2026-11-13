namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The shape of Crocomire's dissolve: how many steps each pass takes, the bands of the melt image that show them, and the
/// distortion each pass stretches his body through.
/// </summary>
internal static class CrocomireMeltingDefinitions
{
    /// <summary>
    /// Steps each dissolve pass takes: one column of the melt image is erased per step, in a fixed order over its
    /// forty-nine columns, and the step count is the picture each band shows.
    /// </summary>
    public const int ColumnCount = 49;

    /// <summary>Bands of the first melt image, one row of its characters each, shown one per update.</summary>
    public const int FirstPassBandCount = 6;

    /// <summary>
    /// Bands of the second melt image. Its seventh band shows what follows the image's six rows of characters, as the
    /// shows it.
    /// </summary>
    public const int SecondPassBandCount = 7;

    /// <summary>Lowest background source row the distortion stretches back to, in both passes.</summary>
    public const int LowestDistortionSourceRow = 88;

    /// <summary>
    /// The lines the melt's stretch can reach, counted from the first gameplay line: the band starts and ends within
    /// them, and those below the screen are never shown.
    /// </summary>
    public const int DistortionLines = 256;
}
