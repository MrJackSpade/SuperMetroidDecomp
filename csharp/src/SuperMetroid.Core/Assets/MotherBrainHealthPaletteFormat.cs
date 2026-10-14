namespace SuperMetroid.Core.Assets;

/// <summary>Installed JSON identity and dimensions for the four final-phase health-dependent palettes selected by native <c>MotherBrainHealthBasedPaletteHandling</c> at <c>$AD:E3D5</c>.</summary>
public static class MotherBrainHealthPaletteFormat
{
    /// <summary>Installed resource filename loaded with area-map presentation assets for Mother Brain's body/brain and rear-leg damage colors.</summary>
    public const string FileName = "mother-brain-health-palette.json";
    /// <summary>Supported schema version requiring paired body and rear-leg arrays with four fifteen-color RGB5 rows each.</summary>
    public const int Version = 1;
    /// <summary>Four health-band selectors in least-to-most-damaged order; excludes the unused fifth entry present in the native pointer tables.</summary>
    public const int StateCount = 4;
}
