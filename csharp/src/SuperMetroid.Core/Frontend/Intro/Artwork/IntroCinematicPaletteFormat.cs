namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    /// <summary>Supported revision of the complete 256-color RGB5 opening-palette schema.</summary>
    public const int Version = 1;
    /// <summary>Native CGRAM palette-row width in RGB5 words.</summary>
    internal const int ColorsPerRow = 16;
    /// <summary>$8C:E4EB-E4F2: reviewed four-shade neutral material cycle.</summary>
    internal const int NeutralCycleLength = IntroCinematicPaintDefinitions.WhiteCycleLength;
    /// <summary>$8C:E4EB-E508: reviewed unit decrement in every RGB5 channel.</summary>
    internal const int NeutralCycleStep = IntroCinematicPaintDefinitions.WhiteCycleStep;
    /// <summary>$8C:E42B-E43A supplies the eight-color gradient also copied atE5AB-E5BA.</summary>
    internal const int SharedCrossFadeSourceRow = 2;
    /// <summary>$8C:E5BB-E5C2: reviewed blue endpoints31/4 derive their equal three-interval decrement.</summary>
    internal const int CrossFadeBlueStep = IntroCinematicPaintDefinitions.CrossfadeBlueStep;
    /// <summary>Editable JSON filename for the opening narration's complete native-precision CGRAM palette.</summary>
    public const string FileName = "intro-narration-palette.json";
}
