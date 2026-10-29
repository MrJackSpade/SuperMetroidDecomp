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
    /// cartridge shows it.
    /// </summary>
    public const int SecondPassBandCount = 7;

    /// <summary>Characters in one row of a melt image, and so between the first characters of consecutive bands.</summary>
    public const int BandRowCharacters = 16;

    /// <summary>Characters a band shows from the start of its row.</summary>
    public const int BandCharacterCount = 11;

    /// <summary>Lowest BG2 source row the distortion stretches back to, in both passes.</summary>
    public const int LowestDistortionSourceRow = 88;

    /// <summary>Height at which the distortion band stops, in both passes.</summary>
    public const int DistortionEndY = 48;

    /// <summary>
    /// The body-layer cells a melt image's drawing shows, from the layer's first: the sixteen rows the cartridge's transfer
    /// leads the image with, then the eight rows the image covers.
    /// </summary>
    public const int ShownMeltCells = 768;
}
