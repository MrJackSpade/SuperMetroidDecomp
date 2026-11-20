using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Defines the document identity and fixed native full-body palette geometry.</summary>
public static class SamusFullBodyCycleColorFormat
{
    /// <summary>JSON filename containing all Samus full-body cycle colors.</summary>
    public const string FileName = "samus-full-body-cycle-colors.json";
    /// <summary>Supported full-body cycle color schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of supported suit variants: Power, Varia, and Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Number of ordered palette shades in each family and suit.</summary>
    public const int ShadesPerSuit = 4;
    /// <summary>Number of RGB5 entries in each complete Samus OBJ palette.</summary>
    public const int ColorsPerPalette = SamusColourSets.ColoursPerSet;
    /// <summary>Four distinct shade palettes in each of four families for three suits.</summary>
    public const int PaletteCount = SuitCount * ShadesPerSuit * 4;


}
