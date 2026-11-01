namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    /// <summary>Supported revision of the complete 256-color RGB5 opening-palette schema.</summary>
    public const int Version = 1;
    /// <summary>Reviewed four-shade neutral material cycle.</summary>
    internal const int NeutralCycleLength = IntroCinematicPaintDefinitions.WhiteCycleLength;
    /// <summary>Reviewed unit decrement in every RGB5 channel.</summary>
    internal const int NeutralCycleStep = IntroCinematicPaintDefinitions.WhiteCycleStep;
    /// <summary>Reviewed blue endpoints31/4 derive their equal three-interval decrement.</summary>
    internal const int CrossFadeBlueStep = IntroCinematicPaintDefinitions.CrossfadeBlueStep;
    /// <summary>Editable JSON filename for the opening narration's complete full-precision palette.</summary>
    public const string FileName = "intro-narration-palette.json";
}
