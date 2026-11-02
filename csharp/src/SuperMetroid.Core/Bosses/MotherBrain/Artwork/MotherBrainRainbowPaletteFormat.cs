namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Installed palette filename and fixed sequence geometry; color content is editable while attack/fade timing and loop termination remain engine-owned.</summary>
public static class MotherBrainRainbowPaletteFormat
{
    /// <summary>Body colour indices for head, plate, tissue, and independent tail inks.</summary>
    internal static class RedOriginLayout
    {
        /// <summary>Darker head pair begins at body color index2.</summary>
        internal const int DarkHead = 2;
        /// <summary>Four plate inks at body indices4..7.</summary>
        internal const int PlateStart = 4, PlateEnd = 7;
        /// <summary>Three interpolation steps between the first and last plate inks.</summary>
        internal const int PlateIntervals = PlateEnd - PlateStart;
        /// <summary>Five tissue inks at body indices8..12.</summary>
        internal const int TissueStart = 8, TissueEnd = 12;
        /// <summary>Four interpolation steps between tissue endpoint inks.</summary>
        internal const int TissueIntervals = TissueEnd - TissueStart;
        /// <summary>Two remaining independent body inks at indices13..14.</summary>
        internal const int TailStart = 13;
    }
    /// <summary>First four head/outline inks have red31/25/19/13; chosen decrement6 and paired other-channel policy are chosen paint-animation content.</summary>
    internal const int HeadRedStep = 6;
    /// <summary>Ten full-palette phases before the terminator in the rainbow palette list.</summary>
    public const int RainbowFrameCount = 10;
    /// <summary>Eight authored steps in each drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;
    /// <summary>38 sampled BGR555 words before the signed loop terminator.</summary>
    public const int BeamCycleColorCount = 38;
}
