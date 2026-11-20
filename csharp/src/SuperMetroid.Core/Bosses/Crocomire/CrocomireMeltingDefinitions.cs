namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The shape of Crocomire's dissolve: how many steps each pass takes, the bands of the melt image that show them, and the
/// distortion each pass stretches his body through.
/// </summary>
internal static class CrocomireMeltingDefinitions
{
    /// <summary>
    /// Steps each dissolve pass takes: one column of the melt image is erased per step, in a fixed order over all its
    /// columns, and the step count is the picture each band shows.
    /// </summary>
    public const int ColumnCount = 49;

    /// <summary>Bands of each melt image, one eight-pixel row of the image each.</summary>
    public const int MeltImageBandCount = 6;

    /// <summary>The updates of one round of the first pass's bands: one band shown an update.</summary>
    public const int FirstPassBandRound = MeltImageBandCount;

    /// <summary>
    /// The updates of one round of the second pass's bands: one band shown an update and, after its six bands, an update
    /// that shows none, the cadence the bare skull's melt keeps.
    /// </summary>
    public const int SecondPassBandRound = MeltImageBandCount + 1;

    /// <summary>The slope each pass's distortion starts at: a 256th of a row further behind each line.</summary>
    public static readonly UnsignedSixteenSixteen FirstDistortionSlope = UnsignedSixteenSixteen.FromPixelFraction(1, 256);

    /// <summary>How much steeper the distortion's slope grows each update it stretches, in rows a line.</summary>
    public static readonly UnsignedSixteenSixteen DistortionSlopeGrowth = UnsignedSixteenSixteen.FromPixelFraction(3, 512);

    /// <summary>The steepest the distortion's slope grows, in rows a line, which ends the dissolve once its band is stretched.</summary>
    public static readonly UnsignedSixteenSixteen SteepestDistortionSlope = UnsignedSixteenSixteen.FromPixelFraction(5, 16);

    /// <summary>Lowest background source row the distortion stretches back to, in both passes.</summary>
    public const int LowestDistortionSourceRow = 88;

    /// <summary>
    /// The lines the melt's stretch can reach, counted from the first gameplay line: the band starts and ends within
    /// them, and those below the screen are never shown.
    /// </summary>
    public const int DistortionLines = 256;

    /// <summary>
    /// The height in the room from which down only the top row of Crocomire's melting body stands above the acid, so his
    /// melting pictures show that row alone: the height at which a sink into the acid ends.
    /// </summary>
    public const int TopRowAboveAcidY = CrocomireBridgeCollapseDefinitions.SunkenY;

    /// <summary>The height in the room from which down, above <see cref="TopRowAboveAcidY"/>, his melting pictures show his top two rows.</summary>
    public const int TopTwoRowsAboveAcidY = TopRowAboveAcidY - CrocomireBridgeCollapseDefinitions.MeltRowHeight;

    /// <summary>
    /// The height in the room from which down, above <see cref="TopTwoRowsAboveAcidY"/>, his melting pictures show his
    /// top three rows; higher than this they show his top four.
    /// </summary>
    public const int TopThreeRowsAboveAcidY = TopTwoRowsAboveAcidY - CrocomireBridgeCollapseDefinitions.MeltRowHeight;

    /// <summary>How far below his body his melting tongue is placed, in pixels (#2182).</summary>
    internal const int TongueBelowBody = 16;

    /// <summary>How far right his melting body shakes, in pixels, on every other half of the shake's count.</summary>
    internal const int ShakeOffset = 4;

    /// <summary>The updates each half of his melting body's shake lasts.</summary>
    internal const int ShakeHalfPeriod = 2;

    /// <summary>How many rows the distortion's source row climbs each update it stretches.</summary>
    internal const int DistortionSourceClimb = 3;

    /// <summary>The highest source row the distortion climbs to; a climb that would pass it starts over here, or ends the stretch once it is steepest.</summary>
    internal const int DistortionSourceTop = 16;

    /// <summary>How far above his tongue, in pixels, the melt's band starts.</summary>
    internal const int BandAboveTongue = 72;
}
