namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    /// <summary>Supported revision of the complete 256-color RGB5 opening-palette schema.</summary>
    public const int Version = 1;
    /// <summary>$8C:E4EB-E4F2: reviewed four-shade neutral material cycle.</summary>
    internal const int NeutralCycleLength = IntroCinematicPaintDefinitions.WhiteCycleLength;
    /// <summary>$8C:E4EB-E508: reviewed unit decrement in every RGB5 channel.</summary>
    internal const int NeutralCycleStep = IntroCinematicPaintDefinitions.WhiteCycleStep;
    /// <summary>The background set ($8C:E42B-E43A) whose eight-color sepia gradient the cross-fade sprite set repeats.</summary>
    internal const int SharedCrossFadeSourceBackgroundSet = 2;
    /// <summary>The first object sprite set ($8C:E4E9-E508), which repeats a neutral ramp after its transparent colour.</summary>
    internal const int NeutralCycleSpriteSet = 0;
    /// <summary>The cross-fade sprite set ($8C:E5A9-E5C8), whose first eight visible inks repeat the shared gradient.</summary>
    internal const int CrossFadeSpriteSet = 6;
    /// <summary>$8C:E5BB-E5C2: reviewed blue endpoints31/4 derive their equal three-interval decrement.</summary>
    internal const int CrossFadeBlueStep = IntroCinematicPaintDefinitions.CrossfadeBlueStep;
    /// <summary>Editable JSON filename for the opening narration's complete native-precision CGRAM palette.</summary>
    public const string FileName = "intro-narration-palette.json";
}
