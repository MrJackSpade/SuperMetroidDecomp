namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed rainbow palette document's fade and beam geometry, which the import reads, writes and recognizes
/// (#1502); the game only plays the compiled colours.
/// </summary>
public static class MotherBrainRainbowPaletteGeometry
{
    /// <summary>Eight authored steps in each drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;

    /// <summary>38 sampled BGR555 words before the signed loop terminator.</summary>
    public const int BeamCycleColorCount = 38;

    /// <summary>The body and brain colours each drained-to-grey image rewrites.</summary>
    public const int DrainedColors = 15;

    /// <summary>The body and brain colours each revival image rewrites.</summary>
    public const int RevivalColors = 13;

    /// <summary>The rear-leg colours each drained or revival image rewrites after the body and brain colours.</summary>
    public const int BackLegCount = 5;

    /// <summary>The colours in each rainbow image's body, brain and rear-leg sets, after their transparent first colour.</summary>
    public const int ColorCount = 15;

    /// <summary>The brain colours each fake-death fade image rewrites, after the transparent first colour.</summary>
    public const int FakeDeathColors = 3;

    /// <summary>First four head/outline inks have red31/25/19/13; chosen decrement6 and paired other-channel policy are chosen paint-animation content.</summary>
    public const int HeadRedStep = 6;

    /// <summary>Body colour indices for head, plate, tissue, and independent tail inks.</summary>
    public static class RedOrigin
    {
        /// <summary>Darker head pair begins at body color index2.</summary>
        public const int DarkHead = 2;
        /// <summary>Four plate inks at body indices4..7.</summary>
        public const int PlateStart = 4, PlateEnd = 7;
        /// <summary>Three interpolation steps between the first and last plate inks.</summary>
        public const int PlateIntervals = PlateEnd - PlateStart;
        /// <summary>Five tissue inks at body indices8..12.</summary>
        public const int TissueStart = 8, TissueEnd = 12;
        /// <summary>Four interpolation steps between tissue endpoint inks.</summary>
        public const int TissueIntervals = TissueEnd - TissueStart;
        /// <summary>Two remaining independent body inks at indices13..14.</summary>
        public const int TailStart = 13;
    }
}
