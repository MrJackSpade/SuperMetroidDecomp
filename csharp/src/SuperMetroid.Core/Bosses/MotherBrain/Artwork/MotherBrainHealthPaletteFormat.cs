namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Installed JSON identity and dimensions for the four final-phase health-dependent palettes selected by <c>MotherBrainHealthBasedPaletteHandling</c>.</summary>
public static class MotherBrainHealthPaletteFormat
{
    /// <summary>Four health-band selectors in least-to-most-damaged order; excludes the unused fifth entry present in the pointer tables.</summary>
    public const int StateCount = 4;
}
