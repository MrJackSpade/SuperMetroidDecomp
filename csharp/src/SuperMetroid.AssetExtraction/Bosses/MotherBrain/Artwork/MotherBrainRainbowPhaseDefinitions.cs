namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Which rainbow phases share another phase's channels or tint, and the shared offsets the import recognizes.</summary>
internal static class MotherBrainRainbowPhaseDefinitions
{
    /// <summary>Original red phase, supplying shared channels; selected phase role is chosen paint-animation content.</summary>
    internal const int RedOriginPhase = 0;
    /// <summary>First green rise shares original red/blue; its independent green inputs are chosen paint-animation content.</summary>
    internal const int FirstGreenRisePhase = 1;
    /// <summary>Phase5 retains original red minus10/green plus5; its independent blue inputs and tint choices are chosen paint-animation content.</summary>
    internal const int MixedShiftPhase = 5;
    /// <summary>Phase6 retains original green and reduced red; its independent blue inputs are chosen paint-animation content.</summary>
    internal const int BlueMaximumPhase = 6;
    /// <summary>Phase7 retains phase6 blue and increases red; its independent green inputs are chosen paint-animation content.</summary>
    internal const int RedRecoveryPhase = 7;
    /// <summary>Phase8 is phase6 with restored red plus10; selected phase mapping/tint are chosen paint-animation content.</summary>
    internal const int RedRestoredPhase = 8;
    /// <summary>Selected green addition5 across all15 inks; independent of red/blue additions5 and chosen paint-animation content.</summary>
    internal const int GreenAddition = 5;
    /// <summary>Selected red recovery addition5; magnitude is chosen paint-animation content independently of blue tint5.</summary>
    internal const int RedRecoveryAddition = 5;
    /// <summary>Phase3 copies phase2 with a selected red reduction; phase mapping is chosen paint-animation content.</summary>
    internal const int RedReductionSourcePhase = 2, RedReducedPhase = 3;
    /// <summary>All15 body inks reduce red by10; chosen magnitude is chosen paint-animation content.</summary>
    internal const int RedReduction = 10;
    /// <summary>Phase4 shifts phase3 from green toward blue; phase mapping is chosen paint-animation content.</summary>
    internal const int BlueGreenShiftPhase = 4;
    /// <summary>All15 body inks subtract2 green/add2 blue; selected amount is chosen paint-animation content.</summary>
    internal const int BlueGreenShift = 2;
    /// <summary>Phase9 copies phase0 with increased blue; selected mapping is chosen paint-animation content.</summary>
    internal const int BlueRaisedPhase = 9;
    /// <summary>All15 body inks add5 blue; selected magnitude is chosen paint-animation content.</summary>
    internal const int BlueAddition = 5;
}
