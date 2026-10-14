namespace SuperMetroid.Core.Assets;

/// <summary>Installed palette filename and fixed native sequence geometry; color content is editable while attack/fade timing and loop termination remain engine-owned.</summary>
public static class MotherBrainRainbowPaletteFormat
{
    /// <summary>Body-row indices for head, plate, tissue, and independent tail inks.</summary>
    internal static class RedOriginLayout
    {
        /// <summary>$AD:E44E: darker head pair begins at body color index2.</summary>
        internal const int DarkHead = 2;
        /// <summary>$AD:E452-E459: four plate inks at body indices4..7.</summary>
        internal const int PlateStart = 4, PlateEnd = 7;
        /// <summary>Three interpolation steps between the first and last plate inks.</summary>
        internal const int PlateIntervals = PlateEnd - PlateStart;
        /// <summary>$AD:E45A-E463: five tissue inks at body indices8..12.</summary>
        internal const int TissueStart = 8, TissueEnd = 12;
        /// <summary>Four interpolation steps between tissue endpoint inks.</summary>
        internal const int TissueIntervals = TissueEnd - TissueStart;
        /// <summary>$AD:E464-E467: two remaining independent body inks at indices13..14.</summary>
        internal const int TailStart = 13;
    }
    /// <summary>$AD:E44A-E451: first four head/outline inks have red31/25/19/13; chosen decrement6 and paired other-channel policy are chosen paint-animation content.</summary>
    internal const int HeadRedStep = 6;
    /// <summary>$AD:E44A: original red phase, supplying shared channels; selected phase role is chosen paint-animation content.</summary>
    internal const int RedOriginPhase = 0;
    /// <summary>$AD:E486: first green rise shares original red/blue; its independent green inputs are chosen paint-animation content.</summary>
    internal const int FirstGreenRisePhase = 1;
    /// <summary>$AD:E576: phase5 retains original red minus10/green plus5; its independent blue inputs and tint choices are chosen paint-animation content.</summary>
    internal const int MixedShiftPhase = 5;
    /// <summary>$AD:E5B2: phase6 retains original green and reduced red; its independent blue inputs are chosen paint-animation content.</summary>
    internal const int BlueMaximumPhase = 6;
    /// <summary>$AD:E5EE: phase7 retains phase6 blue and increases red; its independent green inputs are chosen paint-animation content.</summary>
    internal const int RedRecoveryPhase = 7;
    /// <summary>$AD:E62A: phase8 is phase6 with restored red plus10; selected phase mapping/tint are chosen paint-animation content.</summary>
    internal const int RedRestoredPhase = 8;
    /// <summary>$AD:E576 versusE44A: selected green addition5 across all15 inks; independent of red/blue additions5 and chosen paint-animation content.</summary>
    internal const int GreenAddition = 5;
    /// <summary>$AD:E5EE versusE5B2: selected red recovery addition5; magnitude is chosen paint-animation content independently of blue tint5.</summary>
    internal const int RedRecoveryAddition = 5;
    /// <summary>$AD:E4C2/E4FE: phase3 copies phase2 with a selected red reduction; phase mapping is chosen paint-animation content.</summary>
    internal const int RedReductionSourcePhase = 2, RedReducedPhase = 3;
    /// <summary>$AD:E4FE: all15 body inks reduce red by10; chosen magnitude is chosen paint-animation content.</summary>
    internal const int RedReduction = 10;
    /// <summary>$AD:E53A: phase4 shifts phase3 from green toward blue; phase mapping is chosen paint-animation content.</summary>
    internal const int BlueGreenShiftPhase = 4;
    /// <summary>$AD:E53A: all15 body inks subtract2 green/add2 blue; selected amount is chosen paint-animation content.</summary>
    internal const int BlueGreenShift = 2;
    /// <summary>$AD:E666: phase9 copies phase0 with increased blue; selected mapping is chosen paint-animation content.</summary>
    internal const int BlueRaisedPhase = 9;
    /// <summary>$AD:E666 versusE44A: all15 body inks add5 blue; selected magnitude is chosen paint-animation content.</summary>
    internal const int BlueAddition = 5;
    /// <summary>Installed editable JSON filename for rainbow, drain/revival, fake-death, restoration, and beam-backdrop colors.</summary>
    public const string FileName = "mother-brain-rainbow-palette.json";
    /// <summary>Ten full-palette phases before the zero terminator in the native $AD:E434 rainbow pointer list.</summary>
    public const int RainbowFrameCount = 10;
    /// <summary>Eight authored steps in each native drain and revival palette pointer sequence; cadence and completion are controlled by boss AI.</summary>
    public const int GreyFrameCount = 8;
    /// <summary>38 sampled BGR555 words before the signed bank-$88 loop terminator.</summary>
    public const int BeamCycleColorCount = 38;
}
