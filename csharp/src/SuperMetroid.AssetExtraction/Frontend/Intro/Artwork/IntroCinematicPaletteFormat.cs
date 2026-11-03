namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The shade relationships the opening narration palette is held as.</summary>
public static class IntroCinematicPaletteFormat
{
    /// <summary>Reviewed four-shade neutral material cycle.</summary>
    internal const int NeutralCycleLength = IntroCinematicPaintDefinitions.WhiteCycleLength;
    /// <summary>Reviewed unit decrement in every RGB5 channel.</summary>
    internal const int NeutralCycleStep = IntroCinematicPaintDefinitions.WhiteCycleStep;
    /// <summary>Reviewed blue endpoints31/4 derive their equal three-interval decrement.</summary>
    internal const int CrossFadeBlueStep = IntroCinematicPaintDefinitions.CrossfadeBlueStep;
}
